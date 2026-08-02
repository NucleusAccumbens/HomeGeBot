using Application.Messages.Queries.GetMessageBody;
using Application.Users.Queries.GetUserLanguage;
using Bot.Services;
using Bot.Session;
using Domain.Common;
using MediatR;

namespace Bot.Services;

public interface IManagerNotificationFormatter
{
    Task<(string body, string writeButtonText)> FormatAsync(RentalApplicationDraft draft, long managerChatId, string clientUsername);
}

public class ManagerNotificationFormatter : IManagerNotificationFormatter
{
    private readonly IMediator _mediator;
    private readonly IMessageService _messageService;
    private readonly IBotI18n _i18n;

    public ManagerNotificationFormatter(IMediator mediator, IMessageService messageService, IBotI18n i18n)
    {
        _mediator = mediator;
        _messageService = messageService;
        _i18n = i18n;
    }

    public async Task<(string body, string writeButtonText)> FormatAsync(RentalApplicationDraft draft, long managerChatId, string clientUsername)
    {
        var managerLang = await _mediator.Send(new GetUserLanguageQuery(ChatId.FromLong(managerChatId)));
        var managerBody = await _mediator.Send(new GetMessageBodyQuery("managerNotification", managerLang))
            ?? "<b>Страна:</b> {country}\n<b>Деятельность:</b> {profession}\n<b>Домашние животные:</b> {pets}\n<b>Срок аренды:</b> {term}";

        var countryDisplay = draft.Country == Domain.Enums.Country.Other && !string.IsNullOrWhiteSpace(draft.CountryOther)
            ? _messageService.Escape(draft.CountryOther) : draft.Country?.GetDisplayName();
        var termDisplay = draft.Term == Domain.Enums.Term.Other && !string.IsNullOrWhiteSpace(draft.TermOther)
            ? _messageService.Escape(draft.TermOther) : draft.Term?.GetDisplayName();
        var petsDisplay = draft.HasPets == true
            ? (managerLang == "en" ? "Yes" : managerLang == "ka" ? "დიახ" : "Да")
            : (managerLang == "en" ? "No" : managerLang == "ka" ? "არა" : "Нет");

        managerBody = managerBody
            .Replace("{country}", countryDisplay ?? "—")
            .Replace("{profession}", _messageService.Escape(draft.Profession) ?? "—")
            .Replace("{pets}", petsDisplay)
            .Replace("{term}", termDisplay ?? "—");

        var writeButtonText = _i18n.T("btn.write", managerLang);

        return (managerBody, writeButtonText);
    }
}
