using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RWPatches
{
    public class RWPatchesMod : Mod
    {
        public static RWPatchesMod Instance { get; private set; }
        private static Harmony harmony =  new Harmony("thghca.RWPatches");

        public RWPatchesMod(ModContentPack content) : base(content)
        {
            Log.Message(content.Name + " "  + "Early Init");
            Instance = this;
            var settings = Settings.Instance; //kick rw to read settings

            MinSearchRadius.Patch(harmony);
            RetainingLWM.Patch(harmony);
            RangedAnimalSK_off.Patch(harmony);
            //KyulenFix.Patch(harmony);
        }

        public override void DoSettingsWindowContents(Rect rect) => Settings.Instance.DoSettingsWindowContents(rect);

        public override string SettingsCategory() => "TGC patches";
    }
}
