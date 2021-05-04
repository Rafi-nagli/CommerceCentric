
namespace Amazon.DTO
{
    public class ShippingMethodDTO
    {
        public int Id { get; set; }
        public int ShipmentProviderType { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public bool RequiredPackageSize { get; set; }
        public string ServiceIdentifier { get; set; }
        public string CarrierName { get; set; }

        public bool IsInternational { get; set; }
        public bool AllowOverweight { get; set; }
        public double? MaxWeight { get; set; }
        public int StampsServiceEnumCode { get; set; }
        public int StampsPackageEnumCode { get; set; }

        public bool IsCroppedLabel { get; set; }
        public string CroppedLayout { get; set; }
        public int? RotationAngle { get; set; }
        public bool IsFullPagePrint { get; set; }

        public bool IsSupportReturnToPOBox { get; set; }

        public string PackageNameOnLabel { get; set; }

        public int? PackageWidth { get; set; }
        public int? PackageHeight { get; set; }
        public int? PackageLength { get; set; }
        public string PredefinedPackageDimensions { get; set; }

        public bool IsActive { get; set; }


        public override string ToString()
        {
            return "Id=" + Id +
                   ", ShipmentProviderType=" + ShipmentProviderType +
                   ", Name=" + Name +
                   ", ServiceIdentifier=" + ServiceIdentifier +
                   ", CarrierName=" + CarrierName +
                   ", IsInternational=" + IsInternational +
                   ", AllowOverweight=" + AllowOverweight +
                   ", StampsServiceEnumCode=" + StampsServiceEnumCode +
                   ", StampsPackageEnumCode=" + StampsPackageEnumCode +
                   ", IsCroppedLabel=" + IsCroppedLabel +
                   ", CroppedLayout=" + CroppedLayout +
                   ", IsFullPagePrint=" + IsFullPagePrint;
        }
    }
}
