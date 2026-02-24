using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.UI;
using CI.WSANative.Store;
using CI.WSANative.Common;
using System.Linq;
using UnityEngine.Purchasing.Security;


public class IAPExample : MonoBehaviour, IStoreListener
{

    //#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX

    private static IStoreController m_StoreController;          // The Unity Purchasing system.
    private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

    // Product identifiers for all products capable of being purchased: 
    // "convenience" general identifiers for use with Purchasing, and their store-specific identifier 
    // counterparts for use with and outside of Unity Purchasing. Define store-specific identifiers 
    // also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

    // General product identifiers for the consumable, non-consumable, and subscription products.
    // Use these handles in the code to reference which product to purchase. Also use these values f
    // when defining the Product Identifiers on the store. Except, for illustration purposes, the 
    // kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
    // specific mapping to Unity Purchasing's AddProduct, below.

    //  public static string kProductIDSubscription = "getsugaku1900";
    public static string kProductIDSubscription = "getsugaku1900";
    public static string kProductIDSubscription2 = "nengaku_jpy14900";
    public GameObject PurchaseRequesBord;


    /// <summary>
    /// This is important
    /// </summary>
    // Apple App Store-specific product identifier for the subscription product.
    //private static string kProductNameAppleSubscription = "com.Voisapo.Voisapofortrainer.getsugaku_jpy690";
    // private static string kProductNameAppleSubscription = "getsugaku_jpy690";

    // Google Play Store-specific product identifier subscription product.
    // private static string kProductNameGooglePlaySubscription = "com.Voisapo.Voisapofortrainer";
    //    private static string kProductNameWindowsStoreSubscription = "cuom.Voisapo.Voisapofortrainer";

    //以前3.3で購入時のパスワード入力をオンにしたら購入画面がでてくるようになった。
    //initializepurchaseingが有効になっているとレシートを既に保有しているときにする処理ができなくなった。
    //しばらくしたらID発行ができなくなった。Unityのバージョンを2022に変更したら繋がるようになった。
    //しかしパッケージがWindoesStoreで不合格になるようになった。また課金処理もできない。
    //再度3.3に戻したらネットにつながるようになった。
    //CapablitiesのInternecClientServerオフにしてビルドしたらネットに繋がらなくなった。オンにしたら繋がった。(これが原因。これは一度UWPを消さないとそうはならない)
    //しばらくしたら課金処理にも成功するようになった。
    //storeにアップしたら安定して購入処理できるのでは？
    //処理できないときはレシートがないことになってた。そもそもそのときはInternecClientServerがオフでネットワークに繋がっていなかった可能性。

    //minimumplatformはビルド 14393以上の必要あり

    //可能性:InternecClientServerがオンかつEditoreが3.3なら課金できるんじゃないか？

    //ストアにアップしたら課金できるんじゃないか？やみくもにいじらずに条件をひとつづつメモしながら変えていかないと再現性が保障できない。
    //同条件でもUnity上でビルドしたら課金処理ができなくなった。やはりストアから落とさないと処理が発動しないらしい

    //[SerializeField] GameObject PurchaseBlockBord_CreateScale;
    //[SerializeField] GameObject PurchaseBlockBord_AutoScaleShift;


    // Assign this variable to the Store ID of your subscription add-on.



    void Start()
    {
        // If we haven't set up the Unity Purchasing reference
        //    PurchaseRequestBord = GameObject.Find("PurchaseRequestBord");

        /*
             if (PlayerPrefs.GetInt("subsc", 0) == 1)
             {
                 PurchaseBlockBordKill();
             }
             else
             {
                 PurchaseBlockBordActive();
             }
     */


#if UNITY_STANDALONE_OSX
        if (m_StoreController == null)
        {
            // Begin to configure our connection to Purchasing
            InitializePurchasing();
        }
#endif




#if UNITY_WSA
        if (WSANativeStore.GetAppLicense().AddOnLicenses != null)
        {
            foreach (var addOnLicense in WSANativeStore.GetAppLicense().AddOnLicenses)
            {

                var license = addOnLicense.Value;
                if (license.IsActive)
                {



                    PlayerPrefs.SetInt("subsc", 1);
                    PlayerPrefs.Save();

                    //  PurchaseRequestKill();
                    //PurchaseBlockBordKill();

                    break;

                }


                else
                {
                    PlayerPrefs.SetInt("subsc", 0);
                    PlayerPrefs.Save();
                }
            }
        }
#endif


    }


    public void InitializePurchasing()
    {



        // If we have already connected to Purchasing ...
        if (IsInitialized())
        {
            return;
        }

        var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

        builder.AddProduct(kProductIDSubscription, ProductType.Subscription);
        builder.AddProduct(kProductIDSubscription2, ProductType.Subscription);
        builder.Configure<IMicrosoftConfiguration>().useMockBillingSystem = true;


        UnityPurchasing.Initialize(this, builder);



    }




    public bool IsInitialized()
    {
        return m_StoreController != null && m_StoreExtensionProvider != null;
    }


    public void BuyProductID(string productId)
    {

#if UNITY_STANDALONE_OSX
    
        if (IsInitialized())
        {

            Product product = m_StoreController.products.WithID(productId);

            if (product != null && product.availableToPurchase)
            {
                m_StoreController.InitiatePurchase(product);
            }
            else
            {
                // ... report the product look-up failure situation  
                // Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                // statustext.text = "BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase";
            }
        }
        // Otherwise ...
        else
        {
            // ... report the fact Purchasing has not succeeded initializing yet. Consider waiting longer or 
            // retrying initiailization.
            //  Debug.Log("BuyProductID FAIL. Not initialized.");
            // statustext.text = "BuyProductID FAIL. Not initialized.";
        }
        
#endif
    }





