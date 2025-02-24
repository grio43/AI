/*
 * Created by SharpDevelop.
 * User: duketwo
 * Date: 21.05.2016
 * Time: 00:14
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using SharedComponents.LINQtoCSV;
using System;

//01 Date; - DATETIME
//02 Mission; - STRING
//03 Time;	- INT
//04 Isk; - LONG
//05 IskReward; - INT
//06 Loot; - LONG
//07 LP; - INT
//08 DroneRecalls; - INT
//09 LostDrones; - INT
//10 AmmoConsumption; - INT
//11 AmmoValue; - INT
//12 Panics; - INT
//13 LowestShield; - INT
//14 LowestArmor; - INT
//15 LowestCap; - INT
//16 RepairCycles; - INT
//17 AfterMissionsalvageTime; - INT
//18 TotalMissionTime; - INT
//19 MissionXMLAvailable; - BOOL
//20 Faction; - STRING
//21 SolarSystem; - STRING
//22 DungeonID; - STRING
//23 OutOfDronesCount; - INT
//24 ISKWallet; - DECIMAL
//25 ISKLootHangarItems - LONG

namespace SharedComponents.EVE
{
    public class StatisticsEntryCSV
    {
        #region Fields

        private static int ISKperLP = 500;

        private decimal? _ISKperHour;

        private decimal? _MillionISKperHour;

        #endregion Fields

        #region Properties

        [CsvColumn(FieldIndex = 17, Name = "AfterMissionsalvageTime", CanBeNull = true)]
        public long AfterMissionsalvageTime { get; set; }

        [CsvColumn(FieldIndex = 10, Name = "AmmoConsumption", CanBeNull = true)]
        public long AmmoConsumption { get; set; }

        [CsvColumn(FieldIndex = 11, Name = "AmmoValue", CanBeNull = true)]
        public long AmmoValue { get; set; }

        public string Charname { get; set; }

        [CsvColumn(FieldIndex = 1, Name = "Date", CanBeNull = true)]
        public DateTime Date { get; set; }

        [CsvColumn(FieldIndex = 8, Name = "DroneRecalls", CanBeNull = true)]
        public long DroneRecalls { get; set; }

        [CsvColumn(FieldIndex = 22, Name = "DungeonID", CanBeNull = true)]
        public string DungeonID { get; set; }

        [CsvColumn(FieldIndex = 20, Name = "Faction", CanBeNull = true)]
        public string Faction { get; set; }

        [CsvColumn(FieldIndex = 28, Name = "FactionStanding", CanBeNull = true)]
        public decimal FactionStanding { get; set; }

        [CsvColumn(FieldIndex = 4, Name = "Isk", CanBeNull = true)]
        public long Isk { get; set; }

        [CsvColumn(FieldIndex = 25, Name = "ISKLootHangarItems", CanBeNull = true)]
        public long ISKLootHangarItems { get; set; }

        public decimal? ISKperHour
        {
            get
            {
                if (_ISKperHour != null)
                    return _ISKperHour;

                if (Time <= 0)
                {
                    _ISKperHour = 0;
                    return _ISKperHour;
                }

                long Isk = this.Isk + IskReward + Loot + LP * ISKperLP;
                _ISKperHour = Isk * 60 / Time;
                return _ISKperHour;
            }
        }

        [CsvColumn(FieldIndex = 5, Name = "IskReward", CanBeNull = true)]
        public int IskReward { get; set; }

        [CsvColumn(FieldIndex = 24, Name = "ISKWallet", CanBeNull = true)]
        public decimal ISKWallet { get; set; }

        [CsvColumn(FieldIndex = 6, Name = "Loot", CanBeNull = true)]
        public long Loot { get; set; }

        [CsvColumn(FieldIndex = 9, Name = "LostDrones", CanBeNull = true)]
        public long LostDrones { get; set; }

        [CsvColumn(FieldIndex = 14, Name = "LowestArmor", CanBeNull = true)]
        public long LowestArmor { get; set; }

        [CsvColumn(FieldIndex = 15, Name = "LowestCap", CanBeNull = true)]
        public long LowestCap { get; set; }

        [CsvColumn(FieldIndex = 13, Name = "LowestShield", CanBeNull = true)]
        public long LowestShield { get; set; }

        [CsvColumn(FieldIndex = 7, Name = "LP", CanBeNull = true)]
        public long LP { get; set; }

        public decimal? MillionISKperHour
        {
            get
            {
                if (_MillionISKperHour != null)
                    return _MillionISKperHour;
                _MillionISKperHour = ISKperHour / 1000000;
                return _MillionISKperHour;
            }
        }

        [CsvColumn(FieldIndex = 27, Name = "MinStandingAgentCorpFaction", CanBeNull = true)]
        public decimal MinStandingAgentCorpFaction { get; set; }

        [CsvColumn(FieldIndex = 2, Name = "Mission", CanBeNull = true)]
        public string Mission { get; set; }

        [CsvColumn(FieldIndex = 19, Name = "MissionXMLAvailable", CanBeNull = true)]
        public bool MissionXMLAvailable { get; set; }

        [CsvColumn(FieldIndex = 23, Name = "OutOfDronesCount", CanBeNull = true)]
        public long OutOfDronesCount { get; set; }

        [CsvColumn(FieldIndex = 12, Name = "Panics", CanBeNull = true)]
        public long Panics { get; set; }

        [CsvColumn(FieldIndex = 16, Name = "RepairCycles", CanBeNull = true)]
        public long RepairCycles { get; set; }

        [CsvColumn(FieldIndex = 21, Name = "SolarSystem", CanBeNull = true)]
        public string SolarSystem { get; set; }

        [CsvColumn(FieldIndex = 3, Name = "Time", CanBeNull = true)]
        public int Time { get; set; }

        [CsvColumn(FieldIndex = 26, Name = "TotalLP", CanBeNull = true)]
        public long TotalLP { get; set; }

        [CsvColumn(FieldIndex = 18, Name = "TotalMissionTime", CanBeNull = true)]
        public long TotalMissionTime { get; set; }

        #endregion Properties

        #region Methods

        public static int GetISKperLP()
        {
            return ISKperLP;
        }

        public static void SetISKperLP(int iskPerLP)
        {
            ISKperLP = iskPerLP;
        }

        public override string ToString()
        {
            return
                string.Format(
                    "Date: {0}, Mission: {1}, Time: {2}, Isk: {3}, IskReward: {4}, Loot: {5}, LP: {6}, DroneRecalls: {7}, LostDrones: {8}, AmmoConsumption: {9}, AmmoValue: {10}, Panics: {11}, LowestShield: {12}, LowestArmor: {13}, LowestCap: {14}, RepairCycles: {15}, AfterMissionsalvageTime: {16}, TotalMissionTime: {17}, MissionXMLAvailable: {18}, Faction: {19}, SolarSystem: {20}, DungeonID: {21}, OutOfDronesCount: {22}, ISKWallet: {23}, ISKLootHangarItems: {24}, ISKperHour: {25}, MillionISKperHour: {26}, Charname: {27}, TotalLP: {28}",
                    Date, Mission, Time, Isk, IskReward, Loot, LP, DroneRecalls, LostDrones, AmmoConsumption, AmmoValue,
                    Panics, LowestShield, LowestArmor, LowestCap, RepairCycles, AfterMissionsalvageTime, TotalMissionTime,
                    MissionXMLAvailable, Faction, SolarSystem, DungeonID, OutOfDronesCount, ISKWallet, ISKLootHangarItems, ISKperHour, MillionISKperHour,
                    Charname, TotalLP);
        }

        #endregion Methods
    }
}