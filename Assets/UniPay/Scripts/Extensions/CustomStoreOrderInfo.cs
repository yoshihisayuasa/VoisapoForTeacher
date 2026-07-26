/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Collections.Generic;

namespace UniPay
{
    using UnityEngine.Purchasing;

    /// <summary>
    /// Used when recreating Orders, requiring receipt Info on custom stores.
    /// </summary>
    class CustomStoreOrderInfo : IOrderInfo
    {
        public IAppleOrderInfo Apple { get => null; }
        public IGoogleOrderInfo Google { get => null; }

        public List<IPurchasedProductInfo> PurchasedProductInfo { get; set; }

        public string Receipt => _receipt;
        public string TransactionID => _transactionId;

        private readonly string _receipt;
        private readonly string _transactionId;


        public CustomStoreOrderInfo(string receipt, string transactionID)
        {
            _receipt = receipt;
            _transactionId = transactionID;
        }
    }
}