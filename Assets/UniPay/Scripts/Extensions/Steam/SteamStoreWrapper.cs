/*  This file is part of the "UniPay" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from an official reseller (Unity Asset Store).
 *  You shall not license, sublicense, sell, resell, transfer, assign, distribute or otherwise make available to any third party the Service or the Content. */

namespace UniPay
{
    #if STEAM_IAP
    using UnityEngine.Purchasing;
    using UnityEngine.Purchasing.Extension;

    /// <summary>
    /// Wrapper defining instance and store name for Steam.
    /// </summary>
    class SteamStoreWrapper : IStoreWrapper
    {
        public string name { get; }
        private string _name => IAPPlatform.SteamStore.ToString();

        public Store instance => _instance;
        private readonly SteamStore _instance;
        

        public SteamStoreWrapper(SteamStore instance)
        {
            name = _name;
            _instance = instance;
        }


        public ConnectionState GetStoreConnectionState()
        {
            return _instance.GetStoreConnectionState();
        }
    }
    #endif
}