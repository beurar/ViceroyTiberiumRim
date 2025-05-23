﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.AI;
using RimWorld;

namespace TiberiumRim
{
    public class JobGiver_UnloadAtRefinery : ThinkNode_JobGiver
    {
        protected override Job TryGiveJob(Pawn pawn)
        {
            Harvester harvester = pawn as Harvester;
            if (harvester.CurrentPriority != HarvesterPriority.Unload) return null;
            if (harvester.IsUnloading) return null;
            //
            if (harvester.RefineryComp.HarvesterCount > 1)
            {
                //if(!)
                if (!harvester.AtRefinery && harvester.Refinery.IsReserved(out Pawn claimant) && claimant != harvester)
                {
                    return JobMaker.MakeJob(JobDefOf.Goto, harvester.Refinery.InteractionCell);
                }
            }
            if(harvester.CanReserveAndReach(harvester.Refinery, PathEndMode.InteractionCell, Danger.Deadly))
            {
                JobDef job = DefDatabase<JobDef>.GetNamed("UnloadAtRefinery");
                return JobMaker.MakeJob(job, harvester.Refinery);
            }
            return null;
        }
    }

    public class JobDriver_UnloadAtRefinery : JobDriver
    {
        private Comp_TiberiumRefinery Refinery => Harvester.RefineryComp;

        private Harvester Harvester => (Harvester)pawn;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            return pawn.CanReserve(TargetA) && pawn.Reserve(TargetA, job);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            Toil gotoToil = Toils_Goto.GotoCell(TargetA.Thing.InteractionCell, PathEndMode.OnCell);
            gotoToil.FailOnDespawnedOrNull(TargetIndex.A);
            yield return gotoToil;
            Toil unload = new Toil();
            unload.initAction = delegate
            {
                Harvester.pather.StopDead();
                Harvester.Rotation = Refinery.parent.Rotation.Opposite;
            };
            unload.tickAction = delegate
            {
                if (!Refinery?.Container.IsFull ?? false)
                {
                    if (Harvester.ContainerComp.StoredPercent > 0f)
                    {
                        foreach (var storedTiberiumType in Harvester.ContainerComp.StoredValues)
                        {
                            Refinery.InjectTiberium(storedTiberiumType.Key, storedTiberiumType.Value);
                        }                        
                    }
                    else
                    {
                        EndJobWith(JobCondition.Succeeded);
                    }
                }
                else
                {
                    EndJobWith(JobCondition.InterruptForced);
                }
            };
            unload.FailOnDespawnedOrNull(TargetIndex.A);
            unload.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            unload.FailOn(() => !((Building) TargetA.Thing).IsPowered(out bool usesPower) && usesPower);
            unload.defaultCompleteMode = ToilCompleteMode.Never;
            yield return unload;
        }
    }
}
