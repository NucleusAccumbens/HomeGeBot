using Domain.Common;

namespace Application.Dashboard.Dtos;

public class ManagerDto
{
    public ChatId ChatId { get; set; }
    public string? Username { get; set; }
    public string? Name { get; set; }
    public int ApplicationCount { get; set; }
    public bool IsSuperAdmin { get; set; }
}
