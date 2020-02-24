﻿using System;
using System.Collections.Generic;
using System.Linq;
using TwitchToolkit.Settings;
using UnityEngine;
using Verse;

namespace TwitchToolkit.Storytellers.StorytellerPackWindows
{
    public class Window_HodlBotSettings : Window
    {
        public Window_HodlBotSettings()
        {
            this.doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            Text.Font = GameFont.Medium;
            listing.Label("<color=#4BB543>HodlBot</color> Settings");
            Text.Font = GameFont.Small;

            listing.Gap();

            listing.GapLine();

            listing.Label("HodlBot chooses events from a random category or type. The chance of one of these categories/types being picked is based on weights below. Setting to 0% disables it.");

            listing.Gap();

            string hodlbotMTBDays = Math.Truncate(((double)ToolkitSettings.HodlBotMTBDays * 100) / 100).ToString();
            listing.TextFieldNumericLabeled<float>("Average Days Between Events", ref ToolkitSettings.HodlBotMTBDays, ref hodlbotMTBDays, 0.5f, 10f);

            listing.Gap();

            if (listing.ButtonTextLabeled("Default HodlBot Weights", "Reset Weights"))
            {
                Settings_Storyteller.NewVoteCategoryWeightsHodlBot();
                Settings_Storyteller.NewVoteTypeWeightsHodlBot();
            }

            listing.Gap();

            listing.Label("Random Category Weights");

            listing.Gap();

            List<string> VoteCategoryList = ToolkitSettings.VoteCategoryWeights.Keys.ToList();
            List<float> VoteCategoryFloatList = ToolkitSettings.VoteCategoryWeights.Values.ToList();

            int newWeights = 0;

            for (int i = 0; i < VoteCategoryList.Count(); i++)
            {
                string buffer = VoteCategoryFloatList[i].ToString();
                float newValue = VoteCategoryFloatList[i];
                float percentage = (float)Math.Round((newValue / totalWeightsForCategories) * 100f, 2);
                listing.TextFieldNumericLabeled<float>(VoteCategoryList[i] + " " + percentage + "% - ", ref newValue, ref buffer);

                ToolkitSettings.VoteCategoryWeights[VoteCategoryList[i]] = newValue;
                newWeights += (int)newValue;
            }

            totalWeightsForCategories = newWeights;

            listing.Gap();

            listing.Label("Random Type Weights");

            listing.Gap();

            List<string> VoteTypeList = ToolkitSettings.VoteTypeWeights.Keys.ToList();
            List<float> VoteTypeFloatList = ToolkitSettings.VoteTypeWeights.Values.ToList();

            newWeights = 0;

            for (int i = 0; i < VoteTypeList.Count(); i++)
            {
                string buffer = VoteTypeFloatList[i].ToString();
                float newValue = VoteTypeFloatList[i];
                float percentage = (float)Math.Round((newValue / totalWeightsForKarma) * 100f, 2);
                listing.TextFieldNumericLabeled<float>(VoteTypeList[i] + " " + percentage + "% - ", ref newValue, ref buffer);

                ToolkitSettings.VoteTypeWeights[VoteTypeList[i]] = newValue;
                newWeights += (int)newValue;
            }

            totalWeightsForKarma = newWeights;

            listing.End();
        }

        int totalWeightsForCategories = 1;
        int totalWeightsForKarma = 1;

        public override Vector2 InitialSize => new Vector2(500f, 700f);
    }
}
