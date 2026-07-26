/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UniPay
{
    using UnityEngine.Purchasing;

    /// <summary>
    /// Unity IAP cross-platform wrapper for real money purchases, as well as for virtual ingame purchases (for virtual currency).
    /// Initializes the Unity IAP billing system, handles different store interfaces and integrates their callbacks respectively.
    /// </summary>
    public class IAPManager : MonoBehaviour
    {
        /// <summary>
        /// Reference to the IAP configuration asset made in the Project Settings window.
        /// </summary>
        public IAPScriptableObject asset;

        /// <summary>
        /// Toggle that defines whether the IAPManager should initialize itself on first load.
        /// </summary> 
        public bool autoInitialize = true;

        /// <summary>
        /// Reference to this manager instance.
        /// </summary>
        public static IAPManager Instance { get; private set; }

        /// <summary>
        /// Debug messages are enabled in Development build automatically.
        /// </summary>
        public static bool isDebug;

        /// <summary>
        /// Instantiated shop items in the current scene, accessible via their product ID.
        /// </summary>
        public static Dictionary<string, ShopItem2D> shopItems;

        /// <summary>
        /// fired when Unity IAP initialization completes
        /// </summary>
        public static event Action initializeSucceededEvent;

        /// <summary>
        /// fired when Unity IAP initialization fails, providing error text
        /// </summary>
        public static event Action<string> initializeFailedEvent;

        /// <summary>
        /// fired when a purchase is initiated, delivering its product id
        /// </summary>
        public static event Action<string> purchaseStartedEvent;

        /// <summary>
        /// fired when a purchase succeeds, delivering its product id
        /// </summary>
        public static event Action<string> purchaseSucceededEvent;

        /// <summary>
        /// fired when a purchase fails, providing error text
        /// </summary>
        public static event Action<string> purchaseFailedEvent;

        /// <summary>
        /// fired when a consume is initiated, delivering its product id
        /// </summary>
        public static event Action<string> consumeStartedEvent;

        /// <summary>
        /// fired when a consume succeeds, delivering its product id
        /// </summary>
        public static event Action<string> consumeSucceededEvent;

        /// <summary>
        /// fired when a consume fails, providing error text
        /// </summary>
        public static event Action<string> consumeFailedEvent;

        /// <summary>
        /// fired when a restore transactions workflow is initiated
        /// </summary>
        public static event Action restoreTransactionsStartedEvent;

        /// <summary>
        /// fired when a restore transactions workflow completes, delivering its state
        /// </summary>
        public static event Action<bool> restoreTransactionsFinishedEvent;

        /// <summary>
        /// fired when billing initialized and would be ready for validating current products
        /// </summary>
        public static event Action receiptValidationInitializeEvent;

        /// <summary>
        /// fired when a purchase transaction completes or restores locally, delivering the Order
        /// </summary>
        public static event Action<Order> receiptValidationPurchaseEvent;

        #pragma warning disable 0649
        //fired when trying to purchase virtual product from remote source
        internal static event Action<IAPProduct> remotePurchaseVirtualEvent;
        //fired when trying to consume a product purchase from remote source
        internal static event Action<IAPProduct, int> remoteConsumePurchaseEvent;
        #pragma warning restore 0649

        public static StoreController controller;
        public static bool Initialized => Instance.isInitialized;
        //internal flag storing whether billing initialized
        private bool isInitialized = false;
        //retry policy to be used for store reconnecting
        private ExponentialBackOffRetryPolicy retryPolicy = new ExponentialBackOffRetryPolicy();


        //set static variables to support disabling domain reload
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RuntimeInitializeLoadOnLoad()
        {
            Instance = null;
            isDebug = false;
            shopItems = new Dictionary<string, ShopItem2D>();
            controller = null;
        }


        //initialize IAPs, billing systems and database,
        //as well as shop components in this order
        void Awake()
        {
            //make sure we keep one instance of this script
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            DontDestroyOnLoad(gameObject);
            isDebug = Debug.isDebugBuild;

            //set static reference
            Instance = this;
            asset = Instantiate(asset);

            //set up components
            GetComponent<DBManager>().Init();
            //let the ShopManager instantiate items with local data
            SceneManager.sceneLoaded += OnSceneWasLoaded;
            DBManager.dataUpdateEvent += RefreshShopItem;

            //disable auto initialization to e.g. to wait for an external login callback
            //and for querying store later after the external service has been initialized first
            if (autoInitialize) Initialize();
        }


        /// <summary>
        /// Initializes the IAP service with a platform-dependant billing connection.
        /// </summary>
        public async void Initialize()
        {
            //setup ran already
            if(controller != null)
            {
                //but not fully initialized yet
                if(!Initialized)
                {
                    if (isDebug) Debug.Log("IAPManager Initialize: Retrying.");

                    //retry from last step
                    ReadOnlyObservableCollection<Product> storeProducts = UnityIAPServices.StoreController().GetProducts();
                    if (storeProducts.Count == 0)
                        FetchProducts();
                    else
                        OnProductsFetched(storeProducts.ToList());
                }

                return;
            }

            //register custom stores on current platform selected
            new CustomPurchasingModule().Configure();

            //initialize Unity IAP
            controller = UnityIAPServices.StoreController();
            controller.SetStoreReconnectionRetryPolicyOnDisconnection(retryPolicy);
            controller.ProcessPendingOrdersOnPurchasesFetched(true);

            //subscribe to IAP callbacks
            controller.OnStoreConnected += OnInitialized;
            controller.OnStoreDisconnected += OnInitializeFailed;
            controller.OnProductsFetched += OnProductsFetched;
            controller.OnProductsFetchFailed += OnProductsFetchFailed;
            controller.OnPurchasesFetched += OnPurchasesFetched;
            controller.OnPurchasesFetchFailed += OnPurchasesFetchFailed;
            controller.OnPurchasePending += OnPurchasePending;
            controller.OnPurchaseFailed += OnPurchaseFailed;
            controller.OnPurchaseDeferred += OnPurchaseDeferred;
            controller.OnPurchaseConfirmed += OnPurchaseConfirmed;

            await controller.Connect();

            FetchProducts();
        }


        /// <summary>
        /// Reload ShopItem state visualizations on scene change.
        /// </summary>
        public void OnSceneWasLoaded(Scene scene, LoadSceneMode m)
        {
            shopItems.Clear();
        }


        //construct IAP product data with their App Store identifiers
        private void FetchProducts()
        {
            //configure product catalog
            string[] products = GetStoreIDs();
            if(products.Length == 0) return;

            List<ProductDefinition> productsRequest = new List<ProductDefinition>();
            string currentStore = UnityIAPServices.GetDefaultStore();

            for(int i = 0; i < products.Length; i++)
            {
                IAPProduct product = GetIAPProduct(products[i]);
                productsRequest.Add(new ProductDefinition(product.ID, product.GetStoreID(currentStore), product.type));
            }

            controller.FetchProducts(productsRequest, retryPolicy);
        }


        //StoreController.OnProductsFetched
        //Once we've received the product list, we overwrite the existing shop item values with this online data.
        private void OnProductsFetched(List<Product> products)
        {
            OverwriteWithFetch(products);

            controller.FetchPurchases();
        }


        //StoreController.OnPurchasesFetched: fired when Unity IAP initialization completes successfully.
        //StoreController.RestoreTransactions: called at a later point manually, this method fires again.
        private void OnPurchasesFetched(Orders orders)
        {
            //initialization callbacks fire here
            //Optionally: verify old purchases online
            if (!Initialized)
            { 
                isInitialized = true;

                //auto processing of pending orders in Unity IAP is already implemented for built-in stores
                //copy pasting BuiltinStores from 'Library/PackageCache/com.unity.purchasing/Runtime/Purchasing/Core/Purchasing/PurchaseService.cs'
                //we need it for custom stores too e.g. for Amazon or OculusStore
                string currentStore = UnityIAPServices.GetDefaultStore();
                IReadOnlyList<string> BuiltinStores = new[] { GooglePlay.Name, AppleAppStore.Name, MacAppStore.Name };
                bool ProcessPendingOrdersOnPurchasesFetched = !BuiltinStores.Contains(currentStore);
                if(ProcessPendingOrdersOnPurchasesFetched)
                {
                    foreach(PendingOrder pendingOrder in orders.PendingOrders)
                        OnPurchasePending(pendingOrder);
                }

                //auto restore of confirmed orders to behavior in Unity IAP 4.x
                //server validation will remove this afterwards if not in user inventory
                RestoreTransactions(orders, false);

                initializeSucceededEvent?.Invoke();
                receiptValidationInitializeEvent?.Invoke();
                return;
            }

            //StoreController.RestoreTransactions was called
            RestoreTransactions(orders, true);
        }


        private void OverwriteWithFetch(List<Product> products)
        {
            foreach(Product p in products)
            {
                IAPProduct product = GetIAPProduct(p.definition.id);
                if (product == null || !product.fetch) continue;

                //cache
                string title = p.metadata.localizedTitle;
                string descr = p.metadata.localizedDescription;
                string price = p.metadata.localizedPriceString;

                //always check for empty strings (product missing on store)
                if (!string.IsNullOrEmpty(title))
                {
                    //do not populate item with fake data from Unity IAP in test mode
                    if (title.StartsWith("FAKE", StringComparison.OrdinalIgnoreCase))
                        return;

                    //normally, the online item name received from the App Store
                    //has the application name attached, so we remove that here
                    int cap = title.IndexOf("(");
                    if (cap > 0)
                        title = title.Substring(0, cap - 1);

                    product.title = title;
                }

                if (!string.IsNullOrEmpty(descr))
                {
                    //replace line breaks with proper formatting
                    product.description = descr.Replace("\\n", "\n");
                }

                if (!string.IsNullOrEmpty(price))
                {
                    IAPExchangeObject exchange = product.priceList.Find(x => x.type == IAPExchangeObject.ExchangeType.RealMoney);
                    if (exchange != null) exchange.realPrice = price;
                }
            }
        }

			
        /// <summary>
        /// Purchase product based on its product identifier.
        /// Our delegates then fire the appropriate started/succeeded/fail event.
        /// </summary>
        public static void Purchase(string productID)
        {
            IAPProduct product = GetIAPProduct(productID);
            if(product == null)
			{
			    if (isDebug) Debug.LogError("Product " + productID + " not found in IAP Settings.");
				return;
            }

            //product is set to already owned, this should not happen
            if (product.type == ProductType.NonConsumable && DBManager.GetPurchase(productID) > 0)
            {
                OnPurchaseFailed(productID, "Product already owned.");
                return;
            }

            purchaseStartedEvent?.Invoke(productID);
            //distinguish between virtual and real products
            switch (product.IsVirtual())
            {
                case false:
                    if (!Initialized)
                    {
                        OnPurchaseFailed(productID, "Billing is not available, please try again later.");
                        if (isDebug) Debug.LogError("Unity IAP is not initialized correctly! Please check your billing settings.");
                        Instance.Initialize();
                        return;
                    }

                    Product p = controller.GetProducts().FirstOrDefault(product => product.definition.id == productID);

                    if (product == null)
                    {
                        OnPurchaseFailed(productID, "The Product you are trying to purchase does not exist.");
                        return;
                    }

                    IAPCategory category = Instance.asset.categoryList.Find(x => x.referenceID == product.category.referenceID);
                    controller.PurchaseProduct(p);
                    break;


                case true:
                    //check whether the player has enough funds locally
                    bool canPurchase = DBManager.CanPurchaseVirtual(product);
                    if (isDebug) Debug.Log("Purchasing virtual product " + productID + ", canPurchase: " + canPurchase);

                    if (!canPurchase)
                    {
                        OnPurchaseFailed(productID, "Not enough currency.");
                        return;
                    }

                    if (remotePurchaseVirtualEvent != null)
                    {
                        remotePurchaseVirtualEvent(product);
                        return;
                    }

                    //on success, substract the purchase funds locally
                    //non-consumables are saved to the database. After that fire the succeeded event
                    DBManager.PurchaseVirtual(product);
                    Instance.CompletePurchase(product.ID);
                    break;
            }
        }


        /// <summary>
        /// Tries to consume a product with default or amount specified.
        /// Fully consuming a product will ultimately remove it from the user's inventory. 
        /// </summary>
        public static void Consume(string productID, int amount = 1)
        {
            IAPProduct product = GetIAPProduct(productID);
            if (product == null)
            {
                if (isDebug) Debug.LogError("Product " + productID + " not found in IAP Settings.");
                return;
            }

            consumeStartedEvent?.Invoke(productID);
            if (amount > DBManager.GetPurchase(productID))
            {
                OnConsumeFailed("Not enough inventory to consume.");
                return;
            }

            if (remoteConsumePurchaseEvent != null)
            {
                remoteConsumePurchaseEvent(product, amount);
                return;
            }

            Instance.CompleteConsume(productID, amount);
        }


        //StoreController.OnPurchasePending. This will be called when a purchase completes.
        //Pending purchases are optionally processed with local and server-side validation
        private void OnPurchasePending(PendingOrder order)
        {
            Product storeProduct = GetFirstProductInOrder(order);

            //product not defined or nothing to validate without receipt
            if(storeProduct == null || GetIAPProduct(storeProduct.definition.id) == null || string.IsNullOrEmpty(order.Info.TransactionID))
            {
                string productID = storeProduct != null ? storeProduct.definition.id : "";
                purchaseFailedEvent?.Invoke("Purchase " + productID + " is unavailable or invalid." );
                controller.ConfirmPurchase(order);
                return;
            }

            #if UNITY_EDITOR
                if (isDebug && UnityIAPServices.GetDefaultStore() == "fake")
                    Debug.Log("In-App Purchasing in the Unity Editor is fake, deploy to a mobile device for real testing.");
            #endif

            //also done when auto-restoring transactions on first app launch
            if(receiptValidationPurchaseEvent != null)
            {
                receiptValidationPurchaseEvent(order);
                return;
            }

            //indicate we have handled this purchase, we will not be informed of it again
            CompletePurchase(storeProduct.definition.id);               
            controller.ConfirmPurchase(order);
        }


        //StoreController.OnPurchaseConfirmed
        private void OnPurchaseConfirmed(Order order)
        {
            switch (order)
            {
                //in case Unity IAP failed the transaction right away, display it to the user
                case FailedOrder failedOrder:
                    OnPurchaseFailed(failedOrder);
                    break;

                //as per best-practice, the transaction was processed in OnPurchasePending, nothing else needs to be done
                case ConfirmedOrder confirmedOrder:
                    if (isDebug) Debug.Log("IAPManager OnPurchaseConfirmed. Product: " + GetFirstProductInOrder(confirmedOrder).definition.id);
                    break;
            }
        }

		
        /// <summary>
        /// Sets a product to purchased after successful validation (or without).
        /// This alters the database entry for non-consumable or products with usage as well.
        /// </summary>
        public void CompletePurchase(string productID, bool withEvent = true)
        {
            IAPProduct product = GetIAPProduct(productID);
            if (product == null) return;

            foreach (IAPExchangeObject exchange in product.rewardList)
            {
                switch(exchange.type)
                {
                    case IAPExchangeObject.ExchangeType.VirtualCurrency:
                        DBManager.AddCurrency(exchange.currency.ID, exchange.amount);
                        break;

                    case IAPExchangeObject.ExchangeType.VirtualProduct:

                        switch(exchange.product.type)
                        {
                            case ProductType.Consumable:
                                DBManager.AddPurchase(exchange.product.ID, exchange.amount);
                                break;

                            //a non-consumable product should definitely be granted to the player
                            case ProductType.NonConsumable:
                            case ProductType.Subscription:
                                DBManager.AddPurchase(exchange.product.ID, 1);
                                break;
                        }
                        break;
                }               
            }

            if (withEvent)
            {
                purchaseSucceededEvent?.Invoke(product.ID);
            }
        }


        /// <summary>
        /// Consumes a purchase after successful validation (or without).
        /// This alters the database entry for this specific product.
        /// </summary>
        public void CompleteConsume(string productID, int amount)
        {
            DBManager.ConsumePurchase(productID, amount);
            consumeSucceededEvent?.Invoke(productID);
        }


        /// <summary>
        /// Restore already purchased user's transactions for non consumable IAPs.
        /// If receipt validation is used, the restored receipts are also getting validated again.
        /// </summary>
        public static void RestoreTransactions()
        {
            if (!Initialized)
            {
                OnPurchaseFailed("Restore", "Billing is not available, please try again later.");
                Instance.Initialize();
                return;
            }

            restoreTransactionsStartedEvent?.Invoke();

            //Define natively supported stores for restore
            List<string> supportedStores = new List<string>()
            {
                IAPPlatform.GooglePlay.ToString(),
                IAPPlatform.AppleAppStore.ToString(),
                IAPPlatform.MacAppStore.ToString()
            };

            //initiate native workflow
            if(supportedStores.Contains(UnityIAPServices.GetDefaultStore()))
                controller.RestoreTransactions(Instance.OnTransactionsRestored);
            //otherwise fetch purchases again
            else
            {
                controller.FetchPurchases();
                Instance.OnTransactionsRestored(true, string.Empty);
            }
        }


        //StoreController.RestoreTransactions > StoreController.OnPurchasesFetched > this
        private async void RestoreTransactions(Orders orders, bool withValidation = false)
        {
            foreach (Order order in orders.ConfirmedOrders)
            {
                Product storeProduct = GetFirstProductInOrder(order);

                if (storeProduct.definition.type == ProductType.Consumable || string.IsNullOrEmpty(order.Info.TransactionID))
                    continue;

                if(withValidation && receiptValidationPurchaseEvent != null)
                {
                    Delegate[] subscribers = receiptValidationPurchaseEvent.GetInvocationList();
                    if(subscribers[0].Method.DeclaringType != typeof(ReceiptValidatorClient))
                        await Task.Delay(UnityEngine.Random.Range(2000, 5000));

                    receiptValidationPurchaseEvent(order);
                }
                else
                    CompletePurchase(storeProduct.definition.id, false);
            }
        }


        //callback from RestoreTransactions
        private void OnTransactionsRestored(bool success, string error)
        {
			if (isDebug && !success)
                Debug.LogWarning("IAPManager OnTransactionsRestored. Error: " + error);

            //the IAPListener will try to present a transaction restore message
            //if you are using server validation, it is likely that the restore requests take longer
            //than the transactions loop so instead a generic product restored message will be shown
            restoreTransactionsFinishedEvent?.Invoke(success);
        }


        //StoreController.OnStoreConnected
        private void OnInitialized()
        {
            if (isDebug)
                Debug.Log("IAPManager OnInitialized established connection to native billing library.");

            //not being used
        }


        //StoreController.OnStoreDisconnected
        private void OnInitializeFailed(StoreConnectionFailureDescription error)
        {
            if (isDebug) Debug.LogError("IAPManager OnInitializeFailed. Error: " + error.Message);

            initializeFailedEvent?.Invoke(error.Message);
        }


        //StoreController.OnProductsFetchFailed
        private void OnProductsFetchFailed(ProductFetchFailed error)
        {
            if(UnityIAPServices.StoreController().GetProducts().Count > 0)
            {
                if (isDebug)
                {
                    string failedIDs = string.Join(", ", error.FailedFetchProducts.Select(x => x.id));
                    Debug.Log("IAPManager OnProductsFetchFailed: " + failedIDs);
                    return;
                }
            }

            if (isDebug) Debug.LogError("IAPManager OnProductsFetchFailed. Error: " + error);

            initializeFailedEvent?.Invoke(error.FailureReason);
        }


        //StoreController.OnPurchasesFetchFailed
        private void OnPurchasesFetchFailed(PurchasesFetchFailureDescription error)
        {
            if (isDebug) Debug.LogError("IAPManager OnPurchasesFetchFailed. Error: " + error.Message);

            initializeFailedEvent?.Invoke(error.Message);
        }


        //StoreController.OnPurchaseFailed
        private void OnPurchaseFailed(FailedOrder order)
        {
            if (isDebug) Debug.Log("IAPManager OnPurchaseFailed. Product: " + GetFirstProductInOrder(order).definition.id + ", Error: " + order.FailureReason);

            purchaseFailedEvent?.Invoke(order.FailureReason + "\n" + order.Details);
        }


        //StoreController.OnPurchaseDeferred
        private void OnPurchaseDeferred(DeferredOrder order)
        {
            Product product = GetFirstProductInOrder(order);
            if (isDebug) Debug.Log("IAPManager OnPurchaseDeferred. Product: " + product.definition.id);

            #if UNITY_ANDROID || UNITY_IOS
                purchaseFailedEvent?.Invoke("Purchase of product " + product.metadata.localizedTitle + " is pending.");
            #endif
        }

	
		/// <summary>
        /// Override called manually in case billing has not initialized yet.
        /// E.g. due to network connection issues or not being logged in on mobile device.
		/// </summary>
        public static void OnPurchaseFailed(string productId, string error)
        {
            if (isDebug) Debug.Log("IAPManager OnPurchaseFailed. Product: " + productId + ", Error: " + error);

            purchaseFailedEvent?.Invoke(error);
        }


		/// <summary>
        /// Called when a consumption attempt fails.
        /// Used internally or from external stores, e.g. PlayFab.
		/// </summary>
        public static void OnConsumeFailed(string error)
        {
            if (isDebug) Debug.Log("IAPManager OnConsumeFailed. Error: " + error);
            
            consumeFailedEvent?.Invoke(error);
        }


        /// <summary>
        /// Refreshes the visual representation of all shop items based on previous actions
        /// or user interaction, meaning we set them to 'purchased' or 'selected' in the GUI.
        /// You can call this manually in case PlayerData (unlock requirements) have changed.
        /// Can only refresh shop items which were active and added to the shopItems dic before.
        /// </summary>
        public void RefreshShopItemAll()
        {
            foreach (string key in shopItems.Keys)
                RefreshShopItem(key);
        }


        /// <summary>
        /// Refreshes the visual representation of a specific group from the Project Settings.
        /// Same as RefreshShopItemAll(), but only for items within this specific group name.
        /// You can call this manually i.e. when manually changing product selections in a group.
        /// Can only refresh shop items which were active and added to the shopItems dic before.
        /// </summary>
        public void RefreshShopItemGroup(string groupName)
        {
            IAPCategory category = asset.categoryList.SingleOrDefault(x => x.ID == groupName);

            if (category == null)
            {
                if (isDebug) Debug.LogWarning("IAPManager RefreshGroup: groupName not found.");
                return;
            }

            List<IAPProduct> products = asset.productList.FindAll(x => x.category == category);

            for (int i = 0; i < products.Count; i++)
                RefreshShopItem(products[i].ID);
        }


        /// <summary>
        /// Refreshes the visual representation of a specific shop item. This is called automatically
        /// because of subscribing to the DBManager update event. It also means saving performance due
        /// to not refreshing all items on each database change every time.
        /// Can only refresh shop items which were active and added to the shopItems dic before.
        /// </summary>
        public void RefreshShopItem(string productID)
        {
            if (shopItems.ContainsKey(productID))
                shopItems[productID].Refresh();
        }


        /// <summary>
        /// Utility method for reading first product in Order > Cart > Items.
        /// </summary>
        public static Product GetFirstProductInOrder(Order order)
        {
            return order.CartOrdered.Items().First()?.Product;
        }


        /// <summary>
        /// Returns a list of all upgrade IDs associated to a product.
        /// </summary>
        public static List<string> GetAllUpgrades(string productId)
        {
            List<string> list = new List<string>();
            IAPProduct product = GetIAPProduct(productId);

            if (product == null)
            {
                if (isDebug)
                    Debug.LogError("Product " + productId + " not found in IAP Settings. Make sure "
                                   + "to remove your app from the device before deploying it again!");
            }
            else
            {
                while (product != null && product.nextUpgrade != null)
                {
                    list.Add(product.nextUpgrade.ID);
                    product = product.nextUpgrade;
                }
            }
           
            return list;
        }


        /// <summary>
        /// Returns the last purchased upgrade ID of a product,
        /// or the main product itself if it hasn't been purchased yet.
        /// </summary>
        public static string GetCurrentUpgrade(string productId)
        {
            if (DBManager.GetPurchase(productId) == 0)
                return productId;

            string id = productId;
            List<string> upgrades = GetAllUpgrades(productId);

            for (int i = upgrades.Count - 1; i >= 0; i--)
            {
                if (DBManager.GetPurchase(upgrades[i]) > 0)
                {
                    id = upgrades[i];
                    break;
                }
            }

            return id;
        }


        /// <summary>
        /// Returns the next unpurchased upgrade ID of a product.
        /// </summary>
        public static string GetNextUpgrade(string productId)
        {
            string currentID = GetCurrentUpgrade(productId);
            IAPProduct product = GetIAPProduct(currentID);

            if (DBManager.GetPurchase(currentID) == 0 || product == null || product.nextUpgrade == null) return currentID;
            else return product.nextUpgrade.ID;
        }


        /// <summary>
        /// Returns the global identifier of an in-app product, specified in the IAP Project Settings.
        /// </summary>
        public static string GetProductGlobalIdentifier(string storeId)
        {
            //confirmed GetProductById also works with global identifier too
            if(controller != null && controller.GetProducts().Count > 0)
            {
                Product p = controller.GetProductById(storeId);

                if (p != null)
                    return p.definition.id;
            }

            //fallback in case Unity IAP has not been initialized yet
            foreach (IAPProduct product in Instance.asset.productList)
            {
                if (product.storeIDs.Exists(x => x.active && x.ID == storeId))
                    return product.ID;
            }

            return storeId;
        }
      
        
        /// <summary>
        /// Returns the list of products used when initializing Unity IAP.
        /// </summary>
        public static ProductDefinition[] GetProductDefinitions()
        {
            List<ProductDefinition> definitions = new List<ProductDefinition>(); 
            foreach (Product product in controller.GetProducts())
                definitions.Add(product.definition);

            return definitions.ToArray();
        }


        /// <summary>
        /// Returns whether there are any deferred/pending purchases.
        /// Supported on App Stores offering this functionality, like Apple App Store and Google Play.
        /// </summary>
        public static bool HasPendingPurchases()
        {
            if (!Initialized) return false;

            ReadOnlyObservableCollection<Order> orders = controller.GetPurchases();
            foreach(Order order in orders)
            {
                if(order is DeferredOrder)
                    return true;
            }

            return false;
        }


        /// <summary>
        /// Returns whether the product either has currency or product rewards except from itself.
        /// </summary>
        public static bool HasProductRewards(string productId)
        {
            IAPProduct product = GetIAPProduct(productId);
            if (product == null) return false;

            if (product.rewardList.Exists(x => x.currency != null) || product.rewardList.Exists(x => x.product != null && x.product.ID != productId))
                return true;
            else
                return false;
        }


        /// <summary>
        /// Returns the product's reward list which consists of currency/product ID and amount.
        /// </summary>
        public static List<KeyValuePairStringInt> GetProductRewards(string productId)
        {
            List<KeyValuePairStringInt> items = new List<KeyValuePairStringInt>();
            IAPProduct product = GetIAPProduct(productId);
            if (product == null) return items;

            foreach (IAPExchangeObject obj in product.rewardList)
            {
                switch (obj.type)
                {
                    case IAPExchangeObject.ExchangeType.VirtualCurrency:
                        items.Add(new KeyValuePairStringInt() { Key = obj.currency.ID, Value = obj.amount });
                        break;
                    case IAPExchangeObject.ExchangeType.VirtualProduct:
                        //ignore self-references on non-consumables
                        if (obj.product.ID == product.ID) continue;
                        items.Add(new KeyValuePairStringInt() { Key = obj.product.ID, Value = obj.amount });
                        break;
                }
            }

            return items;
        }

        /// <summary>
        /// Returns a string array of all IAP IDs.
        /// </summary>
        public static string[] GetAllIDs()
        {
            return Instance.asset.productList.Select(product => product.ID).ToArray();
        }
		
		
		/// <summary>
        /// Returns a string array of all real money IAP IDs only.
        /// </summary>
        public static string[] GetRealMoneyIDs()
        {
            return Instance.asset.productList.Where(x => x.priceList.Exists(z => z.type == IAPExchangeObject.ExchangeType.RealMoney)).Select(x => x.ID).ToArray();
        }


        /// <summary>
        /// Returns a string array of all real money IAP IDs for the current store only.
        /// This is still the global ID, with inactive products excluded.
        /// </summary>
        public static string[] GetStoreIDs()
        {
            List<string> IDs = new List<string>();

            if (Instance.asset.productList.Count == 0)
            {
                Debug.LogWarning("No products found. Are you sure you've added them to the Project Settings?");
                return null;
            }

            //loop over all products
            for (int i = 0; i < Instance.asset.productList.Count; i++)
            {
                IAPProduct product = Instance.asset.productList[i];

                if (string.IsNullOrEmpty(product.ID))
                {
                    Debug.LogError("Found IAP Object in IAP Settings without an identifier. Skipping product.");
                    continue;
                }

                if (product.IsVirtual())
                    continue;

                //check overrides
                IAPCategory category = Instance.asset.categoryList.Find(x => x.referenceID == product.category.referenceID);
                if (category.storeIDs.Find(x => x.store == UnityIAPServices.GetDefaultStore() && !x.active) != null) continue;
                else if (product.storeIDs.Find(x => x.store == UnityIAPServices.GetDefaultStore() && !x.active) != null) continue;

                IDs.Add(product.ID);
            }

            return IDs.ToArray();
        }


        /// <summary>
        /// Returns the IAPProduct with a specific global ID.
        /// </summary>
        public static IAPProduct GetIAPProduct(string productID)
        {
            if (!Instance || string.IsNullOrEmpty(productID)) return null;

            IAPProduct product = Instance.asset.productList.SingleOrDefault(x => x.ID == productID);

            if (product == null)
            {
                //fallback in case we have passed in a storeID
                string globalID = GetProductGlobalIdentifier(productID);
                if(productID != globalID)
                    product = Instance.asset.productList.SingleOrDefault(x => x.ID == globalID);
            }

            return product;
        }


        /// <summary>
        /// Returns instantiated IAPItem shop item reference.
        /// Null if there is none in the current scene.
        /// </summary>
        public static ShopItem2D GetShopItem(string productID)
        {
            if (shopItems.ContainsKey(productID))
                return shopItems[productID];
            else
                return null;
        }


        /// <summary>
        /// Returns the group name of a specific product ID.
        /// <summary>
        public static string GetProductCategoryName(string productID)
        {
            IAPProduct product = GetIAPProduct(productID);
            return (product != null && product.category != null) ? product.category.ID : null;
        }


        //unregister callbacks
        void OnDestroy()
        {
            if (Instance != this)
                return;

            SceneManager.sceneLoaded -= OnSceneWasLoaded;
            DBManager.dataUpdateEvent -= RefreshShopItem;
        }
    }
}