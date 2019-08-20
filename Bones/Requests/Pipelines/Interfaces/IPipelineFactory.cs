using Chronos.Domain.Interfaces;

namespace Chronos.Domain.Pipelines.Interfaces
{
    public interface IPipelineFactory
    {
        IPipelineFactory<TRequest> Create<TRequest>(PipelineMode mode = PipelineMode.And);
    }

    public interface IPipelineFactory<TRequest>
    {
        IPipelineFactory<TRequest> Add<TMiddleware>() where TMiddleware : IMiddleware<TRequest>;
        IPipelineFactory<TRequest> Add(IMiddleware<TRequest> middleware);
        IPipeline<TRequest> Finally<TMiddleware>() where TMiddleware : IMiddleware<TRequest>;
        IPipeline<TRequest> Finally(IMiddleware<TRequest> middleware);
        IPipeline<TRequest> Build();
    }
}