/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace UniPay
{
    #if AMAZON_IAP
    using com.amazon.device.iap.cpt;
    using AmazonPurchase = com.amazon.device.iap.cpt.PurchaseResponse;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;

    /// <summary>
    /// Represents the Unity IAP store implementation for the Amazon App Store.
    /// </summary>
    public class AmazonStore : Store
    {      
        //bridge to Amazon IAP library
        private static IAmazonIapV2 service = null;

        //keeping track of the product cart that is currently being processed
        private ICart currentCart = null;


        public override void Connect()
        {
            service = AmazonIapV2Impl.Instance;
            service.AddGetProductDataResponseListener(OnProductsFetched);
            service.AddPurchaseResponseListener(OnPurchaseSucceeded);
            service.AddGetPurchaseUpdatesResponseListener(OnPurchasesFetched);
            
            //service.AddGetUserDataResponseListener(OnUserDataFetched);
            //service.GetUserData();
            //{"requestId":"x","amazonUserData":{"userId":"y","marketplace":"US"},"status":"SUCCESSFUL"}

            ConnectCallback?.OnStoreConnectionSucceeded();
        }


        public override void FetchProducts(IReadOnlyCollection<ProductDefinition> definitions)
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

            SkusInput request = new SkusInput();
            request.Skus = skus;
            service.GetProductData(request);
        }


        private void OnProductsFetched(GetProductDataResponse result)
        {            
            //{"requestId":"x","productDataMap":{"no_ads":{"sku":"no_ads","productType":"ENTITLED","price":"$2,49","title":"no ads","description":"example for a permanent product.","smallIconUrl":"http://","coinsReward":null}},"unavailableSkus":["coins_small","abo_monthly","abo_weekly","coin_pack","bundle","big_coin_pack"],"status":"SUCCESSFUL"}

            if(result.Status != "SUCCESSFUL")
            {
                PurchaseFetchCallback?.OnPurchasesRetrievalFailed(new PurchasesFetchFailureDescription(
                    PurchasesFetchFailureReason.Unknown,
                    "Error on getting Products."
                ));

                return;
            }

            List<ProductDescription> descriptions = new List<ProductDescription>();

            foreach (ProductData product in result.ProductDataMap.Values)
            {
                descriptions.Add(new ProductDescription(product.Sku, 
                    new ProductMetadata(product.Price,
                                        product.Title,
                                        product.Description,
                                        "USD", 1)));
            }

            ProductsCallback?.OnProductsFetched(descriptions);
        }


        public override void FetchPurchases()
        {           
            ResetInput request = new ResetInput();
            request.Reset = true;

            service.GetPurchaseUpdates(request);
        }


        private void OnPurchasesFetched(GetPurchaseUpdatesResponse result)
        {
            //{"requestId":"x","amazonUserData":{"userId":"y","marketplace":"US"},"receipts":[{"receiptId":"z","cancelDate":0,"purchaseDate":1762638822000,"sku":"coins","productType":"CONSUMABLE","deferredDate":0,"deferredSku":null,"termSku":null}],"status":"SUCCESSFUL","hasMore":false}
            //{"requestId":"x","amazonUserData":{"userId":"y","marketplace":"US"},"receipts":[{"receiptId":"z","cancelDate":1769723080000,"purchaseDate":1769720928000,"sku":"no_ads","productType":"ENTITLED","deferredDate":0,"deferredSku":null,"termSku":null}],"status":"SUCCESSFUL","hasMore":false}

            if(result.Status != "SUCCESSFUL")
            {
                PurchaseFetchCallback?.OnPurchasesRetrievalFailed(new PurchasesFetchFailureDescription(
                    PurchasesFetchFailureReason.Unknown,
                    "Error on getting Purchase Inventory."
                ));

                return;
            }

            List<Order> orders = new List<Order>();

            foreach(PurchaseReceipt purchase in result.Receipts)
            {
                //1: product could be null if it was purchased but removed in a later build, then storeProduct is null too
                //2: product could be set if it was purchased and is still is defined, but set to not available on this store so storeProduct is null
                IAPProduct product = IAPManager.GetIAPProduct(purchase.Sku);
                Product storeProduct = product == null ? null : ReadOnlyProductCache.Find(product.ID);

                //recreate purchase receipt
                UnifiedReceipt receipt = new UnifiedReceipt()
                {
                    Payload = string.Empty,
                    Store = IAPPlatform.AmazonAppStore.ToString(),
                    TransactionID = purchase.ReceiptId
                };

                //check for non-consumed consumables
                if (product != null && product.type == ProductType.Consumable)
                {
                    if(storeProduct != null)
                        orders.Add(new PendingOrder(new CustomStoreCart(storeProduct, 1), new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
                    
                    //internal cleanup
                    if (DBManager.IsPurchased(product.ID))
                        DBManager.ConsumePurchase(product.ID);
                    
                    continue;
                }

                //works for non-consumables, remove if cancelled/refunded
                //a subscription could still be active though
                if(purchase.CancelDate > 0)
                {
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
            currentCart = cart;
            Product currentProduct = cart.Items().First().Product;

            SkuInput request = new SkuInput();
            request.Sku = currentProduct.definition.storeSpecificId;

            service.Purchase(request);
        }


        //the async purchase product callback finished (be it successful or not)
        private void OnPurchaseSucceeded(AmazonPurchase result)
        {
            //{"requestId":"x","amazonUserData":{"userId":"x","marketplace":"US"},"purchaseReceipt":null,"status":"FAILED"}
            //{"requestId":"x","amazonUserData":{"userId":"x","marketplace":"US"},"purchaseReceipt":{"receiptId":"y","cancelDate":0,"purchaseDate":1769720239000,"sku":"coins","productType":"CONSUMABLE","deferredDate":0,"deferredSku":null,"termSku":null},"status":"SUCCESSFUL"}

            if(result.Status != "SUCCESSFUL")
            {
                OnPurchaseFailed(PurchaseFailureReason.PurchaseMissing, "Purchase cancelled: " + result.Status);
                return;
            }

            UnifiedReceipt receipt = new UnifiedReceipt()
            {
                Payload = string.Empty,
                Store = IAPPlatform.AmazonAppStore.ToString(),
                TransactionID = result.PurchaseReceipt.ReceiptId
            };

            PurchaseCallback?.OnPurchaseSucceeded(new PendingOrder(currentCart, new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
        }


        private void OnPurchaseFailed(PurchaseFailureReason reason, string error)
        {
            PurchaseCallback?.OnPurchaseFailed(new FailedOrder(currentCart, reason, error));
        }


        public override void FinishTransaction(PendingOrder order)
        {
            NotifyFulfillmentInput request = new NotifyFulfillmentInput();
            request.ReceiptId = order.Info.TransactionID;
            request.FulfillmentResult = "FULFILLED";

            service.NotifyFulfillment(request);
        }


        internal ConnectionState GetStoreConnectionState()
        {
            return service != null ? ConnectionState.Connected : ConnectionState.Unavailable;
        }
    }
    #endif
}