/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;

namespace UniPay
{
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;
    using UniPay.SimpleJSON;

    /// <summary>
    /// Represents the Unity IAP store implementation for PayPal.
    /// </summary>
    class PayPalStore : Store
    {
        /// <summary>
        /// Reference to this store class, since the user needs to confirm the purchase
        /// transaction manually in-game, thus calling the confirm method of this script.
        /// </summary>
        public static PayPalStore Instance { get; private set; }

        /// <summary>
        /// Callback for hooking into IAPManager methods.
        /// This is basically a stripped down version of the IStoreCallback.
        /// </summary>
        public IAPManager iapManager;

        //configuration file for client and secret keys
        private PayPalStoreConfig config;

        //generated OAuth acess token returned by PayPal servers on client validation
        private AccessToken accessToken;

        //keeping track of the order that is currently being processed, so we can confirm and finish it later on
        private string orderId;

        //keeping track of the product cart that is currently being processed
        private ICart currentCart = null;


        //set static variables to support disabling domain reload
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RuntimeInitializeLoadOnLoad()
        {
            Instance = null;
        }


        public PayPalStore(IAPManager iapManager)
        {
            Instance = this;
            this.iapManager = iapManager;

            config = iapManager.asset.customStoreConfig.PayPal;
        }


        public override async void Connect()
        {
            //delay initialization until after Start, otherwise it finishes instantly
            //this fixes an issue with script not being subscribed to receiptValidationInitializeEvent in Start() yet
            await Task.Delay(500);

            ConnectCallback?.OnStoreConnectionSucceeded();
        }


        public override void FetchProducts(IReadOnlyCollection<ProductDefinition> definitions)
        {
            List<ProductDescription> descriptions = new List<ProductDescription>();

            foreach (ProductDefinition definition in definitions)
            {
                IAPProduct product = IAPManager.GetIAPProduct(definition.id);

                descriptions.Add(new ProductDescription(definition.storeSpecificId, 
                    new ProductMetadata(product.GetPriceString(),
                                        product.title,
                                        product.description,
                                        "USD", 1)));
            }

            ProductsCallback?.OnProductsFetched(descriptions);
        }


        public override void FetchPurchases()
        {
            //nothing to do here, PayPal does not store purchases

            PurchaseFetchCallback?.OnAllPurchasesRetrieved(new List<Order>());
        }


        public override void CheckEntitlement(ProductDefinition product)
        {
            //nothing to do here, PayPal does not store purchases
        }


        public override void Purchase(ICart cart)
        {
            iapManager.StartCoroutine(PurchaseRequest(cart));
        }


        public override void FinishTransaction(PendingOrder order)
        {
            //callback expects a ConfirmedOrder
            ConfirmCallback?.OnConfirmOrderSucceeded(new ConfirmedOrder(order.CartOrdered, order.Info).Info.TransactionID);
        }


        /// <summary>
        /// Manually triggering purchase confirmation after a PayPal payment has been made.
        /// This is so that the transaction gets finished and PayPal actually substracts funds.
        /// </summary>
        public void ConfirmPurchase()
        {
            if (string.IsNullOrEmpty(orderId))
            {
                IAPManager.OnPurchaseFailed(null, PurchaseFailureReason.PurchaseMissing.ToString());
                return;
            }

            //without IAPGUARD: finish transaction here manually in UI (capture)
            #if !RECEIPT_VALIDATION
                iapManager.StartCoroutine(ApproveTransaction());
                return;
            #endif

            //with IAPGUARD: this is handled automatically in ReceiptValidatorServer
            //OnPurchasePending will try to process the PendingOrder and invoke validation
            #pragma warning disable 0162
            DeferredOrder deferredOrder = UnityIAPServices.StoreController().GetPurchases().OfType<DeferredOrder>().FirstOrDefault(x => x.Info.TransactionID == orderId);                        
            PurchaseCallback?.OnPurchaseSucceeded(new PendingOrder(deferredOrder.CartOrdered, deferredOrder.Info));
            #pragma warning restore 0162
        }


        IEnumerator GetAccessToken()
        {
            WWWForm form = new WWWForm();
            form.AddField("grant_type", "client_credentials");

            using (UnityWebRequest www = UnityWebRequest.Post(GetUrl("token"), form))
            {
                string auth = IAPManager.isDebug ? (config.sandbox.clientID + ":" + config.sandbox.secretKey) : (config.live.clientID + ":" + config.live.secretKey);
                auth = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
                auth = "Basic " + auth;
                www.SetRequestHeader("Authorization", auth);

                yield return www.SendWebRequest();

                if (IAPManager.isDebug && www.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("PayPalStore token error: " + www.error);
                }
                else
                {
                    JSONNode response = JSON.Parse(www.downloadHandler.text);
                    accessToken = new AccessToken(response["access_token"], response["expires_in"].AsInt);
                }
            }
        }


