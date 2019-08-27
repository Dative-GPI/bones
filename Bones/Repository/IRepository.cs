using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bones.Repository
{
    public interface IRepository<TEntity> where TEntity : class
    {
        void Add(TEntity obj);
        void AddRange(IEnumerable<TEntity> objs);
        Task<TEntity> GetById(params object[] parameters);
        Task<IEnumerable<TEntity>> GetAll();
        void Update(TEntity obj);
        Task Remove(params object[] parameters);
        void Remove(TEntity obj);
        void RemoveRange(IEnumerable<TEntity> objs);

    }
}
