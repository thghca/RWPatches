using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace RWPatches
{
    class RetainingLWMComp:ThingComp
    {
        private bool isRetaining = false;

        private Texture2D cachedCommandTex;

        private Texture2D CommandTex
        {
            get
            {
                if (this.cachedCommandTex == null)
                {
                    this.cachedCommandTex = ContentFinder<UnityEngine.Texture2D>.Get("Buttons/RetainingZone", true);
                }
                return this.cachedCommandTex;
            }
        }

        public bool IsRetaining
        {
            get => isRetaining; 
            set
            {
                isRetaining = value;
                Log.Message("isRetaining" + isRetaining.ToString());
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref isRetaining, "isRetaining", false, false);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            foreach (Gizmo gizmo in base.CompGetGizmosExtra())
                yield return gizmo;

            if (!RetainingLWM.Enabled) yield break;

            yield return new Command_Toggle
            {
                icon = CommandTex,
                defaultLabel = "HaulExplicitly.RetainingZoneLabel".Translate(),
                defaultDesc = "HaulExplicitly.RetainingZoneDesc".Translate(),
                isActive = (() => isRetaining),
                toggleAction = delegate { this.isRetaining = !this.isRetaining; },
                hotKey = null
            };
        }

        public RetainingLWMCompProperties Props => (RetainingLWMCompProperties)this.props;
    }

    public class RetainingLWMCompProperties : CompProperties
    {
        public RetainingLWMCompProperties()
        {
            this.compClass = typeof(RetainingLWMComp);
        }

        public RetainingLWMCompProperties(Type compClass) : base(compClass)
        {
            this.compClass = compClass;
        }
    }
}
