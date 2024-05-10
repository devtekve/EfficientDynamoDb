using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EfficientDynamoDb.Attributes;
using EfficientDynamoDb.Converters;
using EfficientDynamoDb.Exceptions;
using EfficientDynamoDb.Internal.Reader;

namespace EfficientDynamoDb.Internal.Metadata
{
    internal sealed class DdbClassInfo
    {
        public Type Type { get; }
        
        public DdbClassType ClassType { get; }
        
        public Type? ElementType { get; }
        
        public DdbClassInfo? ElementClassInfo { get; }
        
        public DdbConverter ConverterBase { get; }
        
        public Type ConverterType { get; }
        
        public Dictionary<string, DdbPropertyInfo> AttributesMap { get; }
        
        public Dictionary<string, DdbPropertyInfo> PropertiesMap { get; }
        
        public JsonReaderDictionary<DdbPropertyInfo> JsonProperties { get; }
        
        public DdbPropertyInfo[] Properties { get; }
        
        public Func<object>? Constructor { get; }
        
        public string? TableName { get; }
        
        public DdbPropertyInfo? PartitionKey { get; }
        
        public DdbPropertyInfo? SortKey { get; }
        
        public DdbPropertyInfo? Version { get; }

        public DdbClassInfo(Type type, DynamoDbContextMetadata metadata, DdbConverter converter)
        {
            Type = type;
            
            var properties = new Dictionary<string, DdbPropertyInfo>();
            var jsonProperties = new JsonReaderDictionary<DdbPropertyInfo>();
            
            ConverterBase = converter;
            ConverterType = converter.GetType();
            ClassType = ConverterBase.ClassType;
            var typeInterfaces = new Stack<Type>(type.GetInterfaces());

            // Check if any of the interfaces has DynamoDbAutoPropertyAttribute
            var autoProperty = typeInterfaces.Any(x => x.GetCustomAttribute<DynamoDbAutoPropertyAttribute>() != null);
            
            // We store explicitly ignored properties to avoid adding them implicitly via auto property
            var explicitlyIgnored = typeInterfaces.SelectMany(x => x.GetProperties())
                .Where(x => x.GetCustomAttribute<DynamoDbIgnoreAttribute>() != null)
                .Select(p => p.Name)
                .ToHashSet();

            switch (ClassType)
            {
                case DdbClassType.Object:
                {
                    for (var currentType = type; currentType != null || typeInterfaces.Count > 0; currentType = currentType?.BaseType)
                    {
                        if (currentType?.BaseType == null && typeInterfaces.Count <= 0)
                            continue;
                        
                        currentType ??= typeInterfaces.Pop();
                        
                        // If auto property is not set, check if the current type has DynamoDbAutoPropertyAttribute
                        if(!autoProperty)
                            autoProperty = currentType.GetCustomAttribute<DynamoDbAutoPropertyAttribute>() != null;
                        
                        const BindingFlags bindingFlags =
                            BindingFlags.Instance |
                            BindingFlags.Public |
                            BindingFlags.NonPublic |
                            BindingFlags.DeclaredOnly;

                        foreach (PropertyInfo propertyInfo in currentType.GetProperties(bindingFlags))
                        {
                            if (propertyInfo.GetCustomAttribute<DynamoDbIgnoreAttribute>() != null)
                                continue;
                            
                            var attribute = propertyInfo.GetCustomAttribute<DynamoDbPropertyAttribute>();
                            
                            // If property is an auto property and doesn't have a DynamoDbPropertyAttribute and it's not explicitly ignored
                            if (autoProperty && attribute == null && !explicitlyIgnored.Contains(propertyInfo.Name)) 
                                attribute ??= new DynamoDbPropertyAttribute(propertyInfo.Name);
                            
                            if (attribute == null)
                                continue;

                            if (properties.ContainsKey(attribute.Name))
                                continue;

                            var propertyConverter = metadata.GetOrAddConverter(propertyInfo.PropertyType, attribute.DdbConverterType);

                            var ddbPropertyInfo = propertyConverter.CreateDdbPropertyInfo(propertyInfo, attribute.Name, attribute.AttributeType, metadata);
                            properties.Add(attribute.Name, ddbPropertyInfo);
                            jsonProperties.Add(attribute.Name, ddbPropertyInfo);

                            switch (attribute.AttributeType)
                            {
                                case DynamoDbAttributeType.PartitionKey:
                                    if (PartitionKey != null)
                                        throw new DdbException($"An entity {Type.FullName} contains multiple partition key attributes");
                                    PartitionKey = ddbPropertyInfo;
                                    break;
                                case DynamoDbAttributeType.SortKey:
                                    if (SortKey != null)
                                        throw new DdbException($"An entity {Type.FullName} contains multiple sort key attributes");
                                    SortKey = ddbPropertyInfo;
                                    break;
                            }

                            if (Version == null && propertyInfo.GetCustomAttribute<DynamoDbVersionAttribute>() != null)
                                Version = ddbPropertyInfo;
                        }

                        TableName ??= currentType.GetCustomAttribute<DynamoDbTableAttribute>()?.TableName;
                    }
                    Constructor = EmitMemberAccessor.CreateConstructor(type) ?? throw new InvalidOperationException($"Can't generate constructor delegate for type '{type}'.");
                    
                    break;
                }
                case DdbClassType.Enumerable:
                case DdbClassType.Dictionary:
                {
                    ElementType = ConverterBase.ElementType;
                    ElementClassInfo = metadata.GetOrAddClassInfo(ElementType!);
                    break;
                }
            }


            AttributesMap = properties;
            PropertiesMap = properties.Values.ToDictionary(x => x.PropertyInfo.Name);
            JsonProperties = jsonProperties;
            Properties = properties.Values.ToArray();
        }
    }
}