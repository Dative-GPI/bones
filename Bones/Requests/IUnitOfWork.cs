using System;
using System.Threading.Tasks;

namespace Bones.Requests
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();
    }
}
