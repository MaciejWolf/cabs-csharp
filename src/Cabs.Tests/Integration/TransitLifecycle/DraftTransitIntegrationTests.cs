using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Cabs.Tests.Integration.TransitLifecycle;

public class DraftTransitIntegrationTests : IAsyncLifetime
{
    private readonly CabsApp _app;

    private ITransitService TransitService => _app.TransitService;
    
    private IDriverSessionService DriverSessionService => _app.DriverSessionService;

    private IDriverTrackingService DriverTrackingService => _app.DriverTrackingService;

    private Fixtures Fixtures => _app.Fixtures;

    private Mock<IGeocodingService> _geocodingService;
    private Mock<IClock> _clock; 

    private Client _client;

    public DraftTransitIntegrationTests()
    {
        _geocodingService = SetupGeocodingServiceMock();
        _clock = SetupClockMock();
        
        _app = CabsApp.CreateInstance(services =>
        {
            services.AddSingleton(_geocodingService.Object);
            services.AddSingleton(_clock.Object);
        });
    }

    private static Mock<IGeocodingService> SetupGeocodingServiceMock()
    {
        var geocodingService = new Mock<IGeocodingService>();

        geocodingService
            .Setup(x => x.GeocodeAddress(It.IsAny<Address>()))
            .Returns(new[] { 1d, 1d });

        return geocodingService;
    }

    private static Mock<IClock> SetupClockMock()
    {
        return new Mock<IClock>();
    }

    public async Task InitializeAsync()
    {
        _client = await Fixtures.AClient();
        await Fixtures.AnActiveCarCategory(CarType.CarClasses.Van);
    }

    public Task DisposeAsync() => _app.DisposeAsync().AsTask();

    [Fact]
    public async Task ShouldCreateTransitFromDto()
    {
        // Arrange
        var addresses = await GenerateAddresses(2);
        var dto = TransitDto(addresses[0], addresses[1]);

        // Act
        var transit = await TransitService.CreateTransit(dto);

        // Assert
        var loaded = await LoadTransit();
        loaded.Should().BeEquivalentTo(new TransitDto(transit));
    }

    [Fact]
    public async Task ShouldChangeTransitPickupAddress()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        await CreateTransit(addresses[0], addresses[1]);

        // Act
        await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        var loaded = await LoadTransit();
        loaded.From.Should().BeEquivalentTo(new AddressDto(addresses[2]));
    }

    [Fact]
    public async Task CannotChangePickupAddressThreeTimes()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        var transit = await CreateTransit(addresses[0], addresses[1]);

        await ChangeTransitAddressFrom(addresses[2]);
        await ChangeTransitAddressFrom(addresses[1]);
        await ChangeTransitAddressFrom(addresses[2]);

        // Act
        var act = async () => await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>("Address 'from' cannot be changed, id = " + transit.Id);
    }

    [Fact]
    public async Task CannotChangePickupAddressTooFarAway()
    {
        // Arrange
        var addresses = await GenerateAddresses(3);
        AddressIsFarAway(addresses[2]);

        var transit = await CreateTransit(addresses[0], addresses[1]);

        // Act
        var act = async () => await ChangeTransitAddressFrom(addresses[2]);

        // Assert
        await act.Should().ThrowExactlyAsync<InvalidOperationException>("Address 'from' cannot be changed, id = " + transit.Id);
    }

    [Fact]
    public async Task ShouldCancelTransit()
    {
        // Arrange
        await CreateTransit();

        // Act
        await CancelTransit();
        
        // Assert
        var loaded = await LoadTransit();
        loaded.Status.Should().Be(Transit.Statuses.Cancelled);
        loaded.Driver.Should().BeNull();
        loaded.GetDistance("km").Should().Be("0km");
    }

    [Fact]
    public async Task WhenPublishedWithNoNearbyDrivers_ShouldHaveStatusDriverAssignmentFailed()
    {
        // Arrange
        await CreateTransit();
        var time = GenerateDatesAsInstants(1).Single();

        SetTime(time);

        // Act
        await PublishTransit();

        // Assert
        var loaded = await LoadTransit();
        loaded.Status.Should().Be(Transit.Statuses.DriverAssignmentFailed);
        loaded.Published.Should().Be(time);
    }

    [Fact]
    public async Task CanPublishTransit()
    {
        // Arrange
        await CreateTransit();

        await ANearbyDriver("ASD123");
        await ANearbyDriver("ASD124");

        var time = GenerateDatesAsInstants(1).Single();

        SetTime(time);

        // Act
        await PublishTransit();

        // Assert
        var loaded = await LoadTransit();
        loaded.Status.Should().Be(Transit.Statuses.WaitingForDriverAssignment);
        loaded.Published.Should().Be(time);
    }

    private async Task<long?> ANearbyDriver(string plateNumber)
    {
        var driver = await Fixtures.ADriver();
        await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
        await DriverSessionService.LogIn(driver.Id, plateNumber, CarType.CarClasses.Van, "BRAND");
        await DriverTrackingService.RegisterPosition(driver.Id, 1, 1);
        return driver.Id;
    }

    private TransitDto TransitDto(Address from, Address to)
    {
        var dto = new TransitDto
        {
            From = new AddressDto(from),
            To = new AddressDto(to),
            ClientDto = new ClientDto
            {
                Id = _client.Id
            }
        };

        return dto;
    }

    private void SetTime(Instant instant)
    {
        _clock.Setup(x => x.GetCurrentInstant()).Returns(instant);
    }

    private async Task<Transit> CreateTransit()
    {
        var addresses = await GenerateAddresses(2);
        return await CreateTransit(addresses[0], addresses[1]);
    }

    private Task<Transit> CreateTransit(Address from, Address to)
        => TransitService.CreateTransit(TransitDto(from, to));

    private Task ChangeTransitAddressFrom(Address address)
        => TransitService.ChangeTransitAddressFrom(_client.Id, address);

    private Task CancelTransit()
    => TransitService.CancelTransit(_client.Id);

    private Task PublishTransit()
    => TransitService.PublishTransit(_client.Id);

    private Task<TransitDto> LoadTransit()
        => TransitService.LoadTransit(_client.Id);

    private Task<Address[]> GenerateAddresses(int count) 
        => Task.WhenAll(GenerateAddressTasks(count));

    private IEnumerable<Task<Address>> GenerateAddressTasks(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            yield return Fixtures.AnAddress("PL", "Warsaw", "Dolna", i);
        }
    }

    private static IEnumerable<Instant> GenerateDatesAsInstants(int count)
    {
        for (var i = 1; i <= count; i++)
        {
            var time = new DateTime(2020, 1, 1, 15, 30, 00, DateTimeKind.Utc).AddDays(i);
            yield return Instant.FromDateTimeUtc(time);
        }
    }

    private void AddressIsFarAway(Address address)
    {
        _geocodingService
            .Setup(x => x.GeocodeAddress(It.Is<Address>(a => a == address)))
            .Returns(new[] { 1000d, 1000d });
    }
}
