using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Rest;
using Moq;
using PagedList.Core;
using VirtoCommerce.Storefront.AutoRestClients.CustomerReviewsModule.WebModuleApi;
using VirtoCommerce.Storefront.Domain.CustomerReview;
using VirtoCommerce.Storefront.Infrastructure;
using VirtoCommerce.Storefront.Model.Caching;
using VirtoCommerce.Storefront.Model.CustomerReviews;
using Xunit;

namespace VirtoCommerce.Storefront.Tests.Domain.CustomerReview
{
    public class CustomerReviewServiceTests
    {
        private readonly Fixture randomizer;
        private readonly Mock<ICustomerReviewsModule> customerReviewsApi;
        private readonly Mock<IStorefrontMemoryCache> memoryCache;
        private readonly Mock<IApiChangesWatcher> apiChangesWatcher;
        private readonly CustomerReviewService service;

        public CustomerReviewServiceTests()
        {
            randomizer = new Fixture();
            customerReviewsApi = new Mock<ICustomerReviewsModule>();
            memoryCache = new Mock<IStorefrontMemoryCache>();
            apiChangesWatcher = new Mock<IApiChangesWatcher>();

            service = new CustomerReviewService(customerReviewsApi.Object, memoryCache.Object, apiChangesWatcher.Object);
        }

        [Fact]
        public async Task SearchReviewsAsync_ShouldReturnStaticPagedList()
        {
            //arrange
            var criteria = randomizer.Create<CustomerReviewSearchCriteria>();
            var reviewResult = randomizer.Create<AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.GenericSearchResultCustomerReview>();
            var cacheOptions = randomizer.Create<MemoryCacheEntryOptions>();
            memoryCache
                .Setup(m => m.GetDefaultCacheEntryOptions())
                .Returns(cacheOptions);
            object res = new StaticPagedList<Storefront.Model.CustomerReviews.CustomerReview>(new[]
                {
                    new Storefront.Model.CustomerReviews.CustomerReview
                    {
                        Id = reviewResult.Results[0].Id,
                        AuthorNickname = reviewResult.Results[0].AuthorNickname,
                        Content = reviewResult.Results[0].Content,
                        CreatedBy = reviewResult.Results[0].CreatedBy,
                        CreatedDate = reviewResult.Results[0].CreatedDate,
                        IsActive = reviewResult.Results[0].IsActive,
                        ModifiedBy = reviewResult.Results[0].ModifiedBy,
                        ModifiedDate = reviewResult.Results[0].ModifiedDate,
                        ProductId = reviewResult.Results[0].ProductId,
                        Rating = reviewResult.Results[0].Rating
                    }
                },
                criteria.PageNumber, criteria.PageSize, reviewResult.TotalCount.Value
                );
            memoryCache
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out res))
                .Returns(true);

            customerReviewsApi
                .Setup(m => m.SearchCustomerReviewsWithHttpMessagesAsync(
                    It.IsAny<AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.CustomerReviewSearchCriteria>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpOperationResponse<AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.GenericSearchResultCustomerReview>
                {
                    Body = reviewResult
                });

            //act
            var result = await service.SearchReviewsAsync(criteria);

            //assert
            result.Should().BeEquivalentTo(
                new StaticPagedList<Storefront.Model.CustomerReviews.CustomerReview>(new[]
                {
                    new Storefront.Model.CustomerReviews.CustomerReview
                    {
                        Id = reviewResult.Results[0].Id,
                        AuthorNickname = reviewResult.Results[0].AuthorNickname,
                        Content = reviewResult.Results[0].Content,
                        CreatedBy = reviewResult.Results[0].CreatedBy,
                        CreatedDate = reviewResult.Results[0].CreatedDate,
                        IsActive = reviewResult.Results[0].IsActive,
                        ModifiedBy = reviewResult.Results[0].ModifiedBy,
                        ModifiedDate = reviewResult.Results[0].ModifiedDate,
                        ProductId = reviewResult.Results[0].ProductId,
                        Rating = reviewResult.Results[0].Rating
                    }
                },
                criteria.PageNumber, criteria.PageSize, reviewResult.TotalCount.Value
                ), options => options.ComparingByMembers<Storefront.Model.CustomerReviews.CustomerReview>());
        }

        [Fact]
        public void SearchReviews_ShouldReturnStaticPagedList()
        {
            //arrange
            var criteria = randomizer.Create<CustomerReviewSearchCriteria>();
            var reviewResult = randomizer.Create<AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.GenericSearchResultCustomerReview>();
            var cacheOptions = randomizer.Create<MemoryCacheEntryOptions>();
            memoryCache
                .Setup(m => m.GetDefaultCacheEntryOptions())
                .Returns(cacheOptions);
            object res = new StaticPagedList<Storefront.Model.CustomerReviews.CustomerReview>(new[]
                {
                    new Storefront.Model.CustomerReviews.CustomerReview
                    {
                        Id = reviewResult.Results[0].Id,
                        AuthorNickname = reviewResult.Results[0].AuthorNickname,
                        Content = reviewResult.Results[0].Content,
                        CreatedBy = reviewResult.Results[0].CreatedBy,
                        CreatedDate = reviewResult.Results[0].CreatedDate,
                        IsActive = reviewResult.Results[0].IsActive,
                        ModifiedBy = reviewResult.Results[0].ModifiedBy,
                        ModifiedDate = reviewResult.Results[0].ModifiedDate,
                        ProductId = reviewResult.Results[0].ProductId,
                        Rating = reviewResult.Results[0].Rating
                    }
                },
                criteria.PageNumber, criteria.PageSize, reviewResult.TotalCount.Value
                );
            memoryCache
                .Setup(m => m.TryGetValue(It.IsAny<object>(), out res))
                .Returns(true);

            customerReviewsApi
                .Setup(m => m.SearchCustomerReviewsWithHttpMessagesAsync(
                    It.IsAny<AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.CustomerReviewSearchCriteria>(),
                    It.IsAny<Dictionary<string, List<string>>>(),
                    It.IsAny<CancellationToken>()))
                .ReturnsAsync(new HttpOperationResponse<AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.GenericSearchResultCustomerReview>
                {
                    Body = reviewResult
                });

            //act
            var result = service.SearchReviews(criteria);

            //assert
            result.Should().BeEquivalentTo(
                new StaticPagedList<Storefront.Model.CustomerReviews.CustomerReview>(new[]
                {
                    new Storefront.Model.CustomerReviews.CustomerReview
                    {
                        Id = reviewResult.Results[0].Id,
                        AuthorNickname = reviewResult.Results[0].AuthorNickname,
                        Content = reviewResult.Results[0].Content,
                        CreatedBy = reviewResult.Results[0].CreatedBy,
                        CreatedDate = reviewResult.Results[0].CreatedDate,
                        IsActive = reviewResult.Results[0].IsActive,
                        ModifiedBy = reviewResult.Results[0].ModifiedBy,
                        ModifiedDate = reviewResult.Results[0].ModifiedDate,
                        ProductId = reviewResult.Results[0].ProductId,
                        Rating = reviewResult.Results[0].Rating
                    }
                },
                criteria.PageNumber, criteria.PageSize, reviewResult.TotalCount.Value
                ), options => options.ComparingByMembers<Storefront.Model.CustomerReviews.CustomerReview>());
        }
    }
}
