
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;


namespace MicroRenamerWeb.Services
{
  public class FileService
  {
    // Save uploaded files to a temp folder
    public async Task<List<string>> SaveFiles(List<Radzen.FileInfo> files, string folderName)
    {
      var savedFiles = new List<string>(); // list of saved file paths
      var tempFolder = Path.Combine(Path.GetTempPath(), folderName); // build folder path

      if (Directory.Exists(tempFolder))
        Directory.Delete(tempFolder, true); // delete old files
      Directory.CreateDirectory(tempFolder); // create new folder

      foreach (var file in files) // loop through uploaded files
      {
        var filePath = Path.Combine(tempFolder, file.Name); // full file path
        using var stream = file.OpenReadStream(100_000_000); // open upload stream
        using var fs = File.Create(filePath); // create file on disk
        await stream.CopyToAsync(fs); // copy file to disk
        savedFiles.Add(filePath); // track saved file
      }

      return savedFiles; // return saved file paths
    }


  

// Check image validity + HEIC + renamed HEIC detection


  // Resize images to 800x600 and convert to JPG
  public async Task<List<string>> ResizeImages(List<string> inputFiles, string outputFolderName)
    {
      var outputFiles = new List<string>(); // list of resized file paths
      var outputFolder = Path.Combine(Path.GetTempPath(), outputFolderName); // create output folder path

      if (Directory.Exists(outputFolder))
        Directory.Delete(outputFolder, true); // delete old output
      Directory.CreateDirectory(outputFolder); // create fresh output folder

      foreach (var filePath in inputFiles) // loop through input images
      {
        using var image = await Image.LoadAsync(filePath); // load image
        image.Mutate(x => x.Resize(800, 600)); // resize to 800x600
        var newFileName = Path.GetFileNameWithoutExtension(filePath) + ".jpg"; // force JPG name
        var outputPath = Path.Combine(outputFolder, newFileName); // build output path
        await image.SaveAsJpegAsync(outputPath, new JpegEncoder { Quality = 90 }); // save as JPG
        outputFiles.Add(outputPath); // track output file
      }

      return outputFiles; // return resized files
    }

    // Check image validity + HEIC + renamed HEIC detection
    public (bool IsValid, bool IsHeic, bool IsRenamedHeic, string FormatName) CheckImage(string filePath)
    {
      try
      {
        var extension = Path.GetExtension(filePath).ToLower(); // get file extension

        using var stream = File.OpenRead(filePath); // open file stream

        var format = Image.DetectFormat(stream); // detect actual format

        // CASE 1: real HEIC file (not detected but extension is .heic)
        if (format == null && extension == ".heic")
          return (false, true, false, "HEIC");

        // CASE 2: renamed HEIC (format says HEIC but extension is NOT .heic)
        if (format != null && (format.Name == "HEIF" || format.Name == "HEIC"))
        {
          if (extension != ".heic")
            return (false, true, true, format.Name); // renamed HEIC

          return (false, true, false, format.Name); // normal HEIC
        }

        // CASE 3: format not detected → check extension fallback
        if (format == null)
        {
          // If file claims to be JPG but isn't readable → likely renamed HEIC
          if (extension == ".jpg" || extension == ".jpeg")
            return (false, true, true, "HEIC");

          return (false, false, false, "Unknown"); // truly invalid
        }

        var formatName = format.Name; // detected format name

        // CASE 4: allowed formats (JPEG, PNG, GIF)
        if (formatName == "JPEG" || formatName == "PNG" || formatName == "GIF")
        {
          stream.Position = 0; // reset stream

          using var image = Image.Load(stream); // load image

          if (image.Width == 0 || image.Height == 0)
            return (false, false, false, formatName); // invalid image

          return (true, false, false, formatName); // valid image
        }

        return (false, false, false, formatName); // unsupported format
      }
      catch
      {
        return (false, false, false, "Unknown"); // not readable
      }
    }





  }//rnd class and namespace
}