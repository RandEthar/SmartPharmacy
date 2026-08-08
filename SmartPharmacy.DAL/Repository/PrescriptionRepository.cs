using SmartPharmacy.DAL.Data;
using SmartPharmacy.DAL.Models;

namespace SmartPharmacy.DAL.Repository
{
    public class PrescriptionRepository : GenericRepository<Prescription>, IPrescriptionRepository
    {
        public PrescriptionRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
