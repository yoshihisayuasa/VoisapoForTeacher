/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

namespace UniPay
{
    #if STEAM_IAP
    using Steamworks;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;

    /// <summary>
    /// Represents the Unity IAP store implementation for Steam.
    /// Using Steam Inventory Services. 
    /// </summary>
    public class SteamStore : Store
    {      
        //definitions sent on initialization that should be requested from the provider.
        private IReadOnlyCollection<ProductDefinition> definitions;
        //keeping track of the product cart that is currently being processed
        private ICart currentCart = null;
        //default Steam currency or overwritten with local currency after initialization
        private string currencyCode = "USD";
        private CultureInfo cultureInfo = new CultureInfo("en-US");

        #pragma warning disable 0414
        protected SteamInventoryResult_t steamInventoryResult = SteamInventoryResult_t.Invalid;
        protected CallResult<SteamInventoryRequestPricesResult_t> steamRequestCurrencyResult;
        protected CallResult<SteamInventoryStartPurchaseResult_t> steamStartPurchaseResult;
        protected Callback<SteamInventoryResultReady_t> steamInventoryResultReady;
        protected Callback<MicroTxnAuthorizationResponse_t> steamMicroTxnAuthorizationResponse;
        #pragma warning restore 0414


        public override void Connect()
        {
            if (!SteamManager.Initialized)
            {
                ConnectCallback?.OnStoreConnectionFailed(new StoreConnectionFailureDescription("SteamManager is not initialized yet."));
                return;
            }

            steamStartPurchaseResult = CallResult<SteamInventoryStartPurchaseResult_t>.Create(OnPurchaseStarted);
            steamMicroTxnAuthorizationResponse = Callback<MicroTxnAuthorizationResponse_t>.Create(OnMicroTxnAuthorization);

            ConnectCallback?.OnStoreConnectionSucceeded();
        }


        public override void FetchProducts(IReadOnlyCollection<ProductDefinition> definitions)
        {
            this.definitions = definitions;

            steamInventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(OnSteamInventoryResult);
            bool result = SteamInventory.GetAllItems(out steamInventoryResult);

            if (!result)
            {
                ProductsCallback?.OnProductsFetchFailed(new ProductFetchFailureDescription(
                    ProductFetchFailureReason.ProviderUnavailable,
                    "Error on getting SteamInventory."
                ));
                steamInventoryResultReady.Unregister();
                SteamInventory.DestroyResult(steamInventoryResult);
            }
        }


        void OnSteamInventoryResult(SteamInventoryResultReady_t pCallback)
        {
            steamInventoryResultReady.Unregister();

            if (pCallback.m_result != EResult.k_EResultOK)
            {
                ProductsCallback?.OnProductsFetchFailed(new ProductFetchFailureDescription(
                    ProductFetchFailureReason.ProductsUnavailable,
                    "Error on getting SteamInventory."
                ));
                return;
            }

            steamRequestCurrencyResult = CallResult<SteamInventoryRequestPricesResult_t>.Create(OnSteamInventoryRequestPricesResult);
            SteamAPICall_t requestPricesHandle = SteamInventory.RequestPrices();
            steamRequestCurrencyResult.Set(requestPricesHandle);
        }


