using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ABS.Data.DynamoDbRepository
{
    public interface IRepository<TKey, TEntity> where TEntity : class
    {
        Task Add(TEntity item);
        Task Delete(TKey key);
        Task<TEntity> Get(TKey key);
        Task<IList<TEntity>> GetList();
        Task Update(TEntity item);
    }
}
