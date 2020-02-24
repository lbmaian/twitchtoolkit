﻿using System.Collections.Generic;
using RimWorld;
using TwitchToolkit.Storytellers;
using Verse;

namespace TwitchToolkit.Votes
{
    public class Vote_Mercurius : VoteIncidentDef
    {
        public Vote_Mercurius(Dictionary<int, IncidentDef> incidents, StorytellerComp source, IncidentParms parms = null, string title = null) : base(incidents, source, parms)
        {
            this.pack = DefDatabase<StorytellerPack>.GetNamed("Mercurius");
            this.title = title;
        }

        public override void StartVote()
        {
            // if streamers has both voting chat messagse and voting window off, create the window still
            if (ToolkitSettings.VotingWindow || (!ToolkitSettings.VotingWindow && !ToolkitSettings.VotingChatMsgs))
            {
                VoteWindow window = new VoteWindow(this, "<color=#BF0030>" + title + "</color>");
                Find.WindowStack.Add(window);
            }

            if (ToolkitSettings.VotingChatMsgs)
            {
                Toolkit.client.SendMessage(title ?? "TwitchStoriesChatMessageNewVote".Translate() + ": " + "TwitchToolKitVoteInstructions".Translate());
                foreach (KeyValuePair<int, IncidentDef> pair in incidents)
                {
                    Toolkit.client.SendMessage($"[{pair.Key + 1}]  {VoteKeyLabel(pair.Key)}");
                }
            }
        }

        public override void EndVote()
        {
            Current.Game.GetComponent<StoryTellerVoteTracker>().LogStorytellerCompVote(pack);
            base.EndVote();
        }

        StorytellerPack pack;
    }
}
