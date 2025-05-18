using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TeleCore;
using Verse;

namespace TiberiumRim
{
    public class Alert_Debug : Alert_Critical
    {
        public Alert_Debug()
        {
            defaultLabel = "Debug: Mapcomp Info";
        }

        public override TaggedString GetExplanation()
        {
            
            MapComponent_Particles particles = Find.CurrentMap.GetComponent<MapComponent_Particles>();
            MapComponent_Tiberium tiberium = Find.CurrentMap.GetComponent<MapComponent_Tiberium>();
            TiberiumMapInfo mapinfo = tiberium.Info;
            MapComponent_TeleCore tnwManager = Find.CurrentMap.GetComponent<MapComponent_TeleCore>();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Current Particles: " + particles.SavedParticles.Count);
            sb.AppendLine("Total AllProducers: " + tiberium.StructureInfo.AllProducers.Count);
            int TibCount = tiberium.Info.TotalCount;
            sb.AppendLine("Total Tiberium: " + TibCount);
            sb.AppendLine("Total Cells: " + tiberium.Info.TotalCount);
            sb.AppendLine("Active percent: " + tiberium.Info.Coverage.ToStringPercent());
            sb.AppendLine("MapInfo:\n Valuables: " + mapinfo.TiberiumCrystals[HarvestType.Valuable].Count + " - " +
                          mapinfo.TiberiumCrystalTypes[HarvestType.Valuable].Count + " types" + "\n Unvaluables: " +
                          mapinfo.TiberiumCrystals[HarvestType.Unvaluable].Count + " - " +
                          mapinfo.TiberiumCrystalTypes[HarvestType.Unvaluable].Count + " types");
            sb.AppendLine("All Trackers: " + tiberium.PollutionInfo.PollutionTrackers.Count + "Tr/" + Find.CurrentMap.regionGrid.allRooms.Count + "Rooms [" + tiberium.PollutionInfo.PollutionTrackers.Sum(k => k.Pollution) + "]");
            //sb.AppendLine("All RoomGroups: " + tiberium.PollutionInfo.RoomGroups.Count + "/" + Find.CurrentMap.regionGrid.allRooms.Select(r => r.Group).Distinct().Count());
            return sb.ToString();
        }

        public override AlertReport GetReport()
        {
            if (DebugSettings.godMode)
            {
                return AlertReport.Active;
            }
            return false;
        }
    }
}
