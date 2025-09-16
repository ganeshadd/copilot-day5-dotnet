using FileProcessorLib;
using System;

class Program
{
    static void Main(string[] args)
    {
        System.Console.WriteLine("FileProcessorApp is running.");
        
        var processor = new FileProcessor();
        
        // Test with the current program file
        string currentFile = System.Reflection.Assembly.GetExecutingAssembly().Location;
        currentFile = currentFile.Replace(".dll", ".exe");
        
        try
        {
            if (System.IO.File.Exists(currentFile))
            {
                int length = processor.GetFileLength(currentFile);
                System.Console.WriteLine($"File: {System.IO.Path.GetFileName(currentFile)}");
                System.Console.WriteLine($"Length: {length} characters");
            }
            
            // Test with a text file if provided as argument
            if (args.Length > 0)
            {
                string filePath = args[0];
                if (System.IO.File.Exists(filePath))
                {
                    int length = processor.GetFileLength(filePath);
                    System.Console.WriteLine($"\nFile: {System.IO.Path.GetFileName(filePath)}");
                    System.Console.WriteLine($"Length: {length} characters");
                }
                else
                {
                    System.Console.WriteLine($"File not found: {filePath}");
                }
            }
            else
            {
                System.Console.WriteLine("\nUsage: dotnet run <filepath>");
                System.Console.WriteLine("Example: dotnet run \"C:\\temp\\myfile.txt\"");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"Error: {ex.Message}");
        }
    }
}
