using Amazon.Common.ExcelExport;
using Amazon.Common.Helpers;
using Amazon.Core;
using Amazon.Core.Contracts;
using Amazon.Core.Entities.Enums;
using Amazon.Core.Exports.Attributes;
using Amazon.DTO;
using Ccen.Web;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Amazon.Web.ViewModels.Customers
{
    public class CustomerViewModel
    {
        public long? Id { get; set; }

        [ExcelSerializable("Orders Count", Order = 8, Width = 25)]
        public int OrdersCount { get; set; }

        public DateTime? CreateDate { get; set; }

        [ExcelSerializable("Name", Order = 0, Width = 25)]
        public string BuyerName { get; set; }

        [ExcelSerializable("Email", Order = 7, Width = 25)]
        public string BuyerEmail { get; set; }
        public string AmazonEmail { get; set; }

        public string PersonName { get; set; }

        [ExcelSerializable("Country", Order = 5, Width = 25)]
        public string ShippingCountry { get; set; }

        public string ShippingAddress1 { get; set; }

        public string ShippingAddress2 { get; set; }

        [ExcelSerializable("Street", Order = 1, Width = 25)]
        public string Street
        {
            get { return StringHelper.JoinTwo(" ", ShippingAddress1, ShippingAddress2); }
        }

        [ExcelSerializable("City", Order = 2, Width = 25)]
        public string ShippingCity { get; set; }

        [ExcelSerializable("State", Order = 3, Width = 25)]
        public string ShippingState { get; set; }

        [ExcelSerializable("Zip", Order = 4, Width = 25)]
        public string ShippingZip { get; set; }

        public string ShippingZipAddon { get; set; }

        [ExcelSerializable("Phone", Order = 6, Width = 25)]
        public string ShippingPhone { get; set; }

        public string OrderNumber { get; set; }

        public CustomerViewModel()
        {

        }

        public CustomerViewModel(DTOOrder blDto)
        {
            if (blDto == null)
                return;

            Id = blDto.Id;
            CreateDate = blDto.CreateDate;

            CreateDate = blDto.OrderDate;
            BuyerName = blDto.BuyerName;
            BuyerEmail = blDto.BuyerEmail;
            AmazonEmail = blDto.AmazonEmail;

            PersonName = blDto.PersonName;
            ShippingCountry = blDto.ShippingCountry;
            ShippingAddress1 = blDto.ShippingAddress1;
            ShippingAddress2 = blDto.ShippingAddress2;
            ShippingCity = blDto.ShippingCity;
            ShippingState = blDto.ShippingState;
            ShippingZip = blDto.ShippingZip;
            ShippingZipAddon = blDto.ShippingZipAddon;
            ShippingPhone = blDto.ShippingPhone;
        }

        public static MemoryStream ExportToExcel(ILogService log,
            ITime time,
            IUnitOfWork db,
            CustomerFilterViewModel filter)
        {
            var templateName = AppSettings.CustomerReportTemplate;
            var gridItems = GetAll(db, filter).ToList();

            return ExcelHelper.ExportIntoFile(HttpContext.Current.Server.MapPath(templateName),
                "Template",
                gridItems,
                null,
                1);
        }


        public static IEnumerable<CustomerViewModel> GetAll(IUnitOfWork db,
                    CustomerFilterViewModel filter)
        {
            var orderByCustomerQuery = from o in db.Orders.GetAll()
                                       where o.OrderStatus != OrderStatusEnumEx.Pending
                                       group o by o.CustomerId into byCustomer
                                       select new
                                       {
                                           CustomerId = byCustomer.Key,
                                           OrdersCount = byCustomer.Count(),
                                       };

            var sourceOrderQuery = (from c in db.Customers.GetAll()
                                    join o in orderByCustomerQuery on c.Id equals o.CustomerId into withCustomer
                                    from o in withCustomer.DefaultIfEmpty()
                                    select new CustomerViewModel
                                    {
                                        Id = c.Id,

                                        CreateDate = c.CreateDate,

                                        BuyerName = c.Name,
                                        ShippingAddress1 = c.Address1,
                                        ShippingAddress2 = c.Address2,
                                        ShippingCity = c.City,
                                        ShippingZip = c.Zip,
                                        ShippingZipAddon = c.ZipAddon,
                                        ShippingState = c.State,
                                        ShippingCountry = c.Country,
                                        ShippingPhone = c.Phone,
                                        BuyerEmail = c.Email,

                                        OrdersCount = ((int?)o.OrdersCount) ?? 0,
                                    });

            if (filter.DateFrom.HasValue)
                sourceOrderQuery = sourceOrderQuery.Where(o => o.CreateDate >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                sourceOrderQuery = sourceOrderQuery.Where(o => o.CreateDate <= filter.DateTo.Value);

            if (!String.IsNullOrEmpty(filter.BuyerName))
                sourceOrderQuery = sourceOrderQuery.Where(o => o.BuyerName.Contains(filter.BuyerName));

            if (!String.IsNullOrEmpty(filter.OrderNumber))
                sourceOrderQuery = sourceOrderQuery.Where(o => o.OrderNumber == filter.OrderNumber);

            var results = sourceOrderQuery.ToList();

            return results.OrderByDescending(o => o.CreateDate);
        }
    }
}