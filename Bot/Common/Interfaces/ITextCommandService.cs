namespace Bot.Common.Interfaces;

public interface ITextCommandService
{
    bool CheckMessageIsCommand(string message);

    string CheckStringLessThan500(string message);
}
