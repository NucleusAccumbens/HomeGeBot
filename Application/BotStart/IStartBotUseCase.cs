namespace Application.BotStart;

public interface IStartBotUseCase
{
    Task<StartBotResult> ExecuteAsync(StartBotRequest request);
}
