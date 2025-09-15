
using System;
using System.IO;
using FileProcessorLib;
using Xunit;

namespace FileProcessorTests
{
    public class FileProcessorTests : IDisposable
    {
        private readonly string _tempPath;

        public FileProcessorTests()
        {
            _tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            File.WriteAllText(_tempPath, "hello world"); // small file for baseline
        }

        [Fact]
        public void GetFileLength_ReturnsCorrectLength()
        {
            var proc = new FileProcessor();
            int len = proc.GetFileLength(_tempPath);
            Assert.Equal(11, len);
        }

        public void Dispose()
        {
            if (File.Exists(_tempPath)) File.Delete(_tempPath);
        }
    }
}
