/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

namespace UniPay
{
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;

    /// <summary>
    /// Custom Unity IAP purchasing module for overwriting default store subsystems.
    /// </summary>
    public class CustomPurchasingModule
    {  
        public void Configure()
		{
            #pragma warning disable 0219
            Store storeInstance = null;
            IStoreWrapper storeWrapper = null;
            #pragma warning restore 0219

            //Native
            #if STEAM_IAP
                storeInstance = new SteamStore();
                storeWrapper = new SteamStoreWrapper(storeInstance as SteamStore);
            #endif

            #if PAYPAL_IAP
                storeInstance = new PayPalStore(IAPManager.Instance);
                storeWrapper = new PayPalStoreWrapper(storeInstance as PayPalStore);
            #endif

            #if AMAZON_IAP
                storeInstance = new AmazonStore();
                storeWrapper = new AmazonStoreWrapper(storeInstance as AmazonStore);
            #endif
            
            //VR
            #if OCULUS_IAP
                storeInstance = new OculusStore();
                storeWrapper = new OculusStoreWrapper(storeInstance as OculusStore);
            #endif

            if(storeWrapper != null)
            {
                UnityIAPServices.AddNewCustomStore(storeWrapper);
                UnityIAPServices.SetStoreAsDefault(storeWrapper.name);
            }
        }
    }
}