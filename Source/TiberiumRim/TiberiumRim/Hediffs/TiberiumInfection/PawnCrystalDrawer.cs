using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using  Verse;
using  RimWorld;

namespace TiberiumRim
{
    public class PawnCrystalDrawer : PawnRenderNodeWorker
    {
        private CrystalOverlay overlay;

        private Pawn pawn;

        public void Init(Pawn pawn)
        {
            this.pawn = pawn;
        }

        public override void AppendDrawRequests(PawnRenderNode node, PawnDrawParms parms, List<PawnGraphicDrawRequest> requests)
        {
            var pawnFromNode = node.parent.tree.pawn;
            var pawn = this.pawn ?? pawnFromNode;

            if (pawn == null || pawn.Dead || pawn.Drawer?.renderer == null)
                return;

            bool hasImmunity = pawn.health?.hediffSet?.HasHediff(TRHediffDefOf.TiberiumImmunity) ?? false;
            bool hasTrait = pawn.story?.traits?.HasTrait(TraitDef.Named("TiberiumTrait")) ?? false;

            if (!hasImmunity && !hasTrait)
                return;

            overlay ??= new CrystalOverlay(pawn, hasImmunity);

            // RimWorld 1.5: Extract render values from matrix
            Vector3 drawLoc = parms.matrix.MultiplyPoint(Vector3.zero);
            Quaternion rotation = parms.matrix.rotation;
            Rot4 rotBody = parms.facing;
            Rot4 rotHead = pawn.Rotation; // optionally use something smarter if needed

            bool forPortrait = parms.Portrait;

            // Render manually
            overlay.DrawCrystal(drawLoc, rotation, rotBody, rotHead, forPortrait, hasTrait);
        }

    }


    public class CrystalOverlay
    {
        private Pawn pawn;
        private Material mat;
        private Graphic Head;
        private Graphic Body;
        private Quaternion quat;
        private static readonly Vector2 crystalSpan = new Vector2(0.5f, 0.7f);

        public CrystalOverlay(Pawn pawn, bool HasImmunity)
        {
            this.pawn = pawn;
            if (pawn.RaceProps.Humanlike)
            {
                TRUtils.GetTiberiumMutant(pawn, out Head, out Body);
            }

            //Body for animals
            if (Head == null)
            {
                Vector2 drawSize = pawn.Drawer.renderer.BodyGraphic.drawSize;
                string path = pawn.Drawer.renderer.BodyGraphic.path;
                if (ContentFinder<Texture2D>.Get(path + "_TibBody", false) != null) Body = GraphicDatabase.Get(typeof(Graphic_Multi), path + "_TibBody", ShaderDatabase.Cutout, drawSize, Color.white, Color.white);
                else if (ContentFinder<Texture2D>.Get("Pawns/TiberiumOverlays" + pawn.def.defName + "/" + pawn.def.defName + "_TibBody", false) != null)
                    Body = GraphicDatabase.Get(typeof(Graphic_Multi), "Pawns/TiberiumOverlays" + pawn.def.defName + "/" + pawn.def.defName + "_TibBody", ShaderDatabase.Cutout, drawSize, Color.white, Color.white);
                else
                    Body = null;
                
            }
            if (Body == null)
            {
                this.mat = MaterialPool.MatFrom("Pawns/TiberiumMutant/Bodies/Fat_north", ShaderDatabase.MoteGlow, Color.white);
                float num = pawn.GetHashCode();
                float rand = Mathf.Clamp(num / 24f, 0, 360);
                this.quat = Quaternion.AngleAxis(rand, Vector3.up);
            }
        }

        public void DrawCrystal(Vector3 drawLoc, Quaternion bodyQuat, Rot4 bodyRot, Rot4 headRot, bool forPortrait, bool alpha)
        {
            bool hasOverlay = Body != null;

            if (pawn.RaceProps.Humanlike)
            {
                Vector3 bodyDrawLoc = drawLoc;

                // HEAD DRAW
                if (Head != null)
                {
                    Vector3 headDrawLoc = drawLoc;

                    if (bodyRot != Rot4.North)
                    {
                        headDrawLoc.y += 0.02734375f;
                        bodyDrawLoc.y += 0.0234375f;
                    }
                    else
                    {
                        headDrawLoc.y += 0.0234375f;
                        bodyDrawLoc.y += 0.02734375f;
                    }

                    Vector3 headOffset = pawn.Drawer.renderer.BaseHeadOffsetAt(headRot);
                    Material headMat = Head.MatAt(headRot);

                    if (headMat != null)
                    {
                        Mesh headMesh = MeshPool.GetMeshSetForSize(new Vector2(MeshPool.HumanlikeHeadAverageWidth, 1f)).MeshAt(headRot);
                        GenDraw.DrawMeshNowOrLater(headMesh, headDrawLoc + headOffset, bodyQuat, headMat, forPortrait);
                    }
                }

                // BODY DRAW
                bool covered = pawn.apparel != null && pawn.apparel.BodyPartGroupIsCovered(BodyPartGroupDefOf.Torso);
                if (!covered && Body != null)
                {
                    Material bodyMat = Body.MatAt(bodyRot);
                    Mesh bodyMesh = MeshPool.GetMeshSetForSize(new Vector2(MeshPool.HumanlikeBodyWidth, 1f)).MeshAt(bodyRot);
                    GenDraw.DrawMeshNowOrLater(bodyMesh, bodyDrawLoc, bodyQuat, bodyMat, forPortrait);
                }
            }
            else
            {
                // NON-HUMANLIKE fallback (animals, mechanoids, etc.)
                Mesh mesh = hasOverlay ? Body.MeshAt(bodyRot) : MeshPool.plane14;
                Quaternion quat = hasOverlay ? bodyQuat : this.quat;
                Material drawMat = hasOverlay ? Body.MatAt(bodyRot, pawn) : mat;

                GenDraw.DrawMeshNowOrLater(mesh, drawLoc, quat, drawMat, forPortrait);
            }
        }

    }
}
