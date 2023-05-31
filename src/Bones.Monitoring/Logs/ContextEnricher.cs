using System.Linq;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Bones.Monitoring.Core.Logging
{
    public class ContextEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ContextEnricher(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            
            if(httpContext != null)
            {
                logEvent.AddPropertyIfAbsent(new LogEventProperty("UserId", new ScalarValue(httpContext?.User?.Claims?.FirstOrDefault(c => c.Type == "UserId")?.Value ?? "anonymous")));
            }
        }
    }
}