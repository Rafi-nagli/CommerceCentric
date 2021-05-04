using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Amazon.Core.Entities.Enums
{
    public enum OrderStatusEnum : byte
    {
        Pending = 1,
        Unshipped = 2,
        Shipped = 3,
        PartiallyShipped = 4,
        Canceled = 5,
    }

    public static class OrderStatusEnumEx
    {
        public const string Pending = "Pending";
        public const string Unshipped = "Unshipped";
        public const string Shipped = "Shipped";
        public const string Canceled = "Canceled";
        public const string PartiallyShipped = "PartiallyShipped";
        public const string OnHold = "OnHold";
        public const string Refunded = "Refunded";

        public static string Str(this OrderStatusEnum en)
        {
            return en.ToString();
        }

        public static string[] AllUnshipped
        {
            get
            {
                return new[]
                {
                    OrderStatusEnum.Unshipped.Str(),
                    OrderStatusEnum.PartiallyShipped.Str()
                };
            }
        }

        public static string[] AllUnshippedWithShipped
        {
            get
            {
                return new[]
                {
                    OrderStatusEnum.Unshipped.Str(),
                    OrderStatusEnum.PartiallyShipped.Str(),
                    OrderStatusEnum.Shipped.Str()
                };
            }
        }

        public static string[] AllWithoutCanceled
        {
            get
            {
                return new string[]
                {
                    OrderStatusEnum.Pending.Str(),
                    OrderStatusEnum.Unshipped.Str(),
                    OrderStatusEnum.PartiallyShipped.Str(),
                    OrderStatusEnum.Shipped.Str(),
                    OrderStatusEnum.Canceled.Str()
                };
            }
        }
    }
}
