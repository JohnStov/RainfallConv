using System.Collections.Generic;
using System.IO;

namespace RainfallConv
{
    public class LineReader
    {
        public IEnumerable<string> GetLines(string path)
        {
            var reader = new StreamReader(path);
            while (!reader.EndOfStream)
                yield return reader.ReadLine();
        }
    }
}
