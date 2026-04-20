namespace Facilities.Domain.ValueObjects;

public sealed record ImageUrl
{
    public string Value { get; } = null!;

    private ImageUrl() { }

    private ImageUrl(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Image URL cannot be empty.", nameof(value));
        }

        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
        {
            throw new ArgumentException("Invalid URI format.", nameof(value));
        }

        Value = value;
    }

    public static ImageUrl Create(string value) => new(value);

    public override string ToString() => Value;

    public static implicit operator string(ImageUrl url) => url.Value;
    public static implicit operator ImageUrl(string value) => new(value);
}