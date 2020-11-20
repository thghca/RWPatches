using RimWorld;
using System;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace RWPatches
{
    public class Settings : ModSettings
    {
        #region static
        private static Settings _instance;
        public static Settings Instance => _instance ?? (_instance = RWPatchesMod.Instance.GetSettings<Settings>());
        #endregion

        public void DoSettingsWindowContents(Rect rect)
        {
            Listing_Standard options = new Listing_Standard();

            options.Begin(rect);
            {
                options.Gap(20f);
                MinSearchRadius.DoSettings(options);
                options.Gap(20f);
                RetainingLWM.DoSettings(options);
            }
            options.End();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            MinSearchRadius.ExposeSettings();

            RetainingLWM.ExposeSettings();
        }

    }
}