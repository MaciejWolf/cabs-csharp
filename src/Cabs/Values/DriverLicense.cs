using System.Text.RegularExpressions;

namespace LegacyFighter.Cabs.Values;

public record DriverLicense
{
    private const string DriverLicenseRegex = "^[A-Z9]{5}\\d{6}[A-Z9]{2}\\d[A-Z]{2}$";

    public string ValueAsString { get; private set; }

    private DriverLicense(string value)
    {
        ValueAsString = value;
    }

    protected DriverLicense()
    {
        // For EF
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
}
