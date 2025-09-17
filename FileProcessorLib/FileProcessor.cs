
using System;
using System.IO;
using System.Threading.Tasks;

namespace FileProcessorLib
{   
    //Refactor this method to improve performance while preserving behavior. Add unit tests that demonstrate identical output for sample inputs.
    // Performance-optimized implementation using async streaming to avoid blocking and memory issues.
    public class FileProcessor
    {
        // Legacy synchronous method preserved for backward compatibility
        public int GetFileLength(string path)
        {
            try
            {
                return GetFileLengthAsync(path).GetAwaiter().GetResult();
            }
            catch (AggregateException ex) when (ex.InnerException != null)
            {
                throw ex.InnerException;
            }
        }

        // New async method that streams file content without loading entire file into memory
        // This method preserves the exact behavior of File.ReadAllText().Length
        // Best for: Very large files, memory-constrained environments
        public async Task<int> GetFileLengthAsync(string path)
        {
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 8192, useAsync: true);
            using var reader = new StreamReader(stream, detectEncodingFromByteOrderMarks: true);

            int totalLength = 0;
            var buffer = new char[8192];
            int charsRead;

            while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                totalLength += charsRead;
            }
            return totalLength;
        }

        // Alternative method using FileInfo for very large files (gets length without reading content)
        public long GetFileSizeInBytes(string path)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            
            var fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
                throw new FileNotFoundException($"File not found: {path}", path);
                
            return fileInfo.Length;
        }
    }
}
