namespace Application.Common.Localization;

public sealed class TmaLabelProvider : ITmaLabelProvider
{
    private static readonly Dictionary<string, TmaSubmitApplicationLabels> Labels = new(StringComparer.OrdinalIgnoreCase)
    {
        ["en"] = new()
        {
            Title = "Application data received!",
            Country = "Country",
            Profession = "Profession",
            Pets = "Pets",
            Term = "Rental term",
            Footer = "To complete the application, forward a property post from the @propertyintbilisi channel to this chat.",
            PetsYes = "Yes",
            PetsNo = "No"
        },
        ["ka"] = new()
        {
            Title = "განაცხადის მონაცემები მიღებულია!",
            Country = "ქვეყანა",
            Profession = "საქმიანობა",
            Pets = "შინაური ცხოველები",
            Term = "იჯარის ვადა",
            Footer = "განაცხადის დასასრულებლად, გადააგზავნეთ ბინის პოსტი @propertyintbilisi არხიდან ამ ჩატში.",
            PetsYes = "დიახ",
            PetsNo = "არა"
        }
    };

    public TmaSubmitApplicationLabels GetSubmitApplicationLabels(string language)
    {
        if (Labels.TryGetValue(language, out var labels))
        {
            return labels;
        }

        return Labels.TryGetValue(SupportedLanguages.Default, out var defaultLabels)
            ? defaultLabels
            : new TmaSubmitApplicationLabels();
    }
}
