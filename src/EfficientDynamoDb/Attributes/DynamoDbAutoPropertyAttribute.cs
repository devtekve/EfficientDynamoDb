using System;

namespace EfficientDynamoDb.Attributes
{
    /// <summary>
    /// Marks a class (or interface) that all properties from it, and it's derivates should be
    /// automatically mapped to DynamoDb attributes, that means all child properties except those
    /// marked with <see cref="DynamoDbIgnoreAttribute"/>. 
    /// </summary>
    /// <remarks>
    /// Even if the property is not defined in the class or the interface, but it's defined on a child class that
    /// implements it will be automatically mapped to DynamoDb attribute
    /// unless they are explicitly marked to be ignored with <see cref="DynamoDbIgnoreAttribute"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class)]
    public sealed class DynamoDbAutoPropertyAttribute : Attribute
    {
    }
}