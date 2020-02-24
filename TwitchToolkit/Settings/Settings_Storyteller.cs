﻿using System;
using System.Collections.Generic;
using TwitchToolkit.Storytellers.StorytellerPackWindows;
using TwitchToolkit.Votes;
using UnityEngine;
using Verse;

namespace TwitchToolkit.Settings
{
    [StaticConstructorOnStartup]
    public static class Settings_Storyteller
    {
        static Settings_Storyteller()
        {
            if (ToolkitSettings.VoteTypeWeights.Count < 1)
            {
                NewVoteTypeWeightsHodlBot();
            }

            if (ToolkitSettings.VoteCategoryWeights.Count < 1)
            {
                NewVoteCategoryWeightsHodlBot();
            }
        }

        public static void NewVoteCategoryWeightsHodlBot()
        {
            ToolkitSettings.VoteCategoryWeights = new Dictionary<string, float>
            {
                { EventCategory.Animal.ToString(), 100 },
                { EventCategory.Colonist.ToString(), 100 },
                { EventCategory.Disease.ToString(), 100 },
                { EventCategory.Drop.ToString(), 100 },
                { EventCategory.Environment.ToString(), 100 },
                { EventCategory.Foreigner.ToString(), 100 },
                { EventCategory.Hazard.ToString(), 100 },
                { EventCategory.Invasion.ToString(), 150 },
                { EventCategory.Mind.ToString(), 100 },
                { EventCategory.Weather.ToString(), 100 }
            };
        }

        public static void NewVoteTypeWeightsHodlBot()
        {
            ToolkitSettings.VoteTypeWeights = new Dictionary<string, float>
            {
                { Votes.EventType.Bad.ToString(), 125 },
                { Votes.EventType.Good.ToString(), 100 },
                { Votes.EventType.Neutral.ToString(), 100 }
            };
        }

        public static void DoWindowContents(Rect rect, Listing_Standard optionsListing)
        {
            optionsListing.Label("All");
            optionsListing.GapLine();

            optionsListing.SliderLabeled("TwitchToolkitVoteTime".Translate(), ref ToolkitSettings.VoteTime, Math.Round((double)ToolkitSettings.VoteTime).ToString(), 1f, 15f);
            optionsListing.SliderLabeled("TwitchToolkitVoteOptions".Translate(), ref ToolkitSettings.VoteOptions, Math.Round((double)ToolkitSettings.VoteOptions).ToString(), 2f, 5f);
            optionsListing.CheckboxLabeled("TwitchToolkitVotingChatMsgs".Translate(), ref ToolkitSettings.VotingChatMsgs);
            optionsListing.CheckboxLabeled("TwitchToolkitVotingWindow".Translate(), ref ToolkitSettings.VotingWindow);
            optionsListing.CheckboxLabeled("TwitchToolkitLargeVotingWindow".Translate(), ref ToolkitSettings.LargeVotingWindow);
            optionsListing.CheckboxLabeled("TwitchToolkitAlwaysUseFullVoteEventLabel".Translate(), ref ToolkitSettings.AlwaysUseFullVoteEventLabel);
            optionsListing.AddLabeledTextField("TwitchToolkitVoteWindowTitle", ref ToolkitSettings.VoteWindowTitle);

            optionsListing.Gap();

            if (optionsListing.ButtonTextLabeled("Edit Storyteller Packs", "Storyteller Packs"))
            {
                Window_StorytellerPacks window = new Window_StorytellerPacks();
                Find.WindowStack.TryRemove(window.GetType());
                Find.WindowStack.Add(window);
            }
        }
    }
}
