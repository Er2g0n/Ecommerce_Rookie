using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Structure_Core.OrderManagement;

namespace Nash_WebMVC.Utility
{
    public static class CartHelper
    {
        private const string CartSessionKey = "Cart";

        // Get cart from session
        public static List<OrderDetailDTO> GetCart(ISession session)
        {
            var cartJson = session.GetString(CartSessionKey);
            if (string.IsNullOrEmpty(cartJson))
            {
                return new List<OrderDetailDTO>();
            }
            return JsonConvert.DeserializeObject<List<OrderDetailDTO>>(cartJson) ?? new List<OrderDetailDTO>();
        }

        // Save cart to session
        public static void SaveCart(ISession session, List<OrderDetailDTO> cart)
        {
            var cartJson = JsonConvert.SerializeObject(cart);
            session.SetString(CartSessionKey, cartJson);
        }

        // Add item to cart
        public static void AddToCart(ISession session, OrderDetailDTO item)
        {
            var cart = GetCart(session);
            var existingItem = cart.FirstOrDefault(x => x.ProductCode == item.ProductCode);
            if (existingItem != null)
            {
                existingItem.Quantity += item.Quantity; // Update quantity if item exists
            }
            else
            {
                cart.Add(item); // Add new item
            }
            SaveCart(session, cart);
        }

        // Clear cart
        public static void ClearCart(ISession session)
        {
            session.Remove(CartSessionKey);
        }
    }
}