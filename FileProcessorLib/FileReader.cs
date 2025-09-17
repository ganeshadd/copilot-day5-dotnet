using System.Threading.Tasks;

namespace FileProcessorLib
{
    public class FileReader : IFileReader
    {
        public async Task<string> ReadAllTextAsync(string path)
        {
            return await System.IO.File.ReadAllTextAsync(path);
        }
    }
}
