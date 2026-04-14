using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.JSInterop;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Radzen;
using Radzen.Blazor;
using System.IO.Compression;

namespace MicroRenamerWeb.Components.Pages
{
    public partial class Tool
    {
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

        public string StatusMessage { get; set; }
        public List<string> FoundFiles { get; set; } = new List<string>();
        private string uploadedFilePath;

        // This method runs when a file is uploaded
        public async Task OnUpload(dynamic args)
        {
            // Get the first uploaded file from the upload control
            var file = args.Files[0];

            // Create a path to a temporary folder on the server
            var tempFolder = Path.Combine(Path.GetTempPath(), "myapp");

            // Ensure the temp folder exists (create it if it doesn't)
            Directory.CreateDirectory(tempFolder);

            // Build the full file path where the uploaded file will be saved
            var filePath = Path.Combine(tempFolder, file.Name);

            // Open a stream to read the uploaded file (limit = 100MB)
            using var stream = file.OpenReadStream(100_000_000);

            // Create a file on disk at the target path
            using var fs = File.Create(filePath);

            // Copy the uploaded file data into the file on disk
            await stream.CopyToAsync(fs);

            // Store the saved file path so we can use it later (Process button)
            uploadedFilePath = filePath;

            // Print the saved file path to the output window for testing
            //Console.WriteLine("Saved: " + uploadedFilePath);
            StatusMessage = "Processing: " + uploadedFilePath;
        }


// This method runs when the user clicks the "Process" button
public void ProcessFiles()
{
    // Check if a file has been uploaded
    if (string.IsNullOrEmpty(uploadedFilePath))
    {
        Console.WriteLine("No file uploaded");
        return;
    }

    // Create a folder to extract the ZIP into
    var extractPath = Path.Combine(Path.GetTempPath(), "myapp_extract");

    // If the folder already exists, delete it first (clean start)
    if (Directory.Exists(extractPath))
    {
        Directory.Delete(extractPath, true);
    }

    // Create the extraction folder
    Directory.CreateDirectory(extractPath);

    // Extract the ZIP file into the folder
    System.IO.Compression.ZipFile.ExtractToDirectory(uploadedFilePath, extractPath);

    // Print where files were extracted
    Console.WriteLine("Extracted to: " + extractPath);

    // List all files inside the extracted folder
    var files = Directory.GetFiles(extractPath);

    // Loop through each file and print its name
    foreach (var file in files)
    {
        Console.WriteLine("Found file: " + file);
    }
}




    }
}