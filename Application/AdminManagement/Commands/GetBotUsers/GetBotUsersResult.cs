using Application.AdminManagement.Dtos;

namespace Application.AdminManagement.Commands.GetBotUsers;

public class GetBotUsersResult
{
    public static GetBotUsersResult Success(List<BotUserDto> users) => new() { Users = users };

    public static GetBotUsersResult Failure(string error) => new() { ErrorMessage = error };

    public List<BotUserDto> Users { get; set; } = new();

    public string? ErrorMessage { get; set; }
}
