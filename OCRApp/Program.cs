using Microsoft.Extensions.Configuration;
using System;
using System.IO;
using System.Linq;
using Tesseract;

class Program
{
    static void Main()
    {
        // Load configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(Path.Combine(AppContext.BaseDirectory, "appsettings.json"))
            .Build();

        // Read paths and settings from configuration
        string folderPath = configuration["OcrSettings:ImageDirectory"] ?? string.Empty;
        string resultsDirectory = configuration["OcrSettings:ResultsDirectory"] ?? string.Empty;
        string language = configuration["OcrSettings:Language"] ?? string.Empty;
        string timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");

        // Create output file path with timestamp
        string outputFilePath = Path.Combine(
            resultsDirectory,
            $"result-{timestamp}.txt"
        );

        // Ensure the output directory exists
        if (!Directory.Exists(resultsDirectory))
        {
            Directory.CreateDirectory(resultsDirectory);
        }

        // Path to Tesseract traineddata
        string tesseractDataPath = Path.Combine(AppContext.BaseDirectory, "testdata");

        try
        {
            // Initialize Tesseract OCR Engine
            using (var engine = new TesseractEngine(tesseractDataPath, language, EngineMode.Default))
            {
                // Scan supported image file types
                var imageFiles = Directory.GetFiles(folderPath, "*.*")
                    .Where(file => file.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                                   || file.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                                   || file.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                                   || file.EndsWith(".tiff", StringComparison.OrdinalIgnoreCase));

                // Write results to the output file
                using (var writer = new StreamWriter(outputFilePath, false))
                {
                    foreach (var imageFile in imageFiles)
                    {
                        Console.WriteLine($"Scanning: {imageFile}");
                        using (var img = Pix.LoadFromFile(imageFile))
                        {
                            using (var page = engine.Process(img))
                            {
                                writer.WriteLine($"Result for {Path.GetFileName(imageFile)}:");
                                writer.WriteLine(page.GetText());
                                writer.WriteLine("==========================================");
                            }
                        }
                    }
                }

                Console.WriteLine($"Results saved to: {outputFilePath}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error initializing Tesseract OCR: {ex.Message}");
        }
    }
}
