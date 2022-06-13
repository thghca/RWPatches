using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWPatches
{
    public static class RangedAnimalSK_off
    {
        public static bool Enabled = false;

        public static void Patch(Harmony harmony)
        {
            if (!Enabled) return;
            harmony.Unpatch(
                original: AccessTools.Method(
                    type: typeof(Pawn),
                    name: nameof(Pawn.TryGetAttackVerb)),
                 patch: AccessTools.Method(
                     typeColonName: "SK.Patch_Pawn_TryGetAttackVerb:Postfix"));
        }

        public static void DoSettings(Listing_Standard options)
        {
            options.Label("RWPatches.RangedAnimalSK_off.Header".Translate());
            options.CheckboxLabeled("RWPatches.RangedAnimalSK_off.Enabled".Translate(), ref Enabled);
        }

        public static void ExposeSettings()
        {
            Scribe_Values.Look(ref Enabled, "RangedAnimalSK_off", false);
        }
    }
}
