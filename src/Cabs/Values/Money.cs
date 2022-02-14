namespace LegacyFighter.Cabs.Values;

public record Money
{
    public static Money Zero => new(0);

    private readonly int _value;

    public int IntValue => _value;

    private Money(int value)
    {
        _value = value;
    }

    public static Money OfValue(int value) => new(value);

    public Money Percentage(int percentage) => OfValue((int)Math.Round(percentage * _value / 100.0));

    public static Money operator +(Money money, Money other) => new(money._value + other._value);

    public static Money operator -(Money money, Money other) => new(money._value - other._value);
}
