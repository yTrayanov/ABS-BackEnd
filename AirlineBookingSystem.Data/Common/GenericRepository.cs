using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AirlineBookingSystem.Data.Common
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {

        private readonly ABSContext _context;
        private readonly DbSet<T> _table;


        public GenericRepository(ABSContext context)
        {
            this._context = context;
            this._table = _context.Set<T>();
        }

        public async Task Delete(int id)
        {
            var entity = await _table.FindAsync(id);
            _table.Remove(entity);
        }

        public void DeleteRange(IEnumerable<T> entities)
        {
            _table.RemoveRange(entities);
        }

        public async Task<T> Get(Expression<Func<T, bool>> expression, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null)
        {
            IQueryable<T> query = _table;


            if(include != null)
            {
                query = include(query);
            }


            return await query.AsNoTracking().FirstOrDefaultAsync(expression);
        }
        public async Task<IList<T>> GetAll(Expression<Func<T, bool>> expression = null, Func<IQueryable<T>, IIncludableQueryable<T, object>> include = null, Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null)
        {
            IQueryable<T> query = _table;
            if(expression != null)
            {
                query = query.Where(expression);
            }

            if (include != null)
            {
                query = include(query);   
            }

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            return await query.AsNoTracking().ToListAsync();
        }


        public async Task Insert(T entity)
        {
            await _table.AddAsync(entity);
        }

        public async Task InsertRange(IEnumerable<T> entities)
        {
            await _table.AddRangeAsync(entities);
        }

        public void Update(T entity)
        {
            _table.Attach(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }
    }
}
