using System.ComponentModel.DataAnnotations;

namespace Domain.Enums;

public enum Country
{
    [Display(Name = "Россия")] Russia,
    [Display(Name = "Беларусь")] Belarus,
    [Display(Name = "Украина")] Ukraine,
    [Display(Name = "Другое")] Other
}
