using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public interface IPrescriptionService
    {
        Task<PrescriptionResponse?> UploadPrescription(string userId, PrescriptionRequest request);
        Task<List<PrescriptionResponse>> GetOrderPrescriptions(string userId, int orderId);
        Task<List<PrescriptionResponse>> GetPrescriptions(PrescriptionStatusEnum status);
        Task<PrescriptionResponse?> ReviewPrescription(int prescriptionId, UpdatePrescriptionStatusRequest request);
    }
}
