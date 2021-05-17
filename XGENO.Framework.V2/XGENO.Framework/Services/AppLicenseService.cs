using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Windows.Services.Store;
using Windows.Networking.Connectivity;
using Windows.Storage;
using XGENO.Framework.Common;

namespace XGENO.Framework.Services
{
    public enum ProductLicenseType
    {
        AppLicense,
        ProductLicense
    }

    public class AppLicenseService
    {

        private StoreContext _storeContext;
        private StoreAppLicense _storeLicense;
        private ProductLicenseType _productLicenseType;

        public string Key { get; set; }

        private string SKUKey { get; set; }

        public bool IsExpired
        {
            get
            {
                return Expires < DateTime.Now;
            }
        }

        public bool IsPurchased
        {
            get
            {
                try
                {
                    if (_productLicenseType == ProductLicenseType.ProductLicense)
                        return _storeLicense.AddOnLicenses[this.SKUKey].IsActive;
                    else
                        return (_storeLicense.IsActive && !_storeLicense.IsTrial);
                }
                catch (Exception e)
                {
                }

                return false;
            }
        }

        public bool IsTrial
        {
            get
            {
                return !IsPurchased;
            }
        }

        public DateTime Expires
        {
            get
            {
                try
                {
                    if (_productLicenseType == ProductLicenseType.ProductLicense)
                        return _storeLicense.AddOnLicenses[this.SKUKey].ExpirationDate.DateTime;
                    else
                        return _storeLicense.ExpirationDate.DateTime;

                }
                catch (Exception e)
                {
                }

                return DateTime.MinValue;
            }
        }


        public AppLicenseService()
        {
            Key = "";
            _productLicenseType = ProductLicenseType.AppLicense;
        }

        public AppLicenseService(string featureKey)
        {
            Key = featureKey;
            _productLicenseType = ProductLicenseType.ProductLicense;
        }


        public async Task Initialize()
        {
            try
            {
                _storeContext = StoreContext.GetDefault();
                _storeContext.OfflineLicensesChanged += OfflineLicensesChanged;
            }
            catch (Exception e)
            {
            }

            return;
        }

        private void OfflineLicensesChanged(StoreContext sender, object args)
        {
            var task = WindowWrapper.Default().Dispatcher.DispatchAsync(async () =>
            {
                await GetLicenseState();
            });

        }

        private async Task GetLicenseState()
        {
            _storeLicense = await _storeContext.GetAppLicenseAsync();

            if(_productLicenseType == ProductLicenseType.ProductLicense)
            {
                var products = await _storeContext.GetAssociatedStoreProductsAsync(new List<string>() { "Durable" });

                foreach(KeyValuePair<string, StoreProduct> item in products.Products)
                {
                    if(item.Key == Key)
                    {
                        SKUKey = item.Value.Skus[0].StoreId;
                    }
                }
            }
        }
          

        public async Task<bool> Purchase()
        {
            var dialogService = new DialogService();

            await GetLicenseState();


            if (IsPurchased)
            {
                await dialogService.ShowAsync("Thank you very much for your puchase and support. The full version has been unlocked. Please restart the application for the changes to take affect.", "PURCHASE SUCCESS");

                return true;
            }

            try
            {
                if (_productLicenseType == ProductLicenseType.ProductLicense)
                {

                    StorePurchaseResult result = await _storeContext.RequestPurchaseAsync(SKUKey);

                    if (result.ExtendedError != null)
                        throw new Exception(result.ExtendedError.Message);


                    switch (result.Status)
                    {
                        case StorePurchaseStatus.AlreadyPurchased:
                        case StorePurchaseStatus.Succeeded:
                            return true;
                            break;

                        case StorePurchaseStatus.NotPurchased:
                        case StorePurchaseStatus.NetworkError:
                        case StorePurchaseStatus.ServerError:
                            return false;
                            break;

                        default:
                            break;
                    }

                    _storeContext.OfflineLicensesChanged -= OfflineLicensesChanged;

                    await Initialize();
                    await GetLicenseState();

                }
                else
                {
                    StoreProductResult productResult = await _storeContext.GetStoreProductForCurrentAppAsync();

                    if (productResult.ExtendedError != null)
                        throw new Exception(productResult.ExtendedError.Message);

                    StorePurchaseResult result = await productResult.Product.RequestPurchaseAsync();

                    if (result.ExtendedError != null)
                        throw new Exception(result.ExtendedError.Message);

                    //switch (result.Status)
                    //{
                    //    case StorePurchaseStatus.AlreadyPurchased:
                    //        rootPage.NotifyUser($"You already bought this app and have a fully-licensed version.", NotifyType.ErrorMessage);
                    //        break;

                    //    case StorePurchaseStatus.Succeeded:
                    //        // License will refresh automatically using the StoreContext.OfflineLicensesChanged event
                    //        break;

                    //    case StorePurchaseStatus.NotPurchased:
                    //        rootPage.NotifyUser("Product was not purchased, it may have been canceled.", NotifyType.ErrorMessage);
                    //        break;

                    //    case StorePurchaseStatus.NetworkError:
                    //        rootPage.NotifyUser("Product was not purchased due to a Network Error.", NotifyType.ErrorMessage);
                    //        break;

                    //    case StorePurchaseStatus.ServerError:
                    //        rootPage.NotifyUser("Product was not purchased due to a Server Error.", NotifyType.ErrorMessage);
                    //        break;

                    //    default:
                    //        rootPage.NotifyUser("Product was not purchased due to an Unknown Error.", NotifyType.ErrorMessage);
                    //        break;
                    //}

                    _storeContext.OfflineLicensesChanged -= OfflineLicensesChanged;

                    await Initialize();
                    await GetLicenseState();

                }


                if (this.IsPurchased)
                {
                    await dialogService.ShowAsync("Thank you very much for your puchase and support. The full version has been unlocked. Please restart the application for the changes to take affect.", "PURCHASE SUCCESS");
                }
            }
            catch (Exception e)
            {
                await dialogService.ShowAsync("Something went wrong and the purchase could not be completed. Please try again after some time. If the problem persists, please contact Support at WinApps@xgeno.com." + Environment.NewLine + e.Message, "PURCHASE ERROR");

                throw(e);
            }

            return IsPurchased;
        }

        
    }
}
