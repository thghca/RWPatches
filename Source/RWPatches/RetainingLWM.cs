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
    [StaticConstructorOnStartup]
    public class RetainingLWM : GameComponent
    {
        public static bool Enabled = false;

        private static RetainingLWM _instance;
        public static RetainingLWM Instance { get => _instance ?? (_instance = Current.Game.GetComponent<RetainingLWM>()); }

        public RetainingLWM() { _instance = this; }
        public RetainingLWM(Game game) : this() { }

        public static void Patch(Harmony harmony)
        {
            if (!Enabled) return;

            harmony.Patch(
                original: AccessTools.Method(typeof(Building_Storage), name: nameof(Building_Storage.GetGizmos)),
                postfix: new HarmonyMethod(
                    methodType: typeof(RetainingLWM),
                    methodName: nameof(Building_Storage_GetGizmos_Postfix)));

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

            harmony.Patch(
                original: AccessTools.Method(typeof(Building), name: nameof(Building.DeSpawn)),
                postfix: new HarmonyMethod(
                   methodType: typeof(RetainingLWM),
                   methodName: nameof(Building_DeSpawn_Postfix)));

        }

        static IEnumerable<Gizmo> Building_Storage_GetGizmos_Postfix(IEnumerable<Gizmo> gizmos, Building_Storage __instance)
        {

            foreach (Gizmo gizmo in gizmos)
                yield return gizmo;

            if (!Enabled) yield break;

            yield return new Command_Toggle
            {
                icon = ContentFinder<UnityEngine.Texture2D>.Get("Buttons/RetainingZone", true),
                defaultLabel = "HaulExplicitly.RetainingZoneLabel".Translate(),
                defaultDesc = "HaulExplicitly.RetainingZoneDesc".Translate(),
                isActive = (() => __instance.IsRetaining()),
                toggleAction = delegate { __instance.SetRetaining(!__instance.IsRetaining()); },
                hotKey = null
            };
        }

        static bool StoreUtility_TryFindBestBetterStorageFor_Prefix(Thing t, Map map, ref bool __result)
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

        static void Building_DeSpawn_Postfix(Building __instance)
        {
            if (__instance is Building_Storage bs) GetRetainingBuildings().Remove(bs);
        }

        public static HashSet<Building_Storage> GetRetainingBuildings()
        {
            return Instance.retainingBuildings;
        }

        public static void DoSettings(Listing_Standard options)
        {
            options.Gap(20f);
            options.GapLine();
            options.Label("RWPatches.RetainingLWM.Header".Translate());
            options.CheckboxLabeled("RWPatches.RetainingLWM".Translate(), ref Enabled);
        }

        public static void ExposeSettings()
        {
            Scribe_Values.Look(ref Enabled, "RetainingLWMEnabled", false);
        }

        public static void CleanGarbage()
        {
            if (!Enabled)
            {
                GetRetainingBuildings().Clear();
                return;
            }

            var all_buildings = new List<Building_Storage>();
            foreach (Map map in Find.Maps)
            {
                all_buildings.AddRange(map.listerBuildings.allBuildingsColonist.OfType<Building_Storage>());
                all_buildings.AddRange(map.listerBuildings.allBuildingsNonColonist.OfType<Building_Storage>());
            }
            GetRetainingBuildings().IntersectWith(all_buildings);
        }

        private HashSet<Building_Storage> retainingBuildings = new HashSet<Building_Storage>();

        public override void ExposeData()
        {
            base.ExposeData();
            if (!Enabled) return;

            if (Scribe.mode == LoadSaveMode.Saving)
                CleanGarbage();

            Scribe_Collections.Look(ref retainingBuildings, "retainingBuildings", LookMode.Reference);
        }

    }

    public static class RetainingBuildingStorageUtility
    {
        public static bool IsRetaining(this Building_Storage bs)
        {
            var hs = RetainingLWM.GetRetainingBuildings();
            return hs.Contains(bs);
        }

        public static void SetRetaining(this Building_Storage bs, bool value)
        {
            var hs = RetainingLWM.GetRetainingBuildings();
            if (value)
                hs.Add(bs);
            else
                hs.Remove(bs);
        }
    }
}
