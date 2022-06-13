using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWPatches
{
    [DefOf]
    public static class PawnKindDefOf
    {
        public static PawnKindDef Ninetailfox;
        public static PawnKindDef Ninetailfoxwt;
    }

    public static class KyulenFix
    {
        public static bool KyulenSurvivalToolsAlertFix;
        public static bool KyulenRangedWeaponAlertFix;
        public static bool KyulenAlienRacesFix;
        public static void Patch(Harmony harmony)
        {
            // remove survival tools need tools alert
            if (KyulenSurvivalToolsAlertFix)
            {
                try
                {
                    harmony.Patch(
                        original: AccessTools.Method("SurvivalToolsLite.Alert_ColonistNeedsSurvivalTool:WorkingToolless"),
                        prefix: new HarmonyMethod(
                            methodType: typeof(KyulenFix),
                            methodName: nameof(WorkingToolless_Prefix)));
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
            // remove hunting without ranged weapon alert
            if (KyulenRangedWeaponAlertFix)
            {
                try
                {
                    harmony.Patch(
                        original: AccessTools.Method(typeof(RimWorld.WorkGiver_HunterHunt),nameof(RimWorld.WorkGiver_HunterHunt.HasHuntingWeapon)),
                        postfix: new HarmonyMethod(
                            methodType: typeof(KyulenFix),
                            methodName: nameof(HasHuntingWeapon_Postfix)));
                    harmony.Patch(
                        original: AccessTools.Method(typeof(RimWorld.WorkGiver_HunterHunt), nameof(RimWorld.WorkGiver_HunterHunt.HasHuntingWeapon)),
                        postfix: new HarmonyMethod(
                            methodType: typeof(KyulenFix),
                            methodName: nameof(HasHuntingWeapon_Postfix)));
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
            //// fix nullref in SelectMany. Kyulin not alienrace
            //if (KyulenAlienRacesFix)
            //{
            //    try
            //    {
            //        harmony.Patch(
            //            original: AccessTools.Method("AlienRace.HarmonyPatches:UpdateColonistRaces"),
            //            transpiler: new HarmonyMethod(
            //                methodType: typeof(KyulenFix),
            //                methodName: nameof(UpdateColonistRaces_transpiler)));
            //    }
            //    catch (Exception e)
            //    {
            //        Log.Error(e.ToString());
            //    }
            //}
        }

        public static bool isKyulen(Pawn pawn) => pawn.kindDef == PawnKindDefOf.Ninetailfox || pawn.kindDef == PawnKindDefOf.Ninetailfoxwt;
        public static bool isKyulen(ThingDef def) => def == PawnKindDefOf.Ninetailfox.race || def == PawnKindDefOf.Ninetailfoxwt.race;

        public static bool WorkingToolless_Prefix(Pawn pawn, bool __result)
        {
            if(isKyulen(pawn))
            {
                __result = false;
                return false;
            }
            return true;
        }

        public static void HasHuntingWeapon_Postfix(Pawn p, bool __result)
        {
            if (isKyulen(p))
            {
                __result = true;
            }
        }

        //public static IEnumerable<CodeInstruction> UpdateColonistRaces_transpiler(IEnumerable<CodeInstruction> instructions)
        //{
        //    var matcher = new CodeMatcher(instructions);
        //    matcher.MatchStartForward(
        //        new CodeMatch(
        //            opcode: OpCodes.Newobj,
        //            operand: AccessTools.Constructor(
        //                type: typeof(HashSet<ThingDef>),
        //                parameters: new Type[] { typeof(IEnumerable<ThingDef>) })))
        //        .Advance(1)
        //        .Insert(new CodeInstruction(
        //            opcode: OpCodes.Call,
        //            operand: AccessTools.Method(
        //                type: typeof(KyulenFix),
        //                name: nameof(FilterKyulin))));
        //    return matcher.InstructionEnumeration();
        //}

        //public static HashSet<ThingDef> FilterKyulin(HashSet<ThingDef> set)
        //{
        //    set.RemoveWhere(def => isKyulen(def));
        //    return set;
        //}


        public static void DoSettings(Listing_Standard options)
        {
            options.Label("RWPatches.KyulenFix.Header".Translate());
            options.CheckboxLabeled("RWPatches.KyulenSurvivalToolsAlertFix".Translate(), ref KyulenSurvivalToolsAlertFix);
            options.CheckboxLabeled("RWPatches.KyulenRangedWeaponAlertFix".Translate(), ref KyulenRangedWeaponAlertFix);
            //options.CheckboxLabeled("RWPatches.KyulenAlienRacesFix".Translate(), ref KyulenAlienRacesFix);
        }

        public static void ExposeSettings()
        {
            Scribe_Values.Look(ref KyulenSurvivalToolsAlertFix, nameof(KyulenSurvivalToolsAlertFix), false);
            Scribe_Values.Look(ref KyulenRangedWeaponAlertFix, nameof(KyulenRangedWeaponAlertFix), false);
            //Scribe_Values.Look(ref KyulenAlienRacesFix, nameof(KyulenAlienRacesFix), false);
        }
    }
}
