using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.PLL.services;
using System.Security.Claims;

namespace SmartPharmacy.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly IPrescriptionService _prescriptionService;

        public PrescriptionsController(IPrescriptionService prescriptionService)
        {
            _prescriptionService = prescriptionService;
        }

        private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier);

        [HttpPost]
        public async Task<IActionResult> UploadPrescription([FromForm] PrescriptionRequest request)
        {
            var prescription = await _prescriptionService.UploadPrescription(UserId, request);
            if (prescription == null)
            {
                return BadRequest("Prescription could not be uploaded.");
            }
            return Ok(prescription);
        }

        [HttpGet("order/{orderId}")]
        public async Task<IActionResult> GetOrderPrescriptions(int orderId)
        {
            var prescriptions = await _prescriptionService.GetOrderPrescriptions(UserId, orderId);
            return Ok(prescriptions);
        }

        [HttpGet]
        //[Authorize(Roles = "Pharmacist")]
        public async Task<IActionResult> GetPrescriptions([FromQuery] PrescriptionStatusEnum status)
        {
            var prescriptions = await _prescriptionService.GetPrescriptions(status);
            return Ok(prescriptions);
        }

        [HttpPatch("{id}/status")]
        //[Authorize(Roles = "Pharmacist")]
        public async Task<IActionResult> ReviewPrescription(int id, UpdatePrescriptionStatusRequest request)
        {
            var prescription = await _prescriptionService.ReviewPrescription(id, request);
            if (prescription == null)
            {
                return BadRequest("Prescription status cannot be changed.");
            }
            return Ok(prescription);
        }
    }
}
