﻿using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace TwitchToolkit.Votes
{
    public class VoteIncidentDef : Vote
    {

        public VoteIncidentDef(Dictionary<int, IncidentDef> incidents, StorytellerComp source, IncidentParms parms = null, string title = null) : base(new List<int>(incidents.Keys))
        {
            this.parms = parms;
            this.title = title;
            try
            {
                this.incidents = incidents;
                this.source = source;
            }
            catch (InvalidCastException e)
            {
                Helper.ErrorLog("Invalid VoteIncidentDef. " + e.Message);
            }
        }

        public override void StartVote()
        {
            // if streamers has both voting chat messagse and voting window off, create the window still
            if (ToolkitSettings.VotingWindow || (!ToolkitSettings.VotingWindow && !ToolkitSettings.VotingChatMsgs))
            {
                VoteWindow window = new VoteWindow(this);
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
            Ticker.FiringIncidents.Enqueue(new FiringIncident(incidents[DecideWinner()], source, parms));
            Ticker.lastEvent = DateTime.Now;
            Find.WindowStack.TryRemove(typeof(VoteWindow));
            Messages.Message(new Message("Chat voted for: " + incidents[DecideWinner()].LabelCap, MessageTypeDefOf.NeutralEvent), true);
        }

        public override string VoteKeyLabel(int id)
        {
            return incidents[id].LabelCap;
        }

        public Dictionary<int, IncidentDef> incidents = null;
        public StorytellerComp source = null;
        public IncidentParms parms = null;
        public string title = null;
    }
}
