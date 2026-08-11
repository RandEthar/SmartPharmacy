using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SmartPharmacy.DAL.Data;
using System.Threading.Tasks;

namespace SmartPharmacy.DAL.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IDbContextTransaction> BeginTransactionAsync() =>
            _context.Database.BeginTransactionAsync();
    }
}
