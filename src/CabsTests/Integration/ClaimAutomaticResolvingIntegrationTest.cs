using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using TddXt.XNSubstitute;

namespace LegacyFighter.CabsTests.Integration;

public class ClaimAutomaticResolvingIntegrationTest
{
  private IClaimService ClaimService => _app.ClaimService;
  private IClientNotificationService ClientNotificationService { get; set; } = default!;
  private IDriverNotificationService DriverNotificationService { get; set; } = default!;
  private IAwardsService AwardsService { get; set; } = default!;
  private IAppProperties Properties { get; set; } = default!;
  private Fixtures Fixtures => _app.Fixtures;
  private CabsApp _app = default!;

  [SetUp]
  public void InitializeApp()
  {
    ClientNotificationService = Substitute.For<IClientNotificationService>();
    DriverNotificationService = Substitute.For<IDriverNotificationService>();
    AwardsService = Substitute.For<IAwardsService>();
    Properties = Substitute.For<IAppProperties>();

    _app = CabsApp.CreateInstance(ctx =>
    {
      ctx.AddSingleton(ClientNotificationService);
      ctx.AddSingleton(DriverNotificationService);
      ctx.AddSingleton(AwardsService);
      ctx.AddSingleton(Properties);
    });
  }

  [TearDown]
  public void DisposeOfApp()
  {
    _app.Dispose();
  }

  [Test]
  public async Task SecondClaimForTheSameTransitWillBeEscalated()
  {
    //given
    LowCostThresholdIs(40);
    //and
    var driver = await Fixtures.ADriver();
    //and
    var client = await Fixtures.AClient(Client.Types.Vip);
    //and
    var transit = await ATransit(client, driver, 39);
    //and
    var claim = await Fixtures.CreateClaim(client, transit);
    //and
    claim = await ClaimService.TryToResolveAutomatically(claim.Id);
    //and
    var claim2 = await Fixtures.CreateClaim(client, transit);

    //when
    claim2 = await ClaimService.TryToResolveAutomatically(claim2.Id);

    //then
    Assert.AreEqual(Claim.Statuses.Refunded, claim.Status);
    Assert.AreEqual(Claim.CompletionModes.Automatic, claim.CompletionMode);
    Assert.AreEqual(Claim.Statuses.Escalated, claim2.Status);
    Assert.AreEqual(Claim.CompletionModes.Manual, claim2.CompletionMode);
  }

  [Test]
  public async Task LowCostTransitsAreRefundedIfClientIsVip()
  {
    _app.StartReuseRequestScope();
    //given
    LowCostThresholdIs(40);
    //and
    var client = await Fixtures.AClientWithClaims(Client.Types.Vip, 3);
    //and
    var driver = await Fixtures.ADriver();
    //and
    var transit = await ATransit(client, driver, 39);
    //and
    var claim = await Fixtures.CreateClaim(client, transit);
    _app.EndReuseRequestScope();

    //when
    AwardsService.ClearReceivedCalls();
    ClientNotificationService.ClearReceivedCalls();
    claim = await ClaimService.TryToResolveAutomatically(claim.Id);

    //then
    Assert.AreEqual(Claim.Statuses.Refunded, claim.Status);
    Assert.AreEqual(Claim.CompletionModes.Automatic, claim.CompletionMode);
    ClientNotificationService.Received(1).NotifyClientAboutRefund(claim.ClaimNo, claim.Owner.Id);
    await AwardsService.Received(1).RegisterNonExpiringMiles(claim.Owner.Id, 10);
  }

  [Test]
  public async Task HighCostTransitsAreEscalatedEvenWhenClientIsVip()
  {
    _app.StartReuseRequestScope();
    //given
    LowCostThresholdIs(40);
    //and
    var client = await Fixtures.AClientWithClaims(Client.Types.Vip, 3);
    //and
    var driver = await Fixtures.ADriver();
    //and
    var transit = await ATransit(client, driver, 50);
    //and
    var claim = await Fixtures.CreateClaim(client, transit);
    _app.EndReuseRequestScope();

    //when
    AwardsService.ClearReceivedCalls();
    ClientNotificationService.ClearReceivedCalls();
    claim = await ClaimService.TryToResolveAutomatically(claim.Id);

    //then
    Assert.AreEqual(Claim.Statuses.Escalated, claim.Status);
    Assert.AreEqual(Claim.CompletionModes.Manual, claim.CompletionMode);
    DriverNotificationService.Received(1).AskDriverForDetailsAboutClaim(claim.ClaimNo, driver.Id);
    AwardsService.ReceivedNothing();
  }

