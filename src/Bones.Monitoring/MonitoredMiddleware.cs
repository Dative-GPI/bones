using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Bones.Monitoring.Core
{
    public abstract class MonitoredMiddleware
    {
        private RequestDelegate _next;
        private ActivitySource _source;
        protected IServiceProvider ServiceProvider;
        private string _name;
        private string _after { get { return _name + "_after"; } }
        private string _before { get { return _name + "_before"; } }

        private const string MIDDLEWARE_TAG_NAME = "application.middleware";

        public MonitoredMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            ServiceProvider = serviceProvider;
            _source = serviceProvider.GetRequiredService<ActivitySource>();
            _name = this.GetType().Name;
        }


        public async Task InvokeAsync(HttpContext context)
        {
            using var globalScope = ServiceProvider.CreateScope();
            var factory = globalScope.ServiceProvider.GetRequiredService<ITraceFactory>();
            var trace = factory.Create(_source, _before);
            trace.SetTag(MIDDLEWARE_TAG_NAME, _before);

            await HandleAsync(context, async (ctx) => 
                {
                    trace.Dispose();
                    await _next(ctx);
                    trace = factory.Create(_source,  _after);
                    trace.SetTag(MIDDLEWARE_TAG_NAME, _after);
                }
            );  

            trace.Dispose();
        }

        protected abstract Task HandleAsync(HttpContext context, RequestDelegate next);
    }
}