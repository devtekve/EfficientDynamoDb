using System;

namespace EfficientDynamoDb.Attributes
{
    /// <summary>
    /// When used on a class that is marked with <see cref="DynamoDbAutoPropertyAttribute"/>
    /// it will ignore the property from being mapped to DynamoDb attribute automatically.
    /// </summary>
    /// <remarks>
    /// Even if a parent property is marked with <see cref="DynamoDbIgnoreAttribute"/>,
    /// you can still override it in a child class with <see cref="DynamoDbPropertyAttribute"/>, as you are explicitly
    /// requesting to map it to DynamoDb attribute.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class DynamoDbIgnoreAttribute : Attribute
    {
        
    }
}