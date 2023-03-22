using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;


public class Client : BaseAuditableEntity
{
    public long ChatId { get; set; }
    public Country? Country { get; set; }
    public string? Profession { get; set; }
    public bool? HasPets { get; set; }
    public Term? Term { get; set; }
    public long AdminChatId { get; set; }
}
