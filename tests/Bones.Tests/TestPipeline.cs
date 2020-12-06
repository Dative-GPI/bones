
using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

using Bones.Requests;
using Bones.Requests.Pipelines;
using Bones.Requests.Pipelines.Interfaces;

namespace UnitTest
{
   
    public class TestPipeline
    {
        private ServiceProvider _sp;

        private class Command
        {
            public int Age { get; set; }
        }

        private class Middleware1 : IMiddleware<Command>
        {
            public async Task<RequestResult> HandleAsync(Command request, Func<Task<RequestResult>> next, CancellationToken cancellationToken)
            {
                if (request.Age > 18)
                {
                    return await next();
                }
                else
                {
                    return RequestResult.Fail();
                }
            }
        }

        private class Middleware2 : IMiddleware<Command>
        {
            public async Task<RequestResult> HandleAsync(Command request, Func<Task<RequestResult>> next, CancellationToken cancellationToken)
            {
                if (request.Age < 100)
                {
                    return await next();
                }
                else
                {
                    return RequestResult.Fail();
                }
            }
        }

        private class Middleware3 : IMiddleware<Command>
        {
            public Task<RequestResult> HandleAsync(Command request, Func<Task<RequestResult>> next, CancellationToken cancellationToken)
            {
                return Task.FromResult(RequestResult.Success());
            }
        }

        private class UnitOfWork : IUnitOfWork
        {
            public bool Commited = false;

            public Task<bool> Commit()
            {
                Commited = true;
                return Task.FromResult(true);
            }
        }

        public TestPipeline()
        {
            

            IConfigurationBuilder configBuilder = new ConfigurationBuilder();
            IConfiguration config = configBuilder.Build();

            var services = new ServiceCollection();

            services.AddScoped<IPipelineFactory, PipelineFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddLogging();

            services.AddScoped<Middleware1>();
            services.AddScoped<Middleware2>();
            services.AddScoped<Middleware3>();

            services.AddScoped<ICommandHandler<Command>>(sp =>
            {
                var pipeline = sp.GetService<IPipelineFactory>()
                    .Create<Command>(PipelineMode.And)
                    .Add<Middleware1>()
                    .Add<Middleware2>()
                    .Finally<Middleware3>();

                return pipeline;
            });

            services.AddScoped<IQueryHandler<Command>>(sp =>
            {
                var pipeline = sp.GetService<IPipelineFactory>()
                    .Create<Command>(PipelineMode.And)
                    .Add<Middleware1>()
                    .Add<Middleware2>()
                    .Finally<Middleware3>();

                return pipeline;
            });

            services.AddScoped<IMiddleware<Command>>(sp =>
            {
                var pipeline = sp.GetService<IPipelineFactory>()
                    .Create<Command>(PipelineMode.And)
                    .Add<Middleware1>()
                    .Add<Middleware2>()
                    .Finally<Middleware3>();

                return pipeline;
            });

            _sp = services.BuildServiceProvider();

        }

        [Theory]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(99)]
        [InlineData(100)]
        public async Task TestAsync_1(int age)
        {
            Command c = new Command { Age = age };
            var handler = _sp.GetService<ICommandHandler<Command>>();
            var result = await handler.HandleAsync(c, CancellationToken.None);

            if (18 < age && age < 100)
            {
                Assert.True(result.Succeed);
                Assert.True((_sp.GetService<IUnitOfWork>() as UnitOfWork).Commited);
            }
            else
            {
                Assert.False(result.Succeed);
                Assert.False((_sp.GetService<IUnitOfWork>() as UnitOfWork).Commited);
            }
        }

        [Theory]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(99)]
        [InlineData(100)]
        public async Task TestAsync_2(int age)
        {
            Command c = new Command { Age = age };
            var handler = _sp.GetService<IQueryHandler<Command>>();
            var result = await handler.HandleAsync(c, CancellationToken.None);

            Assert.False((_sp.GetService<IUnitOfWork>() as UnitOfWork).Commited);

            if (18 < age && age < 100)
                Assert.True(result.Succeed);
            else
                Assert.False(result.Succeed);
        }

        [Theory]
        [InlineData(18)]
        [InlineData(19)]
        [InlineData(99)]
        [InlineData(100)]
        public async Task TestAsync_3(int age)
        {
            Command c = new Command { Age = age };
            var middleware = _sp.GetService<IMiddleware<Command>>();
            var result = await middleware.HandleAsync(c, () =>
            {
                return Task.FromResult(RequestResult.Success(true));
            }, CancellationToken.None);

            Assert.False((_sp.GetService<IUnitOfWork>() as UnitOfWork).Commited);

            if (18 < age && age < 100)
                Assert.True(result.EnsureSuccess<bool>());
            else
                Assert.False(result.Succeed);
        }

        [Fact]
        public async Task TestAsync_4()
        {
            CancellationToken ct = new CancellationToken(true);

            Command c = new Command { Age = 50 };
            var handler = _sp.GetService<IQueryHandler<Command>>();

            await Assert.ThrowsAsync<OperationCanceledException>(async () => { await handler.HandleAsync(c, ct); });
        }
    }
}