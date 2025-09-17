
using System;

namespace FileProcessorLib
{
    /// <summary>
    /// Provides async, streaming, and robust file processing utilities.
    /// </summary>
    public class FileProcessor
    {
        /// <summary>
        /// Gets the length of a file's content asynchronously using streaming for memory efficiency.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>The length of the file's content.</returns>
        /// <exception cref="ArgumentException">Thrown if path is null, empty, or whitespace.</exception>
        /// <exception cref="FileNotFoundException">Thrown if file does not exist.</exception>
        /// <exception cref="FileProcessorException">Thrown for IO errors.</exception>
        public async Task<long> GetFileLengthAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path cannot be null, empty, or whitespace.", nameof(path));
            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}", path);
            try
            {
                long length = 0;
                using (var stream = new System.IO.StreamReader(path))
                {
                    char[] buffer = new char[8192];
                    int read;
                    while ((read = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    {
                        length += read;
                    }
                }
                return length;
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException)
            {
                throw new FileProcessorException("Error reading file length.", ex);
            }
        }

        /// <summary>
        /// Reads all lines from a file asynchronously as a stream.
        /// </summary>
        public async IAsyncEnumerable<string> ReadLinesAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("File path cannot be null, empty, or whitespace.", nameof(path));
            if (!System.IO.File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}", path);
            try
            {
                using (var stream = new System.IO.StreamReader(path))
                {
                    while (!stream.EndOfStream)
                    {
                        var line = await stream.ReadLineAsync();
                        yield return line ?? string.Empty;
                    }
                }
            }
            catch (Exception ex) when (ex is System.IO.IOException || ex is UnauthorizedAccessException)
            {
                throw new FileProcessorException("Error reading file lines.", ex);
            }
        }
    }

    /// <summary>
    /// Custom exception for FileProcessor errors.
    /// </summary>
    public class FileProcessorException : Exception
    {
        public FileProcessorException(string message, Exception inner) : base(message, inner) { }
    }
}
