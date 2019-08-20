using System;
using System.Threading.Tasks;

namespace Chronos.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        Task<bool> Commit();
    }
}
