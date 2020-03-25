using System;

namespace Bones.Requests
{
    public interface ICustomError
    {
        Exception Exception { get; }
    }
}