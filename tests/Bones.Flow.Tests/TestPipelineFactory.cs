using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

using Bones.Tests.DI;

namespace Bones.Flow.Tests
{
    public class TestPipelineFactory
    {
        private ServiceProvider _provider;

        public TestPipelineFactory(ITestOutputHelper output)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddFlow();
            serviceCollection.AddDebug(output);

            serviceCollection
                .AddOptions()
                .AddScoped<ActorLicitMiddleware>()
                .AddScoped<HelloWorldCommandHandler>()
                .AddScoped(typeof(LogRequestHandler<>))
                .AddScoped(typeof(LogExceptionHandler<>))
                .AddScoped(typeof(NotDefaultMiddleware<>))
                .AddScoped(typeof(NotEmptyMiddleware<>))
                .AddScoped(typeof(DateBoundLicitMiddleware<>))
                .AddScoped(typeof(LogRequestHandler<,>))
                .AddScoped(typeof(LogResultHandler<>))
                .AddScoped<DataQueryHandler>();

            _provider = serviceCollection.BuildServiceProvider();
        }

        [Fact]
        public async Task TestRequestPipelineFactory()
        {
            IPipelineFactory<HelloWorldCommand> factory = _provider.GetPipelineFactory<HelloWorldCommand>();

            ICommandHandler<HelloWorldCommand> pipeline = factory
                .Add<ActorLicitMiddleware>()
                .Add(new AssertHandler<HelloWorldCommand>(cmd => cmd.Message == "toto"))
                .Add<HelloWorldCommandHandler>()

                .OnSuccess<LogRequestHandler<HelloWorldCommand>>()
                .OnFailure<LogExceptionHandler<HelloWorldCommand>>()

                .Build();

            Assert.NotNull(pipeline);

            await pipeline.HandleAsync(new HelloWorldCommand(){
                Message = "toto",
                ActorId = "toto"
            });

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await pipeline.HandleAsync(
                new HelloWorldCommand(){
                    Message = "toto"
                })
            );

            await Assert.ThrowsAsync<ArgumentException>(async () => await pipeline.HandleAsync(
                new HelloWorldCommand(){
                    ActorId = "toto"
                })
            );
        }

        [Fact]
        public async Task TestRequestResultPipelineFactory()
        {
            IPipelineFactory<DataQuery, IEnumerable<string>> factory = _provider.GetPipelineFactory<DataQuery, IEnumerable<string>>();

            IQueryHandler<DataQuery, IEnumerable<string>> pipeline = factory

                .Add<NotDefaultMiddleware<IEnumerable<string>>>()
                .Add<NotEmptyMiddleware<string>>()
                .Add<DateBoundLicitMiddleware<string>>()
                .With<ActorLicitMiddleware>()
                .Add<DataQueryHandler>()

                .OnSuccess<LogRequestHandler<DataQuery>>()
                .OnResult<LogResultHandler<IEnumerable<string>>>()
                .OnResult<LogRequestHandler<DataQuery, IEnumerable<string>>>()
                .OnFailure<LogExceptionHandler<DataQuery>>()

                .Build();

            Assert.NotNull(pipeline);

            var result = await pipeline.HandleAsync(new DataQuery(){
                ActorId = "toto",
                DateMin = DateTime.Now.AddDays(-2),
                DateMax = DateTime.Now
            });

            Assert.Equal(2, result.Count());

            // DateBound va retourner une liste vide, not empty va jeter une exception
            await Assert.ThrowsAsync<Exception>(async() => await pipeline.HandleAsync(new DataQuery(){
                ActorId = "toto",
                DateMax = DateTime.Now.AddDays(-7),
                DateMin = DateTime.Now
            }));
        }

    }
}
