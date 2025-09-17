
using System;
using System.IO;
using System.Text;
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
        private readonly string _emptyTempPath;
        private readonly string _binaryTempPath;

        public FileProcessorTests()
        {
            // Create small test file
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(_tempPath, "hello world"); // small file for baseline

            // Create empty test file
            _emptyTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(_emptyTempPath, string.Empty);

            // Create larger test file (100KB)
            _largeTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            var sb = new StringBuilder();
            for (int i = 0; i < 100 * 1024; i++)
            {
                sb.Append('A');
            }
            File.WriteAllText(_largeTempPath, sb.ToString());

            // Create binary file
            _binaryTempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".bin");
            using (var fs = File.Create(_binaryTempPath))
            {
                byte[] data = new byte[1024];
                for (int i = 0; i < data.Length; i++)
                {
                    data[i] = (byte)(i % 256);
                }
                fs.Write(data, 0, data.Length);
            }
        }

        [Fact]
        public void GetFileLength_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_tempPath);
            Assert.Equal(11, len);
        }

        [Fact]
        public void GetFileLength_EmptyFile_ReturnsZero()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_emptyTempPath);
            Assert.Equal(0, len);
        }

        [Fact]
        public void GetFileLength_LargeFile_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_largeTempPath);
            Assert.Equal(100 * 1024, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = await proc.GetFileLengthAsync(_tempPath);
            Assert.Equal(11, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_WithCancellationToken_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            using var cts = new CancellationTokenSource();
            int len = await proc.GetFileLengthAsync(_tempPath, cts.Token);
            Assert.Equal(11, len);
        }

        [Fact]
        public async Task GetFileLengthStreamingAsync_ReturnsCorrectByteCount()
        {
            var proc = new FileProcessor();
            int len = await proc.GetFileLengthStreamingAsync(_binaryTempPath);
            Assert.Equal(1024, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_CancellationRequested_ThrowsTaskCanceledException()
        {
            var proc = new FileProcessor();
            using var cts = new CancellationTokenSource();

            // Immediately cancel the token
            cts.Cancel();

            // The operation should throw a TaskCanceledException (a subclass of OperationCanceledException)
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await proc.GetFileLengthAsync(_largeTempPath, cts.Token)
            );
        }

        [Fact]
        public async Task GetFileLengthStreamingAsync_CancellationRequested_ThrowsOperationCanceledException()
        {
            var proc = new FileProcessor();
            using var cts = new CancellationTokenSource();

            // Immediately cancel the token
            cts.Cancel();

            // The operation should throw an OperationCanceledException
            await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
                await proc.GetFileLengthStreamingAsync(_largeTempPath, cts.Token)
            );
        }

        [Fact]
        public void GetFileLength_NullPath_ThrowsArgumentNullException()
        {
            var proc = new FileProcessor();

            string? nullPath = null;
            var exception = Assert.Throws<ArgumentNullException>(() => proc.GetFileLength(nullPath!));
            Assert.Equal("path", exception.ParamName);
        }

        [Fact]
        public void GetFileLength_EmptyPath_ThrowsArgumentException()
        {
            var proc = new FileProcessor();

            var exception = Assert.Throws<ArgumentException>(() => proc.GetFileLength(string.Empty));
            Assert.Equal("path", exception.ParamName);
        }

        [Fact]
        public void GetFileLength_WhitespacePath_ThrowsArgumentException()
        {
            var proc = new FileProcessor();

            var exception = Assert.Throws<ArgumentException>(() => proc.GetFileLength("   "));
            Assert.Equal("path", exception.ParamName);
        }

        [Fact]
        public async Task GetFileLengthAsync_NullPath_ThrowsArgumentNullException()
        {
            var proc = new FileProcessor();

            string? nullPath = null;
            var exception = await Assert.ThrowsAsync<ArgumentNullException>(() => proc.GetFileLengthAsync(nullPath!));
            Assert.Equal("path", exception.ParamName);
        }

        [Fact]
        public void GetFileLength_NonExistentFile_ThrowsFileNotFoundException()
        {
            var proc = new FileProcessor();
            string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            Assert.Throws<FileNotFoundException>(() => proc.GetFileLength(nonExistentPath));
        }

        [Fact]
        public async Task GetFileLengthAsync_NonExistentFile_ThrowsFileNotFoundException()
        {
            var proc = new FileProcessor();
            string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            await Assert.ThrowsAsync<FileNotFoundException>(() => proc.GetFileLengthAsync(nonExistentPath));
        }

        [Fact]
        public async Task GetFileLengthStreamingAsync_NonExistentFile_ThrowsFileNotFoundException()
        {
            var proc = new FileProcessor();
            string nonExistentPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            await Assert.ThrowsAsync<FileNotFoundException>(() => proc.GetFileLengthStreamingAsync(nonExistentPath));
        }

        public void Dispose()
        {
            if (File.Exists(_tempPath)) File.Delete(_tempPath);
            if (File.Exists(_largeTempPath)) File.Delete(_largeTempPath);
            if (File.Exists(_emptyTempPath)) File.Delete(_emptyTempPath);
            if (File.Exists(_binaryTempPath)) File.Delete(_binaryTempPath);
        }
    }
}