        IEnumerator PurchaseRequest(ICart cart)
        {
            if (accessToken == null || !accessToken.IsValid())
                yield return iapManager.StartCoroutine(GetAccessToken());

            Product currentProduct = cart.Items().First().Product;

            if (accessToken == null || !accessToken.IsValid())
            {
                IAPManager.OnPurchaseFailed(currentProduct.definition.id, PurchaseFailureReason.SignatureInvalid.ToString());
                yield break;
            }

            IAPProduct product = IAPManager.GetIAPProduct(currentProduct.definition.id);
            if (product == null)
            {
                IAPManager.OnPurchaseFailed(currentProduct.definition.id, PurchaseFailureReason.ProductUnavailable.ToString());
                yield break;
            }

            string postData = GetPostData(product);
            using (UnityWebRequest www = UnityWebRequest.Post(GetUrl("order", product.type), string.Empty, "application/json"))
            {
                UploadHandlerRaw uploadHandler = new UploadHandlerRaw(System.Text.Encoding.UTF8.GetBytes(postData));
                uploadHandler.contentType = "application/json";
                www.uploadHandler = uploadHandler;

                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + accessToken.token);
                www.SetRequestHeader("PayPal-Request-Id", System.Guid.NewGuid().ToString());

                yield return www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    if (IAPManager.isDebug)
                    {
                        Debug.Log("PayPalStore purchase error: " + www.error);
                    }

                    IAPManager.OnPurchaseFailed(currentProduct.definition.id, PurchaseFailureReason.PurchasingUnavailable.ToString());
                }
                else
                {
                    JSONNode response = JSON.Parse(www.downloadHandler.text);
                    orderId = response["id"];
                    currentCart = cart;

                    //get checkout link from HATEOAS links in response
                    string checkoutUrl = string.Empty;
                    JSONArray links = response["links"].AsArray;
                    for(int i = 0; i < links.Count; i++)
                    {
                        if(links[i]["rel"].Value == "payer-action" || links[i]["rel"].Value == "approve")
                        {
                            checkoutUrl = links[i]["href"].Value;
                            break;
                        }
                    }

                    //only show one waiting window at a time
                    UIShopFeedback.ShowLoading(false);
                    //show confirmation pending window and open website
                    UIShopFeedback.ShowConfirmation();

                    //PayPal creates an unconfirmed receipt upon purchase request
                    UnifiedReceipt receipt = new UnifiedReceipt()
                    {
                        Payload = string.Empty,
                        Store = IAPPlatform.PayPal.ToString(),
                        TransactionID = orderId
                    };

                    PurchaseCallback?.OnPurchaseDeferred(new DeferredOrder(currentCart, new CustomStoreOrderInfo(JsonUtility.ToJson(receipt), receipt.TransactionID)));
                    Application.OpenURL(checkoutUrl);
                }
            }
        }


        IEnumerator ApproveTransaction()
        {
            Product currentProduct = currentCart.Items().First().Product;
            IAPProduct product = IAPManager.GetIAPProduct(currentProduct.definition.id);
            if (product == null)
            {
                if(IAPManager.isDebug) Debug.Log("PayPalStore finish error could not load product: " + currentProduct);
                IAPManager.OnPurchaseFailed(null, PurchaseFailureReason.ProductUnavailable.ToString());
                yield break;
            }

            UnityWebRequest www = product.type == ProductType.Subscription ?
                UnityWebRequest.Get(GetUrl("capture", product.type)) : UnityWebRequest.Post(GetUrl("capture", product.type), string.Empty, "application/json");
            
            using (www)
            {
                www.SetRequestHeader("Content-Type", "application/json");
                www.SetRequestHeader("Authorization", "Bearer " + accessToken.token);
                www.SetRequestHeader("PayPal-Request-Id", orderId);

                yield return www.SendWebRequest();

                //payment could still be outstanding when initiating the capture call
                if (www.downloadHandler.text.Contains("APPROVAL_PENDING") || www.downloadHandler.text.Contains("ORDER_NOT_APPROVED"))
                {
                    if (UIShopFeedback.Instance != null)
                        UIShopFeedback.ShowMessage("Order is not approved yet. Please confirm the transaction in your browser.");

                    yield break;
                }
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    if (IAPManager.isDebug)
                    {
                        Debug.Log("PayPalStore finish error: " + www.error);
                    }

                    IAPManager.OnPurchaseFailed(product.ID, PurchaseFailureReason.PaymentDeclined.ToString());
                }
                else
                {
                    JSONNode response = JSON.Parse(www.downloadHandler.text);
                    if (response["status"].Value == "COMPLETED" || response["status"].Value == "ACTIVE")
                    {
                        DeferredOrder deferredOrder = UnityIAPServices.StoreController().GetPurchases().OfType<DeferredOrder>().FirstOrDefault(x => x.Info.TransactionID == orderId);                        
                        orderId = string.Empty;
                        currentCart = null;

                        if (UIShopFeedback.Instance != null && UIShopFeedback.Instance.confirmWindow != null)
                            UIShopFeedback.Instance.confirmWindow.SetActive(false);

                        PurchaseCallback?.OnPurchaseSucceeded(new PendingOrder(deferredOrder.CartOrdered, deferredOrder.Info));
                    }
                }
            }
        }


        string GetPostData(IAPProduct product)
        {
            switch(product.type)
            {
                case ProductType.Subscription:
                    return GetPostDataSubscription(product);
                default:
                    return GetPostDataOneTime(product);
            }
        }


        string GetPostDataOneTime(IAPProduct product)
        {
            JSONNode data = new JSONClass();
            data["intent"] = "CAPTURE";

            StoreMetaDefinition storeDefinition = product.storeIDs.Find(x => x.store == "PayPal" && x.active);
            string price = product.priceList.Find(x => x.type == IAPExchangeObject.ExchangeType.RealMoney).realPrice;
            price = Regex.Match(price, @"[0-9]+\.?[0-9,]*").Value;

            JSONNode unit = new JSONClass();
            JSONNode amount = new JSONClass();
            amount["currency_code"] = config.currencyCode;
            amount["value"] = price;

            JSONNode total = new JSONClass();
            total["currency_code"] = amount["currency_code"].Value;
            total["value"] = amount["value"].Value;

            JSONNode breakdown = new JSONClass();
            breakdown["item_total"] = total;
            amount["breakdown"] = breakdown;

            unit["amount"] = amount;
            unit["description"] = "Goods for " + Application.productName;

            JSONNode item = new JSONClass();
            item["name"] = string.IsNullOrEmpty(product.title) ? product.ID : product.title;
            item["description"] = product.type == ProductType.NonConsumable ? "Non-Consumable" : product.type.ToString();
            item["unit_amount"] = amount;
            item["quantity"] = "1";
            item["sku"] = storeDefinition == null ? product.ID : storeDefinition.ID;

            unit["items"] = new JSONArray();
            unit["items"].Add(item);

            data["purchase_units"] = new JSONArray();
            data["purchase_units"].Add(unit);

            JSONNode paymentSource = new JSONClass();
            JSONNode paypal = new JSONClass();
            JSONNode context = new JSONClass();
            context["return_url"] = config.returnUrl;
            paypal["experience_context"] = context;
            paymentSource["paypal"] = paypal;

            data["payment_source"] = paymentSource;

            return data.ToString();
        }


        string GetPostDataSubscription(IAPProduct product)
        {
            JSONNode data = new JSONClass();

            StoreMetaDefinition storeDefinition = product.storeIDs.Find(x => x.store == "PayPal" && x.active);
            data["plan_id"] = storeDefinition == null ? product.ID : storeDefinition.ID;
            data["quantity"] = "1";

            JSONNode context = new JSONClass();
            context["return_url"] = config.returnUrl;

            #if RECEIPT_VALIDATION
                context["user_action"] = "CONTINUE";
            #endif

            data["application_context"] = context;

            return data.ToString();
        }


        string GetUrl(string api, ProductType type = ProductType.NonConsumable)
        {
            switch(api)
            {
                case "token":
                    if (IAPManager.isDebug) return "https://api-m.sandbox.paypal.com/v1/oauth2/token";
                    else return "https://api-m.paypal.com/v1/oauth2/token";
                case "order":
                    switch (type)
                    {
                        case ProductType.Subscription:
                            if (IAPManager.isDebug) return "https://api-m.sandbox.paypal.com/v1/billing/subscriptions";
                            else return "https://api-m.paypal.com/v1/billing/subscriptions";

                        default:
                            if (IAPManager.isDebug) return "https://api-m.sandbox.paypal.com/v2/checkout/orders";
                            else return "https://api-m.paypal.com/v2/checkout/orders";
                    }
                case "capture":
                    switch(type)
                    {
                        case ProductType.Subscription:
                            return GetUrl("order", type) + "/" + orderId;

                        default:
                            return GetUrl("order") + "/" + orderId + "/capture";
                    }
            }

            return string.Empty;
        }


        [Serializable]
        public class AccessToken
        {
            public string token;
            public long expirationTime;

            public AccessToken(string token, long time)
            {
                this.token = token;
                expirationTime = new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() + time;
            }

            public bool IsValid()
            {
                return new DateTimeOffset(DateTime.Now).ToUnixTimeSeconds() < expirationTime;
            }
        }


        internal ConnectionState GetStoreConnectionState()
        {
            return ConnectionState.Connected;
        }
    }
}
