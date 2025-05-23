﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Comp_CrystalDrawer : ThingComp
    {
        private PawnCrystalDrawer drawer;

        public PawnCrystalDrawer Drawer
        {
            get
            {
                if (drawer == null)
                {
                    var drawer = new PawnCrystalDrawer();
                    drawer.Init(parent as Pawn);
                }
                return drawer;
            }
        }
    }

    public class CompProperties_CrystalDrawer : CompProperties
    {
        public CompProperties_CrystalDrawer()
        {
            compClass = typeof(Comp_CrystalDrawer);
        }
    }
}
