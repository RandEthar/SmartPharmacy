using Microsoft.EntityFrameworkCore.Storage;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Repository
{
    /// <summary>
    /// Lets a service group several repository calls into one database transaction.
    /// All repositories share the same scoped DbContext, so a transaction opened here
    /// covers every repository used within the same request.
    /// </summary>
    public interface IUnitOfWork
    {
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
