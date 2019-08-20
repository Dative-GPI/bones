using Chronos.Domain.Interfaces;
using Chronos.Domain.Requests;

namespace Chronos.Domain.Pipelines.Interfaces
{
    public interface IPipeline<TRequest> : ICommandHandler<TRequest>, 
        IQueryHandler<TRequest>, 
        IMiddleware<TRequest>
    {
    }

    public enum PipelineMode
    {
        Or,
        And
    }
}