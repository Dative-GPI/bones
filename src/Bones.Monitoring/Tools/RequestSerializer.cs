using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Bones.Monitoring.Core
{
    public class RequestSerializer: IRequestSerializer
    {
        public string Serialize(object request)
        {
            return JsonSerializer.Serialize(request, options: new JsonSerializerOptions
            {
                Converters =
                {
                    new RequestJsonConverter()
                }
            });
        }
    }
    
    public class RequestJsonConverter : JsonConverter<object>
    {
        public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            var properties = value.GetType().GetProperties();

            foreach (var prop in properties)
            {
                try
                {
                    var propValue = prop.GetValue(value);
                    JsonSerializer.Serialize(writer, propValue, prop.PropertyType, options);
                }
                catch
                {
                    // Ignorer l'exception et continuer
                    writer.WriteString(prop.Name, "Error: property serializing failed");
                }
            }

            writer.WriteEndObject();
        }
    }
}