//using ServiceStack.Templates;
using SharedComponents.EVE;
using SharedComponents.Extensions;
using SharedComponents.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedComponents.EVE
{
    public sealed class PatternManager
    {
        private static readonly Lazy<PatternManager> lazy =
            new Lazy<PatternManager>(() => new PatternManager());

        public static PatternManager Instance { get { return lazy.Value; } }

        private PatternManager()
        {
        }

        private Random _random = new Random();

        private String XXorYY
        {
            get
            {
                return _random.NextDouble() > 0.5d ? "XX" : "YY";
            }
        }

        public String GenerateNewPattern(int hoursPerWeek, int daysOffPerWeek, List<int> excludedDailyHours)
        {
            if (hoursPerWeek < 7 - daysOffPerWeek)
                throw new Exception("HoursPerWeek needs to be larger than 7 minus daysOffPerWeek.");

            if (daysOffPerWeek > 6)
                throw new Exception("Come on bruh, the bot wants to work at least one day.");

            var dayOfWeekRelationCopy = new Dictionary<string, DayOfWeek>(PatternEval.DayOfWeekRelation);

            // Remove random days off from the dict
            int n = daysOffPerWeek;
            string[] keys = new string[dayOfWeekRelationCopy.Count];
            dayOfWeekRelationCopy.Keys.CopyTo(keys, 0);


            for (int i = 0; i < keys.Length; i++)
            {
                int j = _random.Next(i, keys.Length);
                string temp = keys[i];
                keys[i] = keys[j];
                keys[j] = temp;
            }

            for (int i = 0; i < n; i++)
            {
                dayOfWeekRelationCopy.Remove(keys[i]);
            }

            var output = "";
            foreach (var kv in dayOfWeekRelationCopy)
            {

                // Generate list of available hours
                var allHours = new List<Tuple<int, bool>>();
                var availableBlocks = new List<List<Tuple<int, bool>>>();
                var currentBlock = new List<Tuple<int, bool>>();
                for (int i = 0; i < 24; i++)
                {
                    if (!excludedDailyHours.Contains(i))
                    {
                        allHours.Add(Tuple.Create(i, false));
                        currentBlock.Add(Tuple.Create(i, false));
                    }
                    else
                    {
                        if (currentBlock.Any())
                            availableBlocks.Add(currentBlock);
                        currentBlock = new List<Tuple<int, bool>>();
                    }
                }

                availableBlocks.Add(currentBlock);
                availableBlocks = availableBlocks.Where(e => e.Count > 0).OrderByDescending(e => e.Count).ToList();

                var availDays = dayOfWeekRelationCopy.Count;
                var availHours = allHours.Count;
                var totalAvailHours = availDays * availHours;

                var hoursPerDay = hoursPerWeek / availDays;
                var remainingHours = hoursPerWeek;

                var hourOffSet = 1 * (Util.Coinflip() ? 1 : -1);
                var currentHoursPerDay = hoursPerDay + hourOffSet;
                Debug.WriteLine($"currentHoursPerDay [{currentHoursPerDay}]");

                // Choose from avail hours until
                while (currentHoursPerDay > 0)
                {

                    // If all blocks are filled already, ensure the loop terminates
                    if (availableBlocks.All(e => e.All(f => f.Item2)))
                        break;

                    // Iterate over the given blocks
                    foreach (var block in availableBlocks)
                    {

                        if (block.Any(e => e.Item2))
                        {
                            // Get the index of a true value tuple item and go randomly upwards or downwards until end
                            var anyTrue = block.FirstOrDefault(e => e.Item2);
                            var indexAnyTrue = block.IndexOf(anyTrue);
                            var coinflip = Util.Coinflip();
                            if (coinflip)
                            {
                                // Go upwards until end or first false value and set it true
                                for (int i = indexAnyTrue; i < block.Count; i++)
                                {
                                    var currentItem = block[i];
                                    if (!currentItem.Item2)
                                    {
                                        block[block.IndexOf(currentItem)] = Tuple.Create(currentItem.Item1, true);
                                        currentHoursPerDay--;
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                // Go downwards until end or first false value and set it true
                                for (int i = indexAnyTrue; i >= 0; i--)
                                {
                                    var currentItem = block[i];
                                    if (!currentItem.Item2)
                                    {
                                        block[block.IndexOf(currentItem)] = Tuple.Create(currentItem.Item1, true);
                                        currentHoursPerDay--;
                                        break;
                                    }
                                }
                            }

                        }
                        else // If there was no random startpoint selected yet, choose one
                        {
                            var randomItem = block.Random();

                            if (randomItem == null)
                            {
                                Debug.WriteLine("Random item was null");
                            }

                            block[block.IndexOf(randomItem)] = Tuple.Create(randomItem.Item1, true);
                            currentHoursPerDay--;
                        }
                    }
                }

                Debug.WriteLine("---------------");
                var x = 0;
                foreach (var k in availableBlocks)
                {
                    Debug.WriteLine($"{x} -- " + string.Join(",", k));
                    x++;
                }

                // Now let's build that thing into our format
                foreach (var block in availableBlocks)
                {

                    // Force a minimum of two avail hours
                    if (block.Count(e => e.Item2) < 2)
                        continue;

                    var first = -1;
                    var last = -1;
                    // Find the first true value of the block
                    for (int i = 0; i < block.Count; i++)
                    {
                        if (block[i].Item2)
                        {
                            first = i;
                            break;
                        }
                    }
                    // Find the last true value block
                    for (int i = block.Count - 1; i >= 0; i--)
                    {
                        if (block[i].Item2)
                        {
                            last = i;
                            break;
                        }
                    }

                    // If we've set both values, build the pattern
                    if (first != -1 && last != -1)
                    {
                        output += $"{{{kv.Key}[{block[first].Item1.ToString("00")}:{XXorYY}][{block[last].Item1.ToString("00")}:{XXorYY}]}},";
                    }
                }
            }

            if (output.Any() && output.Last() == ',')
                output = output.Remove(output.Length - 1);
            return output;
        }
    }

}