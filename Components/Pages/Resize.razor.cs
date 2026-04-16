using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Radzen;
using MicroRenamerWeb.Services;

namespace MicroRenamerWeb.Components.Pages
{
  public partial class Resize
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

    // Shared file service
    [Inject]
    protected FileService FileService { get; set; }

    #endregion


    // ================================
    // UI + State variables
    // ================================

    public string StatusMessage { get; set; } // message shown in UI

    private List<Radzen.FileInfo> uploadedFiles = new(); // uploaded files

    public List<string> ProcessedFiles { get; set; } = new(); // output file names


    // ================================
    // Upload Event
    // ================================

    public void OnUpload(UploadChangeEventArgs args)
    {
      uploadedFiles = args.Files.ToList(); // store uploaded files

      StatusMessage = $"{uploadedFiles.Count} images uploaded"; // update UI
    }


    // ================================
    // Process Button
    // ================================

    public async Task ProcessImages()
    {
      if (uploadedFiles.Count == 0) // no files check
      {
        StatusMessage = "No images uploaded";
        return;
      }

      ProcessedFiles.Clear(); // clear previous results

      // Save uploaded files to temp folder
      var savedFiles = await FileService.SaveFiles(uploadedFiles, "resize_input");

      // Lists for classification
      var validFiles = new List<string>(); // valid images
      var heicFiles = new List<string>(); // real HEIC (optional, may not trigger reliably)
      var invalidFiles = new List<string>(); // invalid or unsupported

      // Validate each file
      foreach (var filePath in savedFiles)
      {
        var result = FileService.CheckImage(filePath); // check file

        // Real HEIC (rarely detected depending on library support)
        if (result.IsHeic)
        {
          heicFiles.Add(Path.GetFileName(filePath));
          continue;
        }

        // Invalid or unsupported files
        if (!result.IsValid)
        {
          var name = Path.GetFileName(filePath); // file name
          var ext = Path.GetExtension(filePath).ToLower(); // extension

          // If it looks like a JPG but failed → likely renamed HEIC
          if (ext == ".jpg" || ext == ".jpeg")
          {
            invalidFiles.Add($"{name} (invalid - possibly was renamed by someone from HEIC to jpg)");
          }
          else
          {
            invalidFiles.Add(name);
          }

          continue;
        }

        // Valid image
        validFiles.Add(filePath);
      }

      // Resize valid images
      var resizedFiles = await FileService.ResizeImages(validFiles, "resize_output");

      // Show resized file names
      ProcessedFiles = resizedFiles.Select(Path.GetFileName).ToList();

      // ================================
      // Build status message (multi-line)
      // ================================

      StatusMessage = $"Resized {ProcessedFiles.Count} images<br>"; // main line

      if (heicFiles.Count > 0)
      {
        StatusMessage += $"<br><b>HEIC files skipped:</b><br>{string.Join("<br>", heicFiles)}<br>";
      }

      if (invalidFiles.Count > 0)
      {
        StatusMessage += $"<br><b>Invalid files:</b><br>{string.Join("<br>", invalidFiles)}";
      }
    }



  }
}