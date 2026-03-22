using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

using Xunit;
using Xunit.Abstractions;

using Bones.Tests.DI;

namespace Bones.Flow.Tests
{
    public class TestChainOfResponsibilityFactory
    {
        private ServiceProvider _provider;

        public TestChainOfResponsibilityFactory(ITestOutputHelper output)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddFlow();
            serviceCollection.AddDebug(output);

            serviceCollection
                .AddScoped<EvenHandler>()
                .AddScoped<OddHandler>()
                .AddScoped<FallbackHandler>()
                .AddScoped<EvenResultHandler>()
                .AddScoped<OddResultHandler>();

            _provider = serviceCollection.BuildServiceProvider();
        }

        // --- CoR standalone (sans résultat) ---

        [Fact]
        public async Task ChainOfResponsibility_FirstMatchHandles()
        {
            using var scope = _provider.CreateScope();
            var sp = scope.ServiceProvider;

            var cor = sp.GetChainOfResponsibilityFactory<NumberCommand>()
                .Add<EvenHandler>()
                .Add<OddHandler>()
                .Build();

            var cmd = new NumberCommand { Value = 4 };
            await cor.HandleAsync(cmd);

            Assert.Equal(nameof(EvenHandler), cmd.HandledBy);
        }

        [Fact]
        public async Task ChainOfResponsibility_SecondMatchHandles()
        {
            using var scope = _provider.CreateScope();
            var sp = scope.ServiceProvider;

            var cor = sp.GetChainOfResponsibilityFactory<NumberCommand>()
                .Add<EvenHandler>()
                .Add<OddHandler>()
                .Build();

            var cmd = new NumberCommand { Value = 3 };
            await cor.HandleAsync(cmd);

            Assert.Equal(nameof(OddHandler), cmd.HandledBy);
        }

        [Fact]
        public async Task ChainOfResponsibility_NoMatch_DoesNotThrow()
        {
            using var scope = _provider.CreateScope();
            var sp = scope.ServiceProvider;

            var cor = sp.GetChainOfResponsibilityFactory<NumberCommand>()
                .Add<EvenHandler>()
                .Build();

            var cmd = new NumberCommand { Value = 3 };
            await cor.HandleAsync(cmd);

            Assert.Null(cmd.HandledBy);
        }

        // --- CoR standalone (avec résultat) ---

        [Fact]
        public async Task ChainOfResponsibility_WithResult_FirstMatchReturns()
        {
            using var scope = _provider.CreateScope();
            var sp = scope.ServiceProvider;

            var cor = sp.GetChainOfResponsibilityFactory<NumberQuery, string>()
                .Add<EvenResultHandler>()
                .Add<OddResultHandler>()
                .Build();

            var result = await cor.HandleAsync(new NumberQuery { Value = 4 });

            Assert.Equal("even:4", result);
        }

        [Fact]
        public async Task ChainOfResponsibility_WithResult_NoMatch_ReturnsDefault()
        {
            using var scope = _provider.CreateScope();
            var sp = scope.ServiceProvider;

            var cor = sp.GetChainOfResponsibilityFactory<NumberQuery, string>()
                .Add<EvenResultHandler>()
                .Build();

            var result = await cor.HandleAsync(new NumberQuery { Value = 3 });

            Assert.Null(result);
        }

        // --- CoR composée dans une Pipeline ---

        [Fact]
        public async Task ChainOfResponsibility_ComposedInPipeline()
        {
            using var scope = _provider.CreateScope();
            var sp = scope.ServiceProvider;

            var cor = sp.GetChainOfResponsibilityFactory<NumberCommand>()
                .Add<EvenHandler>()
                .Add<OddHandler>()
                .Build();

            var pipeline = sp.GetPipelineFactory<NumberCommand>()
                .Add(cor)
                .Add<FallbackHandler>()
                .Build();

            var cmd = new NumberCommand { Value = 4 };
            await pipeline.HandleAsync(cmd);

            Assert.Equal(nameof(EvenHandler), cmd.HandledBy);
            Assert.True(cmd.FallbackReached);
        }

        [Fact]
        public async Task ChainOfResponsibility_ResultComposedInPipeline()
        {
            using var scope = _provider.CreateScope();
            var sp = scope.ServiceProvider;

            var cor = sp.GetChainOfResponsibilityFactory<NumberQuery, string>()
                .Add<EvenResultHandler>()
                .Add<OddResultHandler>()
                .Build();

            IQueryHandler<NumberQuery, string> pipeline = sp.GetPipelineFactory<NumberQuery, string>()
                .Add(cor)
                .Build();

            var result = await pipeline.HandleAsync(new NumberQuery { Value = 7 });

            Assert.Equal("odd:7", result);
        }

        // --- Fixtures ---

        public class NumberCommand : IRequest
        {
            public int Value { get; set; }
            public string HandledBy { get; set; }
            public bool FallbackReached { get; set; }
        }

        public class NumberQuery : IRequest<string>
        {
            public int Value { get; set; }
        }

        public class EvenHandler : IChainOfResponsibilityHandler<NumberCommand>
        {
            public Task<bool> HandleAsync(NumberCommand request, CancellationToken ct)
            {
                if (request.Value % 2 != 0)
                    return Task.FromResult(false);

                request.HandledBy = nameof(EvenHandler);
                return Task.FromResult(true);
            }
        }

        public class OddHandler : IChainOfResponsibilityHandler<NumberCommand>
        {
            public Task<bool> HandleAsync(NumberCommand request, CancellationToken ct)
            {
                if (request.Value % 2 == 0)
                    return Task.FromResult(false);

                request.HandledBy = nameof(OddHandler);
                return Task.FromResult(true);
            }
        }

        public class FallbackHandler : IMiddleware<NumberCommand>
        {
            public async Task HandleAsync(NumberCommand request, Func<Task> next, CancellationToken ct)
            {
                request.FallbackReached = true;
                await next();
            }
        }

        public class EvenResultHandler : IChainOfResponsibilityHandler<NumberQuery, string>
        {
            public Task<(bool handled, string result)> HandleAsync(NumberQuery request, CancellationToken ct)
            {
                if (request.Value % 2 != 0)
                    return Task.FromResult((false, (string)null));

                return Task.FromResult((true, $"even:{request.Value}"));
            }
        }

        public class OddResultHandler : IChainOfResponsibilityHandler<NumberQuery, string>
        {
            public Task<(bool handled, string result)> HandleAsync(NumberQuery request, CancellationToken ct)
            {
                if (request.Value % 2 == 0)
                    return Task.FromResult((false, (string)null));

                return Task.FromResult((true, $"odd:{request.Value}"));
            }
        }
    }
}
