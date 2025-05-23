﻿using HarmonyLib;
using System.Reflection;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;
using Verse.Sound;
using System.Runtime.CompilerServices;
using System.IO;
using System.Threading;
using Verse.AI;

namespace TiberiumRim
{
    public class TiberiumRimMod : Mod
    {
        //Static Data
        public static TiberiumRimMod mod;
        public static AssetBundle assetBundle;
        private static Harmony tiberium;

        //
        public TiberiumSettings settings;

        public static Harmony Tiberium => tiberium ??= new Harmony("com.tiberiumrim.rimworld.mod");

        public TiberiumRimMod(ModContentPack content) : base(content)
        {
            Log.Message("[TiberiumRim] - Init");
            settings = GetSettings<TiberiumSettings>();

            Tiberium.PatchAll(Assembly.GetExecutingAssembly());
            mod = this;
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
        }

        public void LoadAssetBundles()
        {
            string path = Path.Combine(Content.RootDir, @"Materials\Shaders\shaderbundle");
            assetBundle = AssetBundle.LoadFromFile(path);
            TiberiumContent.AlphaShader = (Shader)assetBundle.LoadAsset("AlphaShader");
            TiberiumContent.AlphaShaderMaterial = (Material)assetBundle.LoadAsset("ShaderMaterial");
        }

        public void PatchPawnDefs()
        {
            foreach (var def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def?.thingClass == null) continue;
                Type thingClass = def.thingClass;
                if (!thingClass.IsSubclassOf(typeof(Pawn)) && thingClass != typeof(Pawn)) continue;
                if (def.comps == null)
                    def.comps = new List<CompProperties>();
                def.comps.Add(new CompProperties_TiberiumCheck());
                def.comps.Add(new CompProperties_PawnExtraDrawer());
                def.comps.Add(new CompProperties_CrystalDrawer());
            }
        }        
    }
}
