using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface IDriverSessionService
{
    Task<DriverSession> LogIn(long? driverId, string plateNumber, CarType.CarClasses? carClass, string carBrand);
    Task LogOut(long sessionId);
    Task LogOutCurrentSession(long? driverId);
    Task<List<DriverSession>> FindByDriver(long? driverId);
}