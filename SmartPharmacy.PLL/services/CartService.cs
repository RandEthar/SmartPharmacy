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
    public class CartService : ICartService
    {
        private readonly ICartRepository _cartRepository;
        private readonly IProductRepository _productRepository;

        public CartService(ICartRepository cartRepository, IProductRepository productRepository)
        {
            _cartRepository = cartRepository;
            _productRepository = productRepository;
        }

        public async Task<CartResponse> GetCart(string userId)
        {
            var cartItems = await _cartRepository.GetAllAsync(c => c.UserId == userId,
                new[]
                {
                    nameof(CartItem.Product),
                    $"{nameof(CartItem.Product)}.{nameof(Product.ProductTranslations)}",
                });

            var items = cartItems.Adapt<List<CartItemResponse>>();

            return new CartResponse
            {
                Items = items,
                Total = items.Sum(i => i.Subtotal)
            };
        }

        public async Task<CartItemResponse> AddToCart(string userId, CartItemRequest request)
        {
            var existingItem = await _cartRepository.GetOne(
                c => c.UserId == userId && c.ProductId == request.ProductId
                );

            var currentQuantity = existingItem?.Quantity ?? 0;
            var newQuantity = currentQuantity + request.Quantity;

            var product = await _productRepository.GetOne(
                p => p.Id == request.ProductId,
                new[] { nameof(Product.ProductTranslations) }
                );
            if (product == null)
            {
                return null;
            }
            if (newQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException("Cannot add more items than available in stock.");
            }

            if (existingItem != null)
            {
                existingItem.Quantity = newQuantity;
                await _cartRepository.UpdateAsync(existingItem);
            }
            else
            {
                var newItem = request.Adapt<CartItem>();
                newItem.UserId = userId;
                newItem.Quantity = newQuantity;
                await _cartRepository.CreateAsync(newItem);
            }

            var productResponse = product.Adapt<ProductResponse>();
            return new CartItemResponse
            {
                ProductId = product.Id,
                ProductName = productResponse.Name,
                ProductImage = productResponse.Image,
                Price = product.Price,
                Quantity = newQuantity
            };
        }

        public async Task<CartItemResponse> UpdateQuantity(string userId, int productId, UpdateCartItemRequest request)
        {
            var existingItem = await _cartRepository.GetOne(
              c => c.UserId == userId && c.ProductId == productId
              );
            if (existingItem == null)
            {
                return null;
            }

            var newQuantity = existingItem.Quantity + request.Quantity;
            var product = await _productRepository.GetOne(
                p => p.Id == productId,
                new[] { nameof(Product.ProductTranslations) }
            );
            if (product == null)
            {
                return null;
            }
            if (newQuantity > product.StockQuantity)
            {
                throw new InvalidOperationException("Cannot add more items than available in stock.");
            }

            existingItem.Quantity = newQuantity;
            await _cartRepository.UpdateAsync(existingItem);

            var productResponse = product.Adapt<ProductResponse>();
            return new CartItemResponse
            {
                ProductId = product.Id,
                ProductName = productResponse.Name,
                ProductImage = productResponse.Image,
                Price = product.Price,
                Quantity = newQuantity
            };
        }

        public async Task<bool> RemoveFromCart(string userId, int productId)
        {
            var existingItem = await _cartRepository.GetOne(
              c => c.UserId == userId && c.ProductId == productId
              );
            if (existingItem != null) 
            {
                await _cartRepository.DeleteAsync(existingItem);
                return true;
            }return false;
        }

        public async Task<bool> ClearCart(string userId)
        {
          var cartUserItems = await _cartRepository.GetAllAsync(c => c.UserId == userId);
            if (cartUserItems == null || !cartUserItems.Any())
            {
                return false    ;
            }

           await _cartRepository.DeleteRangAsync(cartUserItems);
            return true;
        }
    }
}
