using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

using Bones.Monitoring.Core;

namespace Bones.AspNetCore
{
    public class PostAsGetMiddleware : MonitoredMiddleware
    {
        public PostAsGetMiddleware(
            IServiceProvider serviceProvider,
            RequestDelegate next) : base(next, serviceProvider)
        {
        }

        protected override async Task HandleAsync(HttpContext context, RequestDelegate next)
        {
            if (context.Request.Method == "POST"
                && context.Request.Query.ContainsKey("_method")
                && context.Request.Query["_method"] == "GET"
                && context.Request.HasJsonContentType())
            {
                // Convert the request to a GET request
                context.Request.Method = "GET";
                var doc = await JsonSerializer.DeserializeAsync<Dictionary<string, JsonElement>>(context.Request.Body);
                if (doc != null)
                {
                    var props = Flat(doc);
                    var query = $"?{String.Join("&", props.Select(kv => $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"))}";
                    context.Request.QueryString = new QueryString(query);
                    context.Request.Body = Stream.Null;
                    context.Request.ContentLength = 0;
                }
            }
            await next(context);
            return;
        }

        private Dictionary<string, StringValues> Flat(Dictionary<string, JsonElement> payload)
        {
            Dictionary<string, StringValues> result = new Dictionary<string, StringValues>();
            foreach (var item in payload)
            {
                var flatted = DeepFlat(item.Key, item.Value);
                foreach (var flat in flatted)
                {
                    result.Add(flat.Key, flat.Value);
                }
            }
            return result;
        }

        private IEnumerable<KeyValuePair<string, string>> DeepFlat(string key, JsonElement value)
        {
            switch (value.ValueKind)
            {
                case JsonValueKind.Array when value.GetArrayLength() == 0:
                    return new[] { new KeyValuePair<string, string>($"{key}[]", "") };
                case JsonValueKind.Array when value.GetArrayLength() != 0:
                    return value.EnumerateArray().SelectMany((item, index) => DeepFlat($"{key}[{index}]", item));
                case JsonValueKind.Object:
                    return value.EnumerateObject().SelectMany(prop => DeepFlat($"{key}.{prop.Name}", prop.Value));
                default:
                    return new[] { new KeyValuePair<string, string>(key, value.ToString()) };
            }
        }
    }
}
