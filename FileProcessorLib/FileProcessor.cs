
using System;

namespace FileProcessorLib
{
    // Smelly implementation: synchronous file read which blocks thread pool for large files.
    public class FileProcessor
    {
        public int GetFileLength(string path)
        {
            // Intentionally using blocking IO (File.ReadAllText) to illustrate a fix to async streaming.
            var text = System.IO.File.ReadAllText(path);
            return text.Length;
        }
    }
}
