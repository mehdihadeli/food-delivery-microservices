using FoodDelivery.Services.Catalogs.Products.Exceptions.Domain;
using FoodDelivery.Services.Catalogs.Products.Models.ValueObjects;
using FluentAssertions;
using Xunit;

namespace FoodDelivery.Services.Customers.UnitTests.Products;

public class StockValueObjectTests
{
    [Fact]
    public void Should_Create_Stock_When_All_Values_Are_Valid()
    {
        // Arrange & Act
        var stock = Stock.Of(available: 10, restockThreshold: 5, maxStockThreshold: 100);

        // Assert
        stock.Available.Should().Be(10);
        stock.RestockThreshold.Should().Be(5);
        stock.MaxStockThreshold.Should().Be(100);
    }

    [Fact]
    public void Should_Throw_When_Available_Exceeds_MaxStockThreshold()
    {
        // Arrange & Act
        var act = () => Stock.Of(available: 600, restockThreshold: 5, maxStockThreshold: 100);

        // Assert
        act.Should().Throw<MaxStockThresholdReachedException>()
            .WithMessage("*max stock threshold*");
    }

    [Fact]
    public void Should_Throw_When_Available_Is_Zero()
    {
        // Arrange & Act
        var act = () => Stock.Of(available: 0, restockThreshold: 5, maxStockThreshold: 100);

        // Assert
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void Should_Throw_When_RestockThreshold_Is_Zero()
    {
        // Arrange & Act
        var act = () => Stock.Of(available: 10, restockThreshold: 0, maxStockThreshold: 100);

        // Assert
        act.Should().Throw<Exception>();
    }
}