        void OnSteamInventoryRequestPricesResult(SteamInventoryRequestPricesResult_t pCallback, bool bIOFailure)
        {
            if (pCallback.m_result == EResult.k_EResultOK && !bIOFailure)
            {
                currencyCode = pCallback.m_rgchCurrency;

                CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
                foreach (CultureInfo culture in cultures)
                {
                    RegionInfo regionInfo = new RegionInfo(culture.LCID);
                    if (regionInfo.ISOCurrencySymbol == currencyCode)
                    {
                        cultureInfo = culture;
                        break;
                    }
                }
            }

            List<ProductDescription> descriptions = new List<ProductDescription>();

            foreach (ProductDefinition definition in definitions)
            {
                IAPProduct product = IAPManager.GetIAPProduct(definition.id);
                string priceString = product.GetPriceString();

                if (product.fetch)
                {
                    int steamProductID;
                    ulong currentPrice;
                    if (int.TryParse(definition.storeSpecificId, out steamProductID) && SteamInventory.GetItemPrice(new SteamItemDef_t(steamProductID), out currentPrice, out _))
                    {
                        priceString = string.Format(cultureInfo, "{0:C}", currentPrice / 100M);
                    }
                }

                descriptions.Add(new ProductDescription(definition.storeSpecificId,
                    new ProductMetadata(priceString,
                                        product.title,
                                        product.description,
                                        currencyCode, 1)));
            }

            //if the callback above has an error this is a non-blocking issue,
            //we still continue with the billing initialization without local prices
            ProductsCallback?.OnProductsFetched(descriptions);
        }


