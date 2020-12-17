using System.Text.Json;
using EfficientDynamoDb.DocumentModel.AttributeValues;
using EfficientDynamoDb.Internal.Constants;
using EfficientDynamoDb.Internal.Extensions;

namespace EfficientDynamoDb.Internal.Mapping.Converters.Primitives.Numbers
{
    internal sealed class LongDdbConverter : NumberDdbConverter<long>
    {
        public override long Read(in AttributeValue attributeValue) => attributeValue.AsNumberAttribute().ToLong();
        
        public override void Write(Utf8JsonWriter writer, string attributeName, ref long value)
        {
            writer.WritePropertyName(attributeName);
            
            writer.WriteStartObject();
            writer.WriteString(DdbTypeNames.Number, value);
            writer.WriteEndObject();
        }
    }
}