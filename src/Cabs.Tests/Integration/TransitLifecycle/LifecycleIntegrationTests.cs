using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cabs.Tests.Common;
using FluentAssertions;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NodaTime;
using Xunit;

namespace Cabs.Tests.Integration.TransitLifecycle;

public class LifecycleIntegrationTests : IAsyncLifetime
{
    private readonly CabsApp _app;
    
    private ITransitService TransitService => _app.TransitService;
    private IDriverSessionService DriverSessionService => _app.DriverSessionService;
    private IDriverTrackingService DriverTrackingService => _app.DriverTrackingService;
    private Fixtures Fixtures => _app.Fixtures;
    private Mock<IGeocodingService> _geocodingService;
    private Mock<IClock> _clock; 

    private Client _client;
    
    public LifecycleIntegrationTests()
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
    
  [Fact]
  public async Task CannotChangeDestinationWhenTransitIsCompleted()
  { 
      // Arrange
    var addresses = await GenerateAddresses(3);
    var transit = await CreateTransit(addresses[0], addresses[1]);
    
    var driver = await ANearbyDriver("WU1212");
    await TransitService.PublishTransit(transit.Id);
    await TransitService.AcceptTransit(driver, transit.Id);
    await TransitService.StartTransit(driver, transit.Id);
    await TransitService.CompleteTransit(driver, transit.Id, addresses[1]);

    // Act & Assert
    await TransitService.Awaiting(s => s.ChangeTransitAddressTo(transit.Id,
        addresses[2]))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Fact]
  public async Task CannotChangePickupPlaceAfterTransitIsAccepted()
  {
      // Arrange
      var addresses = await GenerateAddresses(3);
      var transit = await CreateTransit(addresses[0], addresses[1]);

    var driver = await ANearbyDriver("WU1212");
    await TransitService.PublishTransit(transit.Id);
    await TransitService.AcceptTransit(driver, transit.Id);

    // Act & Assert 
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id, addresses[2]))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await TransitService.StartTransit(driver, transit.Id);
    // Act & Assert
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id, addresses[2]))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await TransitService.CompleteTransit(driver, transit.Id, addresses[1]);
    // Act & Assert
    await TransitService.Awaiting(s => s.ChangeTransitAddressFrom(transit.Id, addresses[2]))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }
  
  [Fact]
  public async Task CannotCancelTransitAfterItWasStarted()
  {
      // Arrange
      var addresses = await GenerateAddresses(2);
      var transit = await CreateTransit(addresses[0], addresses[1]);
 
    var driver = await ANearbyDriver("WU1212");

    await TransitService.PublishTransit(transit.Id);

    await TransitService.AcceptTransit(driver, transit.Id);

    await TransitService.StartTransit(driver, transit.Id);
    // Act & Assert
    await TransitService.Awaiting(s => s.CancelTransit(transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();

    //and
    await TransitService.CompleteTransit(driver, transit.Id, addresses[1]);
    // Act & Assert
    await TransitService.Awaiting(s => s.CancelTransit(transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }
  
  [Fact]
  public async Task CanAcceptTransit()
  {
    //given
    var transit = await CreateTransit(new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //when
    await TransitService.AcceptTransit(driver, transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.Equal(Transit.Statuses.TransitToPassenger, loaded.Status);
    Assert.NotNull(loaded.AcceptedAt);
  }

  [Fact]
  public async Task OnlyOneDriverCanAcceptTransit()
  {
    //given
    var transit = await CreateTransit(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    var secondDriver = await ANearbyDriver("DW MARIO");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);

    //expect
    await TransitService.Awaiting(s => s.AcceptTransit(secondDriver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Fact]
  public async Task TransitCannotBeAcceptedByDriverWhoAlreadyRejected()
  {
    //given
    var transit = await CreateTransit(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //and
    await TransitService.RejectTransit(driver, transit.Id);

    //expect
    await TransitService.Awaiting(s => s.AcceptTransit(driver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Fact]
  public async Task TransitCannotBeAcceptedByDriverWhoHasNotSeenProposal()
  {
    //given
    var transit = await CreateTransit(new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var farAwayDriver = await AFarAwayDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //expect
    await TransitService.Awaiting(s => s.AcceptTransit(farAwayDriver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Fact]
  public async Task CanStartTransit()
  {
    //given
    var transit = await CreateTransit(new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);
    //when
    await TransitService.StartTransit(driver, transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.Equal(Transit.Statuses.InTransit, loaded.Status);
    Assert.NotNull(loaded.Started);
  }

  [Fact]
  public async Task CannotStartNotAcceptedTransit()
  {
    //given
    var transit = await CreateTransit();
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //expect
    await TransitService.Awaiting(s => s.StartTransit(driver, transit.Id))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
  }

  [Fact]
  public async Task CanCompleteTransit()
  {
    // Arrange
    var addresses = await GenerateAddresses(2);
    var transit = await CreateTransit(addresses[0], addresses[1]);

    var driver = await ANearbyDriver("WU1212");

    await TransitService.PublishTransit(transit.Id);

    await TransitService.AcceptTransit(driver, transit.Id);

    await TransitService.StartTransit(driver, transit.Id);

    // Act
    await TransitService.CompleteTransit(driver, transit.Id, addresses[1]);

    // Assert
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.Equal(Transit.Statuses.Completed, loaded.Status);
    Assert.NotNull(loaded.Tariff);
    Assert.NotNull(loaded.Price);
    Assert.NotNull(loaded.DriverFee);
    Assert.NotNull(loaded.CompleteAt);
  }

  [Fact]
  public async Task CannotCompleteNotStartedTransit()
  {
    //given
    var addressTo = new AddressDto("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = await CreateTransit(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      addressTo);
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);
    //and
    await TransitService.AcceptTransit(driver, transit.Id);

    //expect
    await TransitService.Awaiting(s => s.CompleteTransit(driver, transit.Id, addressTo))
      .Should().ThrowExactlyAsync<ArgumentException>();
  }

  [Fact]
  public async Task CanRejectTransit()
  {
    //given
    var transit = await CreateTransit(
      new AddressDto("Polska", "Warszawa", "Młynarska", 20),
      new AddressDto("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = await ANearbyDriver("WU1212");
    //and
    await TransitService.PublishTransit(transit.Id);

    //when
    await TransitService.RejectTransit(driver, transit.Id);

    //then
    var loaded = await TransitService.LoadTransit(transit.Id);
    Assert.Equal(Transit.Statuses.WaitingForDriverAssignment, loaded.Status);
    Assert.Null(loaded.AcceptedAt);
  }
    
  private async Task<long?> ANearbyDriver(string plateNumber)
    {
        var driver = await Fixtures.ADriver();
        await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
        await DriverSessionService.LogIn(driver.Id, plateNumber, CarType.CarClasses.Van, "BRAND");
        await DriverTrackingService.RegisterPosition(driver.Id, 1, 1);
        return driver.Id;
    }

  private async Task<long?> AFarAwayDriver(string plateNumber)
  {
      var driver = await Fixtures.ADriver();
      await Fixtures.DriverHasFee(driver, DriverFee.FeeTypes.Flat, 10);
      await DriverSessionService.LogIn(driver.Id, plateNumber, CarType.CarClasses.Van, "BRAND");
      await DriverTrackingService.RegisterPosition(driver.Id, 1000, 1000);
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

    private TransitDto TransitDto(AddressDto from, AddressDto to)
    {
        var dto = new TransitDto
        {
            From = from,
            To = to,
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

    private Task<Transit> CreateTransit(AddressDto from, AddressDto to)
        => TransitService.CreateTransit(TransitDto(from, to));
    
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
  
    public Task DisposeAsync() => _app.DisposeAsync().AsTask();
}