using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using SmartPharmacy.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SmartPharmacy.DAL.Repository
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
      public  GenericRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<T> CreateAsync(T entity)
        {
            await _context.AddAsync(entity);
         await _context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteAsync(T entity)
        {
           _context.Remove(entity);
        var affected  =  await _context.SaveChangesAsync();
            return affected>0;
        }

        public async Task<bool> DeleteRangAsync(List<T> entities)
        {
         _context.RemoveRange(entities);
          var affected = await _context.SaveChangesAsync();
            return affected > 0;
        }

        public async  Task<List<T>> GetAllAsync(Expression<Func<T, bool>>? filter = null, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (filter != null)
                query = query.Where(filter);

            if (includes != null)
            {
                foreach (var item in includes)
                    query = query.Include(item);
            }

            return await  query.ToListAsync();
        }

        public async Task<T?> GetOne(Expression<Func<T, bool>>? filter = null, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>();

            if (filter != null)
                query = query.Where(filter);

            if (includes != null)
            {
                foreach (var item in includes)
                    query = query.Include(item);
            }

            return await query.FirstOrDefaultAsync();
        }

        public IQueryable<T> GetQueryableAsync(Expression<Func<T, bool>>? filter = null, string[]? includes = null)
        {
            IQueryable<T> query = _context.Set<T>();
            if (filter != null)
                query = query.Where(filter);

            if (includes != null)
            {
                foreach (var item in includes)
                    query = query.Include(item);
            }
            return query;
        }

        public async Task<bool> UpdateAsync(T entity)
        {
          _context.Update(entity);
            var affected = await _context.SaveChangesAsync();
            return affected > 0;

        }

        public async Task<bool> UpdateRangAsync(List<T> entities)
        {
            _context.UpdateRange(entities);
            var affected = await _context.SaveChangesAsync();
            return affected > 0;
        }
    }
}
