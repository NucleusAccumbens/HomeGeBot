namespace Domain.Common;

public record struct ChatId(long Value) : IComparable<ChatId>, IComparable
{
    public static ChatId FromLong(long value) => new(value);
    public long ToLong() => Value;

    public static implicit operator long(ChatId chatId) => chatId.Value;
    public static implicit operator ChatId(long value) => new(value);

    public override string ToString() => Value.ToString();

    public int CompareTo(ChatId other) => Value.CompareTo(other.Value);

    public int CompareTo(object? obj)
    {
        if (obj is null) return 1;
        if (obj is ChatId other) return CompareTo(other);
        throw new ArgumentException("Object must be of type ChatId");
    }
}
