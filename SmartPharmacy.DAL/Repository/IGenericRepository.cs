using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Repository
{
    public interface IGenericRepository<T> where T : class
    {
       Task<List<T>> GetAllAsync(Expression<Func<T,bool>>?filter = null, string[]?inclead=null);
        Task<T?> GetOne(Expression<Func<T, bool>>? filter = null, string[]? inclead = null);
        Task<T>  CreateAsync(T entity);
        Task<bool> UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);

        Task<bool> UpdateRangAsync(List<T> entities);
        Task<bool> DeleteRangAsync(List<T> entities);
        IQueryable<T>GetQueryableAsync(Expression<Func<T, bool>>? filter = null, string[]? inclead = null);

    }
}
