using Amazon.Web.Services.Models;
using System;
using System.ServiceModel;

namespace Amazon.Web.Services
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IInventoryAppService" in both code and config file together.
    [ServiceContract]
    public interface IInventoryAppService
    {
        [OperationContract]
        BarcodeInfo GetInfoByBarcode(string barcode);

        [Obsolete]
        [OperationContract]
        BarcodeInfo[] GetFBAPickList(string type);

        [OperationContract]
        BarcodeInfo[] GetLastFBAPickList(string type);

        [OperationContract]
        BarcodeInfo[] GetAllBarcodes();

        [OperationContract]
        void StoreOrderInfo(OrderInfo order);
    }
}
