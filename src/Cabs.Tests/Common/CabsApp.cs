using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Cabs.Tests.Common;

internal class CabsApp : WebApplicationFactory<Program>
{
    private IServiceScope _scope;
    Action<IServiceCollection> _customization;

    private CabsApp(Action<IServiceCollection> customization)
    {
        _customization = customization;
        _scope = base.Services.CreateAsyncScope();
    }

    public static CabsApp CreateInstance() => new(_ => { });

    public static CabsApp CreateInstance(Action<IServiceCollection> customization) => new(customization);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(collection => collection.AddTransient<Fixtures>());
        builder.ConfigureServices(_customization);
    }

    protected override void Dispose(bool disposing)
    {
        _scope.Dispose();
        base.Dispose(disposing);
    }
    private IServiceScope NewRequestScope()
    {
        _scope.Dispose();
        _scope = Services.CreateAsyncScope();
        return _scope;
    }

    public Fixtures Fixtures
    => NewRequestScope().ServiceProvider.GetRequiredService<Fixtures>();

    public IDriverService DriverService
      => NewRequestScope().ServiceProvider.GetRequiredService<IDriverService>();

    public IDriverFeeService DriverFeeService
        => NewRequestScope().ServiceProvider.GetRequiredService<IDriverFeeService>();

    public ITransitService TransitService
        => NewRequestScope().ServiceProvider.GetRequiredService<ITransitService>();

    public IDriverSessionService DriverSessionService
        => NewRequestScope().ServiceProvider.GetRequiredService<IDriverSessionService>();

    public IDriverTrackingService DriverTrackingService
        => NewRequestScope().ServiceProvider.GetRequiredService<IDriverTrackingService>();
}
