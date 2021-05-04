using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Amazon.Core.Contracts.Db;
using Amazon.Core;
using Amazon.Core.Entities.Inventory;
using Amazon.Core.Entities.Orders;
using Amazon.DTO.Inventory;
using Amazon.DTO.Shippings;
using Amazon.DTO.Customers;
using Amazon.DTO;

namespace Amazon.DAL.Repositories
{
    public class CustomerRepository : Repository<Customer>, ICustomerRepository
    {
        public CustomerRepository(IQueryableUnitOfWork unitOfWork)
            : base(unitOfWork)
        {
        }

        public IQueryable<CustomerDTO> GetAllAsDto()
        {
            return AsDto(GetAll());
        }

        public bool CreateIfNotExistFromOrderDto(DTOMarketOrder order, DateTime when)
        {
            var customer = order.GetCustomerInfo();
            var result = CreateIfNotExistOrUpdate(customer);
            order.CustomerId = customer.Id;

            return result;
        }

        public bool CreateIfNotExistOrUpdate(CustomerDTO customer)
        {
            var wasCreated = false;
            Customer dbCustomer = null;
            if (customer.Id.HasValue && customer.Id > 0)
            {
                dbCustomer = GetAll().FirstOrDefault(c => c.Id == customer.Id.Value);
            }

            if (dbCustomer == null && !String.IsNullOrEmpty(customer.Email))
            {
                dbCustomer = GetAll().FirstOrDefault(b => b.Email == customer.Email);
            }
            if (dbCustomer == null
                && !String.IsNullOrEmpty(customer.Phone)
                && !String.IsNullOrEmpty(customer.Name)
                && !String.IsNullOrEmpty(customer.Address1))
            {
                dbCustomer = GetAll().FirstOrDefault(c => c.Phone == customer.Phone
                    && c.Name == customer.Name
                    && c.Address1 == customer.Address1);
            }

            if (dbCustomer == null)
            {
                dbCustomer = new Customer()
                {
                    CreateDate = customer.CreateDate,
                };
                Add(dbCustomer);

                wasCreated = true;
            }

            dbCustomer.Name = customer.Name;
            dbCustomer.Email = customer.Email;
            dbCustomer.Phone = customer.Phone;

            dbCustomer.Address1 = customer.Address1;
            dbCustomer.Address2 = customer.Address2;
            dbCustomer.City = customer.City;
            dbCustomer.State = customer.State;
            dbCustomer.Zip = customer.Zip;
            dbCustomer.ZipAddon = customer.ZipAddon;
            dbCustomer.Country = customer.Country;

            unitOfWork.Commit();

            customer.Id = dbCustomer.Id;

            return wasCreated;
        }

        private IQueryable<CustomerDTO> AsDto(IQueryable<Customer> query)
        {
            return query.Select(c => new CustomerDTO()
            {
                Id = c.Id,

                Name = c.Name,
                Phone = c.Phone,
                Email = c.Email,

                Address1 = c.Address1,
                Address2 = c.Address2,
                City = c.City,
                Zip = c.Zip,
                ZipAddon = c.ZipAddon,
                State = c.State,
                Country = c.Country,

                CreateDate = c.CreateDate,
            });
        }
    }
}
