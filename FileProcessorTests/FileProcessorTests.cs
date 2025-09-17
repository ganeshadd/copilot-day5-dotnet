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
        public MockFileReader(string content) { _content = content; }
        public Task<string> ReadAllTextAsync(string path) => Task.FromResult(_content);
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
    }
}
