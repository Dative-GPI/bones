using System;

namespace Bones.Flow
{
    public interface IPipelineFactory<TRequest> where TRequest: IRequest
    {
        IPipelineFactory<TRequest> Add<TMiddleware>()
            where TMiddleware : IMiddleware<TRequest>;
        IPipelineFactory<TRequest> Add<TMiddleware>(TMiddleware middleware)
            where TMiddleware : IMiddleware<TRequest>;

        IPipelineFactory<TRequest> OnSuccess<THandler>()
            where THandler: ISuccessHandler<TRequest>;
        IPipelineFactory<TRequest> OnSuccess<THandler>(THandler handler)
            where THandler: ISuccessHandler<TRequest>;
        
        IPipelineFactory<TRequest> OnFailure<THandler>()
            where THandler : IFailureHandler<TRequest>;
        IPipelineFactory<TRequest> OnFailure<THandler>(THandler handler)
            where THandler : IFailureHandler<TRequest>;
        
        IPipeline<TRequest> Build();
    }

    public interface IPipelineFactory<TRequest, TResult> where TRequest: IRequest<TResult>
    {
        IBuildablePipelineFactory<TRequest, TResult> Add<TMiddleware>() 
            where TMiddleware : IMiddleware<TRequest, TResult>;

        IPipelineFactory<TRequest, TResult> With<TMiddleware>()
            where TMiddleware : IMiddleware<TRequest>;
    }

    public interface IBuildablePipelineFactory<TRequest, TResult> : IPipelineFactory<TRequest, TResult>
        where TRequest : IRequest<TResult>
    {
        IBuildablePipelineFactory<TRequest, TResult> OnSuccess<THandler>()
            where THandler : ISuccessHandler<TRequest>;

        IBuildablePipelineFactory<TRequest, TResult> OnResult<THandler>()
            where THandler : ISuccessHandler<TRequest, TResult>;
    
        IBuildablePipelineFactory<TRequest, TResult> OnFailure<THandler>()
            where THandler : IFailureHandler<TRequest>;

        IPipeline<TRequest, TResult> Build();
    }
}