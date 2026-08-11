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
        Task<PagenationResponse<PrescriptionResponse>> GetPrescriptions(PrescriptionStatusEnum status, PagenationRequest request);
        Task<PrescriptionResponse?> ReviewPrescription(int prescriptionId, UpdatePrescriptionStatusRequest request);
    }
}
