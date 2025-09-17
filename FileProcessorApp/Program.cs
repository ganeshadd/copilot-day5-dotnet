using FileProcessorLib;

class Program
{
    static void Main(string[] args)
    {
        var fileReader = new FileReader();
        var processor = new FileProcessor(fileReader);
        // Add your file processing logic here
        System.Console.WriteLine("FileProcessorApp is running.");
    }
}
