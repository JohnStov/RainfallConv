using System;
using System.IO;

namespace RainfallConv
{
    public class LineWriter : IDisposable
    {
        private readonly StreamWriter writer;

        public LineWriter(string path)
        {
            writer = new StreamWriter(path);
        }

        public void WriteLine(string line)
        {
            writer.WriteLine(line);
        }

        public void Dispose()
        {
            writer.Flush();
            writer.Dispose();
        }
    }
}