    public void RestorePurchases()
    {
        // If Purchasing has not yet been set up ...
        if (!IsInitialized())
        {
            // ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
            //   Debug.Log("RestorePurchases FAIL. Not initialized.");
            return;
        }

        // If we are running on an Apple device ... 
        if (Application.platform == RuntimePlatform.IPhonePlayer ||
            Application.platform == RuntimePlatform.OSXPlayer)
        {
            // ... begin restoring purchases
            Debug.Log("RestorePurchases started ...");

            var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
            apple.RestoreTransactions((result) =>
            {
                //    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
            });
        }
        // Otherwise ...
        else
        {
            // We are not running on an Apple device. No work is necessary to restore purchases.
            //  Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
        }
    }


    //  
    // --- IStoreListener
    //

    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        // Purchasing has succeeded initializing. Collect our Purchasing references.
        // Debug.Log("OnInitialized: PASS");
        // statustext.text = "OnInitialized: PASS";


        // Overall Purchasing system, configured with products for this application.
        m_StoreController = controller;
        // Store specific subsystem, for accessing device-specific store features.
        m_StoreExtensionProvider = extensions;


        foreach (Product item in controller.products.all)
        {
            if (item.receipt != null)//レシートを所持していれば処理
            {
                if (item.definition.type == ProductType.Subscription)//レシートが定期購入のものであれば処理
                {

                    PlayerPrefs.SetInt("subsc", 1);
                    PlayerPrefs.Save();

                    //  PurchaseRequestKill();
                    //  PurchaseBlockBordKill();


                    break;

                }

                else
                {
                    PlayerPrefs.SetInt("subsc", 0);
                    PlayerPrefs.Save();
                }
            }
            else
            {

                PlayerPrefs.SetInt("subsc", 0);
                PlayerPrefs.Save();

            }

        }



    }


    public void OnInitializeFailed(InitializationFailureReason error)
    {
        // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
        //Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
        // statustext.text = "OnInitializeFailed InitializationFailureReason:" + error;
    }

    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs e)
    {
        bool validPurchase = true; // R.V. のないプラットフォームに有効です

        // Unity IAP の検証ロジックはこれらのプラットフォームにのみ含まれます。
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX
        // エディターの難読化ウィンドウで準備した機密を持つ
        // バリデーターを準備します。
        var validator = new CrossPlatformValidator(GooglePlayTangle.Data(),
            AppleTangle.Data(), Application.identifier);

        try
        {
            // Google Play で、result は 1 つの product ID を取得します
            // Apple stores で、receipts には複数のプロダクトが含まれます
            var result = validator.Validate(e.purchasedProduct.receipt);
            // 情報提供の目的で、ここにレシートをリストします
            /*
                foreach (IPurchaseReceipt productReceipt in result)
                {
                    Debug.Log(productReceipt.productID);
                    Debug.Log(productReceipt.purchaseDate);
                    Debug.Log(productReceipt.transactionID);
                }
                */
        }
        catch (IAPSecurityException)
        {
            validPurchase = false;
        }
#endif

        if (validPurchase)
        {
            PlayerPrefs.SetInt("subsc", 1);
            PlayerPrefs.Save();

            PurchaseRequestKill();
            //   PurchaseBlockBordKill();


        }

        return PurchaseProcessingResult.Complete;
    }


    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
        // this reason with the user to guide their troubleshooting actions.
        //   Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
        //  statustext.text = string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason);
    }


    /// <summary>
    /// To get price string
    /// </summary>
    /// <param name="id">"NO_ADS"</param>
    /// <returns>empty if nothing</returns>
    public string GetProducePriceFromStore(string id)
    {
        if (m_StoreController != null && m_StoreController.products != null)
        {
            return m_StoreController.products.WithID(id).metadata.localizedPriceString;
        }
        else
        {
            return "";
        }
    }

    public void ChutorialBtn()
    {
        Application.OpenURL("https://www.youtube.com/channel/UCmZyG9fzDFzvIiQ--YgOU_w");//""の中には開きたいWebページのURLを入力します
    }
    public void PrivacyPolicy()
    {
        Application.OpenURL("https://voisapo.com/policy");
    }
    public void TermsOfUse()
    {
        Application.OpenURL("https://voisapo.com/policy/#section-7");
    }



    public void PurchaseProductWUP(string productId)
    {
#if UNITY_WSA
        WSANativeStore.GetAddOns(products =>
        {
            if (products.Products != null && products.Products.Count > 0)
            {
                //WSANativeStore.RequestPurchase(products.Products.Keys.First(), result =>
                WSANativeStore.RequestPurchase(productId, result =>
               {

                   if (result.Status == WSAStorePurchaseStatus.Succeeded)
                   {
                       PlayerPrefs.SetInt("subsc", 1);
                       PlayerPrefs.Save();

                       PurchaseRequestKill();
                       //PurchaseBlockBordKill();
                   }
                   else
                   {

                   }
               });
            }
        });
#endif
    }






    void PurchaseRequestKill()
    {


        if (PurchaseRequesBord.activeSelf)
        {
            PurchaseRequesBord.SetActive(false);
        }

    }

    public void OnInitializeFailed(InitializationFailureReason error, string message)
    {
     
    }
}

