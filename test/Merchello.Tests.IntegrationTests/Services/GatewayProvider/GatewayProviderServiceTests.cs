﻿using System.Linq;
using Merchello.Core;
using Merchello.Core.Cache;
using Merchello.Core.Models;
using Merchello.Core.Models.Interfaces;
using Merchello.Core.Persistence.UnitOfWork;
using Merchello.Core.Services;
using NUnit.Framework;
using Umbraco.Core;

namespace Merchello.Tests.IntegrationTests.Services.GatewayProvider
{
    [TestFixture]
    [Category("Service Integration")]
    public class GatewayProviderServiceTests : ServiceIntegrationTestBase
    {
        private IGatewayProviderService _gatewayProviderService;
        private IWarehouseCatalog _catalog;
        private ISettingsService _settingsService;
        private IShippingService _shippingService;
        private IMerchelloContext _merchelloContext;

        [SetUp]
        public void Init()
        {
            _gatewayProviderService = PreTestDataWorker.GatewayProviderService;
            _settingsService = PreTestDataWorker.SettingsService;
            _shippingService = PreTestDataWorker.ShippingService;

            _merchelloContext = new MerchelloContext(new ServiceContext(new PetaPocoUnitOfWorkProvider()),
                new CacheHelper(new NullCacheProvider(),
                    new NullCacheProvider(),
                    new NullCacheProvider()));            

            _catalog = PreTestDataWorker.WarehouseService.GetDefaultWarehouse().WarehouseCatalogs.FirstOrDefault();

            PreTestDataWorker.DeleteAllShipCountries();
            var country = _settingsService.GetCountryByCode("US");
            var shipCountry = new ShipCountry(_catalog.Key, country);
            _shippingService.Save(shipCountry);
           
            var provider = _merchelloContext.Gateways.GetGatewayProviders(GatewayProviderType.Shipping).FirstOrDefault();
            var shippingProvider = _merchelloContext.Gateways.GetShippingGatewayProvider(provider);
            Assert.NotNull(shippingProvider);

            var resource = shippingProvider.ListAvailableMethods().FirstOrDefault();
            var gatewayShipMethod = shippingProvider.CreateShipMethod(resource, shipCountry, "Ground");
            shippingProvider.SaveShipMethod(gatewayShipMethod);
        }

        /// <summary>
        /// Test confirms that a collection of GatewayProviders can be returned by ShipCountry
        /// </summary>
        [Test]
        public void Can_Query_A_List_Of_GatewayProviders_By_Country()
        {
            //// Arrange
            var shipCountry = _shippingService.GetShipCountriesByCatalogKey(_catalog.Key).First();

            
            //// Act
            var gateways = _gatewayProviderService.GetGatewayProvidersByShipCountry(shipCountry);

            //// Assert
            Assert.NotNull(gateways);
            Assert.IsTrue(gateways.Any());
        }
    }
}