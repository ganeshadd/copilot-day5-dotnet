
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileProcessorLib
{
    // Refactored implementation: asynchronous file processing using streaming for better performance.
    public class FileProcessor
    {
        public async Task<int> GetFileLengthAsync(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException("File not found", path);

            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            
            var buffer = new byte[4096];
            int totalLength = 0;
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                totalLength += bytesRead;
            }

            return totalLength;
        }

        // Keep the synchronous version for backward compatibility
        [Obsolete("Use GetFileLengthAsync instead for better performance")]
        public int GetFileLength(string path)
        {
            return GetFileLengthAsync(path).GetAwaiter().GetResult();
        }
    }
}
