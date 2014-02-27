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
            Beginning,
            FindingStart,
            FindingDuration,
            FindingEnd,
            Padding
        }

        private long padItems;
        DateTime startTime = DateTime.MinValue;
        double duration = 0.0;

        public IEnumerable<string> ParseLines(IEnumerable<string> lines)
        {
            var state = State.Beginning;

            foreach (var line in lines)
            {
                var output = ParseLine(line, ref state);

                if (state == State.Padding)
                {
                    for (int i = 0; i < padItems; ++i)
                        yield return "0.000";
                    state = State.FindingDuration;
                }

                if (output != null)
                    yield return output;
            }
        }

        private string ParseLine(string line, ref State state)
        {
            switch (state)
            {
                case State.Beginning:
                    var start =  GetNextDate(line, ref state);
                    if (start.HasValue)
                        startTime = start.Value;
                    return null;

                case State.FindingDuration:
                    var length =  GetNextDuration(line, ref state);
                    if (length.HasValue)
                        duration = length.Value;
                    return null;

                case State.FindingEnd:
                    return CopyValue(line, ref state);
                
                case State.FindingStart:
                    var nextDate = GetNextDate(line, ref state);
                    if (nextDate.HasValue)
                    {
                        var padLength = nextDate.Value - startTime;
                        padItems = (long)(padLength.TotalHours - duration) * 12;
                        startTime = nextDate.Value;
                    }
                    return null;

                default:
                    return null;
            }
        }

        private string CopyValue(string line, ref State state)
        {
            string result = null;
            
            var match = endOrValueRegex.Match(line);
            if (!match.Success)
                state = State.FindingStart;
            else
            {
                var value = match.Groups[1].Value;
                if (value == "END")
                    state = State.FindingStart;
                else
                    result = value;
            }

            return result;
        }

        private double? GetNextDuration(string line, ref State state)
        {
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
                if (state == State.FindingStart)
                    state = State.Padding;
                else
                    state = State.FindingDuration;
                return DateTime.Parse(match.Groups[1].Value);
            }

            return null;
        }
    }
}