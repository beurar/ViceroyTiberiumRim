using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Comp_Suppression : ThingComp
    {
        /* ──────────  SHORT-CUTS  ────────── */
        public CompProperties_Suppression Props => (CompProperties_Suppression)props;

        // Power component and suppression manager are assigned in PostSpawnSetup
        public CompPowerTrader PowerComp;
        public TiberiumSuppressionMapInfo Suppression;

        public bool IsPowered => PowerComp == null || PowerComp.PowerOn;
        public bool SuppressingNow => IsPowered;
        // The manager queries this via reflection-free property
        public bool Enabled => SuppressingNow;

        /* ──────────  FOOTPRINT  ────────── */
        public List<IntVec3> SuppressionCells = new List<IntVec3>();

        /* ═══════════  INITIALISATION  ═══════════ */
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);

            // Grab references
            var tibComp = parent.Map.GetComponent<MapComponent_Tiberium>();
            Suppression = tibComp.Suppression;
            PowerComp = parent.GetComp<CompPowerTrader>();

            // Build first footprint then register
            UpdateSuppressionCells();
            Suppression.RegisterSuppressor(this);
        }

        public override void PostDeSpawn(Map map)
        {
            Suppression.DeregisterSuppressor(this);
            base.PostDeSpawn(map);
        }

        /* ═══════════  QUERY HELPERS  ═══════════ */
        public bool CoversCell(IntVec3 c) => SuppressionCells.Contains(c);
        public bool AffectsCell(IntVec3 c) => Enabled && CoversCell(c);

        /* ═══════════  BUILD FOOTPRINT  ═══════════ */
        public void UpdateSuppressionCells()
        {
            // Choose cells: open sky + line of sight
            bool Valid(IntVec3 c)
            {
                return !c.Roofed(parent.Map) &&
                       GenSight.LineOfSight(parent.Position, c, parent.Map);
            }

            SuppressionCells = TRUtils
                .SectorCells(
                    parent.Position,
                    parent.Map,
                    Props.radius,
                    Props.angle,
                    parent.Rotation.AsAngle,
                    false,
                    Valid)
                .ToList();

            // Notify manager to rebuild its bool grids next tick
            Suppression.MarkDirty(this);
        }

        /* ═══════════  POWER SIGNALS  ═══════════ */
        public override void ReceiveCompSignal(string signal)
        {
            switch (signal)
            {
                case "PowerTurnedOff":
                    Suppression.Toggle(this, false);
                    break;
                case "PowerTurnedOn":
                    Suppression.Toggle(this, true);
                    break;
            }
            base.ReceiveCompSignal(signal);
        }

        /* ═══════════  SELECTION OVERLAY  ═══════════ */
        public override void PostDraw()
        {
            base.PostDraw();

            if (!Find.Selector.IsSelected(parent)) return;

            // Grey = all coverage; Cyan = actively suppressed cells
            GenDraw.DrawFieldEdges(Suppression.CoveredCells.ToList(), Color.gray);
            GenDraw.DrawFieldEdges(Suppression.SuppressedCells.ToList(), Color.cyan);
        }
    }

    /* ──────────  PROPS CLASS  ────────── */
    public class CompProperties_Suppression : CompProperties
    {
        public float radius = 6f;
        public float angle = 360f;

        public CompProperties_Suppression()
        {
            compClass = typeof(Comp_Suppression);
        }
    }
}
