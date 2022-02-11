using System.Text.RegularExpressions;

namespace LegacyFighter.Cabs.Entity;

public record DriverLicense
{
    private const string DriverLicenseRegex = "^[A-Z9]{5}\\d{6}[A-Z9]{2}\\d[A-Z]{2}$";

    private readonly string value;

    private DriverLicense(string value)
    {
        this.value = value;
    }

    public static DriverLicense WithLicense(string driverLicense)
    {
        if (!IsValid(driverLicense))
            throw new ArgumentException("Illegal license no = " + driverLicense);

        return new DriverLicense(driverLicense);
    }

    public static DriverLicense WithoutValidation(string driverLicense) => new(driverLicense);

    private static bool IsValid(string license) 
        => license is not null && license.Any() && Regex.IsMatch(license, DriverLicenseRegex);

    public string AsString() => value;
}
