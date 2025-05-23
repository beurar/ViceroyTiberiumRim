﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;

namespace TiberiumRim
{
    public class PortableContainer : FXThing
    {
        public Comp_TiberiumContainer Container;

        public void PostSetup(Comp_TiberiumContainer container)
        {
            Container = container.MakeCopy(this);
        }

        public override float[] OpacityFloats => new float[1] { Container?.StoredPercent ?? 0f };
        public override Color[] ColorOverrides => new Color[1] { Container?.DominantColor ?? Color.white };
        public override bool[] DrawBools => new bool[1] { true };

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref Container, "tibContainer");
        }

        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                Vector3 v = GenMapUI.LabelDrawPosFor(Position);
                GenMapUI.DrawThingLabel(v, Container.StoredPercent.ToStringPercent(), Color.white);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("TR_PortableContainer".Translate() + ": " + Container.StoredPercent);
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            foreach (Gizmo g in Container.GetGizmos())
            {
                yield return g;
            }
        }
    }
}
