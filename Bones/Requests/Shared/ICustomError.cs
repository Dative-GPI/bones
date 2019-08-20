using System;

namespace Chronos.Domain.Requests
{
    public interface ICustomError
    {
        Exception Exception { get; }
    }
}