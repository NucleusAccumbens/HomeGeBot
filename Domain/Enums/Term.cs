using System.ComponentModel.DataAnnotations;

namespace Domain.Enums;

public enum Term
{
    [Display(Name = "6 месяцев")] SixMonths,
    [Display(Name = "1 год")] OneYear,
    [Display(Name = "Другое")] Other
}
