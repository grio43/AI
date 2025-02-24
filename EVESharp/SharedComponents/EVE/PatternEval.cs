using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SharedComponents.EVE
{
    public static class PatternEval
    {
        private static Random _rnd = new Random();

        public static Dictionary<string, DayOfWeek> DayOfWeekRelation = new Dictionary<string, DayOfWeek>()
        {
            {"mo",DayOfWeek.Monday},
            {"tu",DayOfWeek.Tuesday},
            {"we",DayOfWeek.Wednesday},
            {"th",DayOfWeek.Thursday},
            {"fr",DayOfWeek.Friday},
            {"sa",DayOfWeek.Saturday},
            {"su",DayOfWeek.Sunday},
        };

        public static string GenerateOutput(string input)
        {
            string pattern = @"(?i)(\{(mo|tu|we|th|fr|sa|su|\?)\[[0-2]\d:([0-5]|(X|Y))(\d|(X|Y))\]\[[0-2]\d:([0-5]|(X|Y))(\d|(X|Y))\]\})";
            string patternOut = string.Empty;

            var matches = Regex.Matches(input, pattern);

            int n = 0;
            foreach (Match m in matches)
            {

                int a = 1 == 2 ? 1 : 0;
                DayOfWeek? dow = m.Groups[2].Value.Equals("?") ? (DayOfWeek?)null : DayOfWeekRelation[m.Groups[2].Value];

                var s = ParseStartEndTime(m.Value);

                var result = "{" + m.Groups[2].Value + $"[{s.Item1[0].ToString() + s.Item1[1].ToString() + ":" + s.Item1[2].ToString() + s.Item1[3].ToString()}][{s.Item2[0].ToString() + s.Item2[1].ToString() + ":" + s.Item2[2].ToString() + s.Item2[3].ToString()}]" + "}";

                patternOut += n < matches.Count - 1 ? result + "," : result;
                n++;
            }
            return patternOut;
        }

        public static bool IsAnyPatternMatchingDatetime(EveAccount eA, DateTime dt, int addMinutesToEnd = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(eA.Pattern))
                {
                    if (eA.DebugScheduler)
                        Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] if (string.IsNullOrEmpty(eA.Pattern))");

                    return false;
                }

                string pattern = @"(?i)(\{(mo|tu|we|th|fr|sa|su|\?)\[[0-2]\d:([0-5]|(X|Y))(\d|(X|Y))\]\[[0-2]\d:([0-5]|(X|Y))(\d|(X|Y))\]\})";

                if (String.IsNullOrEmpty(eA.Pattern)) return false;

                var matches = Regex.Matches(eA.Pattern, pattern);

                foreach (Match m in matches)
                {
                    DayOfWeek? dow = m.Groups[2].Value.Equals("?")
                        ? (DayOfWeek?)null
                        : DayOfWeekRelation[m.Groups[2].Value];

                    string timePatters = @"(?i)(\[[0-2]\d:([0-5]|(X|Y))(\d|(X|Y))\])";
                    var timeMatches = Regex.Matches(m.Value, timePatters);
                    var startTime = timeMatches[0].Value.Replace("[", "").Replace("]", "");
                    var endTime = timeMatches[1].Value.Replace("[", "").Replace("]", "");

                    if (eA.DebugScheduler)
                    {
                        Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] pattern [" + eA.Pattern + "]");
                        Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] DayOfWeek [" + m.Value + "]");
                        Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] timeMatches [" + timeMatches[0] + "][" + timeMatches[1] + "]");
                        Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] startTime [" + startTime + "]");
                        Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] endTime [" + endTime + "]");
                    }

                    var startTimeSpan = TimeSpan.ParseExact(startTime, @"hh\:mm", CultureInfo.InvariantCulture);
                    var endTimeSpan = TimeSpan.ParseExact(endTime, @"hh\:mm", CultureInfo.InvariantCulture);

                    if (dow == null || dt.DayOfWeek == dow.Value)
                    {
                        if (dt.TimeOfDay >= startTimeSpan && dt.TimeOfDay <= endTimeSpan.Add(new TimeSpan(0, 0, addMinutesToEnd, 0)))
                        {
                            if (eA.DebugScheduler)
                            {
                                Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] TRUE");
                            }

                            return true;
                        }

                        if (eA.DebugScheduler) Cache.Instance.Log("IsAnyPatternMatchingDatetime [" + eA.Num + "][" + eA.MaskedAccountName + "][" + eA.MaskedCharacterName + "] FALSE");
                        return false;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("EveAccount GUID [" + eA.GUID + "] Exception [" + ex + "]");
                return false;
            }
        }
        public static Tuple<int[], int[]> ParseStartEndTime(string input)
        {
            string timePatters = @"(?i)(\[[0-2]\d:([0-5]|(X|Y))(\d|(X|Y))\])";
            var matches = Regex.Matches(input, timePatters);
            var matchStart = matches[0].Value.Replace("[", "").Replace("]", "").Replace(":", "");
            var matchStartList = new List<char>() { { matchStart[0] }, { matchStart[1] }, { matchStart[2] }, { matchStart[3] } };
            var matchStarResultArray = new int[4];
            var i = 0;
            foreach (var c in matchStartList)
            {

                if (Char.IsDigit(c))
                {
                    matchStarResultArray[i] = (int)Char.GetNumericValue(c);
                }
                else
                {

                    int max = 0;
                    if (i == 0 || i == 2)
                    {
                        max = 5;
                        if (c.ToString().ToLower().Equals("y"))
                            max = 3;
                        matchStarResultArray[i] = _rnd.Next(0, max + 1);
                    }
                    else
                    {
                        max = 9;
                        if (c.ToString().ToLower().Equals("y"))
                            max = 5;
                        matchStarResultArray[i] = _rnd.Next(0, max + 1);
                    }
                }
                i++;
            }

            var matchEnd = matches[1].Value.Replace("[", "").Replace("]", "").Replace(":", "");
            var matchEndList = new List<char>() { { matchEnd[0] }, { matchEnd[1] }, { matchEnd[2] }, { matchEnd[3] } };
            var matchEndResultArray = new int[4];

            i = 0;
            foreach (var c in matchEndList)
            {

                if (Char.IsDigit(c))
                {
                    matchEndResultArray[i] = (int)Char.GetNumericValue(c);
                }
                else
                {

                    int max = 0;
                    if (i == 0 || i == 2)
                    {
                        max = 5;
                        if (c.ToString().ToLower().Equals("y"))
                            max = 3;
                        matchEndResultArray[i] = _rnd.Next(0, max + 1);
                    }
                    else
                    {
                        max = 9;
                        if (c.ToString().ToLower().Equals("y"))
                            max = 5;
                        matchEndResultArray[i] = _rnd.Next(0, max + 1);
                    }
                }
                i++;
            }

            return new Tuple<int[], int[]>(matchStarResultArray, matchEndResultArray);
        }
    }
}
