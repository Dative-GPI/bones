
namespace Bones.Requests.Pipelines.Interfaces
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