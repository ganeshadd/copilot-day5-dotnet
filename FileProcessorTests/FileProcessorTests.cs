
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FileProcessorLib;
using Xunit;

namespace FileProcessorTests
{
    public class FileProcessorTests : IDisposable
    {
        private readonly string _tempPath;
        private readonly string _largeTempPath;

        public FileProcessorTests()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            _largeTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_large.txt");
            
            // Create a small test file
            File.WriteAllText(_tempPath, "hello world"); // 11 characters
            
            // Create a larger test file for streaming tests
            var largeContent = new string('A', 10000) + new string('B', 5000); // 15000 characters
            File.WriteAllText(_largeTempPath, largeContent);
        }

        [Fact]
        public void GetFileLength_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_tempPath);
            Assert.Equal(11, len);
        }

        [Fact]
        public void GetFileLength_ThrowsArgumentException_WhenPathIsNull()
        {
            var proc = new FileProcessor();
            Assert.Throws<ArgumentException>(() => proc.GetFileLength(null!));
        }

        [Fact]
        public void GetFileLength_ThrowsArgumentException_WhenPathIsEmpty()
        {
            var proc = new FileProcessor();
            Assert.Throws<ArgumentException>(() => proc.GetFileLength(""));
        }

        [Fact]
        public void GetFileLength_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            var proc = new FileProcessor();
            Assert.Throws<FileNotFoundException>(() => proc.GetFileLength("nonexistent.txt"));
        }

        [Fact]
        public async Task GetFileLengthAsync_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = await proc.GetFileLengthAsync(_tempPath);
            Assert.Equal(11, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_ReturnsCorrectLength_ForLargeFile()
        {
            var proc = new FileProcessor();
            int len = await proc.GetFileLengthAsync(_largeTempPath);
            Assert.Equal(15000, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_ThrowsArgumentException_WhenPathIsNull()
        {
            var proc = new FileProcessor();
            await Assert.ThrowsAsync<ArgumentException>(() => proc.GetFileLengthAsync(null!));
        }

        [Fact]
        public async Task GetFileLengthAsync_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            var proc = new FileProcessor();
            await Assert.ThrowsAsync<FileNotFoundException>(() => proc.GetFileLengthAsync("nonexistent.txt"));
        }

        [Fact]
        public async Task GetFileLengthAsync_SupportsCancellation()
        {
            var proc = new FileProcessor();
            using var cts = new CancellationTokenSource();
            cts.Cancel(); // Cancel immediately
            
            await Assert.ThrowsAsync<OperationCanceledException>(() => 
                proc.GetFileLengthAsync(_tempPath, cts.Token));
        }

        [Fact]
        public void GetFileLengthStreaming_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLengthStreaming(_tempPath);
            Assert.Equal(11, len);
        }

        [Fact]
        public void GetFileLengthStreaming_ReturnsCorrectLength_ForLargeFile()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLengthStreaming(_largeTempPath);
            Assert.Equal(15000, len);
        }

        [Fact]
        public void GetFileLengthStreaming_ThrowsArgumentException_WhenPathIsNull()
        {
            var proc = new FileProcessor();
            Assert.Throws<ArgumentException>(() => proc.GetFileLengthStreaming(null!));
        }

        [Fact]
        public void GetFileSizeInBytes_ReturnsCorrectSize()
        {
            var proc = new FileProcessor();
            long size = proc.GetFileSizeInBytes(_tempPath);
            Assert.Equal(11, size); // "hello world" is 11 bytes in UTF-8
        }

        [Fact]
        public void GetFileSizeInBytes_ThrowsArgumentException_WhenPathIsNull()
        {
            var proc = new FileProcessor();
            Assert.Throws<ArgumentException>(() => proc.GetFileSizeInBytes(null!));
        }

        [Fact]
        public void GetFileSizeInBytes_ThrowsFileNotFoundException_WhenFileDoesNotExist()
        {
            var proc = new FileProcessor();
            Assert.Throws<FileNotFoundException>(() => proc.GetFileSizeInBytes("nonexistent.txt"));
        }

        [Fact]
        public void AllMethods_ReturnSameLength_ForSameFile()
        {
            var proc = new FileProcessor();
            
            int syncLength = proc.GetFileLength(_tempPath);
            int streamingLength = proc.GetFileLengthStreaming(_tempPath);
            
            Assert.Equal(syncLength, streamingLength);
        }

        [Fact]
        public async Task AsyncAndSyncMethods_ReturnSameLength_ForSameFile()
        {
            var proc = new FileProcessor();
            
            int syncLength = proc.GetFileLength(_tempPath);
            int asyncLength = await proc.GetFileLengthAsync(_tempPath);
            
            Assert.Equal(syncLength, asyncLength);
        }

        // CORNER CASES - Testing edge cases and unusual scenarios

        [Fact]
        public void GetFileLength_ReturnsZero_ForEmptyFile()
        {
            // Corner Case: Empty file should return 0 length
            var emptyFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_empty.txt");
            File.WriteAllText(emptyFilePath, "");
            
            try
            {
                var proc = new FileProcessor();
                int length = proc.GetFileLength(emptyFilePath);
                Assert.Equal(0, length);
            }
            finally
            {
                if (File.Exists(emptyFilePath)) File.Delete(emptyFilePath);
            }
        }

        [Fact]
        public async Task GetFileLengthAsync_ReturnsZero_ForEmptyFile()
        {
            // Corner Case: Empty file should return 0 length (async version)
            var emptyFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_empty.txt");
            File.WriteAllText(emptyFilePath, "");
            
            try
            {
                var proc = new FileProcessor();
                int length = await proc.GetFileLengthAsync(emptyFilePath);
                Assert.Equal(0, length);
            }
            finally
            {
                if (File.Exists(emptyFilePath)) File.Delete(emptyFilePath);
            }
        }

        [Fact]
        public void GetFileLengthStreaming_ReturnsZero_ForEmptyFile()
        {
            // Corner Case: Empty file should return 0 length (streaming version)
            var emptyFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_empty.txt");
            File.WriteAllText(emptyFilePath, "");
            
            try
            {
                var proc = new FileProcessor();
                int length = proc.GetFileLengthStreaming(emptyFilePath);
                Assert.Equal(0, length);
            }
            finally
            {
                if (File.Exists(emptyFilePath)) File.Delete(emptyFilePath);
            }
        }

        [Fact]
        public void GetFileLength_HandlesUnicodeCharacters()
        {
            // Corner Case: Unicode characters may have different byte vs character counts
            var unicodeFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_unicode.txt");
            var unicodeContent = "Hello 🌍 World αβγ 中文"; // Mix of ASCII, emoji, Greek, Chinese
            File.WriteAllText(unicodeFilePath, unicodeContent, System.Text.Encoding.UTF8);
            
            try
            {
                var proc = new FileProcessor();
                int charLength = proc.GetFileLength(unicodeFilePath);
                long byteSize = proc.GetFileSizeInBytes(unicodeFilePath);
                
                // Character count should be less than byte count due to multi-byte UTF-8 characters
                Assert.True(charLength <= byteSize, "Character count should be <= byte count for Unicode content");
                Assert.Equal(unicodeContent.Length, charLength);
            }
            finally
            {
                if (File.Exists(unicodeFilePath)) File.Delete(unicodeFilePath);
            }
        }

        [Fact]
        public void GetFileLength_ThrowsFileNotFoundException_ForInvalidPath()
        {
            // Corner Case: Invalid path characters that don't trigger ArgumentException 
            // will still result in FileNotFoundException since the file won't exist
            var proc = new FileProcessor();
            var invalidPath = "invalid<>|path";
            
            Assert.Throws<FileNotFoundException>(() => proc.GetFileLength(invalidPath));
        }

        [Fact]
        public void GetFileLength_ThrowsArgumentException_ForWhitespaceOnlyPath()
        {
            // Corner Case: Path with only whitespace should be treated as invalid
            var proc = new FileProcessor();
            
            Assert.Throws<ArgumentException>(() => proc.GetFileLength("   "));
            Assert.Throws<ArgumentException>(() => proc.GetFileLength("\t\n"));
        }

        [Fact]
        public async Task GetFileLengthAsync_HandlesConcurrentAccess()
        {
            // Corner Case: Multiple concurrent reads of the same file should work
            var proc = new FileProcessor();
            
            var tasks = new Task<int>[5];
            for (int i = 0; i < 5; i++)
            {
                tasks[i] = proc.GetFileLengthAsync(_tempPath);
            }
            
            var results = await Task.WhenAll(tasks);
            
            // All concurrent reads should return the same result
            foreach (var result in results)
            {
                Assert.Equal(11, result);
            }
        }

        [Fact]
        public void GetFileLengthStreaming_HandlesVeryLargeFile()
        {
            // Corner Case: Test streaming with a larger file to verify memory efficiency
            var largeFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_verylarge.txt");
            
            try
            {
                // Create a 100KB file
                var largeContent = new string('X', 100000);
                File.WriteAllText(largeFilePath, largeContent);
                
                var proc = new FileProcessor();
                int streamingLength = proc.GetFileLengthStreaming(largeFilePath);
                int syncLength = proc.GetFileLength(largeFilePath);
                
                Assert.Equal(100000, streamingLength);
                Assert.Equal(syncLength, streamingLength);
            }
            finally
            {
                if (File.Exists(largeFilePath)) File.Delete(largeFilePath);
            }
        }

        [Fact]
        public void GetFileSizeInBytes_MatchesFileSystemSize()
        {
            // Corner Case: Verify that our byte size calculation matches the file system
            var proc = new FileProcessor();
            long ourCalculation = proc.GetFileSizeInBytes(_tempPath);
            long fileSystemSize = new FileInfo(_tempPath).Length;
            
            Assert.Equal(fileSystemSize, ourCalculation);
        }

        [Fact]
        public async Task GetFileLengthAsync_ThrowsArgumentException_ForWhitespaceOnlyPath()
        {
            // Corner Case: Async version should also handle whitespace-only paths
            var proc = new FileProcessor();
            
            await Assert.ThrowsAsync<ArgumentException>(() => proc.GetFileLengthAsync("   "));
            await Assert.ThrowsAsync<ArgumentException>(() => proc.GetFileLengthAsync("\t\n"));
        }

        // SECURITY TESTS - Testing security vulnerabilities

        [Fact]
        public void GetFileLength_ThrowsUnauthorizedAccessException_ForPathTraversalAttempt()
        {
            // Security Test: Path traversal should be blocked
            var proc = new FileProcessor();
            
            Assert.Throws<UnauthorizedAccessException>(() => proc.GetFileLength("../../../sensitive-file.txt"));
            Assert.Throws<UnauthorizedAccessException>(() => proc.GetFileLength("..\\..\\..\\sensitive-file.txt"));
        }

        [Fact]
        public void GetFileLength_ThrowsInvalidOperationException_ForLargeFile()
        {
            // Security Test: Large files should be rejected to prevent DoS
            var largeFilePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_toolarge.txt");
            
            try
            {
                // Create a file larger than 50MB (51MB)
                using (var writer = new StreamWriter(largeFilePath))
                {
                    var largeContent = new string('X', 1024); // 1KB chunk
                    for (int i = 0; i < 52 * 1024; i++) // 52MB total
                    {
                        writer.Write(largeContent);
                    }
                }
                
                var proc = new FileProcessor();
                
                Assert.Throws<InvalidOperationException>(() => proc.GetFileLength(largeFilePath));
            }
            finally
            {
                if (File.Exists(largeFilePath)) File.Delete(largeFilePath);
            }
        }

        public void Dispose()
        {
            if (File.Exists(_tempPath)) File.Delete(_tempPath);
            if (File.Exists(_largeTempPath)) File.Delete(_largeTempPath);
        }
    }
}
