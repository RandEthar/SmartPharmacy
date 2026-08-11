using Mapster;
using SmartPharmacy.DAL.DTO.Request;
using SmartPharmacy.DAL.DTO.Response;
using SmartPharmacy.DAL.Extentions;
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
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFileService _fileService;

        public PrescriptionService(
            IPrescriptionRepository prescriptionRepository,
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            IUnitOfWork unitOfWork,
            IFileService fileService)
        {
            _prescriptionRepository = prescriptionRepository;
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _unitOfWork = unitOfWork;
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

        public async Task<PagenationResponse<PrescriptionResponse>> GetPrescriptions(
            PrescriptionStatusEnum status, PagenationRequest request)
        {
            var query = _prescriptionRepository
                .GetQueryableAsync(p => p.Status == status)
                .OrderByDescending(p => p.CreatedAt)
                .ThenByDescending(p => p.Id);

            var prescriptions = await query.ApplyPagenation(request.Page, request.Limit);

            return new PagenationResponse<PrescriptionResponse>
            {
                Data = prescriptions.Data.Adapt<List<PrescriptionResponse>>(),
                TotalCount = prescriptions.TotalCount,
                Page = prescriptions.Page,
                Limit = prescriptions.Limit
            };
        }

        public async Task<PrescriptionResponse?> ReviewPrescription(int prescriptionId, UpdatePrescriptionStatusRequest request)
        {
            var prescription = await _prescriptionRepository.GetOne(
                p => p.Id == prescriptionId,
                new[]
                {
                    nameof(Prescription.Order),
                    $"{nameof(Prescription.Order)}.{nameof(Order.OrderItems)}"
                });

            if (prescription == null) return null;
            if (prescription.Status != PrescriptionStatusEnum.Pending) return null;
            if (request.Status != PrescriptionStatusEnum.Approved && request.Status != PrescriptionStatusEnum.Rejected)
                return null;

            prescription.Status = request.Status;
            await _prescriptionRepository.UpdateAsync(prescription);

            if (request.Status == PrescriptionStatusEnum.Rejected)
            {
                // Cancelling the order has to hand the reserved stock back, otherwise a rejected
                // prescription would quietly keep the medicine out of circulation.
                await using var transaction = await _unitOfWork.BeginTransactionAsync();

                var heldStock = prescription.Order.OrderStatus != OrderStatusEnum.Cancelled;

                prescription.Order.OrderStatus = OrderStatusEnum.Cancelled;
                await _orderRepository.UpdateAsync(prescription.Order);

                // After the save, so the tracked order graph cannot write a stale quantity
                // back over the restored stock.
                if (heldStock)
                {
                    await _productRepository.RestoreStock(prescription.Order.OrderItems);
                }

                await transaction.CommitAsync();
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
