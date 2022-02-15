namespace LegacyFighter.Cabs.Values;

public record Money
{
    public static Money Zero => new(0);

    public int IntValue { get; }

    private Money(int value)
    {
        IntValue = value;
    }

    protected Money()
    {
        // for EF
    }

    public static Money OfValue(int value) => new(value);

    public Money Percentage(int percentage) => OfValue((int)Math.Round(percentage * IntValue / 100.0));

    public static Money operator +(Money money, Money other) => OfValue(money.IntValue + other.IntValue);

    public static Money operator -(Money money, Money other) => OfValue(money.IntValue - other.IntValue);
}
