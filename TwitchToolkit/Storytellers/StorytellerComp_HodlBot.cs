﻿using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TwitchToolkit.Votes;
using Verse;
using static TwitchToolkit.Votes.Vote_VotingIncident;

namespace TwitchToolkit.Storytellers
{
    public class StorytellerComp_HodlBot : StorytellerComp_ToryTalker
    {
        private VoteLabelType voteType = VoteLabelType.Category;

        protected new StorytellerCompProperties_HodlBot Props
        {
            get
            {
                return (StorytellerCompProperties_HodlBot)this.props;
            }
        }

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            if (VoteHandler.voteActive || !Rand.MTBEventOccurs(ToolkitSettings.HodlBotMTBDays, 60000f, 1000f))
            {
                if (!forced)
                {
                    yield break;
                }
            }

            if (Rand.Range(0, 1) == 1)
            {
                voteType = Vote_VotingIncident.VoteLabelType.Type;
            }

            List<VotingIncidentEntry> source = VotingIncidentsByWeight();
            List<VotingIncidentEntry> winners = new List<VotingIncidentEntry>();

            string str = null;

            switch (voteType)
            {
                case Vote_VotingIncident.VoteLabelType.Category:
                    {
                        EventCategory category = RandomCategory();
                        source = (from s in source
                                  where s.incident.eventCategory == category
                                  select s).ToList();
                        str = "Which " + category.ToString() + " event should happen?";
                        Helper.Log("rand cat picked " + category);
                        break;
                    }
                case Vote_VotingIncident.VoteLabelType.Type:
                    {
                        EventType randType = RandomType();
                        source = (from s in source
                                  where s.incident.eventType == randType
                                  select s).ToList();
                        str = "Which " + randType + " event should happen?";
                        Helper.Log("rand type picked " + randType);
                        break;
                    }
            }

            for (int i = 0; i < ToolkitSettings.VoteOptions; i++)
            {
                if (GenCollection.TryRandomElementByWeight<VotingIncidentEntry>
                    (from s in source
                     where !winners.Contains(s)
                     select s, (Func<VotingIncidentEntry, float>)((VotingIncidentEntry vi) => vi.weight), out VotingIncidentEntry votingIncidentEntry))
                {
                    votingIncidentEntry.incident.Helper.target = target;

                    if (votingIncidentEntry.incident.Helper.IsPossible())
                    {
                        winners.Add(votingIncidentEntry);
                    }
                }
            }

            if (winners.Count < 3)
            {
                Helper.Log("Less than 3 possible votes were found");
                yield break;
            }

            Dictionary<int, VotingIncident> dictionary = new Dictionary<int, VotingIncident>();

            for (int i = 0; i < winners.Count; i++)
            {
                dictionary.Add(i, winners[i].incident);
            }

            StorytellerPack named = DefDatabase<StorytellerPack>.GetNamed("HodlBot", true);
            VoteHandler.QueueVote(new Vote_HodlBot(dictionary, named, voteType, str));
        }

        public EventType RandomType()
        {
            List<EventType> list = Enum.GetValues(typeof(EventType)).Cast<EventType>().ToList<EventType>();

            Dictionary<EventType, float> dictionary = new Dictionary<EventType, float>();

            foreach (EventType key in list)
            {
                dictionary.Add(key, ToolkitSettings.VoteCategoryWeights[key.ToString()]);
            }

            return dictionary.RandomElementByWeight((KeyValuePair<EventType, float> pair) => pair.Value).Key;
        }

        public EventCategory RandomCategory()
        {
            List<EventCategory> list = Enum.GetValues(typeof(EventCategory)).Cast<EventCategory>().ToList<EventCategory>();

            Dictionary<EventCategory, float> dictionary = new Dictionary<EventCategory, float>();

            foreach (EventCategory key in list)
            {
                dictionary.Add(key, ToolkitSettings.VoteCategoryWeights[key.ToString()]);
            }

            return dictionary.RandomElementByWeight((KeyValuePair<EventCategory, float> pair) => pair.Value).Key;
        }
    }
}
