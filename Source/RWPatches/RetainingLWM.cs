using HarmonyLib;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace RWPatches
{
    public static class RetainingLWM
    {
        public static bool Enabled = false;
        public static bool RetainingDefault = false;
        public static bool RetainingForcedForAll = false;

        public static void Patch(Harmony harmony)
        {
            if (!Enabled) return;

            harmony.Patch(
                original: AccessTools.Method(typeof(StoreUtility), name: nameof(StoreUtility.TryFindBestBetterStorageFor)),
                prefix: new HarmonyMethod(
                    methodType: typeof(RetainingLWM),
                    methodName: nameof(StoreUtility_TryFindBestBetterStorageFor_Prefix)));

            harmony.Patch(
                original: AccessTools.Method(typeof(StoreUtility), name: nameof(StoreUtility.TryFindBestBetterStoreCellFor)),
                prefix: new HarmonyMethod(
                    methodType: typeof(RetainingLWM),
                    methodName: nameof(StoreUtility_TryFindBestBetterStoreCellFor_Prefix)));

            //?maybe not needed
            harmony.Patch(
                original: AccessTools.Method(typeof(StoreUtility), name: nameof(StoreUtility.IsInValidBestStorage)),
                prefix: new HarmonyMethod(
                    methodType: typeof(RetainingLWM),
                    methodName: nameof(StoreUtility_IsInValidBestStorage_Prefix)));
        }

        static bool StoreUtility_TryFindBestBetterStorageFor_Prefix(Thing t, ref bool __result)
        {
            if (!Enabled) return true;

            var bs = t?.GetSlotGroup()?.parent as Building_Storage;
            if (bs != null && bs.IsRetaining())
            {
                __result = false;
                return false;
            }
            return true;
        }

        static bool StoreUtility_IsInValidBestStorage_Prefix(Thing t, ref bool __result)
        {
            if (!Enabled) return true;

            var bs = t?.GetSlotGroup()?.parent as Building_Storage;
            if (bs != null && bs.IsRetaining())
            {
                __result = true;
                return false;
            }

            return true;
        }

        static bool StoreUtility_TryFindBestBetterStoreCellFor_Prefix(Thing t, Map map, ref bool __result)
        {
            if (!Enabled) return true;

            var bs = t?.GetSlotGroup()?.parent as Building_Storage;
            if (bs != null && bs.IsRetaining())
            {
                __result = false;
                return false;
            }
            return true;
        }

        public static void DoSettings(Listing_Standard options)
        {
            options.Label("RWPatches.RetainingLWM.Header".Translate());
            options.CheckboxLabeled("RWPatches.RetainingLWM.Enabled".Translate(), ref Enabled);
            options.CheckboxLabeled("RWPatches.RetainingLWM.RetainingDefault".Translate(), ref RetainingDefault);
            options.CheckboxLabeled("RWPatches.RetainingLWM.RetainingForcedForAll".Translate(), ref RetainingForcedForAll);
        }

        public static void ExposeSettings()
        {
            Scribe_Values.Look(ref Enabled, "RetainingLWMEnabled", false);
            Scribe_Values.Look(ref RetainingDefault, "RetainingDefault", false);
            Scribe_Values.Look(ref RetainingForcedForAll, "RetainingForcedForAll", false);
        }

        public static bool IsRetaining(this Building_Storage bs)
        {
            if (RetainingLWM.RetainingForcedForAll) return true;
            
            return bs.GetComp<RetainingLWMComp>()?.IsRetaining ?? false;
        }
    }
}
