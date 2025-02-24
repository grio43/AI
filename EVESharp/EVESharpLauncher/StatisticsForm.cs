/*
 * Created by SharpDevelop.
 * User: duketwo
 * Date: 07.05.2016
 * Time: 16:17
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using OxyPlot;
using OxyPlot.Annotations;
using OxyPlot.Axes;
using OxyPlot.Series;
using SharedComponents.EVE;
using SharedComponents.Extensions;
using SharedComponents.Utility;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
//using ServiceStack.OrmLite;
//using SharedComponents.SQLite;
using HorizontalAlignment = OxyPlot.HorizontalAlignment;

namespace EVESharpLauncher
{
    /// <summary>
    ///     Description of StatisticsForm.
    /// </summary>
    public partial class StatisticsForm : Form
    {
        #region Fields

        private const string ALL_AVAILABLE = "All available";

        private ConcurrentDictionary<string, IEnumerable<StatisticsEntry>> _generateStatisticLists;

        #endregion Fields

        #region Constructors

        public StatisticsForm()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            _statsEntries = new List<StatisticsEntry>();
            _distintCharNames = new List<string>();
            InitializeComponent();
        }

        #endregion Constructors

        #region Enums

        private enum EarningType
        {
            Hourly,
            Daily,
            Monthly,
            Yearly,
        };

        #endregion Enums

        #region Properties

        private string CurrentSelectedCharacter { get; set; }
        private string CurrentSelectedStatistic { get; set; }
        private int? CurrentSelectedTimeDomain { get; set; }
        private int[] TimeDomainValues { get; set; }


        private List<StatisticsEntry> _statsEntries;
        private List<String> _distintCharNames;

        private List<StatisticsEntry> StatsEntries(bool forceReload = false)
        {
            if (_statsEntries.Any() && !forceReload)
                return _statsEntries;

            //using (var rc = ReadConn.Open())
            //{
            //    _statsEntries = rc.DB.Select<StatisticsEntry>();
            //}

            return _statsEntries;
        }

        #endregion Properties

        #region Methods

        public List<String> GetDistintCharNames(bool forceReload = false)
        {
            if (_distintCharNames.Any() && !forceReload)
                return _distintCharNames;

            //_distintCharNames = StatsEntries().Where(c => !string.IsNullOrEmpty(c.Charname)).Select(w => w.Charname).Where(e => Cache.Instance.EveAccountSerializeableSortableBindingList.List.Any(x => x.CharacterName.Equals(e))).ToList();
            _distintCharNames = StatsEntries().Where(c => !string.IsNullOrEmpty(c.Charname)).Select(w => w.Charname).Distinct().ToList();
            return _distintCharNames;
        }

        public IEnumerable<StatisticsEntry> CombinedStatisticList()
        {
            var ret = new List<StatisticsEntry>();
            foreach (var kv in StatisticListsForAvailableCharacters())
                ret.AddRange(kv.Value);
            return ret;
        }

        public void ForceReload()
        {
            StatisticListsForAvailableCharacters(true);
            RedrawCurrentSelectedStatistic();
        }

        public IEnumerable<StatisticsEntry> GetStatisticListForCurrentSelection()
        {
            var ret = CurrentSelectedCharacter != null
                ? StatisticListsForAvailableCharacters()[CurrentSelectedCharacter]
                : CombinedStatisticList();
            return CurrentSelectedTimeDomain != null ? ret.Where(e => e.Date > DateTime.UtcNow.AddDays((int)-CurrentSelectedTimeDomain + 1)) : ret;
        }

        public ConcurrentDictionary<string, IEnumerable<StatisticsEntry>> StatisticListsForAvailableCharacters(bool forceReload = false)
        {
            //if (_GenerateStatisticLists != null && !forceReload)
            //    return _GenerateStatisticLists;

            var availCharsDict = GetDistintCharNames(forceReload).ToList();

            var ret = new ConcurrentDictionary<string, IEnumerable<StatisticsEntry>>();

            List<StatisticsEntry> statsBase = StatsEntries(forceReload);

            var tx = new Thread(() =>
            {
                var threads = new List<Thread>();
                foreach (var charName in availCharsDict)
                {
                    var t = new Thread(() =>
                    {
                        var stats = statsBase.Where(w => w.Charname == charName && !String.IsNullOrWhiteSpace(w.Mission) && !String.IsNullOrEmpty(w.Mission))
                            .ToList();
                        var statsSortedByDateDesc = stats.OrderByDescending(f => f.Date).ToList();
                        if (statsSortedByDateDesc.Any())
                        {
                        try
                        {
                                var item = statsSortedByDateDesc.FirstOrDefault(s => s.ISKLootHangarItems != 0);
                                var maxISKLootHangarItemsValue = item != null ? item.ISKLootHangarItems : statsSortedByDateDesc.Max(s => s.ISKLootHangarItems);

                                var index = 0;
                                foreach (var s in statsSortedByDateDesc)
                            {
                                if (s.ISKLootHangarItems == 0)
                                    statsSortedByDateDesc[index].ISKLootHangarItems = maxISKLootHangarItemsValue;
                                if (s.ISKLootHangarItems < maxISKLootHangarItemsValue)
                                    maxISKLootHangarItemsValue = s.ISKLootHangarItems;
                                index++;
                            }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e);
                            }

                            try
                            {
                                var first = statsSortedByDateDesc.FirstOrDefault();
                                var last = statsSortedByDateDesc.LastOrDefault();
                                var current = DateTime.UtcNow;

                            while (current.Date > last.Date)
                            {
                                if (!statsSortedByDateDesc.Any(s => s.Date.Date == current.Date))
                                {
                                        var newItem = first.Copy();
                                    newItem.Date = current.Date;
                                    newItem.Mission = null;
                                    newItem.Isk = 0;
                                    newItem.IskReward = 0;
                                    newItem.LP = 0;
                                    newItem.Loot = 0;
                                    newItem.Date = newItem.Date.AddDays(1).AddMilliseconds(-1);
                                    statsSortedByDateDesc.Add(newItem);
                                }
                                else
                                {
                                        first = statsSortedByDateDesc.FirstOrDefault(s => s.Date.Date == current.Date);
                                }
                                current = current.AddDays(-1);
                            }

                            statsSortedByDateDesc = statsSortedByDateDesc.OrderByDescending(f => f.Date).ToList();
                        }
                            catch (Exception e)
                        {
                                Console.WriteLine(e);
                        }
                        }

                        ret[charName] = statsSortedByDateDesc;
                    });
                    threads.Add(t);
                }

                foreach (var t in threads)
                {
                    t.Start();
                }

                foreach (var t in threads)
                {
                    t.Join();
                }
            });

            tx.Start();
            while (tx.IsAlive)
            {
                Thread.Sleep(1);
                Application.DoEvents();
            }

            _generateStatisticLists = ret;
            return null; //_GenerateStatisticLists;
        }

        private void ButtonReload_Click(object sender, EventArgs e)
        {
            ForceReload();
        }

        private void EarningsColumnSeries(EarningType earningType)
        {
            var list = GetStatisticListForCurrentSelection();

            var et = earningType == EarningType.Hourly
                ? "yyyyMMddHH"
                : earningType == EarningType.Daily
                    ? "yyyyMMdd"
                    : earningType == EarningType.Monthly
                        ? "yyyyMM"
                        : "yyyy";
            var title = earningType == EarningType.Hourly
                ? "EARNINGS_HOURLY"
                : earningType == EarningType.Daily
                    ? "EARNINGS_DAILY"
                    : earningType == EarningType.Monthly
                        ? "EARNINGS_MONTHLY"
                        : "EARNINGS_YEARLY";

            var hours = earningType == EarningType.Hourly
                ? 1 * 60
                : earningType == EarningType.Daily
                    ? 24 * 60
                    : earningType == EarningType.Monthly
                        ? 24 * 30 * 60
                        : 24 * 30 * 12 * 60;

            var x = list.Where(m => m.Mission != null).GroupBy(s => new { date = s.Date.ToString(et), charname = s.Charname });

            var k = x.Select(g => new
            {
                Date = g.Key.date,
                Charname = g.Key.charname,
                //Earnings =
                //        g.OrderBy(p => p.Date).Last().ISKLootHangarItems + g.OrderBy(p => p.Date).Last().ISKWallet +
                //        g.OrderBy(p => p.Date).Last().TotalLP * StatisticsEntryCSV.GetISKperLP() -
                //        (g.OrderBy(p => p.Date).First().ISKLootHangarItems + g.OrderBy(p => p.Date).First().ISKWallet +
                //         g.OrderBy(p => p.Date).First().TotalLP * StatisticsEntryCSV.GetISKperLP()),
                //Earnings = (g.Sum(p => (long)p.Isk) + g.Sum(p => (long)p.IskReward) + g.Sum(p => p.Loot) + (g.Sum(p => p.LP) * StatisticsEntryCSV.GetISKperLP())),
                Loot = earningType == EarningType.Hourly ? g.Sum(p => p.Loot) * (hours / (double)g.Sum(p => p.TotalMissionTime)) : g.Sum(p => p.Loot),
                LP = earningType == EarningType.Hourly
                    ? g.Sum(p => p.LP) * StatisticsEntry.GetISKperLP() * (hours / (double)g.Sum(p => p.TotalMissionTime))
                    : g.Sum(p => p.LP) * StatisticsEntry.GetISKperLP(),
                Isk = earningType == EarningType.Hourly
                    ? (g.Sum(p => p.Isk) + g.Sum(p => (long)p.IskReward)) * (hours / (double)g.Sum(p => p.TotalMissionTime))
                    : g.Sum(p => p.Isk) + g.Sum(p => (long)p.IskReward),
                TotalEarnings = earningType == EarningType.Hourly
                    ? (g.Sum(p => p.Isk) + g.Sum(p => (long)p.IskReward) + g.Sum(p => p.Loot) + g.Sum(p => p.LP) * StatisticsEntry.GetISKperLP()) *
                      (hours / (double)g.Sum(p => p.TotalMissionTime))
                    : g.Sum(p => p.Isk) + g.Sum(p => (long)p.IskReward) + g.Sum(p => p.Loot) + g.Sum(p => p.LP) * StatisticsEntry.GetISKperLP(),
            });
            var filter = k
                .GroupBy(s => new { date = s.Date })
                .Select(g => new
                {
                    Date = g.Key.date,
                    TotalEarnings = g.Sum(p => p.TotalEarnings),
                    Isk = g.Sum(p => p.Isk),
                    Loot = g.Sum(p => p.Loot),
                    LP = g.Sum(p => p.LP),
                })
                .OrderBy(l => l.Date);


            if (!filter.Any())
            {
                Cache.Instance.Log("The list is empty.");
                return;
            }

            var lblFormat = earningType == EarningType.Hourly ? "{0:.000}" : "{0:.00}";

            var sourceTotal = new List<ColumnItem>();
            var sourceLoot = new List<ColumnItem>();
            var sourceIsk = new List<ColumnItem>();
            var sourceLp = new List<ColumnItem>();
            foreach (var t in filter)
            {
                sourceTotal.Add(new ColumnItem((double)t.TotalEarnings / 1000000000));
                sourceLoot.Add(new ColumnItem((double)t.Loot / 1000000000));
                sourceIsk.Add(new ColumnItem((double)t.Isk / 1000000000));
                sourceLp.Add(new ColumnItem((double)t.LP / 1000000000));
            }

            var model = new PlotModel
            {
                Title = title,
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
            };

            var seriesLoot = new ColumnSeries
            {
                Title = "Loot",
                ItemsSource = sourceLoot,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = lblFormat,
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var seriesISK = new ColumnSeries
            {
                Title = "ISK",
                ItemsSource = sourceIsk,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = lblFormat,
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var seriesLP = new ColumnSeries
            {
                Title = "LP",
                ItemsSource = sourceLp,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = lblFormat,
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var seriesTotal = new ColumnSeries
            {
                Title = "Total",
                ItemsSource = sourceTotal,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = lblFormat,
                //FillColor = OxyColor.FromRgb(0, 204, 204),
                FillColor = OxyColor.FromRgb(105, 105, 105),
                FontWeight = FontWeights.Bold,
                Font = "Tahoma",
                TextColor = OxyColor.FromRgb(255, 255, 255),
                FontSize = 9,
                ColumnWidth = 0.5,
            };

            model.Series.Add(seriesISK);
            model.Series.Add(seriesLoot);
            model.Series.Add(seriesLP);
            model.Series.Add(seriesTotal);

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Axis",
                ItemsSource = filter.Select(s => s.Date).ToList(),
                FontSize = 10,
                GapWidth = 0.1,
            });

            model.Axes.Add(new LinearAxis()
            {
                IsAxisVisible = false,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });

            plot1.Model = model;
        }

        private void EARNINGSDAILYChartTypeColumnSeriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;
            try
            {
                EarningsColumnSeries(EarningType.Daily);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void EARNINGSHOURLYChartTypeColumnSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;
            try
            {
                EarningsColumnSeries(EarningType.Hourly);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void EARNINGSMONTHLYChartTypeColumnSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;
            try
            {
                EarningsColumnSeries(EarningType.Monthly);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void EARNINGSYEARLYChartTypeColumnSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;
            try
            {
                EarningsColumnSeries(EarningType.Yearly);
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void IskPerHourMissionsBarSeries()
        {
            var list = GetStatisticListForCurrentSelection().Where(m => m.Mission != null);
            var filter = list.GroupBy(s => new { mission = s.Mission })
                .Select(g => new
                {
                    Mission = g.Key.mission,
                    MillionIskPerHourAverage = g.Average(p => p.MillionISKperHour),
                })
                .OrderBy(l => l.MillionIskPerHourAverage).ToList();

            if (!filter.Any())
            {
                Cache.Instance.Log("The list is empty.");
                return;
            }

            var source = new List<BarItem>();
            var total = filter.Count();
            var i = 1;
            foreach (var statEntry in filter)
            {
                var p = (int)Math.Round((double)(100 * i) / total);
                var color = p < 33.33 ? OxyColors.LightPink : p < 66.66 ? OxyColors.Yellow : OxyColors.LightGreen;
                source.Add(new BarItem { Value = (double)statEntry.MillionIskPerHourAverage, Color = color });
                i++;
            }

            var barSeries = new BarSeries
            {
                ItemsSource = source,
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "[{0:.00}] * 10^6 ISK/h",
                FontWeight = FontWeights.Normal,
                Font = "Lucida Console",
            };

            var pm = new PlotModel
            {
                Title = "MISSIONS_ISK_PER_HOUR",
                PlotType = PlotType.XY,
                Background = OxyColors.White,
                DefaultFont = "Lucida Console",
            };

            pm.Series.Add(barSeries);

            var totalAmountMissions = list.Count();
            var missionsWithPercentage = new List<string>();
            var missions = filter.Select(s => s.Mission).ToList();

            if (missions.Count == 0)
            {
                Cache.Instance.Log("The mission list is empty.");
                return;
            }

            var maxLengthMissionName = 0;

            try
            {
                maxLengthMissionName = missions.Max(m => m.Length) + 1;
            }
            catch (Exception)
            {

            }
            
            double avgIskPerHourAllMissions = 0;
            foreach (var mission in missions)
            {
                var currentMissionAmount = list.Count(m => m.Mission.Equals(mission));
                var sumTimeCurrentMission = list.Where(m => m.Mission.Equals(mission)).Sum(s => s.TotalMissionTime);
                var sumLostDrones = list.Where(m => m.Mission.Equals(mission)).Sum(s => s.LostDrones);
                var sumPanics = list.Where(m => m.Mission.Equals(mission)).Sum(s => s.Panics);
                var avgTimeCurrentMission = sumTimeCurrentMission / currentMissionAmount;
                var avgLostDrones = (double)((double)sumLostDrones / (double)currentMissionAmount);
                var avgPanics = (double)((double)sumPanics / (double)currentMissionAmount);
                var p = (double)currentMissionAmount / (double)totalAmountMissions * 100;
                var pString = "[" + String.Format("{0:0.00}", p) + "%]";
                var avgTimeCurrentMissionString = "[" + avgTimeCurrentMission + " m]";
                var avgLostDronesString = "[" + String.Format("{0:0.00}", avgLostDrones) + " lost drones]";
                var panicsString = "[" + String.Format("{0:0.00}", avgPanics) + " panics]";
                avgIskPerHourAllMissions += p / 100 * Convert.ToDouble(list.Where(m => m.Mission.Equals(mission)).Average(m => m.MillionISKperHour));
                missionsWithPercentage.Add(String.Format("{0," + (maxLengthMissionName * -1).ToString() + "}{1,-8}{2,-7}{3,-20}{4,0}", mission, pString,
                    avgTimeCurrentMissionString, avgLostDronesString, panicsString));
            }

            decimal max = 0.0m;
            try
            {
                max = filter.Max(f => f.MillionIskPerHourAverage).Value;
            }
            catch (Exception)
            {

            }
            

            pm.Annotations.Add(
                new TextAnnotation
                {
                    TextPosition = new DataPoint((int)max, 0),
                    TextRotation = 0,
                    TextHorizontalAlignment = HorizontalAlignment.Right,
                    TextVerticalAlignment = VerticalAlignment.Bottom,
                    Text = string.Format("Avg [{0:0.00}]", avgIskPerHourAllMissions) + " * 10^6 ISK/h"
                }
            );

            pm.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Right,
                Key = "Axis",
                ItemsSource = missionsWithPercentage,
                Font = "Lucida Console",
                IsZoomEnabled = false,
            });

            pm.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Top,
                Key = "Axis",
                Font = "Lucida Console",
                IsZoomEnabled = false,
            });

            plot1.Model = pm;
        }

        private void IskPerHourMissionsToolStripMenuItemClick(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;
            try
            {
                IskPerHourMissionsBarSeries();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void RedrawCurrentSelectedStatistic()
        {
            foreach (var item in statisticsToolStripMenuItem.DropDownItems)
            {
                var i = (ToolStripMenuItem)item;
                if (i.Text.Equals(CurrentSelectedStatistic))
                    i.PerformClick();
            }
        }

        private void SelectCharacterToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            foreach (var item in selectCharacterToolStripMenuItem.DropDownItems)
            {
                if (item.GetType() == typeof(ToolStripSeparator))
                    continue;

                var i = (ToolStripMenuItem)item;

                if (CurrentSelectedCharacter == null && i.Text.Equals(ALL_AVAILABLE))
                {
                    i.BackColor = Color.LightGray;
                    continue;
                }

                if (CurrentSelectedCharacter != null && i.Text.Equals(CurrentSelectedCharacter))
                    i.BackColor = Color.LightGray;
                else
                    i.BackColor = SystemColors.Control;
            }
        }

        private void STANDINGSDAILYChartTypeColumnSeriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;

            try
            {
                StandingsDailyColumnSeries();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void StandingsDailyColumnSeries()
        {
            var list = GetStatisticListForCurrentSelection();

            var filter = list.GroupBy(s => new { date = s.Date.ToString("yyyyMMdd"), charname = s.Charname })
                .Select(g => new
                {
                    Date = g.Key.date,
                    Charname = g.Key.charname,
                    MinStandingAgentCorpFaction = g.Min(k => k.MinStandingAgentCorpFaction),
                    MaxFactionStanding = g.Min(k => k.FactionStanding),
                })
                .OrderBy(l => l.Date);

            var sourceTotalMinStandingAgentCorpFaction = new List<ColumnItem>();
            var sourceTotalMaxFactionStanding = new List<ColumnItem>();

            foreach (var t in filter)
            {
                sourceTotalMinStandingAgentCorpFaction.Add(new ColumnItem((double)t.MinStandingAgentCorpFaction));
                sourceTotalMaxFactionStanding.Add(new ColumnItem((double)t.MaxFactionStanding));
            }

            if (!filter.Any())
            {
                Cache.Instance.Log("The list is empty.");
                return;
            }

            var model = new PlotModel
            {
                Title = "STANDINGS_DAILY",
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
            };

            var seriesMinStandingAgentCorpFaction = new ColumnSeries
            {
                Title = "MinStanding (agent, corp, faction)",
                ItemsSource = sourceTotalMinStandingAgentCorpFaction,
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 8,
                IsStacked = true,
            };

            var seriesTotalMaxFactionStanding = new ColumnSeries
            {
                Title = "FactionStanding",
                ItemsSource = sourceTotalMaxFactionStanding,
                LabelPlacement = LabelPlacement.Inside,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 8,
                IsStacked = true,
            };

            model.Series.Add(seriesTotalMaxFactionStanding);
            model.Series.Add(seriesMinStandingAgentCorpFaction);

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Axis",
                ItemsSource = filter.Select(s => s.Charname + " " + s.Date).ToList(),
                Angle = -80,
                Font = "Lucida Console",
            });

            model.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Key = "Axis",
                Font = "Lucida Console",
                IsZoomEnabled = false,
            });

            plot1.Model = model;
        }

        private void StatisticsForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            Dispose();
        }

        private void StatisticsFormLoad(object sender, EventArgs e)
        {
            TimeDomainValues = new int[] { 2, 3, 5, 8, 13, 21, 34, 55, 89 };
            CurrentSelectedTimeDomain = TimeDomainValues[4];

            var dict = GetDistintCharNames();

            var item = new ToolStripMenuItem(ALL_AVAILABLE);
            selectCharacterToolStripMenuItem.DropDownItems.Add(item);
            WeakEventManager<ToolStripMenuItem, EventArgs>.AddHandler(item, "Click", (s, a) =>
            {
                CurrentSelectedCharacter = null;
                RedrawCurrentSelectedStatistic();
            });
            selectCharacterToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            CurrentSelectedCharacter = null;

            foreach (var kv in dict)
            {
                var menuItem = new ToolStripMenuItem(kv);
                WeakEventManager<ToolStripMenuItem, EventArgs>.AddHandler(menuItem, "Click", (s, a) =>
                {
                    CurrentSelectedCharacter = ((ToolStripMenuItem)s).Text;
                    RedrawCurrentSelectedStatistic();
                });
                selectCharacterToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            var tsItem = new ToolStripMenuItem(ALL_AVAILABLE);
            timeDomainToolStripMenuItem.DropDownItems.Add(tsItem);
            WeakEventManager<ToolStripMenuItem, EventArgs>.AddHandler(tsItem, "Click", (s, a) =>
            {
                CurrentSelectedTimeDomain = null;
                RedrawCurrentSelectedStatistic();
            });
            timeDomainToolStripMenuItem.DropDownItems.Add(new ToolStripSeparator());
            foreach (var val in TimeDomainValues.OrderByDescending(i => i))
            {
                var menuItem = new ToolStripMenuItem(val.ToString());
                WeakEventManager<ToolStripMenuItem, EventArgs>.AddHandler(menuItem, "Click", (s, a) =>
                {
                    CurrentSelectedTimeDomain = int.Parse(((ToolStripMenuItem)s).Text);
                    RedrawCurrentSelectedStatistic();
                });
                timeDomainToolStripMenuItem.DropDownItems.Add(menuItem);
            }

            CurrentSelectedStatistic = statisticsToolStripMenuItem.DropDownItems[0].Text;
            RedrawCurrentSelectedStatistic();
        }

        private void StatisticsToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            foreach (var item in statisticsToolStripMenuItem.DropDownItems)
            {
                if (item.GetType() == typeof(ToolStripSeparator))
                    continue;

                var i = (ToolStripMenuItem)item;
                if (i.Text.Equals(CurrentSelectedStatistic))
                    i.BackColor = Color.LightGray;
                else
                    i.BackColor = SystemColors.Control;
            }
        }

        private void TimeDomainToolStripMenuItemDropDownOpening(object sender, EventArgs e)
        {
            foreach (var item in timeDomainToolStripMenuItem.DropDownItems)
            {
                if (item.GetType() == typeof(ToolStripSeparator))
                    continue;

                var i = (ToolStripMenuItem)item;

                if (CurrentSelectedTimeDomain == null && i.Text.Equals(ALL_AVAILABLE))
                {
                    i.BackColor = Color.LightGray;
                    continue;
                }

                if (CurrentSelectedTimeDomain != null && i.Text.Equals(CurrentSelectedTimeDomain.ToString()))
                    i.BackColor = Color.LightGray;
                else
                    i.BackColor = SystemColors.Control;
            }
        }

        private void TotalISKByChar()
        {
            var list = GetStatisticListForCurrentSelection();

            var filter = list.OrderBy(k => k.Date).GroupBy(s => new { charname = s.Charname })
                .Select(g => new
                {
                    Charname = g.Key.charname,
                    MaxISKLootHangarItems = g.Last().ISKLootHangarItems,
                    MaxISKWallet = g.Last().ISKWallet,
                    MaxLPIskValue = g.Last().TotalLP * StatisticsEntry.GetISKperLP(),
                    CombinedValue = g.Last().ISKLootHangarItems + g.Last().ISKWallet + g.Last().TotalLP * StatisticsEntry.GetISKperLP(),
                }).OrderBy(l => l.Charname);

            var sourceMaxISKLootHangarItems = new List<ColumnItem>();
            var sourceMaxISKWallet = new List<ColumnItem>();
            var sourceMaxLPIskValue = new List<ColumnItem>();
            var sourceCombinedValue = new List<ColumnItem>();

            if (!filter.Any())
            {
                Cache.Instance.Log("The list is empty.");
                return;
            }

            foreach (var t in filter)
            {
                sourceMaxISKLootHangarItems.Add(new ColumnItem((double)t.MaxISKLootHangarItems / 1000000000));
                sourceMaxISKWallet.Add(new ColumnItem((double)t.MaxISKWallet / 1000000000));
                sourceMaxLPIskValue.Add(new ColumnItem((double)t.MaxLPIskValue / 1000000000));
                sourceCombinedValue.Add(new ColumnItem((double)t.CombinedValue / 1000000000));
            }

            var model = new PlotModel
            {
                Title = "TOTAL_ISK_BY_CHARNAME",
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
            };

            var seriesTotalMaxISKLootHangarItems = new ColumnSeries
            {
                Title = "Loot",
                ItemsSource = sourceMaxISKLootHangarItems,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var seriesTotalMaxISKWallet = new ColumnSeries
            {
                Title = "ISK",
                ItemsSource = sourceMaxISKWallet,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var seriesTotalMaxLPIskValue = new ColumnSeries
            {
                Title = "LP",
                ItemsSource = sourceMaxLPIskValue,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var serieseTotalCombinedValue = new ColumnSeries
            {
                Title = "Total",
                ItemsSource = sourceCombinedValue,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.0}",
                FillColor = OxyColor.FromRgb(105, 105, 105),
                FontWeight = FontWeights.Bold,
                Font = "Tahoma",
                FontSize = 9,
                TextColor = OxyColor.FromRgb(255, 255, 255),
                ColumnWidth = 0.5,
            };

            model.Series.Add(seriesTotalMaxISKWallet);
            model.Series.Add(seriesTotalMaxISKLootHangarItems);
            model.Series.Add(seriesTotalMaxLPIskValue);
            model.Series.Add(serieseTotalCombinedValue);

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Axis",
                ItemsSource = filter.Select(s => s.Charname).ToList(),
                FontSize = 10,
                GapWidth = 0.1,
            });

            model.Axes.Add(new LinearAxis()
            {
                IsAxisVisible = false,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });

            plot1.Model = model;
        }


        private void TOTALISKChartTypeColumnSeriesToolStripMenuItemClick(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;

            try
            {
                TotalIskDailyColumnSeries();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
            }
        }

        private void TotalIskDailyColumnSeries()
        {
            var list = GetStatisticListForCurrentSelection();

            var filter = list.GroupBy(s => new { date = s.Date.ToString("yyyyMMdd"), charname = s.Charname })
                .Select(g => new
                {
                    Date = g.Key.date,
                    Charname = g.Key.charname,
                    MaxISKLootHangarItems = g.Last()?.ISKLootHangarItems,
                    MaxISKWallet = g.Last()?.ISKWallet,
                    MaxLPIskValue = g.Last().TotalLP * StatisticsEntry.GetISKperLP(),
                    CombinedValue = g.Last()?.ISKLootHangarItems + g.Last()?.ISKWallet + g.Last().TotalLP * StatisticsEntry.GetISKperLP(),
                })
                .GroupBy(s => new { date = s.Date })
                .Select(g => new
                {
                    Date = g.Key.date,
                    TotalMaxISKLootHangarItems = g.Sum(p => p.MaxISKLootHangarItems),
                    TotalMaxISKWallet = g.Sum(p => p.MaxISKWallet),
                    TotalMaxLPIskValue = g.Sum(p => p.MaxLPIskValue),
                    TotalCombinedValue = g.Sum(p => p.CombinedValue),
                })
                .OrderBy(l => l.Date);

            var sourceTotalMaxISKLootHangarItems = new List<ColumnItem>();
            var sourceTotalMaxISKWallet = new List<ColumnItem>();
            var sourceTotalMaxLPIskValue = new List<ColumnItem>();
            var sourceTotalCombinedValue = new List<ColumnItem>();

            foreach (var t in filter)
            {
                sourceTotalMaxISKLootHangarItems.Add(new ColumnItem((double)t.TotalMaxISKLootHangarItems / 1000000000));
                sourceTotalMaxISKWallet.Add(new ColumnItem((double)t.TotalMaxISKWallet / 1000000000));
                sourceTotalMaxLPIskValue.Add(new ColumnItem((double)t.TotalMaxLPIskValue / 1000000000));
                sourceTotalCombinedValue.Add(new ColumnItem((double)t.TotalCombinedValue / 1000000000));
            }

            if (!filter.Any())
            {
                Cache.Instance.Log("The list is empty.");
                return;
            }

            var model = new PlotModel
            {
                Title = "TOTAL_ISK_DAILY",
                LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.BottomCenter,
                LegendOrientation = LegendOrientation.Horizontal,
            };

            var seriesTotalMaxISKLootHangarItems = new ColumnSeries
            {
                Title = "Loot",
                ItemsSource = sourceTotalMaxISKLootHangarItems,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var seriesTotalMaxISKWallet = new ColumnSeries
            {
                Title = "ISK",
                ItemsSource = sourceTotalMaxISKWallet,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var seriesTotalMaxLPIskValue = new ColumnSeries
            {
                Title = "LP",
                ItemsSource = sourceTotalMaxLPIskValue,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.00}",
                FontWeight = FontWeights.Normal,
                Font = "Tahoma",
                FontSize = 10,
                IsStacked = true,
            };

            var serieseTotalCombinedValue = new ColumnSeries
            {
                Title = "Total",
                ItemsSource = sourceTotalCombinedValue,
                LabelPlacement = LabelPlacement.Middle,
                LabelFormatString = "{0:.0}",
                FillColor = OxyColor.FromRgb(105, 105, 105),
                FontWeight = FontWeights.Bold,
                Font = "Tahoma",
                FontSize = 9,
                TextColor = OxyColor.FromRgb(255, 255, 255),
                ColumnWidth = 0.5,
            };

            model.Series.Add(seriesTotalMaxISKWallet);
            model.Series.Add(seriesTotalMaxISKLootHangarItems);
            model.Series.Add(seriesTotalMaxLPIskValue);
            model.Series.Add(serieseTotalCombinedValue);

            model.Axes.Add(new CategoryAxis
            {
                Position = AxisPosition.Bottom,
                Key = "Axis",
                ItemsSource = filter.Select(s => s.Date).ToList(),
                FontSize = 10,
                GapWidth = 0.1,
            });

            model.Axes.Add(new LinearAxis()
            {
                IsAxisVisible = false,
                IsZoomEnabled = false,
                IsPanEnabled = false
            });

            plot1.Model = model;
        }

        private void tOTALISKBYCHARNAMEChartTypeColumnSeriesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CurrentSelectedStatistic = ((ToolStripMenuItem)sender).Text;
            try
            {
                TotalISKByChar();
            }
            catch (Exception ex)
            {
                Cache.Instance.Log("Exception: " + ex);
    }
}

        #endregion Methods
    }
}