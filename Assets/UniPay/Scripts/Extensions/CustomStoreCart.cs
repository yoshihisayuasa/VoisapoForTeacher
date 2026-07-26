/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;

namespace UniPay
{
    using UnityEngine.Purchasing;

    /// <summary>
    /// Used when recreating Orders, requiring product Cart on custom stores.
    /// </summary>
    class CustomStoreCart : ICart
    {
        public IReadOnlyList<CartItem> Items() => _items;

        private readonly List<CartItem> _items;


        public CustomStoreCart(Product product, int quantity = 1)
        {
            _items = new List<CartItem>();

            CartItem cartItem = new CartItem(product, quantity);
            _items.Add(cartItem);
        }
    }
}