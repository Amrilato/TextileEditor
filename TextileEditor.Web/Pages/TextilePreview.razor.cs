﻿using R3;
using Microsoft.AspNetCore.Components;
using TextileEditor.Shared.Services;
using TextileEditor.Web.Services;

namespace TextileEditor.Web.Pages;

public partial class TextilePreview : IDisposable
{
    [Inject]
    public required ILocalizer Localizer { get; init; }
    [CascadingParameter(Name = CascadingParameterNames.Session)]
    public TextileSession? Session { get; set; }

    private IDisposable? disposable;
    protected override void OnParametersSet()
    {
        disposable?.Dispose();
        disposable = Localizer.ChangeCulture.Subscribe(c => StateHasChanged());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        disposable?.Dispose();
    }
}
