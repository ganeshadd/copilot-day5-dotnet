using System;
using System.Threading.Tasks;

namespace FileProcessorLib
{
    public class FileProcessor
    {
        private readonly IFileReader _fileReader;

        public FileProcessor(IFileReader fileReader)
        {
            _fileReader = fileReader ?? throw new ArgumentNullException(nameof(fileReader));
        }

        public async Task<int> GetFileLengthAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path cannot be null or empty.", nameof(path));
            try
            {
                var text = await _fileReader.ReadAllTextAsync(path);
                return text?.Length ?? 0;
            }
            catch (System.IO.FileNotFoundException)
            {
                // File not found, return 0 or handle as needed
                return 0;
            }
            catch (System.IO.IOException ex)
            {
                // Log or handle IO exceptions
                throw new InvalidOperationException($"IO error reading file: {ex.Message}", ex);
            }
        }
    }
}
