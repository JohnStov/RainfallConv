using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace RainfallConv
{
    public class LineParser
    {
        private readonly Regex dateRegex = new Regex(@"(\d{2}\/\d{2}\/\d{2} \d{2}:\d{2})");
        private readonly Regex durationRegex = new Regex(@"Duration\s*=\s*(\d+\.\d+)\s*hours");
        private readonly Regex endOrValueRegex = new Regex(@"((?:\d+\.\d+)|(?:END))");

        enum State
        {
            FindingStart,
            FindingDuration,
            FindingEnd,
            Padding
        }

        private long padItems;
        DateTime startTime = DateTime.Parse("01/01/1971 00:00");
        DateTime currentTime = DateTime.Parse("01/01/1971 00:00");
        double expectedDuration = 0.0;
        long copiedItems = 0;

        public IEnumerable<Tuple<DateTime, double>> ParseLines(IEnumerable<string> lines)
        {
            var state = State.FindingStart;

            foreach (var line in lines)
            {
                var output = ParseLine(line, ref state);

                if (state == State.Padding)
                {
                    for (int i = 0; i < padItems; ++i)
                        yield return GetLineValue("0.000");
                    state = State.FindingDuration;
                }

                if (output != null)
                    yield return output;
            }

            padItems = CalculatePadding(DateTime.Parse("01/01/2001 00:00"));
            for (int i = 0; i < padItems; ++i)
                yield return GetLineValue("0.000");
        }

        private Tuple<DateTime, double> ParseLine(string line, ref State state)
        {
            switch (state)
            {
                case State.FindingDuration:
                    copiedItems = 0;
                    var length =  GetNextDuration(line, ref state);
                    if (length.HasValue)
                        expectedDuration = length.Value;
                    return null;

                case State.FindingEnd:
                    return CopyValue(line, ref state);
                
                case State.FindingStart:
                    var nextDate = GetNextDate(line, ref state);
                    if (nextDate.HasValue)
                    {
                        padItems = CalculatePadding(nextDate.Value);
                        startTime = nextDate.Value;
                    }
                    return null;

                default:
                    return null;
            }
        }

        private long CalculatePadding(DateTime nextDate)
        {
            var padLength = nextDate - startTime;
            var hours = padLength.TotalHours;
            var spanItems = (long)(hours * 12.0);

            if (spanItems < copiedItems)
            {
                var eventDuration = TimeSpan.FromHours(copiedItems/12.0);
                var eventFinish = startTime + eventDuration;
                Console.WriteLine("Bad Data: Event at {0} finished at {1}, next event starts at {2}", startTime, eventFinish, nextDate);
            }

            var paddingItems = spanItems - copiedItems;

            return paddingItems;
        }

        private Tuple<DateTime, double> CopyValue(string line, ref State state)
        {
            Tuple<DateTime, double> result = null;
            
            var match = endOrValueRegex.Match(line);
            if (!match.Success)
                state = State.FindingStart;
            else
            {
                var value = match.Groups[1].Value;
                if (value == "END")
                {
                    state = State.FindingStart;
                    if (copiedItems != (int)(expectedDuration * 12))
                    {
                        Console.WriteLine("Event at {0}: Expected duration {1} hour(s), Real duration {2} hour(s)", startTime, expectedDuration, copiedItems / 12.0);
                    }
                }
                else
                {
                    ++copiedItems;
                    result = GetLineValue(value);
                }
            }

            return result;
        }

        private Tuple<DateTime, double> GetLineValue(string value)
        {
            var result = new Tuple<DateTime, double>(currentTime, double.Parse(value));
            currentTime = currentTime + TimeSpan.FromMinutes(5);
            return result;
        }
        
        private double? GetNextDuration(string line, ref State state)
        {
            currentTime = startTime;
            var match = durationRegex.Match(line);
            if (match.Success)
            {
                state = State.FindingEnd;
                return double.Parse(match.Groups[1].Value);
            }
            return null;
        }

        private DateTime? GetNextDate(string line, ref State state)
        {
            var match = dateRegex.Match(line);
            if (match.Success)
            {
                state = State.Padding;
                var result = DateTime.Parse(match.Groups[1].Value);
               return result;
            }

            return null;
        }
    }
}