namespace Web.Pages.Shared;

/// <summary>
/// Model for _TmaOtherInputPartial — a conditional text field shown when
/// a &lt;select&gt; has the "Другое" option selected.
/// </summary>
public class _TmaOtherInput
{
    public string WrapId { get; set; } = "";
    public string InputId { get; set; } = "";
    public string InputName { get; set; } = "";
    public string Placeholder { get; set; } = "";
    public string I18nKey { get; set; } = "";
}

/// <summary>
/// Model for _TmaNavButtonsPartial — a button group for wizard step navigation.
/// </summary>
public class _TmaNavButtons
{
    public string? PrevStep { get; set; }
    public string? NextStep { get; set; }
    public bool IsSubmit { get; set; }
    public bool IsDone { get; set; }
}
