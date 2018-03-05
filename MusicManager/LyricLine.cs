using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MusicCollection.MusicManager
{
    class LyricLine
    {
        public LyricLine(string line, TimeSpan startTime)
        {
            Content = line;
            StartTime = startTime;
        }
        public LyricLine(string line)
        {
            if (line.StartsWith("[ti:"))
            {
                Content = SplitInfo(line);
            }
            else if (line.StartsWith("[ar:"))
            {
                Content = "歌手：" + SplitInfo(line);
            }
            //else if (line.StartsWith("[al:"))
            //{
            //    Console.WriteLine(SplitInfo(line));
            //}
            //else if (line.StartsWith("[by:"))
            //{
            //    Console.WriteLine(SplitInfo(line));
            //}
            //else if (line.StartsWith("[offset:"))
            //{
            //    Console.WriteLine(SplitInfo(line));
            //}
            else if (line.Length > 0)
            {
                var pattern = @"\[([0-9.:]*)\]+(.*)";
                var word = line;
                while (Regex.IsMatch(word, pattern))
                {
                    MatchCollection mc = Regex.Matches(word, pattern);
                    TimeSpan time = new TimeSpan();
                    var Iscorrect = TimeSpan.TryParse("00:" + mc[0].Groups[1].Value, out time);
                    word = mc[0].Groups[2].Value;
                    if (Iscorrect)
                    {
                        Content = Regex.Replace(word, @"\[([0-9.:]*)\]", "");
                        StartTime = time;
                    }
                }
            }
        }
        private static string SplitInfo(string line)
        {
            return line.Substring(line.IndexOf(":") + 1).TrimEnd(']');
        }

        public TimeSpan StartTime { get; set; } = new TimeSpan();
        public string Content { get; set; }
    }
}