  [Test]
  public async Task FirstThreeClaimsAreRefunded()
  {
    _app.StartReuseRequestScope();
    //given
    LowCostThresholdIs(40);
    //and
    NoOfTransitsForAutomaticRefundIs(10);
    //and
    var client = await AClient(Client.Types.Normal);
    //and
    var driver = await Fixtures.ADriver();

    //when
    var claim1 = await ClaimService.TryToResolveAutomatically((await Fixtures.CreateClaim(client, await ATransit(client, driver, 50))).Id);
    var claim2 = await ClaimService.TryToResolveAutomatically((await Fixtures.CreateClaim(client, await ATransit(client, driver, 50))).Id);
    var claim3 = await ClaimService.TryToResolveAutomatically((await Fixtures.CreateClaim(client, await ATransit(client, driver, 50))).Id);
    var claim4 = await ClaimService.TryToResolveAutomatically((await Fixtures.CreateClaim(client, await ATransit(client, driver, 50))).Id);

    //then
    Assert.AreEqual(Claim.Statuses.Refunded, claim1.Status);
    Assert.AreEqual(Claim.Statuses.Refunded, claim2.Status);
    Assert.AreEqual(Claim.Statuses.Refunded, claim3.Status);
    Assert.AreEqual(Claim.Statuses.Escalated, claim4.Status);
    Assert.AreEqual(Claim.CompletionModes.Automatic, claim1.CompletionMode);
    Assert.AreEqual(Claim.CompletionModes.Automatic, claim2.CompletionMode);
    Assert.AreEqual(Claim.CompletionModes.Automatic, claim3.CompletionMode);
    Assert.AreEqual(Claim.CompletionModes.Manual, claim4.CompletionMode);

    ClientNotificationService.Received(1).NotifyClientAboutRefund(claim1.ClaimNo, client.Id);
    ClientNotificationService.Received(1).NotifyClientAboutRefund(claim2.ClaimNo, client.Id);
    ClientNotificationService.Received(1).NotifyClientAboutRefund(claim3.ClaimNo, client.Id);
    AwardsService.ReceivedNothing();
    _app.EndReuseRequestScope();
  }

  [Test]
  public async Task LowCostTransitsAreRefundedWhenManyTransits()
  {
    _app.StartReuseRequestScope();
    //given
    LowCostThresholdIs(40);
    //and
    NoOfTransitsForAutomaticRefundIs(10);
    //and
    var client = await Fixtures.AClientWithClaims(Client.Types.Normal, 3);
    //and
    await Fixtures.ClientHasDoneTransits(client, 12);
    //and
    var transit = await ATransit(client, await Fixtures.ADriver(), 39);
    //and
    var claim = await Fixtures.CreateClaim(client, transit);
    _app.EndReuseRequestScope();

    //when
    claim = await ClaimService.TryToResolveAutomatically(claim.Id);

    //then
    Assert.AreEqual(Claim.Statuses.Refunded, claim.Status);
    Assert.AreEqual(Claim.CompletionModes.Automatic, claim.CompletionMode);
    ClientNotificationService.Received(1).NotifyClientAboutRefund(claim.ClaimNo, client.Id);
    AwardsService.ReceivedNothing();
  }

  [Test]
  public async Task HighCostTransitsAreEscalatedEvenWithManyTransits()
  {
    _app.StartReuseRequestScope();
    //given
    LowCostThresholdIs(40);
    //and
    NoOfTransitsForAutomaticRefundIs(10);
    //and
    var client = await Fixtures.AClientWithClaims(Client.Types.Normal, 3);
    //and
    await Fixtures.ClientHasDoneTransits(client, 12);
    //and
    var claim = await Fixtures.CreateClaim(client, await ATransit(client, await Fixtures.ADriver(), 50));
    _app.EndReuseRequestScope();

    //when
    claim = await ClaimService.TryToResolveAutomatically(claim.Id);

    //then
    Assert.AreEqual(Claim.Statuses.Escalated, claim.Status);
    Assert.AreEqual(Claim.CompletionModes.Manual, claim.CompletionMode);
    ClientNotificationService.Received(1).AskForMoreInformation(claim.ClaimNo, client.Id);
    AwardsService.ReceivedNothing();
  }

  [Test]
  public async Task HighCostTransitsAreEscalatedWhenFewTransits()
  {
    _app.StartReuseRequestScope();
    //given
    LowCostThresholdIs(40);
    //and
    NoOfTransitsForAutomaticRefundIs(10);
    //and
    var client = await Fixtures.AClientWithClaims(Client.Types.Normal, 3);
    //and
    await Fixtures.ClientHasDoneTransits(client, 2);
    //and
    var driver = await Fixtures.ADriver();
    //and
    var claim = await Fixtures.CreateClaim(client, await ATransit(client, driver, 50));
    _app.EndReuseRequestScope();

    //when
    claim = await ClaimService.TryToResolveAutomatically(claim.Id);

    //then
    Assert.AreEqual(Claim.Statuses.Escalated, claim.Status);
    Assert.AreEqual(Claim.CompletionModes.Manual, claim.CompletionMode);
    DriverNotificationService.Received(1).AskDriverForDetailsAboutClaim(claim.ClaimNo, driver.Id);
    AwardsService.ReceivedNothing();
  }

  private async Task<Transit> ATransit(Client client, Driver driver, int price) 
  {
    return await Fixtures.ACompletedTransitAt(price, SystemClock.Instance.GetCurrentInstant(), client, driver);
  }

  private void LowCostThresholdIs(int price)
  {
    Properties.AutomaticRefundForVipThreshold.Returns(price);
  }

  private void NoOfTransitsForAutomaticRefundIs(int no)
  {
    Properties.NoOfTransitsForClaimAutomaticRefund.Returns(no);
  }

  private async Task<Client> AClient(Client.Types type) 
  {
    return await Fixtures.AClientWithClaims(type, 0);
  }
}