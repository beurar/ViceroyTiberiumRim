﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;


namespace TiberiumRim
{
    public class DronePlatform : TRBuilding
    {
        private int ticksUntilPortal = 400;
        private bool spawnedPortal = false;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        public override void Tick()
        {
            base.Tick();
            if (ticksUntilPortal <= 0 && !spawnedPortal)
            {
                Predicate<IntVec3> cellCheck = x => x.Standable(Map);
                CellFinder.TryFindRandomCellNear(Position, Map, 6, cellCheck, out IntVec3 result);
                GenPortal.SpawnDronePortal(result, Map);
                spawnedPortal = true;
            }else
                ticksUntilPortal--;
        }
    }
}
