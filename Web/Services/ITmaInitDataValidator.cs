namespace Web.Services;

public interface ITmaInitDataValidator
{
    bool Validate(IReadOnlyDictionary<string, string> data);
}
