using System;
using System.IO;
using System.Threading.Tasks;
using FileProcessorLib;
using Xunit;

namespace FileProcessorTests
{
    public class MockFileReader : IFileReader
    {
        private readonly string _content;
        private readonly Exception _exception;
        public MockFileReader(string content, Exception exception = null)
        {
            _content = content;
            _exception = exception;
        }
        public Task<string> ReadAllTextAsync(string path)
        {
            if (_exception != null) throw _exception;
            return Task.FromResult(_content);
        }
    }

    public class FileProcessorTests
    {
        [Fact]
        public async Task GetFileLengthAsync_ReturnsCorrectLength()
        {
            var mockReader = new MockFileReader("hello world");
            var proc = new FileProcessor(mockReader);
            int len = await proc.GetFileLengthAsync("any-path.txt");
            Assert.Equal(11, len);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetFileLengthAsync_NullOrEmptyPath_ThrowsArgumentException(string path)
        {
            var mockReader = new MockFileReader("irrelevant");
            var proc = new FileProcessor(mockReader);
            await Assert.ThrowsAsync<ArgumentException>(() => proc.GetFileLengthAsync(path));
        }

        [Fact]
        public async Task GetFileLengthAsync_FileNotFound_ReturnsZero()
        {
            var mockReader = new MockFileReader(null, new FileNotFoundException());
            var proc = new FileProcessor(mockReader);
            int len = await proc.GetFileLengthAsync("missing.txt");
            Assert.Equal(0, len);
        }

        [Fact]
        public async Task GetFileLengthAsync_IOException_ThrowsInvalidOperationException()
        {
            var mockReader = new MockFileReader(null, new IOException("IO error"));
            var proc = new FileProcessor(mockReader);
            await Assert.ThrowsAsync<InvalidOperationException>(() => proc.GetFileLengthAsync("io.txt"));
        }

        [Fact]
        public async Task GetFileLengthAsync_NullContent_ReturnsZero()
        {
            var mockReader = new MockFileReader(null);
            var proc = new FileProcessor(mockReader);
            int len = await proc.GetFileLengthAsync("null.txt");
            Assert.Equal(0, len);
        }
    }
}
