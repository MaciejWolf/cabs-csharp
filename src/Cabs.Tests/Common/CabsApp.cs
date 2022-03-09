﻿using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Cabs.Tests.Common;

internal class CabsApp : WebApplicationFactory<Program>
{
    private IServiceScope _scope;

    private CabsApp()
    {
        _scope = base.Services.CreateAsyncScope();
    }
    public static CabsApp CreateInstance() => new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(collection => collection.AddTransient<Fixtures>());
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
}
