
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using FileProcessorLib;
using Xunit;

//Refactor this method to improve performance while preserving behavior. Add unit tests that demonstrate identical output for sample inputs.
namespace FileProcessorTests
{
    public class FileProcessorTests : IDisposable
    {
        private readonly string _smallFilePath;
        private readonly string _largeFilePath;
        private readonly string _emptyFilePath;
        private readonly string _unicodeFilePath;

        public FileProcessorTests()
        {
            var tempDir = Path.GetTempPath();
            _smallFilePath = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".txt");
            _largeFilePath = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".txt");
            _emptyFilePath = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".txt");
            _unicodeFilePath = Path.Combine(tempDir, Guid.NewGuid().ToString() + ".txt");

            // Create test files with different characteristics
            File.WriteAllText(_smallFilePath, "hello world"); // 11 characters
            File.WriteAllText(_emptyFilePath, ""); // 0 characters
            File.WriteAllText(_unicodeFilePath, "Hello 🌍 Unicode: ñáéíóú", Encoding.UTF8); // Unicode content
            
            // Create a larger file for performance testing
            var largeContent = new StringBuilder();
            for (int i = 0; i < 1000; i++)
            {
                largeContent.AppendLine($"This is line number {i} with some content to make it longer.");
            }
            File.WriteAllText(_largeFilePath, largeContent.ToString());
        }

        [Fact]
        public void GetFileLength_SmallFile_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_smallFilePath);
            Assert.Equal(11, len);
        }

        [Fact]
        public void GetFileLength_EmptyFile_ReturnsZero()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_emptyFilePath);
            Assert.Equal(0, len);
        }

        [Fact]
        public void GetFileLength_UnicodeFile_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_unicodeFilePath);
            
            // Verify against File.ReadAllText to ensure identical behavior
            var expectedLength = File.ReadAllText(_unicodeFilePath).Length;
            Assert.Equal(expectedLength, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_SmallFile_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = await proc.GetFileLengthAsync(_smallFilePath);
            Assert.Equal(11, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_EmptyFile_ReturnsZero()
        {
            var proc = new FileProcessor();
            int len = await proc.GetFileLengthAsync(_emptyFilePath);
            Assert.Equal(0, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_UnicodeFile_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = await proc.GetFileLengthAsync(_unicodeFilePath);
            
            // Verify against File.ReadAllText to ensure identical behavior
            var expectedLength = File.ReadAllText(_unicodeFilePath).Length;
            Assert.Equal(expectedLength, len);
        }

        [Fact]
        public async Task GetFileLength_IdenticalBehavior_SyncVsAsync()
        {
            var proc = new FileProcessor();
            
            // Test that sync and async methods return identical results
            int syncResult = proc.GetFileLength(_smallFilePath);
            int asyncResult = await proc.GetFileLengthAsync(_smallFilePath);
            
            Assert.Equal(syncResult, asyncResult);
        }

        [Fact]
        public void GetFileLength_LargeFile_IdenticalBehavior()
        {
            var proc = new FileProcessor();
            
            // Compare new implementation with original File.ReadAllText approach
            int newImplementation = proc.GetFileLength(_largeFilePath);
            int originalApproach = File.ReadAllText(_largeFilePath).Length;
            
            Assert.Equal(originalApproach, newImplementation);
        }

        [Fact]
        public async Task GetFileLengthAsync_LargeFile_IdenticalBehavior()
        {
            var proc = new FileProcessor();
            
            // Compare async implementation with original File.ReadAllText approach
            int asyncImplementation = await proc.GetFileLengthAsync(_largeFilePath);
            int originalApproach = File.ReadAllText(_largeFilePath).Length;
            
            Assert.Equal(originalApproach, asyncImplementation);
        }

        [Fact]
        public void GetFileLength_NonExistentFile_ThrowsFileNotFoundException()
        {
            var proc = new FileProcessor();
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "non-existent-file.txt");
            
            Assert.Throws<FileNotFoundException>(() => proc.GetFileLength(nonExistentPath));
        }

        [Fact]
        public async Task GetFileLengthAsync_NonExistentFile_ThrowsFileNotFoundException()
        {
            var proc = new FileProcessor();
            var nonExistentPath = Path.Combine(Path.GetTempPath(), "non-existent-file.txt");
            
            await Assert.ThrowsAsync<FileNotFoundException>(() => proc.GetFileLengthAsync(nonExistentPath));
        }

        [Fact]
        public void GetFileSizeInBytes_ReturnsCorrectSize()
        {
            var proc = new FileProcessor();
            long size = proc.GetFileSizeInBytes(_smallFilePath);
            
            var fileInfo = new FileInfo(_smallFilePath);
            Assert.Equal(fileInfo.Length, size);
        }

        public void Dispose()
        {
            DeleteFileIfExists(_smallFilePath);
            DeleteFileIfExists(_largeFilePath);
            DeleteFileIfExists(_emptyFilePath);
            DeleteFileIfExists(_unicodeFilePath);
        }

        private static void DeleteFileIfExists(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}
