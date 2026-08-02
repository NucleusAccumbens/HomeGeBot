using Application.AdminManagement.Dtos;

namespace Application.AdminManagement.Commands.GetBotUsers;

public class GetBotUsersResult
{
    public static GetBotUsersResult Success(List<BotUserDto> users) => new() { Users = users };

    public List<BotUserDto> Users { get; set; } = new();
}
