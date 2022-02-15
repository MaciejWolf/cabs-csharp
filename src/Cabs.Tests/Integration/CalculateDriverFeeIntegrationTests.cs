using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Values;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Integration;

public class CalculateDriverFeeIntegrationTests : IAsyncLifetime
{
    private readonly CabsApp _app = CabsApp.CreateInstance();

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    [Fact]
    public async Task WhenTransitHasSpecifiedFee_ResultIsTransitFee()
    {
        // Arrange
        var driver = await ADriver();
        var transit = await TransitWithFee(driver, 50);

        // Act
        var calculatedFee = await CalculateDriverFee(transit);

        // Assert
        calculatedFee.Should().Be(Money.OfValue(50));
    }

    [Theory]
    [InlineData(50, 90, 40)]
    [InlineData(30, 90, 60)]
    [InlineData(9, 100, 91)]
    [InlineData(100, 10, 0)]
    public async Task ShouldCalculateFlatFee(int driverFee, int transitPrice, int expectedFee)
    {
        // Arrange
        var driver = await ADriver();
        await DriverHasFee(driver, DriverFee.FeeTypes.Flat, driverFee);
        var transit = await TransitWithPrice(driver, transitPrice);

        // Act
        var calculatedFee = await CalculateDriverFee(transit);

        // Assert
        calculatedFee.Should().Be(Money.OfValue(expectedFee));
    }

    [Theory]
    [InlineData(50, 60, 30)]
    [InlineData(25, 300, 75)]
    [InlineData(45, 200, 90)]
    [InlineData(150, 20, 30)]
    public async Task ShouldCalculatePercentageFee(int driverFee, int transitPrice, int expectedFee)
    {
        // Arrange
        var driver = await ADriver();
        await DriverHasFee(driver, DriverFee.FeeTypes.Percentage, driverFee);
        var transit = await TransitWithPrice(driver, transitPrice);

        // Act
        var calculatedFee = await CalculateDriverFee(transit);

        // Assert
        calculatedFee.Should().Be(Money.OfValue(expectedFee));
    }

    [Theory]
    [InlineData(3, 1, 5, 5)]
    [InlineData(7, 1, 5, 6)]
    public async Task WhenCalculatedFeeIsLowerThanMinimalDriverFee_ShouldUseMinimalDriverFee(
        int transitPrice,
        int driverFee,
        int minimalFee,
        int expectedFee)
    {
        // Arrange
        var driver = await ADriver();
        await CreateDriverFee(driver, DriverFee.FeeTypes.Flat, driverFee, minimalFee);
        var transit = await TransitWithPrice(driver, transitPrice);
        
        // Act
        var calculatedFee = await CalculateDriverFee(transit);

        // Assert
        calculatedFee.Should().Be(Money.OfValue(expectedFee));
    }

    private Task<Money> CalculateDriverFee(Transit transit)
        => _app.DriverFeeService.CalculateDriverFee(transit.Id);

    private Task<Driver> ADriver() 
        => _app.DriverService.CreateDriver(
            "FARME100165AB5EW",
            "Kowalski",
            "Jan",
            Driver.Types.Regular,
            Driver.Statuses.Active,
            "xxx");

    private Task<DriverFee> DriverHasFee(
        Driver driver,
        DriverFee.FeeTypes feeType,
        int amount) => CreateDriverFee(driver, feeType, amount, 0);

    private Task<DriverFee> CreateDriverFee(
        Driver driver, 
        DriverFee.FeeTypes feeType,
        int amount,
        int min)
    {
        var fee = new DriverFee(feeType, driver, amount, Money.OfValue(min));
        return _app.DriverFeeRepository.Save(fee);
    }

    private Task<Transit> TransitWithFee(Driver driver, int fee) 
        => _app.TransitRepository.Save(new Transit
        {
            Driver = driver,
            DriversFee = Money.OfValue(fee)
        });

    private Task<Transit> TransitWithPrice(Driver driver, int price)
        => _app.TransitRepository.Save(new Transit 
        { 
            Driver = driver,
            Price = Money.OfValue(price)
        });
}
