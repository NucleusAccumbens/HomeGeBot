using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;


public class Client : BaseAuditableEntity
{
    public ChatId ChatId { get; set; }
    public Country? Country { get; set; }
    public string? CountryOther { get; set; }
    public string? Profession { get; set; }
    public bool? HasPets { get; set; }
    public Term? Term { get; set; }
    public string? TermOther { get; set; }
    public ChatId AdminChatId { get; set; }
    public bool IsCompleted { get; set; }
}
