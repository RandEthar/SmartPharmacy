using Mapster;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Models;
using SmartPharmacy.DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartPharmacy.PLL.services
{
    public class PrescriptionService : IPrescriptionService
    {
        private readonly IPrescriptionRepository _prescriptionRepository;
        private readonly IOrderRepository _orderRepository;
        private readonly IFileService _fileService;

        public PrescriptionService(
            IPrescriptionRepository prescriptionRepository,
            IOrderRepository orderRepository,
            IFileService fileService)
        {
            _prescriptionRepository = prescriptionRepository;
            _orderRepository = orderRepository;
            _fileService = fileService;
        }

        public async Task<PrescriptionResponse?> UploadPrescription(string userId, PrescriptionRequest request)
        {
            var order = await _orderRepository.GetOne(o => o.Id == request.OrderId && o.UserId == userId);
            if (order == null) return null;
            if (order.OrderStatus != OrderStatusEnum.AwaitingPrescription) return null;

            var prescriptionUrl = await _fileService.UploadFileAsync(request.Image);
            if (prescriptionUrl == null) return null;

            var prescription = new Prescription
            {
                ImageUrl = prescriptionUrl,
                Status = PrescriptionStatusEnum.Pending,
                CreatedAt = DateTime.UtcNow,
                OrderId = request.OrderId
            };
            await _prescriptionRepository.CreateAsync(prescription);

            return prescription.Adapt<PrescriptionResponse>();
        }

        public async Task<List<PrescriptionResponse>> GetOrderPrescriptions(string userId, int orderId)
        {
            var order = await _orderRepository.GetOne(o => o.Id == orderId && o.UserId == userId);
            if (order == null) return new List<PrescriptionResponse>();

            var orderPrescriptions = await _prescriptionRepository.GetAllAsync(p => p.OrderId == orderId);

            return orderPrescriptions.Adapt<List<PrescriptionResponse>>();
        }

        public async Task<List<PrescriptionResponse>> GetPrescriptions(PrescriptionStatusEnum status)
        {
            var prescriptions = await _prescriptionRepository.GetAllAsync(p => p.Status == status);

            return prescriptions.Adapt<List<PrescriptionResponse>>();
        }

        public async Task<PrescriptionResponse?> ReviewPrescription(int prescriptionId, UpdatePrescriptionStatusRequest request)
        {
            var prescription = await _prescriptionRepository.GetOne(
                p => p.Id == prescriptionId,
                new[] { nameof(Prescription.Order) });

            if (prescription == null) return null;
            if (prescription.Status != PrescriptionStatusEnum.Pending) return null;
            if (request.Status != PrescriptionStatusEnum.Approved && request.Status != PrescriptionStatusEnum.Rejected)
                return null;

            prescription.Status = request.Status;
            await _prescriptionRepository.UpdateAsync(prescription);

            if (request.Status == PrescriptionStatusEnum.Rejected)
            {
                prescription.Order.OrderStatus = OrderStatusEnum.Cancelled;
                await _orderRepository.UpdateAsync(prescription.Order);
            }
            else if (request.Status == PrescriptionStatusEnum.Approved)
            {
                var orderPrescriptions = await _prescriptionRepository.GetAllAsync(p => p.OrderId == prescription.OrderId);
                var allApproved = orderPrescriptions.All(p => p.Status == PrescriptionStatusEnum.Approved);

                if (allApproved)
                {
                    prescription.Order.OrderStatus = OrderStatusEnum.Pending;
                    await _orderRepository.UpdateAsync(prescription.Order);
                }
            }

            return prescription.Adapt<PrescriptionResponse>();
        }
    }
}
