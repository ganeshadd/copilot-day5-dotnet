
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessorLib
{
    /// <summary>
    /// Provides file processing functionality with improved error handling, 
    /// async support, and memory efficiency.
    /// </summary>
    public class FileProcessor
    {
        private const int DefaultBufferSize = 8192; // 8KB buffer

        /// <summary>
        /// Gets the character length of a file synchronously.
        /// Improved version with input validation and error handling.
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The number of characters in the file</returns>
        /// <exception cref="ArgumentException">Thrown when path is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when file cannot be read</exception>
        public int GetFileLength(string path)
        {
            ValidatePath(path);
            
            try
            {
                // For backward compatibility, keeping the original approach but with validation
                var text = File.ReadAllText(path);
                return text.Length;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied to file: {path}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Failed to read file: {path}", ex);
            }
        }

        /// <summary>
        /// Gets the character length of a file asynchronously using streaming for memory efficiency.
        /// Recommended for large files or high-concurrency scenarios.
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation</param>
        /// <returns>The number of characters in the file</returns>
        /// <exception cref="ArgumentException">Thrown when path is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when file cannot be read</exception>
        public async Task<int> GetFileLengthAsync(string path, CancellationToken cancellationToken = default)
        {
            ValidatePath(path);
            
            try
            {
                using var reader = new StreamReader(path);
                int totalLength = 0;
                char[] buffer = new char[DefaultBufferSize];
                int charsRead;
                
                while ((charsRead = await reader.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false)) > 0)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    totalLength += charsRead;
                }
                
                return totalLength;
            }
            catch (OperationCanceledException)
            {
                throw; // Re-throw cancellation exceptions as-is
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied to file: {path}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Failed to read file: {path}", ex);
            }
        }

        /// <summary>
        /// Gets the character length of a file synchronously using streaming for memory efficiency.
        /// Recommended for large files when async is not available.
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The number of characters in the file</returns>
        /// <exception cref="ArgumentException">Thrown when path is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        /// <exception cref="InvalidOperationException">Thrown when file cannot be read</exception>
        public int GetFileLengthStreaming(string path)
        {
            ValidatePath(path);
            
            try
            {
                using var reader = new StreamReader(path);
                int totalLength = 0;
                char[] buffer = new char[DefaultBufferSize];
                int charsRead;
                
                while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalLength += charsRead;
                }
                
                return totalLength;
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new InvalidOperationException($"Access denied to file: {path}", ex);
            }
            catch (IOException ex)
            {
                throw new InvalidOperationException($"Failed to read file: {path}", ex);
            }
        }

        /// <summary>
        /// Gets the file size in bytes (fast operation, no content reading).
        /// Use this when you need file size rather than character count.
        /// </summary>
        /// <param name="path">The path to the file</param>
        /// <returns>The size of the file in bytes</returns>
        /// <exception cref="ArgumentException">Thrown when path is null or empty</exception>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        public long GetFileSizeInBytes(string path)
        {
            ValidatePath(path);
            
            var fileInfo = new FileInfo(path);
            return fileInfo.Length;
        }

        /// <summary>
        /// Validates the file path and throws appropriate exceptions.
        /// </summary>
        /// <param name="path">The path to validate</param>
        /// <exception cref="ArgumentException">Thrown when path is null, empty, or contains invalid characters</exception>
        /// <exception cref="FileNotFoundException">Thrown when file doesn't exist</exception>
        private static void ValidatePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));
            
            // Check for invalid path characters before checking if file exists
            try
            {
                Path.GetFullPath(path);
            }
            catch (ArgumentException)
            {
                throw new ArgumentException($"Path contains invalid characters: {path}", nameof(path));
            }
            catch (NotSupportedException)
            {
                throw new ArgumentException($"Path format is not supported: {path}", nameof(path));
            }
            
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");
        }
    }
}
