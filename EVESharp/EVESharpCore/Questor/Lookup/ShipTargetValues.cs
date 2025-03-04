﻿/*
 * Created by SharpDevelop.
 * User: duketwo
 * Date: 27.08.2016
 * Time: 14:16
 *
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

namespace EVESharpCore.Lookup
{
    /// <summary>
    ///     Description of ShipTargetValues.
    /// </summary>
    public static class ShipTargetValues
    {
        #region Properties

        public static string GetShipTargetValuesXML => @"<ships>
							  <ship name=""Storyline Battleship"" groupid=""523"" targetvalue=""4"" />
							  <ship name=""Storyline Mission Battleship"" groupid=""534"" targetvalue=""4"" />
							  <ship name=""Asteroid Angel Cartel Battleship"" groupid=""552"" targetvalue=""4"" />
							  <ship name=""Asteroid Blood Raiders Battleship"" groupid=""556"" targetvalue=""4"" />
							  <ship name=""Asteroid Guristas Battleship"" groupid=""560"" targetvalue=""4"" />
							  <ship name=""Asteroid Sansha's Nation Battleship"" groupid=""565"" targetvalue=""4"" />
							  <ship name=""Asteroid Serpentis Battleship"" groupid=""570"" targetvalue=""4"" />
							  <ship name=""Deadspace Angel Cartel Battleship"" groupid=""594"" targetvalue=""4"" />
							  <ship name=""Deadspace Blood Raiders Battleship"" groupid=""603"" targetvalue=""4"" />
							  <ship name=""Deadspace Guristas Battleship"" groupid=""612"" targetvalue=""4"" />
							  <ship name=""Deadspace Sansha's Nation Battleship"" groupid=""621"" targetvalue=""4"" />
							  <ship name=""Deadspace Serpentis Battleship"" groupid=""630"" targetvalue=""4"" />
							  <ship name=""Mission Amarr Empire Battleship"" groupid=""667"" targetvalue=""4"" />
							  <ship name=""Mission Caldari State Battleship"" groupid=""674"" targetvalue=""4"" />
							  <ship name=""Mission Gallente Federation Battleship"" groupid=""680"" targetvalue=""4"" />
							  <ship name=""Mission Khanid Battleship"" groupid=""691"" targetvalue=""4"" />
							  <ship name=""Mission CONCORD Battleship"" groupid=""697"" targetvalue=""4"" />
							  <ship name=""Mission Mordu Battleship"" groupid=""703"" targetvalue=""4"" />
							  <ship name=""Mission Minmatar Republic Battleship"" groupid=""706"" targetvalue=""4"" />
							  <ship name=""Asteroid Rogue Drone Battleship"" groupid=""756"" targetvalue=""4"" />
							  <ship name=""Deadspace Rogue Drone Battleship"" groupid=""802"" targetvalue=""4"" />
							  <ship name=""Mission Generic Battleships"" groupid=""816"" targetvalue=""4"" />
							  <ship name=""Deadspace Overseer Battleship"" groupid=""821"" targetvalue=""4"" />
							  <ship name=""Mission Thukker Battleship"" groupid=""823"" targetvalue=""4"" />
							  <ship name=""Asteroid Rogue Drone Commander Battleship"" groupid=""844"" targetvalue=""4"" />
							  <ship name=""Asteroid Angel Cartel Commander Battleship"" groupid=""848"" targetvalue=""4"" />
							  <ship name=""Asteroid Blood Raiders Commander Battleship"" groupid=""849"" targetvalue=""4"" />
							  <ship name=""Asteroid Guristas Commander Battleship"" groupid=""850"" targetvalue=""4"" />
							  <ship name=""Asteroid Sansha's Nation Commander Battleship"" groupid=""851"" targetvalue=""4"" />
							  <ship name=""Asteroid Serpentis Commander Battleship"" groupid=""852"" targetvalue=""4"" />
							  <ship name=""Mission Faction Battleship"" groupid=""924"" targetvalue=""4"" />
							  <ship name=""Asteroid Angel Cartel BattleCruiser"" groupid=""576"" targetvalue=""5"" />
							  <ship name=""Asteroid Blood Raiders BattleCruiser"" groupid=""578"" targetvalue=""5"" />
							  <ship name=""Asteroid Guristas BattleCruiser"" groupid=""580"" targetvalue=""5"" />
							  <ship name=""Asteroid Sansha's Nation BattleCruiser"" groupid=""582"" targetvalue=""5"" />
							  <ship name=""Asteroid Serpentis BattleCruiser"" groupid=""584"" targetvalue=""5"" />
							  <ship name=""Deadspace Angel Cartel BattleCruiser"" groupid=""593"" targetvalue=""5"" />
							  <ship name=""Deadspace Blood Raiders BattleCruiser"" groupid=""602"" targetvalue=""5"" />
							  <ship name=""Deadspace Guristas BattleCruiser"" groupid=""611"" targetvalue=""5"" />
							  <ship name=""Deadspace Sansha's Nation BattleCruiser"" groupid=""620"" targetvalue=""5"" />
							  <ship name=""Deadspace Serpentis BattleCruiser"" groupid=""629"" targetvalue=""5"" />
							  <ship name=""Mission Amarr Empire Battlecruiser"" groupid=""666"" targetvalue=""5"" />
							  <ship name=""Mission Caldari State Battlecruiser"" groupid=""672"" targetvalue=""5"" />
							  <ship name=""Mission Gallente Federation Battlecruiser"" groupid=""681"" targetvalue=""5"" />
							  <ship name=""Mission Minmatar Republic Battlecruiser"" groupid=""685"" targetvalue=""5"" />
							  <ship name=""Mission Khanid Battlecruiser"" groupid=""690"" targetvalue=""5"" />
							  <ship name=""Mission CONCORD Battlecruiser"" groupid=""696"" targetvalue=""5"" />
							  <ship name=""Mission Mordu Battlecruiser"" groupid=""702"" targetvalue=""5"" />
							  <ship name=""Asteroid Rogue Drone BattleCruiser"" groupid=""755"" targetvalue=""5"" />
							  <ship name=""Asteroid Angel Cartel Commander BattleCruiser"" groupid=""793"" targetvalue=""5"" />
							  <ship name=""Asteroid Blood Raiders Commander BattleCruiser"" groupid=""795"" targetvalue=""5"" />
							  <ship name=""Asteroid Guristas Commander BattleCruiser"" groupid=""797"" targetvalue=""5"" />
							  <ship name=""Deadspace Rogue Drone BattleCruiser"" groupid=""801"" targetvalue=""5"" />
							  <ship name=""Asteroid Sansha's Nation Commander BattleCruiser"" groupid=""807"" targetvalue=""5"" />
							  <ship name=""Asteroid Serpentis Commander BattleCruiser"" groupid=""811"" targetvalue=""5"" />
							  <ship name=""Mission Thukker Battlecruiser"" groupid=""822"" targetvalue=""5"" />
							  <ship name=""Asteroid Rogue Drone Commander BattleCruiser"" groupid=""843"" targetvalue=""5"" />
							  <ship name=""Storyline Cruiser"" groupid=""522"" targetvalue=""2"" />
							  <ship name=""Storyline Mission Cruiser"" groupid=""533"" targetvalue=""2"" />
							  <ship name=""Asteroid Angel Cartel Cruiser"" groupid=""551"" targetvalue=""2"" />
							  <ship name=""Asteroid Blood Raiders Cruiser"" groupid=""555"" targetvalue=""2"" />
							  <ship name=""Asteroid Guristas Cruiser"" groupid=""561"" targetvalue=""2"" />
							  <ship name=""Asteroid Sansha's Nation Cruiser"" groupid=""566"" targetvalue=""2"" />
							  <ship name=""Asteroid Serpentis Cruiser"" groupid=""571"" targetvalue=""2"" />
							  <ship name=""Deadspace Angel Cartel Cruiser"" groupid=""595"" targetvalue=""2"" />
							  <ship name=""Deadspace Blood Raiders Cruiser"" groupid=""604"" targetvalue=""2"" />
							  <ship name=""Deadspace Guristas Cruiser"" groupid=""613"" targetvalue=""2"" />
							  <ship name=""Deadspace Sansha's Nation Cruiser"" groupid=""622"" targetvalue=""2"" />
							  <ship name=""Deadspace Serpentis Cruiser"" groupid=""631"" targetvalue=""2"" />
							  <ship name=""Mission Amarr Empire Cruiser"" groupid=""668"" targetvalue=""2"" />
							  <ship name=""Mission Caldari State Cruiser"" groupid=""673"" targetvalue=""2"" />
							  <ship name=""Mission Gallente Federation Cruiser"" groupid=""678"" targetvalue=""2"" />
							  <ship name=""Mission Khanid Cruiser"" groupid=""689"" targetvalue=""2"" />
							  <ship name=""Mission CONCORD Cruiser"" groupid=""695"" targetvalue=""2"" />
							  <ship name=""Mission Mordu Cruiser"" groupid=""701"" targetvalue=""2"" />
							  <ship name=""Mission Minmatar Republic Cruiser"" groupid=""705"" targetvalue=""2"" />
							  <ship name=""Asteroid Rogue Drone Cruiser"" groupid=""757"" targetvalue=""2"" />
							  <ship name=""Asteroid Angel Cartel Commander Cruiser"" groupid=""790"" targetvalue=""2"" />
							  <ship name=""Asteroid Blood Raiders Commander Cruiser"" groupid=""791"" targetvalue=""2"" />
							  <ship name=""Asteroid Guristas Commander Cruiser"" groupid=""798"" targetvalue=""2"" />
							  <ship name=""Deadspace Rogue Drone Cruiser"" groupid=""803"" targetvalue=""2"" />
							  <ship name=""Asteroid Sansha's Nation Commander Cruiser"" groupid=""808"" targetvalue=""2"" />
							  <ship name=""Asteroid Serpentis Commander Cruiser"" groupid=""812"" targetvalue=""2"" />
							  <ship name=""Mission Generic Cruisers"" groupid=""817"" targetvalue=""2"" />
							  <ship name=""Deadspace Overseer Cruiser"" groupid=""820"" targetvalue=""2"" />
							  <ship name=""Mission Thukker Cruiser"" groupid=""824"" targetvalue=""2"" />
							  <ship name=""Mission Generic Battle Cruisers"" groupid=""828"" targetvalue=""2"" />
							  <ship name=""Asteroid Rogue Drone Commander Cruiser"" groupid=""845"" targetvalue=""2"" />
							  <ship name=""Strategic Cruiser"" groupid=""963"" targetvalue=""2"" />
							  <ship name=""Strategic Cruiser Blueprints"" groupid=""996"" targetvalue=""2"" />
							  <ship name=""Mission Faction Cruiser"" groupid=""1006"" targetvalue=""2"" />
							  <ship name=""Sentry"" groupid=""383"" targetvalue=""0"" />
							  <ship name=""Asteroid Angel Cartel Destroyer"" groupid=""575"" targetvalue=""1"" />
							  <ship name=""Asteroid Blood Raiders Destroyer"" groupid=""577"" targetvalue=""1"" />
							  <ship name=""Asteroid Guristas Destroyer"" groupid=""579"" targetvalue=""1"" />
							  <ship name=""Asteroid Sansha's Nation Destroyer"" groupid=""581"" targetvalue=""1"" />
							  <ship name=""Asteroid Serpentis Destroyer"" groupid=""583"" targetvalue=""1"" />
							  <ship name=""Deadspace Angel Cartel Destroyer"" groupid=""596"" targetvalue=""1"" />
							  <ship name=""Deadspace Blood Raiders Destroyer"" groupid=""605"" targetvalue=""1"" />
							  <ship name=""Deadspace Guristas Destroyer"" groupid=""614"" targetvalue=""1"" />
							  <ship name=""Deadspace Sansha's Nation Destroyer"" groupid=""623"" targetvalue=""1"" />
							  <ship name=""Deadspace Serpentis Destroyer"" groupid=""632"" targetvalue=""1"" />
							  <ship name=""Mission Amarr Empire Destroyer"" groupid=""669"" targetvalue=""1"" />
							  <ship name=""Mission Caldari State Destroyer"" groupid=""676"" targetvalue=""1"" />
							  <ship name=""Mission Gallente Federation Destroyer"" groupid=""679"" targetvalue=""1"" />
							  <ship name=""Mission Minmatar Republic Destroyer"" groupid=""684"" targetvalue=""1"" />
							  <ship name=""Mission Khanid Destroyer"" groupid=""688"" targetvalue=""1"" />
							  <ship name=""Mission CONCORD Destroyer"" groupid=""694"" targetvalue=""1"" />
							  <ship name=""Mission Mordu Destroyer"" groupid=""700"" targetvalue=""1"" />
							  <ship name=""Asteroid Rogue Drone Destroyer"" groupid=""758"" targetvalue=""1"" />
							  <ship name=""Asteroid Angel Cartel Commander Destroyer"" groupid=""794"" targetvalue=""1"" />
							  <ship name=""Asteroid Blood Raiders Commander Destroyer"" groupid=""796"" targetvalue=""1"" />
							  <ship name=""Asteroid Guristas Commander Destroyer"" groupid=""799"" targetvalue=""1"" />
							  <ship name=""Deadspace Rogue Drone Destroyer"" groupid=""804"" targetvalue=""1"" />
							  <ship name=""Asteroid Sansha's Nation Commander Destroyer"" groupid=""809"" targetvalue=""1"" />
							  <ship name=""Asteroid Serpentis Commander Destroyer"" groupid=""813"" targetvalue=""1"" />
							  <ship name=""Mission Thukker Destroyer"" groupid=""825"" targetvalue=""1"" />
							  <ship name=""Mission Generic Destroyers"" groupid=""829"" targetvalue=""1"" />
							  <ship name=""Asteroid Rogue Drone Commander Destroyer"" groupid=""846"" targetvalue=""1"" />
							  <ship name=""asteroid angel cartel frigate"" groupid=""550"" targetvalue=""0"" />
							  <ship name=""asteroid blood raiders frigate"" groupid=""557"" targetvalue=""0"" />
							  <ship name=""asteroid guristas frigate"" groupid=""562"" targetvalue=""0"" />
							  <ship name=""asteroid sansha's nation frigate"" groupid=""567"" targetvalue=""0"" />
							  <ship name=""asteroid serpentis frigate"" groupid=""572"" targetvalue=""0"" />
							  <ship name=""deadspace angel cartel frigate"" groupid=""597"" targetvalue=""0"" />
							  <ship name=""deadspace blood raiders frigate"" groupid=""606"" targetvalue=""0"" />
							  <ship name=""deadspace guristas frigate"" groupid=""615"" targetvalue=""0"" />
							  <ship name=""deadspace sansha's nation frigate"" groupid=""624"" targetvalue=""0"" />
							  <ship name=""deadspace serpentis frigate"" groupid=""633"" targetvalue=""0"" />
							  <ship name=""mission amarr empire frigate"" groupid=""665"" targetvalue=""0"" />
							  <ship name=""mission caldari state frigate"" groupid=""671"" targetvalue=""0"" />
							  <ship name=""mission gallente federation frigate"" groupid=""677"" targetvalue=""0"" />
							  <ship name=""mission minmatar republic frigate"" groupid=""683"" targetvalue=""0"" />
							  <ship name=""mission khanid frigate"" groupid=""687"" targetvalue=""0"" />
							  <ship name=""mission concord frigate"" groupid=""693"" targetvalue=""0"" />
							  <ship name=""mission mordu frigate"" groupid=""699"" targetvalue=""0"" />
							  <ship name=""asteroid rouge drone frigate"" groupid=""759"" targetvalue=""0"" />
							  <ship name=""asteroid angel cartel commander frigate"" groupid=""789"" targetvalue=""0"" />
							  <ship name=""asteroid blood raiders commander frigate"" groupid=""792"" targetvalue=""0"" />
							  <ship name=""asteroid guristas commander frigate"" groupid=""800"" targetvalue=""0"" />
							  <ship name=""asteroid rouge drone frigate"" groupid=""805"" targetvalue=""0"" />
							  <ship name=""asteroid sansha's nation commander frigate"" groupid=""810"" targetvalue=""0"" />
							  <ship name=""asteroid serpentis commander frigate"" groupid=""814"" targetvalue=""0"" />
							  <ship name=""mission generic frigates"" groupid=""818"" targetvalue=""0"" />
							  <ship name=""mission thukker frigate"" groupid=""826"" targetvalue=""0"" />
							  <ship name=""asteroid rouge drone commander frigate"" groupid=""847"" targetvalue=""0"" />
							</ships>";

        #endregion Properties
    }
}