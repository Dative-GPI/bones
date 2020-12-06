using System.Text.Json;

namespace Bones
{
    public static class TextExtensions
    {
        public static JsonSerializerOptions Insensitive = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}