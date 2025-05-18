using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using TeleCore;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class TNW_TiberiumSpike : FXBuilding
    {
        public TiberiumGeyser boundGeyser;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            boundGeyser = this.Position.GetFirstThing(map, TiberiumDefOf.TiberiumGeyser) as TiberiumGeyser;
            boundGeyser.tiberiumSpike = this;
        }

        public CompNetwork CompTNW => this.TryGetComp<CompNetwork>();

        public override float[] OpacityFloats => new float[] { 1f, 1f};
        public override bool[] DrawBools => new bool[] { CompTNW.IsWorking, CompTNW.IsWorking && CompTNW.CompPower.PowerOn};
        public override Color[] ColorOverrides => new Color[] { Color.white, Color.white};

        public override IEnumerable<InspectTabBase> GetInspectTabs()
        {
            return base.GetInspectTabs();
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("\n" + "TR_GeyserContent" + ": " + boundGeyser.ContentPercent.ToStringPercent());
            return sb.ToString().TrimEndNewlines();
        }
    }
}
