using System.Linq;
using AutoFixture;
using FluentAssertions;
using VirtoCommerce.Storefront.Extensions;
using Xunit;

namespace VirtoCommerce.Storefront.Tests.Extensions
{
    public class ListExtensionsTests
    {
        private readonly Fixture randomizer;
        public ListExtensionsTests()
        {
            randomizer = new Fixture();
        }

        [Fact]
        public void AddIf_ShouldAddNewItemIntoSource_IfConditionIsTrue()
        {
            //arrange
            var sourceList = randomizer.CreateMany<TestClass>(1).ToList();
            var condition = true;
            var newItem = randomizer.Create<TestClass>();

            //act
            var result = sourceList.AddIf(condition, () => newItem);

            //assert
            result.Should().BeEquivalentTo(new[]
            {
                new TestClass
                {
                    Id = sourceList[0].Id,
                    Name = sourceList[0].Name
                },
                new TestClass
                {
                    Id = newItem.Id,
                    Name = newItem.Name
                }
            });
        }

        [Fact]
        public void AddIf_ShouldNotAddNewItemIntoSource_IfConditionIsFalse()
        {
            //arrange
            var sourceList = randomizer.CreateMany<TestClass>(1).ToList();
            var condition = false;
            var newItem = randomizer.Create<TestClass>();

            //act
            var result = sourceList.AddIf(condition, () => newItem);

            //assert
            result.Should().BeEquivalentTo(new[]
            {
                new TestClass
                {
                    Id = sourceList[0].Id,
                    Name = sourceList[0].Name
                }
            });
        }

        private class TestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
