
using System;
using System.IO;
using FileProcessorLib;
using Xunit;

namespace FileProcessorTests
{
    public class FileProcessorTests : IDisposable
    {
        private readonly string _tempPath;
        private readonly string _emptyPath;
        private readonly string _unicodePath;
        private readonly string _largePath;
        private readonly string _invalidPath;

        public FileProcessorTests()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(_tempPath, "hello world");

            _emptyPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_empty.txt");
            File.WriteAllText(_emptyPath, "");

            _unicodePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_unicode.txt");
            File.WriteAllText(_unicodePath, "你好，世界🌍\nこんにちは世界\nПривет, мир\n");

            _largePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + "_large.txt");
            using (var sw = new StreamWriter(_largePath))
            {
                for (int i = 0; i < 100000; i++) sw.WriteLine("Line " + i);
            }

            _invalidPath = Path.Combine(Path.GetTempPath(), "nonexistent_" + Guid.NewGuid() + ".txt");
        }

        [Fact]
        public async Task GetFileLengthAsync_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            long len = await proc.GetFileLengthAsync(_tempPath);
            Assert.Equal(11, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_EmptyFile_ReturnsZero()
        {
            var proc = new FileProcessor();
            long len = await proc.GetFileLengthAsync(_emptyPath);
            Assert.Equal(0, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_UnicodeFile_CorrectLength()
        {
            var proc = new FileProcessor();
            long len = await proc.GetFileLengthAsync(_unicodePath);
            Assert.True(len > 0);
        }

        [Fact]
        public async Task GetFileLengthAsync_LargeFile_Performance()
        {
            var proc = new FileProcessor();
            long len = await proc.GetFileLengthAsync(_largePath);
            Assert.True(len > 100000); // Each line has at least 6 chars
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetFileLengthAsync_InvalidPath_ThrowsArgumentException(string path)
        {
            var proc = new FileProcessor();
            await Assert.ThrowsAsync<ArgumentException>(() => proc.GetFileLengthAsync(path));
        }

        [Fact]
        public async Task GetFileLengthAsync_FileNotFound_ThrowsFileNotFoundException()
        {
            var proc = new FileProcessor();
            await Assert.ThrowsAsync<FileNotFoundException>(() => proc.GetFileLengthAsync(_invalidPath));
        }

        [Fact]
        public async Task ReadLinesAsync_ReturnsAllLines()
        {
            var proc = new FileProcessor();
            var lines = new List<string>();
            await foreach (var line in proc.ReadLinesAsync(_tempPath))
                lines.Add(line);
            Assert.Single(lines);
            Assert.Equal("hello world", lines[0]);
        }

        [Fact]
        public async Task ReadLinesAsync_EmptyFile_ReturnsNoLines()
        {
            var proc = new FileProcessor();
            var lines = new List<string>();
            await foreach (var line in proc.ReadLinesAsync(_emptyPath))
                lines.Add(line);
            Assert.Empty(lines);
        }

        [Fact]
        public async Task ReadLinesAsync_UnicodeFile_ReturnsUnicodeLines()
        {
            var proc = new FileProcessor();
            var lines = new List<string>();
            await foreach (var line in proc.ReadLinesAsync(_unicodePath))
                lines.Add(line);
            Assert.Contains(lines, l => l.Contains("🌍"));
        }

        [Fact]
        public async Task ReadLinesAsync_LargeFile_ReturnsAllLines()
        {
            var proc = new FileProcessor();
            int count = 0;
            await foreach (var line in proc.ReadLinesAsync(_largePath))
                count++;
            Assert.Equal(100000, count);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task ReadLinesAsync_InvalidPath_ThrowsArgumentException(string path)
        {
            var proc = new FileProcessor();
            await Assert.ThrowsAsync<ArgumentException>(async () => {
                await foreach (var _ in proc.ReadLinesAsync(path)) { }
            });
        }

        [Fact]
        public async Task ReadLinesAsync_FileNotFound_ThrowsFileNotFoundException()
        {
            var proc = new FileProcessor();
            await Assert.ThrowsAsync<FileNotFoundException>(async () => {
                await foreach (var _ in proc.ReadLinesAsync(_invalidPath)) { }
            });
        }

        [Fact]
        public async Task GetFileLengthAsync_ConcurrentAccess()
        {
            var proc = new FileProcessor();
            var tasks = Enumerable.Range(0, 10).Select(_ => proc.GetFileLengthAsync(_tempPath));
            var results = await Task.WhenAll(tasks);
            Assert.All(results, r => Assert.Equal(11, r));
        }

        [Fact]
        public async Task ReadLinesAsync_ConcurrentAccess()
        {
            var proc = new FileProcessor();
            var tasks = Enumerable.Range(0, 10).Select(async _ => {
                var lines = new List<string>();
                await foreach (var line in proc.ReadLinesAsync(_tempPath))
                    lines.Add(line);
                return lines;
            });
            var results = await Task.WhenAll(tasks);
            Assert.All(results, lines => Assert.Single(lines));
        }

        [Fact]
        public async Task GetFileLengthAsync_MethodConsistency()
        {
            var proc = new FileProcessor();
            long len1 = await proc.GetFileLengthAsync(_tempPath);
            long len2 = await proc.GetFileLengthAsync(_tempPath);
            Assert.Equal(len1, len2);
        }

        [Fact]
        public async Task ReadLinesAsync_MethodConsistency()
        {
            var proc = new FileProcessor();
            var lines1 = new List<string>();
            await foreach (var line in proc.ReadLinesAsync(_tempPath))
                lines1.Add(line);
            var lines2 = new List<string>();
            await foreach (var line in proc.ReadLinesAsync(_tempPath))
                lines2.Add(line);
            Assert.Equal(lines1, lines2);
        }

        [Fact]
        public async Task GetFileLengthAsync_ThrowsCustomExceptionOnIOError()
        {
            var proc = new FileProcessor();
            // Simulate IO error by opening file exclusively
            using (var fs = new FileStream(_tempPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                await Assert.ThrowsAsync<FileProcessorException>(() => proc.GetFileLengthAsync(_tempPath));
            }
        }

        [Fact]
        public async Task ReadLinesAsync_ThrowsCustomExceptionOnIOError()
        {
            var proc = new FileProcessor();
            using (var fs = new FileStream(_tempPath, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                await Assert.ThrowsAsync<FileProcessorException>(async () => {
                    await foreach (var _ in proc.ReadLinesAsync(_tempPath)) { }
                });
            }
        }

        public void Dispose()
        {
            foreach (var path in new[] { _tempPath, _emptyPath, _unicodePath, _largePath })
                if (File.Exists(path)) File.Delete(path);
        }
    }
}
