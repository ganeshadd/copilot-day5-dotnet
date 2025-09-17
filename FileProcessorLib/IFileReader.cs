namespace FileProcessorLib
{
    public interface IFileReader
    {
        Task<string> ReadAllTextAsync(string path);
    }
}
