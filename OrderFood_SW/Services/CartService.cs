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

        public (bool Success, string Message) UpdateNote(string id, string note)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.CartItemId == id);
            if (item == null)
                return (false, "Item not found");

            item.Note = note;
            SaveCart(cart);

            return (true, "Note updated");
        }

        public (bool Success, string Message) DuplicateItemWithNote(string id, string note)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(x => x.CartItemId == id);
            if (item == null)
                return (false, "Item not found");

            var newItem = new OrderCartItem
            {
                CartItemId = Guid.NewGuid().ToString(),
                DishId = item.DishId,
                ImageUrl = item.ImageUrl,
                DishName = item.DishName,
                Price = item.Price,
                Quantity = item.Quantity, // keep original quantity
                Note = note               // assign new note
            };

            cart.Add(newItem);
            SaveCart(cart);

            return (true, "Item duplicated with new note");
        }

    }
}
