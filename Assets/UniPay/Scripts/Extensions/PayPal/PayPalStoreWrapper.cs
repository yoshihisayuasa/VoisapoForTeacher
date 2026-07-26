/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

namespace UniPay
{
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;

    /// <summary>
    /// Wrapper defining instance and store name for PayPal.
    /// </summary>
    class PayPalStoreWrapper : IStoreWrapper
    {
        public string name { get; }
        private string Name => IAPPlatform.PayPal.ToString();

        public Store instance => store;
        private PayPalStore store;


        public PayPalStoreWrapper(PayPalStore instance)
        {
            this.name = Name;
            store = instance;
        }


        public ConnectionState GetStoreConnectionState()
        {
            return store.GetStoreConnectionState();
        }
    }
}