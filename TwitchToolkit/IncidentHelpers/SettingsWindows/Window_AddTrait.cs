﻿using TwitchToolkit.IncidentHelpers.IncidentHelper_Settings;
using UnityEngine;
using Verse;

namespace TwitchToolkit.IncidentHelpers.SettingsWindows
{
    public class Window_AddTrait : Window
    {
        public Window_AddTrait()
        {
            this.doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            listing.Label("Add Trait Settings");

            traitsBuffer = AddTraitSettings.maxTraits.ToString();
            listing.TextFieldNumericLabeled<int>("Maximum Traits", ref AddTraitSettings.maxTraits, ref traitsBuffer, 1f, 10f);

            listing.End();
        }

        private string traitsBuffer = "";
    }
}
