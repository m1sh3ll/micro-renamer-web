using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Radzen;
using MicroRenamerWeb.Services;

namespace MicroRenamerWeb.Components.Pages
{
  public partial class Tool
  {
    #region Inject
    // ================================
    // Radzen / Blazor injected services
    // ================================

    [Inject]
    protected IJSRuntime JSRuntime { get; set; }

    [Inject]
    protected NavigationManager NavigationManager { get; set; }

    [Inject]
    protected DialogService DialogService { get; set; }

    [Inject]
    protected TooltipService TooltipService { get; set; }

    [Inject]
    protected ContextMenuService ContextMenuService { get; set; }

    [Inject]
    protected NotificationService NotificationService { get; set; }

    // Keep service (you’ll reuse later)
    [Inject]
    protected FileService FileService { get; set; }

    #endregion


    // ================================
    // UI + State variables
    // ================================

    // Placeholder message
    public string StatusMessage { get; set; } = "Tool page coming soon...";
  }
}