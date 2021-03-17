using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace LambdaExpressionUtils
{
    public class Segment
    {
        public bool IsMatch { get; private set; }
        public string Value { get; private set; }
        public Segment(bool isMatch, string value) {
            IsMatch = isMatch;
            Value = value;
        }
    }

    public static class StringExtensions
    {
        public static List<Segment> GetSegments(this string template, string pattern = @"{(\d)}") {
            var segments = new List<Segment>();

            var re = new Regex(pattern);

            var lastIndex = 0;
            var counter = 0;
            string segmentValue;

            var result = re.Match(template);
            while (result.Success) {
                segmentValue = template.Substring(lastIndex, result.Index - lastIndex);
                if (segmentValue != string.Empty) {
                    segments.Add(new Segment(false, segmentValue));
                }

                segments.Add(new Segment(true, result.Value));

                lastIndex = result.Index + result.Length;
                counter++;
                result = result.NextMatch();
            }

            segmentValue = template.Substring(lastIndex);
            if (segmentValue != string.Empty) {
                segments.Add(new Segment(false, segmentValue));
            }

            return segments;
        }
    }
}
