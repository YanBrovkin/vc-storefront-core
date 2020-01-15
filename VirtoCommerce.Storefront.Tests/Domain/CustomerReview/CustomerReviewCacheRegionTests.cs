using System;
using System.Threading;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Primitives;
using VirtoCommerce.Storefront.Domain.CustomerReview;
using Xunit;

namespace VirtoCommerce.Storefront.Tests.Domain.CustomerReview
{
    public class CustomerReviewCacheRegionTests
    {
        private readonly Fixture randomizer;
        private readonly CustomerReviewCacheRegion reviewCacheRegion;

        public CustomerReviewCacheRegionTests()
        {
            randomizer = new Fixture();
            reviewCacheRegion = new CustomerReviewCacheRegion();
        }

        [Fact]
        public void CreateCustomerCustomerReviewChangeToken_ShouldThrowException_IfCustomerIdIsNull()
        {
            //arrange
            string customerId = null;

            //act
            Action act = () => CustomerReviewCacheRegion.CreateCustomerCustomerReviewChangeToken(customerId);

            //assert
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be(nameof(customerId));
        }

        [Fact]
        public void CreateCustomerCustomerReviewChangeToken_ShouldReturnCompositeChangeToken_IfCustomerIdIsNotNull()
        {
            //arrange
            var customerId = randomizer.Create<string>();

            //act
            var result = CustomerReviewCacheRegion.CreateCustomerCustomerReviewChangeToken(customerId);

            //assert
            result.Should().BeEquivalentTo(
                new CompositeChangeToken(new[]
                {
                    new CancellationChangeToken((new CancellationTokenSource()).Token),
                    new CancellationChangeToken((new CancellationTokenSource()).Token)
                }));
        }
    }
}
