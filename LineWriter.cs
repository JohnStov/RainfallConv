using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;

namespace RainfallConv
{
    public class LineWriter : IDisposable
    {
        private readonly StreamWriter writer;

        private Dictionary<DateTime, double> values = new Dictionary<DateTime, double>();

        public LineWriter(string path)
        {
            writer = new StreamWriter(path);
        }

        public void WriteLine(DateTime time, double value)
        {
            if (!values.ContainsKey(time))
                values.Add(time, value);
            else
                values[time] = values[time] + value;
        }

        public void Dispose()
        {
            var sortedKeys = values.Keys.ToList();
            sortedKeys.Sort();

            foreach(var key in sortedKeys)
                writer.WriteLine(string.Format("{0},{1:0.000}", key.Month, values[key]));
            
            writer.Flush();
            writer.Dispose();
        }
    }
}