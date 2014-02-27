using System.Security;

namespace RainfallConv
{
    class Program
    {
        private static void Main(string[] args)
        {
            var reader = new LineReader();
            var parser = new LineParser();
            using (var writer = new LineWriter(args[1]))
            {
                foreach (var line in parser.ParseLines(reader.GetLines(args[0])))
                    writer.WriteLine(line);
            }
        }
    }
}
