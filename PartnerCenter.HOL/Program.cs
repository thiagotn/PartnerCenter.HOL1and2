using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Store.PartnerCenter;
using Microsoft.Store.PartnerCenter.Extensions;
using Microsoft.Store.PartnerCenter.Models;
using Microsoft.Store.PartnerCenter.Models.Customers;
using Microsoft.Store.PartnerCenter.Models.Partners;
using Microsoft.Store.PartnerCenter.Models.Licenses;
using Microsoft.Store.PartnerCenter.Models.Query;
using Microsoft.Store.PartnerCenter.Models.Offers;
using Microsoft.Store.PartnerCenter.Models.Orders;
using Microsoft.Store.PartnerCenter.Models.ServiceRequests;
using Microsoft.Store.PartnerCenter.Models.Users;
using Microsoft.Store.PartnerCenter.RequestContext;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace PartnerCenter.HOL
{
    class Program
    {
        static void Main(string[] args)
        {
            var addTokenResult = Program.LoginUserToAd();

            IPartnerCredentials credentials = PartnerCredentials.Instance.GenerateByUserCredentials(
                Configuration.ApplicationId,
                new AuthenticationToken(addTokenResult.AccessToken, addTokenResult.ExpiresOn),
                authToken =>
                {
                    addTokenResult = Program.LoginUserToAd();
                    return Task.FromResult(new AuthenticationToken(
                        addTokenResult.AccessToken, addTokenResult.ExpiresOn));
                });

            IAggregatePartner partner = PartnerService.Instance.CreatePartnerOperations(credentials);

            //CreateCustomer(partner);
            //QueryCustomers(partner);
            //GetOffers(partner);
            //PlaceOrder(partner, "56d94dfc-d613-431e-9c8d-fd78b833a6fc");
            //CreateServiceRequest(partner, GetSupportTopicId(partner));
            //GetServiceRequests(partner);

            //var partnerCredentials = GetPartnerCredentials(LoginUserToAd()); // ????
            var customer = GetExistingCustomer(partner);
            var subscribedSkus = GetCustomerSubscribedSkus(partner, customer);
            //var customerUsers = GetCustomerUsers(partner, customer.Id);
        
            var customerUser = GetCustomerUsers(partner, customer.Id);
            //var createdCustomerUser = CreateCustomerUser(partner, customer, "yourusername01");

            var subscribedSku = subscribedSkus.Items.First(sku => sku.AvailableUnits > 0);

            var customerUserId = customerUser.Items.FirstOrDefault().Id;

            //AssignLicensesToCustomerUser(partner, customer.Id, customerUserId, subscribedSku);

            RemoveLicensesFromCustomerUser(partner, customer.Id, customerUserId, subscribedSku);

            var licenses = GetCustomerUserSubscribedSkus(partner, customer.Id, customerUserId);

            System.Threading.Thread.Sleep(3000);
        }

        private static AuthenticationResult LoginUserToAd()
        {
            var addAuthority = new System.UriBuilder(Configuration.AuthenticationAuthorityEndpoint)
            {
                Path = Configuration.CommonDomain
            };

            UserCredential userCredentials = new UserCredential(
                Configuration.UserName,
                Configuration.Password);

            AuthenticationContext authContext = new AuthenticationContext(addAuthority.Uri.AbsoluteUri);

            return authContext.AcquireToken(
                Configuration.ResourceUrl,
                Configuration.ApplicationId,
                userCredentials);
        }

        private static Customer CreateCustomer(IAggregatePartner partner)
        {
            var customerToCreate = new Customer()
            {
                CompanyProfile = new CustomerCompanyProfile()
                {
                    Domain = "xpto123456.onmicrosoft.com"
                },
                BillingProfile = new CustomerBillingProfile()
                {
                    Culture = "PT-BR",
                    Email = "algumemail@Outlook.com",
                    Language = "PT-BR",
                    CompanyName = "Alguma CIA",
                    DefaultAddress = new Address()
                    {
                        FirstName = "First",
                        LastName = "Last",
                        AddressLine1 = "One M Way",
                        City = "Sao Paulo",
                        State = "sp",
                        Country = "BR",
                        PostalCode = "03103001",
                        PhoneNumber = "4257778899"
                    }
                }
            };

            Customer newCustomer = partner.Customers.Create(customerToCreate);
            Helpers.WriteObject(newCustomer, "New Customer");
            return newCustomer;
        }

        private static ResourceCollection<Customer> QueryCustomers(IAggregatePartner partner)
        {
            IQuery customerQuery = QueryFactory.Instance.BuildIndexedQuery(10);
            ResourceCollection<Customer> customers = partner.Customers.Query(customerQuery);
            Helpers.WriteObject(customers, "Customer List");

            return customers;
        }

        private static ResourceCollection<Offer> GetOffers(IAggregatePartner partner)
        {
            ResourceCollection<Offer> offers = partner.Offers.ByCountry("BR").Get();
            Helpers.WriteObject(offers, "Offer list");

            return offers;
        }

        private static Order PlaceOrder(IAggregatePartner partner, string customerId)
        {
            var lineItems = new List<OrderLineItem>();

            lineItems.Add(new OrderLineItem
            {
                LineItemNumber = 0,
                OfferId = "2B6F895D-DFD3-4FB5-8C8C-1A551C9DB59A",
                FriendlyName = "Office 365 Business Premium",
                Quantity = 1
            });

            var order = new Order
            {
                ReferenceCustomerId = customerId,
                LineItems = lineItems
            };

            Order createOrder = partner.Customers.ById(customerId).Orders.Create(order);
            Helpers.WriteObject(createOrder, "Order Placed");

            return order;
        }

        private static string GetSupportTopicId(IAggregatePartner partner)
        {
            ResourceCollection<SupportTopic> supportTopics = partner.ServiceRequests.SupportTopics.Get();

            return supportTopics.Items.First().Id.ToString();
        }

        private static ServiceRequest CreateServiceRequest(IAggregatePartner partner, string supportTopicId)
        {
            ServiceRequest serviceRequestToCreate = new ServiceRequest()
            {
                Title = "Trial Service Request",
                Description = "Some service is down",
                Severity = ServiceRequestSeverity.Moderate,
                SupportTopicId = supportTopicId
            };

            ServiceRequest serviceRequest = partner.ServiceRequests.Create(serviceRequestToCreate, "en-US");
            Helpers.WriteObject(serviceRequest, "Created Service Request");

            return serviceRequest;
        }

        private static ResourceCollection<ServiceRequest> GetServiceRequests(IAggregatePartner partner)
        {
            ResourceCollection<ServiceRequest> serviceRequests = partner.ServiceRequests.Query(QueryFactory.Instance.BuildIndexedQuery(5));
            Helpers.WriteObject(serviceRequests, "Service Requests");

            return serviceRequests;
        }

        private static Customer GetExistingCustomer(IAggregatePartner partner)
        {
            var customers = partner.Customers.Get();
            var customer = customers.Items.FirstOrDefault();
            Helpers.WriteObject(customer, "Existing Customer");

            return customer;
        }

        private static ResourceCollection<CustomerUser> GetCustomerUsers(IAggregatePartner partner, string customerId)
        {
            IQuery customerUsersQuery = QueryFactory.Instance.BuildIndexedQuery(10);
            var customerUserPageResult = partner.Customers.ById(customerId).Users
                .Query(customerUsersQuery);
            Helpers.WriteObject(customerUserPageResult, "Customer Users List");

            return customerUserPageResult;
        }

        private static CustomerUser GetCustomerUser(IAggregatePartner partner, string customerId)
        {
            var customerUsers = partner.Customers.ById(customerId).Users.Get();
            return customerUsers.Items.FirstOrDefault();
        }

        private static CustomerUser CreateCustomerUser(IAggregatePartner partner, Customer existingCustomer, string userName)
        {
            string existingCustomerId = existingCustomer.Id;
            var userToCreate = new CustomerUser()
            {
                PasswordProfile = new PasswordProfile()
                {
                    ForceChangePassword = true,
                    Password = "Um$2345678"
                },
                DisplayName = "Thiago",
                FirstName = "Thiago",
                LastName = "Nogueira",
                UsageLocation = "US",
                UserPrincipalName = userName + "@" + existingCustomer.CompanyProfile.Domain
            };

            CustomerUser createdCustomerUser = partner.Customers
                .ById(existingCustomerId).Users
                .Create(userToCreate);

            Helpers.WriteObject(createdCustomerUser, "Created Customer User");

            return createdCustomerUser;
        }

        private static ResourceCollection<SubscribedSku> GetCustomerSubscribedSkus(IAggregatePartner partner, Customer customer)
        {
            var customerSubscribedSkus = partner.Customers.ById(customer.Id).SubscribedSkus.Get();
            Helpers.WriteObject(customerSubscribedSkus, "Customer Subscribed SKU");

            if (customerSubscribedSkus != null && customerSubscribedSkus.TotalCount > 0)
            {
                foreach (var customerSubscribedSku in customerSubscribedSkus.Items)
                {
                    var availableLicenses = customerSubscribedSku.AvailableUnits;
                    Console.WriteLine("SKU: " + customerSubscribedSku.ProductSku.SkuPartNumber + " Available licences : " + availableLicenses);
                }
            }

            return customerSubscribedSkus;
        }

        private static void AssignLicensesToCustomerUser(IAggregatePartner partner, string customerId, string customerUserId, SubscribedSku sku)
        {
            LicenseAssignment license = new LicenseAssignment
            {
                SkuId = sku.ProductSku.Id,
                ExcludedPlans = null
            };

            LicenseUpdate licenseUpdate = new LicenseUpdate()
            {
                LicensesToAssign = new List<LicenseAssignment>()
                {
                    license
                }
            };

            var assigndLicensesUpdate = partner.Customers.ById(customerId)
                .Users.ById(customerUserId)
                .LicenseUpdates.Create(licenseUpdate);

            Helpers.WriteObject(assigndLicensesUpdate, "Assign License update for customer user : " + customerUserId);

            var customerUserAssignedLicenses = partner.Customers.ById(customerId)
                .Users.ById(customerUserId)
                .Licenses.Get();

            Helpers.WriteObject(customerUserAssignedLicenses,
                "Assigned Licenses for the customer user : " + customerUserId);
        }

        private static void RemoveLicensesFromCustomerUser(IAggregatePartner partner, string customerId, string customerUserId, SubscribedSku sku)
        {
            LicenseAssignment license = new LicenseAssignment();
            license.SkuId = sku.ProductSku.Id;
            license.ExcludedPlans = null;

            LicenseUpdate licenseUpdate = new LicenseUpdate()
            {
                LicensesToAssign = null,
                LicensesToRemove =  new List<string>()
                {
                    sku.ProductSku.Id
                }
            };

            var removeLicensesUpdate = partner.Customers.ById(customerId).Users.ById(customerUserId).LicenseUpdates.Create(licenseUpdate);

            Helpers.WriteObject(removeLicensesUpdate, "Remove License update for customer user: " + customerUserId);

            var customerUserAssignedLicenses = partner.Customers.ById(customerId)
                .Users.ById(customerUserId)
                .Licenses.Get();

            Helpers.WriteObject(customerUserAssignedLicenses, "Assigned licenses for customer user: " + customerUserId);
        }

        private static ResourceCollection<License> GetCustomerUserSubscribedSkus(IAggregatePartner partner, string customerId, string customerUserId)
        {
            var customerUserAssignedLicenses = partner.Customers.ById(customerId).Users.ById(customerUserId).Licenses.Get();
            Helpers.WriteObject(customerUserAssignedLicenses, "Customer User assigned Licences: " + customerUserAssignedLicenses.ToString());

            return customerUserAssignedLicenses;
        }
    }
}
