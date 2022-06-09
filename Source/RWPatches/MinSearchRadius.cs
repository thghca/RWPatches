using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;
using Verse;
using Verse.Sound;
using static HarmonyLib.AccessTools;

namespace RWPatches
{
    public static class MinSearchRadius
    {
        public static bool Enabled = false;
        public static float MinValue = 1;
        public static float MaxValue = 100;
        public static float RoundTo = 0.5f;

        public static void Patch(Harmony harmony)
        {
            harmony.Patch(
                original: AccessTools.Method(typeof(Dialog_BillConfig),name: nameof(Dialog_BillConfig.DoWindowContents)), 
                transpiler: new HarmonyMethod(
                    methodType: typeof(MinSearchRadius),
                    methodName: nameof(Transpiler)));
        }

        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            bool patched1 = false;
            bool patched2 = false;

            var codes = new List<CodeInstruction>(instructions);

            for (var i = 0; i < codes.Count; i++)
            {


                if (!patched1 && codes[i].Is(OpCodes.Ldflda, AccessTools.Field(typeof(RimWorld.Bill), "ingredientSearchRadius")) &&
                    codes[i + 1].Is(OpCodes.Ldstr, "F0"))
                {
                    codes[i + 1].operand = "F1";
                    patched1 = true;
                }

                if (!patched2 &&
                    codes[i].opcode == OpCodes.Ldc_R4 &&
                    codes[i + 1].Is(OpCodes.Ldc_R4, 3f) &&
                    codes[i + 2].opcode == OpCodes.Ldc_R4 &&
                    codes[i + 3].Is(OpCodes.Callvirt, AccessTools.Method("Verse.Listing_Standard:Slider")))
                {
                    //codes[i + 1].operand = 1f;
                    codes[i + 3].opcode = OpCodes.Call;
                    codes[i+3].operand = AccessTools.Method(typeof(MinSearchRadius),nameof(SearchRadiusSlider));
                    patched2 = true;
                }
            }
            if (!patched1) Log.Error("MinSearchRadius patch1 failed");
            if (!patched2) Log.Error("MinSearchRadius patch2 failed");
            return codes.AsEnumerable();
        }

        //replace Listing_Standard.Slider(float val, float min, float max) call
        public static float SearchRadiusSlider(Listing_Standard listing, float val, float min, float max)
        {
            if(!Enabled) return listing.Slider(val, min, max);

            min = MinValue;
            max = MaxValue;
            float num = Widgets.HorizontalSlider(listing.GetRect(22f), val, min, max, false, RoundTo>0?RoundTo.ToString():null, MinValue.ToString(), MaxValue.ToString(), RoundTo);
            if (num != val)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
                if (num >= MaxValue) num = 999f;
            }
            listing.Gap(listing.verticalSpacing);
            return num;
        }

        public static void DoSettings(Listing_Standard options)
        {
            Rect rect;
            options.Label("RWPatches.MinSearchRadius.Header".Translate());

            options.CheckboxLabeled("RWPatches.MinSearchRadius.Enabled".Translate(), ref Enabled);
            if (Enabled)
            {
                rect = options.GetRect(22f);
                Widgets.Label(rect.LeftHalf(), "RWPatches.MinSearchRadius.MinValue".Translate());
                Widgets.TextFieldNumeric( rect.RightHalf(), ref MinValue, ref minSearchRadiusValueBuffer, 0f, MaxValue);
                options.Gap(options.verticalSpacing);

                rect = options.GetRect(22f);
                Widgets.Label(rect.LeftHalf(), "RWPatches.MinSearchRadius.MaxValue".Translate());
                Widgets.TextFieldNumeric(rect.RightHalf(), ref MaxValue, ref maxSearchRadiusValueBuffer, MinValue, 100f);
                options.Gap(options.verticalSpacing);
            }

            options.Gap();
            rect = options.GetRect(22f);
            Widgets.Label(rect.LeftHalf(), "RWPatches.MinSearchRadius.RoundTo".Translate() + ": " + (RoundTo <= 0 ? "RWPatches.MinSearchRadius.NoRound".TranslateSimple() : RoundTo.ToString("F1")));
            float num = Widgets.HorizontalSlider(rect.RightHalf(), RoundTo <= 0 ? 0 : RoundTo, 0, 10, false, null, "0", "10", 0.1f);
            if (num <= 0) num = -1f;
            if (num != RoundTo)
            {
                SoundDefOf.DragSlider.PlayOneShotOnCamera(null);
            }
            RoundTo = num;
            options.Gap(options.verticalSpacing);
        }

        public static void ExposeSettings()
        {
            Scribe_Values.Look(ref Enabled, "MinSearchRadiusEnabled", false);
            Scribe_Values.Look(ref MinValue, "MinSearchRadiusValue", 1f);
            Scribe_Values.Look(ref RoundTo, "MinSearchRadiusRoundTo", 0.5f);
        }

        private static string minSearchRadiusValueBuffer;
        private static string maxSearchRadiusValueBuffer;
    }
}
