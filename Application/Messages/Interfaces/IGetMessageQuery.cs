namespace Application.Messages.Interfaces;

public interface IGetMessageQuery
{
    Task<string?> GetMessageBodyAsync(string messageName);

    Task<string?> GetMessagePathToPhotoAsync(string messageName);

    Task<Message?> GetMessageAsync(string messageName);
}