        public override void FetchPurchases()
        {
            //verify that the inventory handle is still valid
            //could have been destroyed or expired when trying to restore purchases at a later point
            EResult status = SteamInventory.GetResultStatus(steamInventoryResult);
            if (status != EResult.k_EResultOK)
            {
                PurchaseFetchCallback?.OnPurchasesRetrievalFailed(new PurchasesFetchFailureDescription(
                    PurchasesFetchFailureReason.Unknown,
                    "Error on getting SteamPurchases."
                ));
                return;
            }

            //read owned item count
            uint outItemsArraySize = 0;
            SteamInventory.GetResultItems(steamInventoryResult, null, ref outItemsArraySize);

            //still success with no products owned
            if (outItemsArraySize == 0)
            {
                PurchaseFetchCallback?.OnAllPurchasesRetrieved(new List<Order>());
                return;
            }

            //read owned item details
            SteamItemDetails_t[] steamItemDetails = new SteamItemDetails_t[outItemsArraySize];
            SteamInventory.GetResultItems(steamInventoryResult, steamItemDetails, ref outItemsArraySize);

            List<Order> orders = new List<Order>();

            for (int i = 0; i < steamItemDetails.Length; i++)
            {
                //1: product could be null if it was purchased but removed in a later build, then storeProduct is null too
                //2: product could be set if it was purchased and is still is defined, but set to not available on this store so storeProduct is null
                IAPProduct product = IAPManager.GetIAPProduct(steamItemDetails[i].m_iDefinition.ToString());
                Product storeProduct = product == null ? null : ReadOnlyProductCache.Find(product.ID);
               
                //recreate purchase receipt
                UnifiedReceipt receipt = new UnifiedReceipt()
                {
                    Payload = string.Empty,
                    Store = IAPPlatform.SteamStore.ToString(),
                    TransactionID = steamItemDetails[i].m_itemId.ToString()
                };    

                //check for non-consumed consumables
                if (product != null && product.type == ProductType.Consumable)
                {
                    if(storeProduct != null)
                        orders.Add(new PendingOrder(new CustomStoreCart(storeProduct, steamItemDetails[i].m_unQuantity), new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
                    
                    //internal cleanup
                    if (DBManager.IsPurchased(product.ID))
                        DBManager.ConsumePurchase(product.ID);
                    
                    continue;
                }

                if(storeProduct != null)
                    orders.Add(new ConfirmedOrder(new CustomStoreCart(storeProduct, steamItemDetails[i].m_unQuantity), new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
            }

            PurchaseFetchCallback?.OnAllPurchasesRetrieved(orders);
        }


        public override void CheckEntitlement(ProductDefinition product)
        {
            //TODO
        }


        public override void Purchase(ICart cart)
        {
            Product currentProduct = cart.Items().First().Product;
            currentCart = cart;

            int steamProductID = 0;
            if (int.TryParse(currentProduct.definition.storeSpecificId, out steamProductID))
            {
                steamInventoryResultReady.Unregister();
                steamInventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(OnPurchaseSucceeded);
                
                SteamAPICall_t startPurchaseHandle = SteamInventory.StartPurchase(new SteamItemDef_t[] { (SteamItemDef_t)steamProductID }, new uint[] { 1 }, 1);
                steamStartPurchaseResult.Set(startPurchaseHandle);
            }
            else
            {
                OnPurchaseFailed(PurchaseFailureReason.ProductUnavailable, "Cannot convert selected Product ID to Steam Item ID.");
            }
        }


        //a MicroTxn with Steam was initiated
        private void OnPurchaseStarted(SteamInventoryStartPurchaseResult_t pCallback, bool bIOFailure)
        {
            if (pCallback.m_result != EResult.k_EResultOK || bIOFailure)
            {
                OnPurchaseFailed(PurchaseFailureReason.Unknown, pCallback.m_result.ToString());
                return;
            }
        }


        //a MicroTxn authorization was either granted or canceled in the Steam overlay
        private void OnMicroTxnAuthorization(MicroTxnAuthorizationResponse_t pCallback)
        {
            if(pCallback.m_bAuthorized == 0)
            {
                OnPurchaseFailed(PurchaseFailureReason.UserCancelled, string.Empty);
                return;
            }
        }


        //the async purchase product callback finished (be it successful or not)
        private void OnPurchaseSucceeded(SteamInventoryResultReady_t pCallback)
        {
            if (pCallback.m_result != EResult.k_EResultOK)
            {
                OnPurchaseFailed(PurchaseFailureReason.Unknown, pCallback.m_result.ToString());
                return;
            }

            //get properties of purchased item
            uint ValueBufferSize = 0;
            bool result = SteamInventory.GetResultItemProperty(pCallback.m_handle, 0, null, out string ValueBuffer, ref ValueBufferSize);
            
            string transactionID = "";
            if (result)
            {
                ValueBufferSize = 64;
                SteamInventory.GetResultItemProperty(pCallback.m_handle, 0, "itemID", out transactionID, ref ValueBufferSize);
            }

            //create purchase receipt
            UnifiedReceipt receipt = new UnifiedReceipt()
            {
                Payload = string.Empty,
                Store = IAPPlatform.SteamStore.ToString(),
                TransactionID = transactionID
            };

            PurchaseCallback?.OnPurchaseSucceeded(new PendingOrder(currentCart, new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
        }


        private void OnPurchaseFailed(PurchaseFailureReason reason, string error)
        {
            PurchaseCallback?.OnPurchaseFailed(new FailedOrder(currentCart, reason, error));
        }


        public override void FinishTransaction(PendingOrder order)
        {
            Product storeProduct = IAPManager.GetFirstProductInOrder(order);

            if (storeProduct.definition.type != ProductType.Consumable)
            {
                SteamInventory.DestroyResult(steamInventoryResult);
                return;
            }

            steamInventoryResultReady.Unregister();
            steamInventoryResultReady = Callback<SteamInventoryResultReady_t>.Create(OnTransactionFinished);
            SteamInventory.ConsumeItem(out steamInventoryResult, (SteamItemInstanceID_t)ulong.Parse(order.Info.TransactionID), 1);
        }


        //the async consume product callback finished (be it successful or not)
        private void OnTransactionFinished(SteamInventoryResultReady_t pCallback)
        {
            if (pCallback.m_result != EResult.k_EResultOK)
            {
                //we cannot do anything to prevent a failed result anyway
                //so at least display a meaningful message if there is one
                if(pCallback.m_result != EResult.k_EResultFail)
                {
                    OnPurchaseFailed(PurchaseFailureReason.Unknown, pCallback.m_result.ToString());
                }
                
                return;
            }

            steamInventoryResultReady.Unregister();
            SteamInventory.DestroyResult(pCallback.m_handle);
        }


        internal ConnectionState GetStoreConnectionState()
        {
            return SteamManager.Initialized ? ConnectionState.Connected : ConnectionState.Unavailable;
        }
    }
    #endif
}