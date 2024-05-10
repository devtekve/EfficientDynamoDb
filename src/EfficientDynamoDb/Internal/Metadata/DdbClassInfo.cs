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
        
        private static IEnumerable<Type> GetBaseTypes(Type type)
        {
            var currentType = type.BaseType;
            while (currentType != null)
            {
                yield return currentType;
                currentType = currentType.BaseType;
            }
        }

        private const BindingFlags BindingFlags =
            global::System.Reflection.BindingFlags.Instance |
            global::System.Reflection.BindingFlags.Public |
            global::System.Reflection.BindingFlags.NonPublic |
            global::System.Reflection.BindingFlags.DeclaredOnly;

        public DdbClassInfo(Type type, DynamoDbContextMetadata metadata, DdbConverter converter)
        {
            Type = type;
            
            var properties = new Dictionary<string, DdbPropertyInfo>();
            var jsonProperties = new JsonReaderDictionary<DdbPropertyInfo>();
            
            ConverterBase = converter;
            ConverterType = converter.GetType();
            ClassType = ConverterBase.ClassType;
            TableName ??= type.GetCustomAttribute<DynamoDbTableAttribute>()?.TableName;

            switch (ClassType)
            {
                case DdbClassType.Object:
                {
                    var typeInterfaces = type.GetInterfaces().ToHashSet();
                    var baseTypes = GetBaseTypes(type).ToHashSet();
                    
                    TableName ??= typeInterfaces.Select(x => x.GetCustomAttribute<DynamoDbTableAttribute>()?.TableName).FirstOrDefault(x => x != null) ?? 
                                  baseTypes.Select(x => x.GetCustomAttribute<DynamoDbTableAttribute>()?.TableName).FirstOrDefault(x => x != null);

                    // Check if any of the interfaces has DynamoDbAutoPropertyAttribute
                    var autoProperty = 
                        baseTypes.Any(x => x.GetCustomAttribute<DynamoDbAutoPropertyAttribute>() != null) ||
                        typeInterfaces.Any(x => x.GetCustomAttribute<DynamoDbAutoPropertyAttribute>() != null);
            
                    // We store explicitly ignored properties to avoid adding them implicitly via auto property
                    var explicitlyIgnored = typeInterfaces.SelectMany(x => x.GetProperties())
                        .Where(x => x.GetCustomAttribute<DynamoDbIgnoreAttribute>() != null)
                        .Select(p => p.Name)
                        .ToHashSet();
                    
                    var propList = 
                        baseTypes.SelectMany(x => x.GetProperties(BindingFlags));

                    var allProps = propList.Concat(type.GetProperties(BindingFlags));

                    foreach (PropertyInfo propertyInfo in allProps)
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