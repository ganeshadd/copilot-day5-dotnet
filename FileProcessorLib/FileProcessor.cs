using System;
using System.Threading.Tasks;

namespace FileProcessorLib
{
    public class FileProcessor
    {
        private readonly IFileReader _fileReader;

        public FileProcessor(IFileReader fileReader)
        {
            _fileReader = fileReader;
        }

        public async Task<int> GetFileLengthAsync(string path)
        {
            var text = await _fileReader.ReadAllTextAsync(path);
            return text.Length;
        }
    }
}
