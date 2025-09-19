using Microsoft.AspNetCore.Http;
using OrderFood_SW.Helper;
using OrderFood_SW.Models;
using OrderFood_SW.Repositories;
using OrderFood_SW.ViewModels;

namespace OrderFood_SW.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly CartRepository _cartRepo;

        public CartService(IHttpContextAccessor httpContextAccessor, CartRepository cartRepo)
        {
            _httpContextAccessor = httpContextAccessor;
            _cartRepo = cartRepo;
        }

        private ISession Session => _httpContextAccessor.HttpContext!.Session;

        public List<OrderCartItem> GetCart()
        {
            return Session.GetObject<List<OrderCartItem>>("Cart") ?? new();
        }

        public void SaveCart(List<OrderCartItem> cart)
        {
            Session.SetObject("Cart", cart);
        }

        public int GetCartCount()
        {
            var cart = GetCart();
            return cart.Sum(x => x.Quantity);
        }

        public (bool Success, string Message, int Count) UpdateQuantity(string cartItemId, int change)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.CartItemId == cartItemId);
            if (item == null)
                return (false, "Item not found", cart.Sum(x => x.Quantity));

            item.Quantity += change;
            if (item.Quantity <= 0) cart.Remove(item);

            SaveCart(cart);
            return (true, "Quantity updated", cart.Sum(x => x.Quantity));
        }


        public (bool Success, int Count) RemoveFromCart(string Id)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.CartItemId == Id);
            if (item != null) cart.Remove(item);

            SaveCart(cart);
            return (true, cart.Sum(x => x.Quantity));
        }

        public void ClearCart()
        {
            Session.Remove("Cart");
        }

        public (int? TableId, int? TableNumber) GetCurrentTable()
        {
            var tableId = Session.GetInt32("CurrentTableId");
            var tableNumber = _cartRepo.GetTableNumberById(tableId);
            return (tableId, tableNumber);
        }
    }
}
