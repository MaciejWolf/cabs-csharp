using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using LegacyFighter.Cabs.Values;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Integration;

public class CalculateDriverPeriodicPaymentIntegrationTests : IAsyncLifetime
{
    private readonly CabsApp _app = CabsApp.CreateInstance();

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    public Task InitializeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ShouldCalculateMothlyPayment()
    {
        var driver = await ADriver();
        await CreateDriverFee(driver, DriverFee.FeeTypes.Flat, 10);

        await Transit(driver, 13, 2020, Month.September, 29);
        await Transit(driver, 21, 2020, Month.September, 30);

        await Transit(driver, 34, 2020, Month.October, 1); // 24
        await Transit(driver, 55, 2020, Month.October, 2); // + 45
        await Transit(driver, 89, 2020, Month.October, 30); // + 79
        await Transit(driver, 144, 2020, Month.October, 31); // + 134
        await Transit(driver, 233, 2020, Month.November, 1); // + 223 = 505

        await Transit(driver, 377, 2020, Month.November, 2);

        var payment = await CalculateDriverMonthlyPayment(driver, 2020, Month.October);

        payment.Should().Be(Money.OfValue(505));
    }

    [Fact]
    public async Task ShouldCalculateYearlyPayment()
    {
        var driver = await ADriver();
        await CreateDriverFee(driver, DriverFee.FeeTypes.Flat, 10);

        await Transit(driver, 13, 2019, Month.December, 30);
        await Transit(driver, 21, 2019, Month.December, 31);

        await Transit(driver, 34, 2020, Month.January, 1); // 24
        await Transit(driver, 55, 2020, Month.January, 2); // + 45 = 69
        await Transit(driver, 89, 2020, Month.December, 30); // 79
        await Transit(driver, 144, 2020, Month.December, 31); // + 134

        await Transit(driver, 233, 2021, Month.January, 1); //  + 223 = 436
        await Transit(driver, 377, 2021, Month.January, 2);

        var payments = await CalculateDriverYearlyPayment(driver, 2020);

        payments[Month.January].Should().Be(Money.OfValue(69));
        payments[Month.December].Should().Be(Money.OfValue(436));

        payments
            .ExceptBy(new[] { Month.January, Month.December }, kvp => kvp.Key)
            .Select(kvp => kvp.Value)
            .Should()
            .OnlyContain(v => v == Money.Zero);
    }

    private Task<Money> CalculateDriverMonthlyPayment(Driver driver, int year, Month month) 
        => _app.DriverService.CalculateDriverMonthlyPayment(driver.Id, year, month.Value);

    private Task<Dictionary<Month, Money>> CalculateDriverYearlyPayment(Driver driver, int year)
        => _app.DriverService.CalculateDriverYearlyPayment(driver.Id, year);

    private Task<Driver> ADriver()
        => _app.DriverService.CreateDriver(
            "FARME100165AB5EW",
            "Kowalski",
            "Jan",
            Driver.Types.Regular,
            Driver.Statuses.Active,
            "xxx");

    private Task<DriverFee> CreateDriverFee(
        Driver driver,
        DriverFee.FeeTypes feeType,
        int amount)
    {
        var fee = new DriverFee(feeType, driver, amount, 0);
        return _app.DriverFeeRepository.Save(fee);
    }

    private Task<Transit> Transit(Driver driver, int price, int year, Month month, int day)
        => _app.TransitRepository.Save(new Transit
        {
            Driver = driver,
            Price = Money.OfValue(price),
            DateTime = new LocalDate(year, month.Value, day)
            .AtStartOfDayInZone(DateTimeZoneProviders.Bcl.GetSystemDefault())
            .ToInstant()
        });
}
