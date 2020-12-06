using System;
using System.Collections.Generic;

namespace Bones.Flow.Tests
{
    public interface IDateBoundedRequest<TResult> : IRequest<IEnumerable<TResult>>
    {
        DateTime DateMin { get; }
        DateTime DateMax { get; }
    }
}