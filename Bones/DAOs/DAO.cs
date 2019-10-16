using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Bones.Repository
{
    public class DAO<TEntity> : IDAO<TEntity> where TEntity : class
    {
        protected readonly DbContext db;
        protected readonly DbSet<TEntity> dbSet;
        
        public DAO(DbContext context)
        {
            db = context;
            dbSet = db.Set<TEntity>();
        }

        public virtual void Add(TEntity obj)
        {
            dbSet.Add(obj);
        }

        public virtual void AddRange(IEnumerable<TEntity> objs)
        {
            dbSet.AddRange(objs);
        }

        public virtual async Task<IEnumerable<TEntity>> GetAll()
        {
            return await dbSet.ToListAsync();
        }

        public virtual Task<TEntity> GetById(params object[] parameters)
        {
            return dbSet.FindAsync(parameters);
        }

        public virtual async Task Remove(params object[] parameters)
        {
            dbSet.Remove(await GetById(parameters));
        }

        public virtual void Remove(TEntity obj)
        {
            dbSet.Remove(obj);
        }

        public virtual void Update(TEntity obj)
        {
            dbSet.Update(obj);
        }
        
        public void RemoveRange(IEnumerable<TEntity> objs)
        {
            dbSet.RemoveRange(objs);
        }
    }
}
