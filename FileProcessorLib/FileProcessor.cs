
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FileProcessorLib
{
    /// <summary>
    /// Provides efficient methods to process files, including optimized length calculation.
    /// </summary>
    /// <remarks>
    /// This class implements multiple approaches for file length determination:
    /// - Synchronous metadata-based method (most efficient)
    /// - Asynchronous content reading method (good for text files)
    /// - Streaming-based asynchronous method (best for large files)
    /// </remarks>
    public class FileProcessor
    {
        /// <summary>
        /// Validates that the provided file path is not null, empty, or whitespace.
        /// </summary>
        /// <param name="path">The file path to validate</param>
        /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
        /// <exception cref="ArgumentException">Thrown when path is empty or contains only whitespace</exception>
        private void ValidateFilePath(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path), "File path cannot be null.");
            }

            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("File path cannot be empty or whitespace.", nameof(path));
            }
        }
        // Buffer size for streaming operations, optimized for most file system operations
        private const int DefaultBufferSize = 4096;

        /// <summary>
        /// Gets the length of a file in bytes efficiently without loading its contents.
        /// </summary>
        /// <param name="path">Path to the file to measure</param>
        /// <returns>The length of the file in bytes</returns>
        /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
        /// <exception cref="ArgumentException">Thrown when path is empty or contains only whitespace</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the caller does not have the required permission</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
        public int GetFileLength(string path)
        {
            ValidateFilePath(path);

            try
            {
                // Use FileInfo to get the length of the file without loading the entire content
                var fileInfo = new FileInfo(path);

                if (!fileInfo.Exists)
                {
                    throw new FileNotFoundException("The specified file was not found.", path);
                }

                return (int)fileInfo.Length;
            }
            catch (Exception ex) when (ex is not ArgumentNullException &&
                                     ex is not ArgumentException &&
                                     ex is not FileNotFoundException)
            {
                // Wrap and rethrow with more context
                throw new IOException($"Error accessing file at path: {path}", ex);
            }
        }

        /// <summary>
        /// Gets the length of a file asynchronously by reading its contents.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="ct">Cancellation token to cancel the operation</param>
        /// <returns>The length of the file content in characters</returns>
        /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
        /// <exception cref="ArgumentException">Thrown when path is empty or contains only whitespace</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the caller does not have the required permission</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled</exception>
        public async Task<int> GetFileLengthAsync(string path, CancellationToken ct = default)
        {
            ValidateFilePath(path);

            try
            {
                // Verify file exists before attempting to read it
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("The specified file was not found.", path);
                }

                // Use File.ReadAllTextAsync for true asynchronous IO
                string content = await File.ReadAllTextAsync(path, ct);
                return content.Length;
            }
            catch (Exception ex) when (ex is not ArgumentNullException &&
                                     ex is not ArgumentException &&
                                     ex is not FileNotFoundException &&
                                     ex is not OperationCanceledException)
            {
                // Wrap and rethrow with more context
                throw new IOException($"Error reading file at path: {path}", ex);
            }
        }

        /// <summary>
        /// Gets the length of a file asynchronously using streaming to avoid loading the entire file into memory.
        /// </summary>
        /// <param name="path">Path to the file</param>
        /// <param name="ct">Cancellation token to cancel the operation</param>
        /// <returns>The length of the file in bytes</returns>
        /// <exception cref="ArgumentNullException">Thrown when path is null</exception>
        /// <exception cref="ArgumentException">Thrown when path is empty or contains only whitespace</exception>
        /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the caller does not have the required permission</exception>
        /// <exception cref="IOException">Thrown when an I/O error occurs</exception>
        /// <exception cref="OperationCanceledException">Thrown when the operation is canceled</exception>
        public async Task<int> GetFileLengthStreamingAsync(string path, CancellationToken ct = default)
        {
            ValidateFilePath(path);

            try
            {
                // Verify file exists before attempting to open it
                if (!File.Exists(path))
                {
                    throw new FileNotFoundException("The specified file was not found.", path);
                }

                // Use FileStream for streaming without loading the entire file into memory
                using var fileStream = new FileStream(
                    path,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read,
                    bufferSize: DefaultBufferSize,
                    useAsync: true);

                int length = 0;
                byte[] buffer = new byte[DefaultBufferSize];
                int bytesRead;

                while ((bytesRead = await fileStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                {
                    length += bytesRead;
                }

                return length;
            }
            catch (Exception ex) when (ex is not ArgumentNullException &&
                                     ex is not ArgumentException &&
                                     ex is not FileNotFoundException &&
                                     ex is not OperationCanceledException)
            {
                // Wrap and rethrow with more context
                throw new IOException($"Error streaming file at path: {path}", ex);
            }
        }
    }
}
