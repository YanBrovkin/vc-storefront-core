using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Rest;
using Moq;
using PagedList.Core;
using VirtoCommerce.Storefront.AutoRestClients.CatalogModuleApi;
using VirtoCommerce.Storefront.Domain;
using VirtoCommerce.Storefront.Infrastructure;
using VirtoCommerce.Storefront.Model;
using VirtoCommerce.Storefront.Model.Caching;
using VirtoCommerce.Storefront.Model.Catalog;
using VirtoCommerce.Storefront.Model.Common;
using VirtoCommerce.Storefront.Model.Customer.Services;
using VirtoCommerce.Storefront.Model.CustomerReviews;
using VirtoCommerce.Storefront.Model.Inventory.Services;
using VirtoCommerce.Storefront.Model.Pricing.Services;
using VirtoCommerce.Storefront.Model.Stores;
using VirtoCommerce.Storefront.Model.Subscriptions.Services;
using Xunit;

namespace VirtoCommerce.Storefront.Tests.Domain
{
    public class CatalogServiceTests
    {
        private readonly Fixture randomizer;
        private readonly Mock<IWorkContextAccessor> workContextAccessor;
        private readonly Mock<ICatalogModuleCategories> categoriesApi;
        private readonly Mock<ICatalogModuleProducts> productsApi;
        private readonly Mock<ICatalogModuleSearch> searchApi;
        private readonly Mock<IPricingService> pricingService;
        private readonly Mock<IMemberService> customerService;
        private readonly Mock<ISubscriptionService> subscriptionService;
        private readonly Mock<IInventoryService> inventoryService;
        private readonly Mock<IStorefrontMemoryCache> memoryCache;
        private readonly Mock<IApiChangesWatcher> apiChangesWatcher;
        private readonly Mock<ICustomerReviewService> customerReviewService;
        private readonly CatalogService service;

        public CatalogServiceTests()
        {
            randomizer = new Fixture();
            randomizer.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => randomizer.Behaviors.Remove(b));
            randomizer.Behaviors.Add(new OmitOnRecursionBehavior());
            workContextAccessor = new Mock<IWorkContextAccessor>();
            categoriesApi = new Mock<ICatalogModuleCategories>();
            productsApi = new Mock<ICatalogModuleProducts>();
            searchApi = new Mock<ICatalogModuleSearch>();
            pricingService = new Mock<IPricingService>();
            customerService = new Mock<IMemberService>();
            subscriptionService = new Mock<ISubscriptionService>();
            inventoryService = new Mock<IInventoryService>();
            memoryCache = new Mock<IStorefrontMemoryCache>();
            apiChangesWatcher = new Mock<IApiChangesWatcher>();
            customerReviewService = new Mock<ICustomerReviewService>();
            service = new CatalogService(
                workContextAccessor.Object,
                categoriesApi.Object,
                productsApi.Object,
                searchApi.Object,
                pricingService.Object,
                customerService.Object,
                subscriptionService.Object,
                inventoryService.Object,
                memoryCache.Object,
                apiChangesWatcher.Object,
                customerReviewService.Object);
        }

