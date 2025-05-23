﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace TiberiumRim
{
    public class FXGraphic
    {
        private CompFX parent;
        public Graphic graphicInt;        
        public FXGraphicData data;
        public float altitude;
        public int ticksToBlink = 0;
        public int blinkDuration = 0;
        public int index = 0;

        private bool unused;
        private Material ShaderMaterial;
        
        //Unsaved data
        private GraphicDrawInfo drawInfo;
        private Material drawMat;
        private Color drawColor;
        private float exactRotation;
        private float opacityFloat;
        private float sizeFloat;
        private Matrix4x4 matrix = default(Matrix4x4);
        private readonly MaterialPropertyBlock materialProperties = new MaterialPropertyBlock();

        public FXGraphic(CompFX parent, FXGraphicData data, int index)
        {
            //Log.Message(index + " '" + data.data?.texPath + "'" + " which is " + data.mode);
            this.parent = parent;
            this.data = data;
            this.index = index;
            if (data.skip)
            {
                unused = true;
                return;
            }
            if (data.directAltitudeOffset.HasValue)
                altitude = parent.parent.def.altitudeLayer.AltitudeFor() + data.directAltitudeOffset.Value;
            else if (data.altitude.HasValue)
                altitude = data.altitude.Value.AltitudeFor();
            else
                altitude = parent.parent.def.altitudeLayer.AltitudeFor() + (0.125f * (index + 1));
            altitude += data.extraAltitude;
            ShaderMaterial = new Material(TiberiumContent.AlphaShaderMaterial);
        }

        public void Tick()
        {
            if (unused) return;
            if (data.rotationSpeed > 0)
                exactRotation += data.rotationSpeed * 0.0166666675f;
            if (ticksToBlink > 0 && blinkDuration == 0)
                ticksToBlink--;
            else
            {
                if (blinkDuration > 0)
                    blinkDuration--;
                else
                    ResetBlink();
            }
        }

        private void ResetBlink()
        {
            ticksToBlink = data.blinkInterval;
            blinkDuration = data.blinkDuration;
        }

        public Graphic Graphic
        {
            get
            {
                if (graphicInt == null)
                {
                    if(parent.parent.Graphic is Graphic_Random random)
                    {
                        var path = this.data.data.texPath;
                        var parentName = random.SubGraphicFor(parent.parent).path.Split('/').Last();
                        var lastPart = path.Split('/').Last();
                        path += "/" + lastPart;
                        path += "_" + parentName.Split('_').Last();
                        graphicInt = GraphicDatabase.Get(typeof(Graphic_Single), path, data.data.shaderType.Shader, data.data.drawSize, data.data.color, data.data.colorTwo);
                    }
                    else if (data.data != null)
                    {
                        graphicInt = data.data.Graphic;
                    }
                }
                return graphicInt;
            }
        }

        public void Draw(Vector3 drawPos, Rot4 rot, float? rotation, Action<FXGraphic> action, int index)
        {
            if(action != null)
            {
                action.Invoke(this);
                return;
            }
            drawInfo = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.parent.def).extraData, parent.parent.def);
            var newDrawPos = drawInfo.drawPos + data.drawOffset;
            drawMat = drawInfo.drawMat;
            ApplyTextureParams(drawMat, data);

            drawColor = data.data.color;
            drawColor.a = parent.OpacityFloat(index);
            if (parent.ColorOverride(index) != Color.white)
                drawColor *= parent.ColorOverride(index);

            drawMat.SetTextureOffset("_MainTex", parent.TextureOffset);
            drawMat.SetTextureScale("_MainTex", parent.TextureScale);
            Vector2 drawSize =  Vector2.one;
            switch (data.mode)
            {
                case FXMode.Dynamic:
                    break;
                case FXMode.Mover:
                    ShaderMaterial.SetTexture("_MainTex", drawMat.mainTexture);
                    ShaderMaterial.SetTexture("_MaskTex", ContentFinder<Texture2D>.Get(Graphic.path + "_s"));
                    drawMat = ShaderMaterial;
                    Vector2 offset = new Vector2(0, TRUtils.Cosine(data.startOffset, data.endOffset, data.MoverSpeed, Find.TickManager.TicksGame));
                    drawMat.mainTextureOffset = offset;
                    break;
                case FXMode.Blink:
                    drawColor.a = 0;
                    if (blinkDuration > 0)
                        drawColor.a = 1;
                    break;
                case FXMode.Pulse:
                    var pulse = data.pulse;
                    var tick = Find.TickManager.TicksGame;
                    var opaVal = TRUtils.OscillateBetween(pulse.opacityRange.min, pulse.opacityRange.max, pulse.opacityDuration, tick + parent.tickOffset);
                    var sizeVal = TRUtils.OscillateBetween(pulse.sizeRange.min, pulse.sizeRange.max, pulse.sizeDuration, tick + parent.tickOffset);
                    if(pulse.opacityRange != FloatRange.Zero)
                        drawColor.a = opaVal;
                    if (pulse.sizeRange != FloatRange.Zero)
                        drawSize = drawInfo.drawSize * sizeVal;
                    break;
                default:
                    return;
            }
            materialProperties.SetColor(ShaderPropertyIDs.Color, drawColor);
            matrix.SetTRS(new Vector3(newDrawPos.x, altitude, newDrawPos.z), (exactRotation + (rotation ?? drawInfo.rotation)).ToQuat(), new Vector3(drawSize.x, 1f, drawSize.y));
            Graphics.DrawMesh(drawInfo.drawMesh, matrix, drawMat, 0, null, 0, materialProperties);
        }

        public void Print(SectionLayer layer, Vector3 drawPos, Rot4 rot, float? rotation, Thing parent)
        {
            var info = new GraphicDrawInfo(Graphic, drawPos, rot, ((FXThingDef)parent.def).extraData, parent.def);
            var newDrawPos = info.drawPos + data.drawOffset;
            Printer_Plane.PrintPlane(layer, new Vector3(newDrawPos.x, altitude, newDrawPos.z), info.drawSize, info.drawMat, rotation ?? info.rotation, info.flipUV);
        }

        private static void ApplyTextureParams(Material mat, FXGraphicData fxData)
        {
            if (mat == null || fxData?.textureParams == null)
                return;

            if (fxData?.textureParams?.entries != null)
            {
                foreach (var kv in fxData.textureParams.entries)
                {
                    var tex = ContentFinder<Texture2D>.Get(kv.Value, false);
                    if (tex != null)
                        mat.SetTexture(kv.Key, tex);
                }
            }
        }
    }
}
