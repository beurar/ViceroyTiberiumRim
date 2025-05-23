﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    public class TiberiumSpore : Particle
    {
        public TiberiumCrystalDef crystalDef = TiberiumDefOf.TiberiumPod;
        public TiberiumProducer parent;

        public void SporeSetup(TiberiumCrystalDef crystalDef, TiberiumProducer parent)
        {
            this.crystalDef = crystalDef;
            this.parent = parent;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref parent, "parent");
            Scribe_Defs.Look(ref crystalDef, "crystalDef");
        }

        public override void FinishAction()
        {
            base.FinishAction();
            if (crystalDef != null && !map.roofGrid.Roofed(Position))
            {
                GenTiberium.TrySpawnTiberium(endCell, map, crystalDef, parent);
            }
        }

        public override bool ShouldDestroy => base.ShouldDestroy || Position.Roofed(map);

        public override Color Color
        {
            get
            {
                Color baseColor = base.Color;

                // Ensure valid crystal + networkValueDef
                Color? overlay = crystalDef?.tiberium?.networkValue?.valueColor;
                if (overlay != null)
                    baseColor *= overlay.Value;

                return baseColor;
            }
        }

    }
}
