/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UniPay
{
    using UnityEngine.Networking;
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;
    using UniPay.SimpleJSON;

    /// <summary>
    /// Server-side, remote receipt validation via IAPGUARD (https://iapguard.com).
    /// Supports getting user inventory from cloud storage to not only rely on local purchase data.
    /// </summary>
    public class ReceiptValidatorServer : ReceiptValidator
    {
        #pragma warning disable 0067,0414
        /// <summary>
        /// Unused. Fired when fetching user's cloud inventory finished.
        /// </summary>
        public static event Action inventoryCallback;
        /// <summary>
        /// Unused. Fired when validating a product order finished.
        /// </summary>
        public static event Action<string, JSONNode> purchaseCallback;

        [Header("General Data")]
        public string appID;
        public string userID = "";

        [Header("User Inventory is not supported on the Free plan.", order = 0)]
        [Header("Please leave it on 'Disabled' if you didn't upgrade.", order = 1)]
        public InventoryRequestType inventoryRequestType = InventoryRequestType.Disabled;

        const string validationEndpoint = "https://api.iapguard.com/v1/receipt/";
        const string userEndpoint = "https://api.iapguard.com/v1/user/";

        Dictionary<string, PurchaseResponse> inventory = new Dictionary<string, PurchaseResponse>();
        Dictionary<string, ReceiptRequest> activeRequests = new Dictionary<string, ReceiptRequest>();
        CrossPlatformValidator localValidator = null;
        const string lastInventoryTimestampKey = "fbrv_inventory_timestamp";
        float lastInventoryTime = -1;
        bool inventoryRequestActive = false;
        int inventoryDelay = 1800; //30 minutes
        #pragma warning restore 0067,0414


        #if !RECEIPT_VALIDATION
        void Start()
        {
            #if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS || PAYPAL_IAP
                Debug.LogError("You are using " + GetType() + " but did not add the 'RECEIPT_VALIDATION' define to 'Project Settings > Player > Scripting Define Symbols'. Validation code will not compile!");
            #endif
        }
        #endif


        #if RECEIPT_VALIDATION
        //subscribe to IAPManager events
        void Start()
        {
            if (!CanValidate() || !IAPManager.Instance)
                return;

            IAPManager.receiptValidationInitializeEvent += OnValidationInitialize;
            IAPManager.receiptValidationPurchaseEvent += Validate;
        }


        /// <summary>
        /// Returns whether this type of validation is supported on the runtime platform.
        /// </summary>
        public override bool CanValidate()
        {          
            if (IsLocalValidationSupported() || IsServerValidationSupported())
                return true;

            return false;
        }


        void OnValidationInitialize()
        {
            List<Product> products = IAPManager.controller.GetProducts().ToList();
            List<Order> orders = IAPManager.controller.GetPurchases().ToList();

            //finding products which do not have a local receipt to begin with
            List<Product> toRemove = products.Where(p => 
                DBManager.IsPurchased(p.definition.id) && 
                orders.Any(o => 
                    IAPManager.GetFirstProductInOrder(o).definition.id == p.definition.id && 
                    string.IsNullOrEmpty(o.Info.Receipt)
                )
            ).ToList();
            //and removing them in local database
            foreach (Product p in toRemove)
                RemovePurchase(p.definition.id);
            
            if (IsLocalValidationSupported())
            {
                try
                {
                    localValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
                }
                catch (NotImplementedException) { }
            }                

            if (IsServerValidationSupported())
            {
                if (inventoryRequestType == InventoryRequestType.Disabled || inventoryRequestType == InventoryRequestType.Manual)
                    return;

                RequestInventory();
            }
        }


        /// <summary>
        /// Request inventory from the server, for the user specified as 'userID'.
        /// </summary>
        public void RequestInventory()
        {
            //in case requesting inventory was disabled or limited by delay timing
            if (!CanRequestInventory())
            {
                if (IAPManager.isDebug && inventoryRequestType != InventoryRequestType.Disabled)
                    Debug.LogWarning("IAPGUARD: CanRequestInventory returned false.");

                return;
            }

            //server validation is not supported on this platform, so no inventory is stored either or requests exceeded
            if (IAPManager.Instance == null || IAPManager.controller == null)
            {
                if (IAPManager.isDebug) Debug.LogWarning("IAPGUARD: Inventory Request not supported.");
                return;
            }

            //no purchase detected on this account, RequestInventory call is not necessary and was cancelled
            //if you are sure that this account has purchased products, instruct the user to initiate a restore first
            if(!HasPurchaseActive() && !HasPurchaseHistory())
            {
                if (IAPManager.isDebug) Debug.LogWarning("IAPGUARD: Inventory Request not necessary.");
                return;
            }

            inventoryRequestActive = true;
            StartCoroutine(RequestInventoryRoutine());
        }


        //on purchase (PendingOrder)
        public override void Validate(Order order)
        {   
            if (IsLocalValidationSupported() && localValidator != null)
            {
                Product storeProduct = IAPManager.GetFirstProductInOrder(order);
                PurchaseState state = LocalValidation(order);

                switch (state)
                {
                    case PurchaseState.Purchased: //continue with server validation
                        break;
                    case PurchaseState.Pending: //do nothing
                        if (IAPManager.isDebug) Debug.Log("Client Validation: Product purchase for '" + storeProduct.definition.storeSpecificId + "' is pending.");
                        return;
                    case PurchaseState.Failed: //set transaction finished
                        if (IAPManager.isDebug) Debug.Log("Client Validation: Product purchase for '" + storeProduct.definition.storeSpecificId + "' deemed as invalid.");
                        IAPManager.OnPurchaseFailed(storeProduct.definition.id, "Your Purchase: " + storeProduct.metadata.localizedTitle + "\n\nYour transaction failed validation.");
                        IAPManager.controller.ConfirmPurchase(order as PendingOrder);
                        return;
                }
            }

            if (IsServerValidationSupported())
            {
                StartCoroutine(RequestPurchaseRoutine(order));
            }
        }


        IEnumerator RequestInventoryRoutine()
        {
            using (UnityWebRequest www = UnityWebRequest.Get(userEndpoint + appID + "/" + userID))
            {
                www.SetRequestHeader("content-type", "application/json");
                yield return www.SendWebRequest();

                //raw JSON response
                JSONNode rawResponse = JSON.Parse(www.downloadHandler.text);
                JSONArray purchaseArray = rawResponse["purchases"].AsArray;

                inventory.Clear();
                for (int i = 0; i < purchaseArray.Count; i++)
                {
                    string productId = IAPManager.GetProductGlobalIdentifier(purchaseArray[i]["data"]["productId"].Value);
                    PurchaseResponse purchase = JsonUtility.FromJson<PurchaseResponse>(purchaseArray[i]["data"].ToString());
                    inventory.Add(productId, purchase);

                    //grant products which are not owned yet
                    if (IsOwned(productId))
                    {
                        if (!DBManager.IsPurchased(productId))
                        {
                            if (IAPManager.isDebug) Debug.Log("Inventory unlocked product: '" + productId + ".");
                            DBManager.SetPurchase(productId);
                        }
                    }
                    else
                        RemovePurchase(productId);
                }

                List<Product> products = IAPManager.controller.GetProducts().ToList();
                //removing products which are not included in retrieved user inventory
                List<Product> toRemove = products.Where(p => DBManager.IsPurchased(p.definition.id) && !inventory.ContainsKey(p.definition.id)).ToList();
                foreach (Product p in toRemove)
                    RemovePurchase(p.definition.id);

                SetPurchaseHistory();
            }

            lastInventoryTime = Time.realtimeSinceStartup;
            inventoryRequestActive = false;
            inventoryCallback?.Invoke();
        }


        // handles an online verification request and response
        IEnumerator RequestPurchaseRoutine(Order order)
        {
            Product storeProduct = IAPManager.GetFirstProductInOrder(order);
            string productId = storeProduct.definition.id;
            string storeId = storeProduct.definition.storeSpecificId;
            UnifiedReceipt receiptData = JsonUtility.FromJson<UnifiedReceipt>(order.Info.Receipt);

            ReceiptRequest request = new ReceiptRequest()
            {
                store = receiptData.Store,
                bid = Application.identifier,
                pid = storeProduct.definition.storeSpecificId,
                user = userID,
                type = GetType(storeProduct.definition.type),
                receipt = receiptData.TransactionID
            };
            string postData = JsonUtility.ToJson(request);

            //do not issue another request when there is still one waiting for a response
            if (activeRequests.ContainsKey(request.pid)) yield break;
            activeRequests.Add(request.pid, request);

            JSONNode rawResponse = null;
            bool success = false;
            using (UnityWebRequest www = UnityWebRequest.Put(validationEndpoint + appID, postData))
            {
                www.SetRequestHeader("content-type", "application/json");
                www.SetRequestHeader("X-App-Version", Application.version);
                yield return www.SendWebRequest();
                activeRequests.Remove(request.pid);

                //raw JSON response
                try
                {
                    rawResponse = JSON.Parse(www.downloadHandler.text);
                    success = www.error == null && rawResponse != null && string.IsNullOrEmpty(rawResponse["error"]) && rawResponse.HasKey("data");
                }
                catch
                {
                    //there might be an configuration issue since the response was not valid JSON, do not finish transaction and print error message directly
                    if (IAPManager.isDebug) Debug.Log("IAPGUARD response failed for: '" + storeId + "'.\n" + www.downloadHandler.text);
                    IAPManager.OnPurchaseFailed(productId, "Your Purchase: " + storeProduct.metadata.localizedTitle + "\n\nUnexpected validation response.");
                    yield break;
                }

                if(success)
                {
                    if (IAPManager.isDebug) Debug.Log("IAPGUARD response passed for: '" + productId + "'.");
                    PurchaseResponse thisPurchase = JsonUtility.FromJson<PurchaseResponse>(rawResponse["data"].ToString());

                    //remember this userID for this session if we received a server-generated one
                    if (string.IsNullOrEmpty(userID) && rawResponse.HasKey("user"))
                        userID = rawResponse["user"].Value;

                    if (inventory.ContainsKey(productId)) inventory[productId] = thisPurchase; //already exist, replace
                    else inventory.Add(productId, thisPurchase); //add new to inventory

                    //check purchasing state in raw data
                    if (IsOwned(productId)) IAPManager.Instance.CompletePurchase(productId);
                    else
                    {
                        IAPManager.OnPurchaseFailed(productId, "Your Purchase: " + storeProduct.metadata.localizedTitle + "\n\nYour transaction seems to be expired.");
                        RemovePurchase(productId);
                    }

                    if (receiptData.Store == IAPPlatform.PayPal.ToString() && 
                        UIShopFeedback.Instance != null && UIShopFeedback.Instance.confirmWindow != null)
                        UIShopFeedback.Instance.confirmWindow.SetActive(false);
                }
                else
                {
                    int errorCode = rawResponse.HasKey("error") ? rawResponse["code"].AsInt : -1;
                    //do not complete pending purchases but still leave them open for processing again later
                    if (errorCode == 10130)
                    {
                        //we have a pending transaction for an outside purchase we still have to confirm ourselves, like on PayPal
                        if (UIShopFeedback.Instance != null && UIShopFeedback.Instance.confirmWindow != null && UIShopFeedback.Instance.confirmWindow.activeInHierarchy)
                            UIShopFeedback.ShowMessage("Order is not approved yet. Please confirm the transaction in your browser.");
                            
                        yield break;
                    }

                    if (IAPManager.isDebug) Debug.Log("IAPGUARD response failed for: '" + storeId + "'. Code:" + errorCode + ", " + rawResponse["error"].Value);
                    IAPManager.OnPurchaseFailed(productId, "Your Purchase: " + storeProduct.metadata.localizedTitle + "\n\nYour transaction failed validation.");
                    RemovePurchase(productId);
                }
            }

            if(order is PendingOrder)
                IAPManager.controller.ConfirmPurchase(order as PendingOrder);

            purchaseCallback?.Invoke(productId, rawResponse);
        }


        /// <summary>
        /// Return current user inventory stored in memory.
        /// </summary>
        public Dictionary<string, PurchaseResponse> GetInventory()
        {
            return inventory;
        }


        bool IsOwned(string productId)
        {
            if (inventoryRequestType == InventoryRequestType.Disabled)
            {
                Order order = IAPManager.controller.GetPurchases().First(o => IAPManager.GetFirstProductInOrder(o).definition.id == productId);
                return order != null && !string.IsNullOrEmpty(order.Info.TransactionID);
            }

            int[] purchaseStates = new int[] { 0, 1, 4 };
            if (inventory.ContainsKey(productId) && Array.Exists(purchaseStates, x => x == inventory[productId].status))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns whether getting inventory is currently disabled, limited or not possible.
        /// </summary>
        public bool CanRequestInventory()
        {
            //GetInventory request is already active. This call was cancelled
            if (inventoryRequestActive)
            {
                return false;
            }

            switch (inventoryRequestType)
            {
                //GetInventory call is disabled. If your plan supports User Inventory, select a different Inventory Request Type
                case InventoryRequestType.Disabled:
                    return false;

                //GetInventory call was cancelled because it has already been requested before
                case InventoryRequestType.Once:
                    if (lastInventoryTime > 0)
                    {
                        return false;
                    }
                    break;

                //GetInventory call was cancelled to prevent excessive bandwidth consumption and API limits
                case InventoryRequestType.Delay:
                    if (lastInventoryTime > 0 && Time.realtimeSinceStartup - lastInventoryTime < inventoryDelay)
                    {
                        return false;
                    }
                    break;
            }

            //All checks passed, but a user identifier has not been set
            if (string.IsNullOrEmpty(userID))
            {
                return false;
            }

            return true;
        }


        void RemovePurchase(string productId)
        {
            //check whether the product was set to purchased locally
            IAPProduct product = IAPManager.GetIAPProduct(productId);
            if (product == null || product.type == ProductType.Consumable || !DBManager.IsPurchased(productId))
                return;

            ShopItem2D item = null;
            if (IAPManager.Instance != null)
                item = IAPManager.GetShopItem(productId);
            if (item != null)
                item.Purchased(false);
            
            DBManager.ConsumePurchase(productId);
        }


        void SetPurchaseHistory()
        {
            if (DBManager.IsPlayerData(lastInventoryTimestampKey) && inventory.Count == 0)
            {
                DBManager.ConsumePlayerData(lastInventoryTimestampKey);
                return;
            }

            if (!DBManager.IsPlayerData(lastInventoryTimestampKey) && inventory.Count > 0)
            {
                DBManager.SetPlayerData(lastInventoryTimestampKey, new JSONData(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()));
            }
        }


        bool HasPurchaseHistory()
        {
            if (!DBManager.IsPlayerData(lastInventoryTimestampKey))
                return false;

            long lastTimestamp = long.Parse(DBManager.GetPlayerData(lastInventoryTimestampKey).Value);
            long timestampNow = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            if (timestampNow - lastTimestamp < 2628000) //2628000 seconds = 1 month
            {
                return true;
            }

            DBManager.ConsumePlayerData(lastInventoryTimestampKey);
            return false;
        }


        //check whether Unity IAP returned purchases from the App Store
        bool HasPurchaseActive()
        {
            #if PAYPAL_IAP
                //we simply don't know since PayPal does not store purchases
                //we have to always check for inventory on each user
                return true;
            #else
                return IAPManager.controller.GetPurchases().Count > 0;
            #endif
        }


        PurchaseState LocalValidation(Order order)
        {
            try
            {
                IPurchaseReceipt[] result = localValidator.Validate(order.Info.Receipt);

                foreach (IPurchaseReceipt receipt in result)
                {
                    if (receipt is GooglePlayReceipt googleReceipt)
                    {
                        if ((int)googleReceipt.purchaseState == 2 || (int)googleReceipt.purchaseState == 4)
                        {
                            return PurchaseState.Pending;
                        }
                    }
                }

                return PurchaseState.Purchased;
            }
            //If the purchase is deemed invalid, the validator throws an exception
            catch (IAPSecurityException)
            {
                return PurchaseState.Failed;
            }
        }


        string GetType(ProductType type)
        {
            switch (type)
            {
                case ProductType.Consumable:
                case ProductType.Subscription:
                    return type.ToString();

                default:
                    return "Non-Consumable";
            }
        }


        bool IsLocalValidationSupported()
        {
            //The CrossPlatform validator only supports the GooglePlay Store.
            if (Application.platform == RuntimePlatform.Android && UnityIAPServices.GetDefaultStore() == GooglePlay.Name)
                return true;

            return false;
        }


        bool IsServerValidationSupported(string receipt = null)
        {
            //exclude platforms that could run on other platforms
            #if OCULUS_IAP || STEAM_IAP || AMAZON_IAP
                return false;

            //these are the platforms we have to check
            #elif (!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS)) || PAYPAL_IAP 

            if(string.IsNullOrEmpty(receipt))
                return true;

            string storeName = string.Empty;
            try
            {
              UnifiedReceipt receiptData = JsonUtility.FromJson<UnifiedReceipt>(receipt);
              storeName = receiptData.Store;
            }
            catch(Exception)
            { }

            //IAPGUARD supports the Google Play, Apple's App Stores and PayPal.
            List<string> supportedStores = new List<string>()
            {
                IAPPlatform.GooglePlay.ToString(),
                IAPPlatform.AppleAppStore.ToString(),
                IAPPlatform.MacAppStore.ToString(),
                IAPPlatform.PayPal.ToString()
            };

            return supportedStores.Contains(storeName);
            #else
            return false;
            #endif
        }
        #endif
    }


    /// <summary>
    /// Available options for fetching User Inventory.
    /// </summary>
    public enum InventoryRequestType
    {
        Disabled,
        Manual,
        Once,
        Delay
    }


    enum PurchaseState
    {
        Purchased,
        Pending,
        Failed
    }


    [System.Serializable]
    struct ReceiptRequest
    {
        public string store;
        public string bid;
        public string pid;
        public string type;
        public string user;
        public string receipt;
    }


    [System.Serializable]
    public struct PurchaseResponse
    {
        public int status;
        public string type;
        public int expiresDate;
        public bool autoRenew;
        public bool billingRetry;
        public string productId;
        public bool sandbox;
    }
}