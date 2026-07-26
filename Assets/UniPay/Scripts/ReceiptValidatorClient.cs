/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

using System;
using System.Linq;
using UnityEngine;

namespace UniPay
{
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Security;

    /// <summary>
    /// IAP receipt validation on the client (local, on the device) using Unity IAPs validator class.
    /// This is a lot less secure than server-validation, but better than nothing.
    /// </summary>
    public class ReceiptValidatorClient : ReceiptValidator
    {
        CrossPlatformValidator localValidator;


        #if !RECEIPT_VALIDATION
        void Start()
        {
            #if UNITY_ANDROID
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
            if (IsLocalValidationSupported())
                return true;

            return false;
        }


        private void OnValidationInitialize()
        {
            try
            {
                localValidator = new CrossPlatformValidator(GooglePlayTangle.Data(), Application.identifier);
            }
            catch (NotImplementedException) { }

            //re-validation of all owned products on launch
            if(localValidator != null)
                Validate(null as Order);
        }


        /// <summary>
        /// Overriding the base method for constructing Unity IAP's CrossPlatformValidator and passing in purchase receipts.
        /// The validation result will either grant the item (success) or remove it from the inventory if granted already (failed).
        /// </summary>
        public override void Validate(Order order)
        {
            Order[] orders = new Order[]{ order };
            bool withEvent = order != null;

            if (order == null)
                orders = IAPManager.controller.GetPurchases().ToArray();

            for (int i = 0; i < orders.Length; i++)
            {
                Product storeProduct = IAPManager.GetFirstProductInOrder(orders[i]);
                UnifiedReceipt receiptData = null;

                //we found a receipt for this product on the device, initiate client receipt validation
                //if the purchase is pending validation will throw an exception and retry on the next app launch
                try
                {
                    // On Google Play, result will have a single product Id.
                    receiptData = JsonUtility.FromJson<UnifiedReceipt>(orders[i].Info.Receipt);
                    localValidator.Validate(orders[i].Info.Receipt);

                    if (IAPManager.isDebug) Debug.Log("Client Receipt Validation passed for: '" + storeProduct.definition.id + "'.");
                    IAPManager.Instance.CompletePurchase(storeProduct.definition.id, withEvent);
                }
                catch (Exception ex)
                {                   
                    #if UNITY_EDITOR
                    //complete fake store = test mode purchases successfully anyway, but only in the editor
                    if(ex is NullReferenceException && receiptData != null && receiptData.Store == "fake")
                    {
                        if (IAPManager.isDebug) Debug.Log("Fake Mode. Client Receipt Validation passed for: '" + storeProduct.definition.id + "'.");
                        IAPManager.Instance.CompletePurchase(storeProduct.definition.id, withEvent);
                        continue;
                    }
                    #endif

                    if (IAPManager.isDebug) Debug.Log("Client Receipt Validation failed for: '" + storeProduct.definition.id + "'. Exception: " + ex + ", " + ex.Message);
                    if (order != null) IAPManager.OnPurchaseFailed(storeProduct.definition.id, "Could not verify purchase: " + PurchaseFailureReason.ValidationFailure.ToString());

                    if (ex is NullReferenceException || ex is IAPSecurityException)
                    {
                        RemovePurchase(storeProduct.definition.id);
                    }
                };

                if (orders[i] is PendingOrder)
                    IAPManager.controller.ConfirmPurchase(orders[i] as PendingOrder);
            }

            void RemovePurchase(string productID)
            {
                if (!DBManager.IsPurchased(productID))
                    return;

                ShopItem2D item = null;
                if (IAPManager.Instance != null)
                    item = IAPManager.GetShopItem(productID);
                if (item != null)
                    item.Purchased(false);
                    
                DBManager.ConsumePurchase(productID);
            }
        }


        bool IsLocalValidationSupported()
        {
            //The CrossPlatform validator only supports the GooglePlay Store.
            if (Application.platform == RuntimePlatform.Android && UnityIAPServices.GetDefaultStore() == GooglePlay.Name)
                return true;

            return false;
        }
        #endif
    }
}