        [Fact]
        public async Task LoadProductDependencies_ShouldLoadReviewsAndAverageRating_IfDataIsProper()
        {
            var ids = randomizer.CreateMany<string>(1).ToArray();
            var productsResponse = randomizer
                .Build<AutoRestClients.CatalogModuleApi.Models.Product>()
                .Without(p => p.SeoInfos)
                .Without(p => p.Reviews)
                .With(p => p.Outlines, new List<AutoRestClients.CatalogModuleApi.Models.Outline>())
                .CreateMany(1)
                .ToList();
            var responseGroup = ItemResponseGroup.None;
            var workContext = randomizer
                .Build<WorkContext>()
                .Without(p => p.Countries)
                .Without(p => p.CurrentUser)
                .Without(p => p.CurrentPageSeo)
                .Without(p => p.CurrentCart)
                .Without(p => p.CurrentQuoteRequest)
                .Without(p => p.CurrentLinkLists)
                .Without(p => p.AllStores)
                .Without(p => p.AllCurrencies)
                .Without(p => p.CurrentPricelists)
                .Without(p => p.CurrentProduct)
                .Without(p => p.CurrentCategory)
                .Without(p => p.Categories)
                .Without(p => p.Products)
                .Without(p => p.ProductSearchResult)
                .Without(p => p.CurrentProductSearchCriteria)
                .Without(p => p.Vendors)
                .Without(p => p.CurrentVendor)
                .Without(p => p.CurrentPage)
                .Without(p => p.StaticContentSearchResult)
                .Without(p => p.CurrentBlog)
                .Without(p => p.CurrentBlogArticle)
                .Without(p => p.CurrentOrder)
                .Without(p => p.Pages)
                .Without(p => p.Blogs)
                .Without(p => p.FulfillmentCenters)
                .With(p => p.CurrentLanguage, new Language(randomizer.Create<CultureInfo>().Name))
                .With(p => p.CurrentCurrency, new Currency(new Language(randomizer.Create<CultureInfo>().Name), "RU"))
                .With(p => p.CurrentStore, new Store
                {
                    DefaultLanguage = new Language(randomizer.Create<CultureInfo>().Name),
                    CustomerReviewsEnabled = true
                })
                .Create();
            workContextAccessor.Setup(m => m.WorkContext).Returns(workContext);
            var cacheOptions = randomizer.Create<MemoryCacheEntryOptions>();
            memoryCache
                .Setup(m => m.GetDefaultCacheEntryOptions())
                .Returns(cacheOptions);
            object res = productsResponse;
            memoryCache
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out res))
                .Returns(true);

            productsApi
                .Setup(m => m.GetProductByPlentyIdsWithHttpMessagesAsync(
                    It.IsAny<IList<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpOperationResponse<IList<AutoRestClients.CatalogModuleApi.Models.Product>>
                {
                    Body = productsResponse
                });

            var criteria = randomizer.Create<CustomerReviewSearchCriteria>();
            var resultCount = randomizer.Create<int>();

            // Set rating
            var reviews = randomizer.CreateMany<Storefront.Model.CustomerReviews.CustomerReview>(2).ToList();
            reviews[0].Rating = 1;
            reviews[1].Rating = 3;
            var customerReviews = new StaticPagedList<Storefront.Model.CustomerReviews.CustomerReview>(
                 reviews, criteria.PageNumber, criteria.PageSize, resultCount);
            customerReviewService.Setup(m => m.SearchReviews(It.IsAny<CustomerReviewSearchCriteria>())).Returns(customerReviews);
            customerReviewService.Setup(m => m.GetAverageRatingAsync(It.IsAny<string>())).ReturnsAsync(3);

            //act
            var result = await service.GetProductsAsync(ids, responseGroup);

            //assert
            result.Should().NotBeNull().And.HaveCount(1);
            result.ElementAt(0).CustomerReviews.Should().HaveCount(2);
            result.ElementAt(0).AverageRating.Should().Be(3);
        }

        [Fact]
        public async Task LoadProductDependencies_ShouldNotLoadReviewsAndAverageRating_IfProductsIsNull()
        {
            //arrange
            string[] ids = null;
            var responseGroup = randomizer.Create<ItemResponseGroup>();

            //act
            var result = await service.GetProductsAsync(ids, responseGroup);

            //assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task LoadProductDependencies_ShouldNotLoadReviewsAndAverageRating_IfCustomerReviewsEnableIsFalse()
        {
            var ids = randomizer.CreateMany<string>(1).ToArray();
            var productsResponse = randomizer
                .Build<AutoRestClients.CatalogModuleApi.Models.Product>()
                .Without(p => p.SeoInfos)
                .Without(p => p.Reviews)
                .With(p => p.Outlines, new List<AutoRestClients.CatalogModuleApi.Models.Outline>())
                .CreateMany(1)
                .ToList();
            var responseGroup = ItemResponseGroup.None;
            var workContext = randomizer
                .Build<WorkContext>()
                .Without(p => p.Countries)
                .Without(p => p.CurrentUser)
                .Without(p => p.CurrentPageSeo)
                .Without(p => p.CurrentCart)
                .Without(p => p.CurrentQuoteRequest)
                .Without(p => p.CurrentLinkLists)
                .Without(p => p.AllStores)
                .Without(p => p.AllCurrencies)
                .Without(p => p.CurrentPricelists)
                .Without(p => p.CurrentProduct)
                .Without(p => p.CurrentCategory)
                .Without(p => p.Categories)
                .Without(p => p.Products)
                .Without(p => p.ProductSearchResult)
                .Without(p => p.CurrentProductSearchCriteria)
                .Without(p => p.Vendors)
                .Without(p => p.CurrentVendor)
                .Without(p => p.CurrentPage)
                .Without(p => p.StaticContentSearchResult)
                .Without(p => p.CurrentBlog)
                .Without(p => p.CurrentBlogArticle)
                .Without(p => p.CurrentOrder)
                .Without(p => p.Pages)
                .Without(p => p.Blogs)
                .Without(p => p.FulfillmentCenters)
                .With(p => p.CurrentLanguage, new Language(randomizer.Create<CultureInfo>().Name))
                .With(p => p.CurrentCurrency, new Currency(new Language(randomizer.Create<CultureInfo>().Name), "RU"))
                .With(p => p.CurrentStore, new Store
                {
                    DefaultLanguage = new Language(randomizer.Create<CultureInfo>().Name),
                    CustomerReviewsEnabled = false
                })
                .Create();
            workContextAccessor.Setup(m => m.WorkContext).Returns(workContext);
            var cacheOptions = randomizer.Create<MemoryCacheEntryOptions>();
            memoryCache
                .Setup(m => m.GetDefaultCacheEntryOptions())
                .Returns(cacheOptions);
            object res = productsResponse;
            memoryCache
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out res))
                .Returns(true);

            productsApi
                .Setup(m => m.GetProductByPlentyIdsWithHttpMessagesAsync(
                    It.IsAny<IList<string>>(),
                    It.IsAny<string>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpOperationResponse<IList<AutoRestClients.CatalogModuleApi.Models.Product>>
                {
                    Body = productsResponse
                });

            var criteria = randomizer.Create<CustomerReviewSearchCriteria>();
            var resultCount = randomizer.Create<int>();

            // Set rating
            var reviews = randomizer.CreateMany<Storefront.Model.CustomerReviews.CustomerReview>(2).ToList();
            var customerReviews = new StaticPagedList<Storefront.Model.CustomerReviews.CustomerReview>(
                 reviews, criteria.PageNumber, criteria.PageSize, resultCount);
            customerReviewService.Setup(m => m.SearchReviews(It.IsAny<CustomerReviewSearchCriteria>())).Returns(customerReviews);

            //act
            var result = await service.GetProductsAsync(ids, responseGroup);

            //assert
            result.Should().NotBeNull().And.HaveCount(1);
            result.ElementAt(0).CustomerReviews.Should().BeNull();
            result.ElementAt(0).AverageRating.Should().Be(0);
        }
    }
}
