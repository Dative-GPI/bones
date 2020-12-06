using System.Threading.Tasks;

namespace Bones.Flow
{
    public interface IUnitOfWork
    {
        Task Commit();
    }
}