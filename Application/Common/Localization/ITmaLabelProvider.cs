namespace Application.Common.Localization;

public interface ITmaLabelProvider
{
    TmaSubmitApplicationLabels GetSubmitApplicationLabels(string language);
}
