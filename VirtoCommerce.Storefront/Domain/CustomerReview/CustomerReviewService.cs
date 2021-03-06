using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using PagedList.Core;
using VirtoCommerce.Storefront.AutoRestClients.CustomerReviewsModule.WebModuleApi;
using VirtoCommerce.Storefront.Helpers;
using VirtoCommerce.Storefront.Infrastructure;
using VirtoCommerce.Storefront.Model.Caching;
using VirtoCommerce.Storefront.Model.Common.Caching;
using VirtoCommerce.Storefront.Model.CustomerReviews;

namespace VirtoCommerce.Storefront.Domain.CustomerReview
{
    public class CustomerReviewService : ICustomerReviewService
    {
        private readonly ICustomerReviewsModule _customerReviewsApi;
        private readonly IStorefrontMemoryCache _memoryCache;
        private readonly IApiChangesWatcher _apiChangesWatcher;
        public CustomerReviewService(ICustomerReviewsModule customerReviewsApi, IStorefrontMemoryCache memoryCache, IApiChangesWatcher apiChangesWatcher)
        {
            _customerReviewsApi = customerReviewsApi;
            _memoryCache = memoryCache;
            _apiChangesWatcher = apiChangesWatcher;
        }

        public IPagedList<Model.CustomerReviews.CustomerReview> SearchReviews(Model.CustomerReviews.CustomerReviewSearchCriteria criteria)
        {
            return SearchReviewsAsync(criteria).GetAwaiter().GetResult();
        }

        public async Task<IPagedList<Model.CustomerReviews.CustomerReview>> SearchReviewsAsync(Model.CustomerReviews.CustomerReviewSearchCriteria criteria)
        {
            var cacheKey = CacheKey.With(GetType(), nameof(SearchReviewsAsync), criteria.GetCacheKey());
            return await _memoryCache.GetOrCreateExclusiveAsync(cacheKey, async (cacheEntry) =>
            {
                cacheEntry.AddExpirationToken(CustomerReviewCacheRegion.CreateChangeToken());
                cacheEntry.AddExpirationToken(_apiChangesWatcher.CreateChangeToken());

                var result = await _customerReviewsApi.SearchCustomerReviewsAsync(criteria.ToSearchCriteriaDto());
                return new StaticPagedList<Model.CustomerReviews.CustomerReview>(result.Results.Select(x => x.ToCustomerReview()),
                                                         criteria.PageNumber, criteria.PageSize, result.TotalCount.Value);
            });
        }

        public async Task AddReviewAsync(string authorNickname, ReviewRequest request)
        {
            var newReview = new List<AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.CustomerReview>
            {
                new AutoRestClients.CustomerReviewsModule.WebModuleApi.Models.CustomerReview
                {
                    AuthorNickname = authorNickname,
                    Content = request.Content,
                    CreatedBy = authorNickname,
                    CreatedDate = request.CreatedDate,
                    IsActive = true,
                    ModifiedBy = authorNickname,
                    ModifiedDate = SystemTime.Now,
                    ProductId = request.ProductId,
                    Rating = request.Rating
                }
            };
            await _customerReviewsApi.UpdateAsync(newReview);
            CustomerReviewCacheRegion.ExpireRegion();
        }

        public async Task<int> GetAverageRatingAsync(string productId)
        {
            var averageRating = await _customerReviewsApi.GetAverageRatingAsync(productId);
            return averageRating.Value;
        }
    }
}
