using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Integration;

public class ValidateDriverLicenseIntegrationTests : IAsyncLifetime
{
    private CabsApp _app = default!;

    public Task InitializeAsync()
    {
        _app = CabsApp.CreateInstance();
        return Task.CompletedTask;
    }

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public async Task CannotCreateActiveDriverWithInvalidLicense(string license)
    {
        var act = async () => await CreateActiveDriver(license);
        await act.Should().ThrowExactlyAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public async Task CanCreateInactiveDriverWithInvalidLicense(string license)
    {
        var driver = await CreateInactiveDriver(license);
        var dto = await Load(driver);

        dto.DriverLicense.Should().Be(license);
        dto.Status.Should().Be(Driver.Statuses.Inactive);
    }

    [Fact]
    public async Task CannotCreateInactiveDriverWithNullLicense()
    {
        var act = async () => await CreateInactiveDriver(null!);
        await act.Should().ThrowAsync<Exception>();
    }

    [Theory]
    [InlineData(Driver.Statuses.Active)]
    [InlineData(Driver.Statuses.Inactive)]
    public async Task CanCreateDriverWithValidLicense(Driver.Statuses status)
    {
        const string ValidLicense = "FARME100165AB5EW";

        var driver = await CreateDriver(ValidLicense, status);
        var dto = await Load(driver);

        dto.DriverLicense.Should().Be(ValidLicense);
        dto.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public async Task CannotChangeToInvalidLicense(string license)
    {
        var driver = await CreateInactiveDriver("InitialInvalidLicense");

        var act = async () => await ChangeLicenseNumber(driver, license);

        await act.Should().ThrowExactlyAsync<ArgumentException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidLicense")]
    public async Task CannotActivateDriverWithInvalidLicense(string license)
    {
        var driver = await CreateInactiveDriver(license);
        var act = async () => await ActivateDriver(driver);
        await act.Should().ThrowExactlyAsync<InvalidOperationException>();
    }

    private Task<Driver> CreateActiveDriver(string license) 
        => CreateDriver(license, Driver.Statuses.Active);

    private Task<Driver> CreateInactiveDriver(string license)
        => CreateDriver(license, Driver.Statuses.Inactive);

    private Task<Driver> CreateDriver(string license, Driver.Statuses status)
        => _app.DriverService.CreateDriver(
            license,
            "Kowalski",
            "Jan",
            Driver.Types.Regular,
            status,
            null);

    private Task ChangeLicenseNumber(Driver driver, string newLicense)
        => _app.DriverService.ChangeLicenseNumber(newLicense, driver.Id);

    private Task ActivateDriver(Driver driver)
        => _app.DriverService.ChangeDriverStatus(driver.Id, Driver.Statuses.Active);

    private Task<DriverDto> Load(Driver driver)
        => _app.DriverService.LoadDriver(driver.Id);
}
