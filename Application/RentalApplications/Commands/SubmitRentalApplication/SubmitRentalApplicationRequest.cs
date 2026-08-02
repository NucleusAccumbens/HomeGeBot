using Application.Common.Results;
using Domain.Common;
using Domain.Enums;
using MediatR;

namespace Application.RentalApplications.Commands.SubmitRentalApplication;

public class SubmitRentalApplicationRequest : IRequest<Result<SubmitRentalApplicationResult>>
{
    public ChatId ChatId { get; set; }

    public Country Country { get; set; }

    public string? CountryOther { get; set; }

    public string Profession { get; set; } = "";

    public bool HasPets { get; set; }

    public Term Term { get; set; }

    public string? TermOther { get; set; }
}
