/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UniPay
{
    #if OCULUS_IAP
    using Oculus.Platform;
    using Oculus.Platform.Models;
    using OculusProduct = Oculus.Platform.Models.Product;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;
    using UnityProduct = UnityEngine.Purchasing.Product;
    using UnityProductType = UnityEngine.Purchasing.ProductType;

    /// <summary>
    /// Represents the Unity IAP store implementation for the Oculus Store.
    /// </summary>
    public class OculusStore : Store
    {      
        //keeping track of the product cart that is currently being processed
        private ICart currentCart = null;


        public override async void Connect()
        {
            Message result = await Core.AsyncInitialize();

            if (result.IsError)
            {
                ConnectCallback?.OnStoreConnectionFailed(new StoreConnectionFailureDescription(
                    result.GetError().Code + ", " + result.GetError().HttpCode + ", " + result.GetError().Message + ". " + 
                    result.GetPlatformInitialize().Result.ToString()
                ));

                return;
            }

            ConnectCallback?.OnStoreConnectionSucceeded();
        }


        public override async void FetchProducts(IReadOnlyCollection<ProductDefinition> definitions)
        {
            List<string> skus = new List<string>();
            foreach (ProductDefinition definition in definitions)
            {
                skus.Add(definition.storeSpecificId);
            }

            if(skus.Count == 0)
            {
                ProductsCallback?.OnProductsFetchFailed(new ProductFetchFailureDescription(
                    ProductFetchFailureReason.ProductsUnavailable,
                    "No products defined."
                ));

                return;
            }

            Message<ProductList> result = await IAP.GetProductsBySKU(skus.ToArray());

            if(result.IsError)
            {
                ProductsCallback?.OnProductsFetchFailed(new ProductFetchFailureDescription(
                    ProductFetchFailureReason.ProviderUnavailable,
                    result.GetError().Code + ", " + result.GetError().HttpCode + ", " + result.GetError().Message + "."
                ));

                return;
            }

            List<ProductDescription> descriptions = new List<ProductDescription>();

            foreach (OculusProduct product in result.Data)
            {
                descriptions.Add(new ProductDescription(product.Sku, 
                    new ProductMetadata(product.FormattedPrice,
                                        product.Name,
                                        product.Description,
                                        "USD", 1)));
            }

            ProductsCallback?.OnProductsFetched(descriptions);
        }


        public override async void FetchPurchases()
        {           
            Message<PurchaseList> result = await IAP.GetViewerPurchases();

            if(result.IsError)
            {
                PurchaseFetchCallback?.OnPurchasesRetrievalFailed(new PurchasesFetchFailureDescription(
                    PurchasesFetchFailureReason.Unknown,
                    result.GetError().Code + ", " + result.GetError().HttpCode + ", " + result.GetError().Message + "."
                ));

                return;
            }

            List<Order> orders = new List<Order>();

            foreach(Purchase purchase in result.Data)
            {
                //1: product could be null if it was purchased but removed in a later build, then storeProduct is null too
                //2: product could be set if it was purchased and is still is defined, but set to not available on this store so storeProduct is null
                IAPProduct product = IAPManager.GetIAPProduct(purchase.Sku);
                UnityProduct storeProduct = product == null ? null : ReadOnlyProductCache.Find(product.ID);

                //recreate purchase receipt
                UnifiedReceipt receipt = new UnifiedReceipt()
                {
                    Payload = purchase.DeveloperPayload,
                    Store = IAPPlatform.OculusStore.ToString(),
                    TransactionID = purchase.ID
                };

                //check for non-consumed consumables
                if (product != null && product.type == UnityProductType.Consumable)
                {
                    if(storeProduct != null)
                        orders.Add(new PendingOrder(new CustomStoreCart(storeProduct, 1), new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
                    
                    //internal cleanup
                    if (DBManager.IsPurchased(product.ID))
                        DBManager.ConsumePurchase(product.ID);
                    
                    continue;
                }

                if(storeProduct != null)
                    orders.Add(new ConfirmedOrder(new CustomStoreCart(storeProduct, 1), new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
            }

            PurchaseFetchCallback?.OnAllPurchasesRetrieved(orders);
        }


        public override void CheckEntitlement(ProductDefinition product)
        {
            //TODO
        }


        public override void Purchase(ICart cart)
        {
            UnityProduct currentProduct = cart.Items().First().Product;
            currentCart = cart;

            #if UNITY_EDITOR
                IAPManager.Instance.CompletePurchase(currentProduct.definition.id);
            #else
                IAP.LaunchCheckoutFlow(currentProduct.definition.storeSpecificId).OnComplete(OnPurchaseSucceeded);
            #endif
        }


        //the async purchase product callback finished (be it successful or not)
        private void OnPurchaseSucceeded(Message<Purchase> result)
        {
            if(result.IsError)
            {
                OnPurchaseFailed(result.GetError().Message, result.GetError().Code);
                return;
            }

            UnifiedReceipt receipt = new UnifiedReceipt()
            {
                Payload = result.Data.DeveloperPayload,
                Store = IAPPlatform.OculusStore.ToString(),
                TransactionID = result.Data.ID
            };

            PurchaseCallback?.OnPurchaseSucceeded(new PendingOrder(currentCart, new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
        }


        //mapping error codes to reason for more user-friendly descriptions
        private void OnPurchaseFailed(string error, int code)
        {
            PurchaseFailureReason reason = PurchaseFailureReason.Unknown;

            switch(code)
            {
                case 1:
                case 7:
                case -1010:
                    reason = PurchaseFailureReason.ExistingPurchasePending;
                    break;
                case 10:
                case -1005:
                    reason = PurchaseFailureReason.UserCancelled;
                    error = string.Empty;
                    break;
                case 3:
                case -1009:
                    reason = PurchaseFailureReason.PurchasingUnavailable;
                    break;
                case 4:
                case -1006:
                    reason = PurchaseFailureReason.ProductUnavailable;
                    break;
                case 5:
                case -1002:
                case -1003:
                case -1004:
                    reason = PurchaseFailureReason.SignatureInvalid;
                    break;
            }

            PurchaseCallback?.OnPurchaseFailed(new FailedOrder(currentCart, reason, error));
        }


        public override void FinishTransaction(PendingOrder order)
        {
            UnityProduct storeProduct = IAPManager.GetFirstProductInOrder(order);

            if (storeProduct.definition.type != UnityProductType.Consumable)
                return;

            IAP.ConsumePurchase(storeProduct.definition.storeSpecificId).OnComplete(OnTransactionFinished);
        }


        //the async consume product callback finished (be it successful or not)
        private void OnTransactionFinished(Message result)
        {
            if (result.IsError)
            {
                OnPurchaseFailed(result.GetError().Message, result.GetError().Code);
            }
        }


        internal ConnectionState GetStoreConnectionState()
        {
            return Core.IsInitialized() ? ConnectionState.Connected : ConnectionState.Unavailable;
        }
    }
    #endif
}