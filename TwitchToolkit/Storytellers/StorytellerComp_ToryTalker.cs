using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchToolkit.Votes;
using Verse;

namespace TwitchToolkit.Storytellers
{
    public class StorytellerComp_ToryTalker : StorytellerComp
    {
        private const int MAX_VOTE_OPTIONS = 12;

        protected StorytellerCompProperties_ToryTalker Props
        {
            get
            {
                return (StorytellerCompProperties_ToryTalker)this.props;
            }
        }

        public override IEnumerable<FiringIncident> MakeIntervalIncidents(IIncidentTarget target)
        {
            voteTracker = Current.Game.GetComponent<StoryTellerVoteTracker>();

            bool voteNotActive = !VoteHandler.voteActive;
            bool isTimeToFireAVote = Rand.MTBEventOccurs(ToolkitSettings.ToryTalkerMTBDays, 60000f, 1000f);
            if ((voteNotActive && isTimeToFireAVote) || forced)
            {
                List<VotingIncidentEntry> entries = VotingIncidentsByWeight();
                List<VotingIncidentEntry> winners = new List<VotingIncidentEntry>();

                int targetVoteOptions = Math.Min(ToolkitSettings.VoteOptions, MAX_VOTE_OPTIONS);

                Helper.Log(string.Join(",", entries.Select(x => x.incident.label).ToArray()));

                while (winners.Count < targetVoteOptions && entries.Any())
                {
                    Helper.Log($"Winners at {winners.Count}/{targetVoteOptions}. Entries at {entries.Count}");
                    VotingIncidentEntry entry = entries.RandomElementByWeight((VotingIncidentEntry vi) => vi.weight);

                    VotingHelper helper = VotingIncidentMaker.makeVotingHelper(entry.incident);
                    helper.target = target;

                    bool isIncidentPossible = false;
                    try
                    {
                        isIncidentPossible = helper.IsPossible();
                    }
                    catch (Exception e)
                    {
                        Helper.Log($"Exception checking if incident '{entry.incident.LabelCap}' is possible. Not using: {e.Message}");
                    }

                    if (isIncidentPossible)
                    {
                        Helper.Log($"Incident '{entry.incident.LabelCap}' has won. Adding");
                        entry.incident.helper = helper;
                        winners.Add(entry);
                    }
                    else
                    {
                        Helper.Log($"Incident '{entry.incident.LabelCap}' not possible.");
                    }

                    // If its not possible, we cant use it. If it won, we dont want to use it again
                    entries.Remove(entry);
                }

                Dictionary<int, VotingIncident> incidents = new Dictionary<int, VotingIncident>();

                for (int i = 0; i < winners.Count; i++)
                {
                    incidents.Add(i, winners[i].incident);
                }

                StorytellerPack pack = DefDatabase<StorytellerPack>.GetNamed("ToryTalker");

                if (incidents.Any())
                {
                    VoteHandler.QueueVote(new Vote_ToryTalker(incidents, pack, ToolkitSettings.VoteWindowTitle));
                }
                else
                {
                    Helper.ErrorLog("Need at least one incident to start a vote!");
                }
            }

            yield break;
        }

        public virtual List<VotingIncidentEntry> VotingIncidentsByWeight()
        {
            voteTracker = Current.Game.GetComponent<StoryTellerVoteTracker>();
            List<VotingIncident> candidates;
            if (voteTracker.VoteHistory.ContainsKey(voteTracker.lastID))
            {
                List<KeyValuePair<int, int>> history = voteTracker.VoteHistory.ToList();
                Helper.Log("History count " + history.Count);
                history.OrderBy(s => s.Value);
                IEnumerable<VotingIncident> search = DefDatabase<VotingIncident>.AllDefs.Where(s => s.defName == voteTracker.VoteIDs[history[0].Key]);
                Helper.Log("Search count " + search.Count());
                if (search != null && search.Count() > 0)
                {
                    previousVote = search.ElementAt(0);
                }
            }

            if (previousVote != null)
            {
                candidates = new List<VotingIncident>(DefDatabase<VotingIncident>.AllDefs.Where(s => s != previousVote));

                Helper.Log($"Previous vote was {previousVote.defName}");
            }
            else
            {
                candidates = new List<VotingIncident>(DefDatabase<VotingIncident>.AllDefs);
            }

            List<VotingIncidentEntry> voteEntries = new List<VotingIncidentEntry>();

            foreach (VotingIncident incident in candidates)
            {
                int weight = CalculateVotingIncidentWeight(incident);
                if (weight == 0)
                {
                    Helper.Log($"Incident {incident.LabelCap} weighted at 0. Ignoring...");
                }
                else
                {
                    Helper.Log($"Incident {incident.LabelCap} weighted at {weight}");
                    voteEntries.Add(new VotingIncidentEntry(incident, weight));
                }
                
            }

            return voteEntries;
        }

        public virtual int CalculateVotingIncidentWeight(VotingIncident incident)
        {
            int previousWinsInVotingPeriod = voteTracker.TimesVoteHasBeenWonInVotingPeriod(incident);

            int initialWeight = (5 / incident.weight) * 100;

            int weightRemovedFromVotingPeriodWins = previousWinsInVotingPeriod * 20;

            int weightRemovedFromPreviousCategory = incident.eventCategory == voteTracker.previousCategory ? 50 : 0;

            int weightRemovedFromPreviousType = incident.eventType == voteTracker.previousType ? 50 : 0;

            int tallyWeight = initialWeight - weightRemovedFromVotingPeriodWins;
            tallyWeight -= weightRemovedFromPreviousCategory;
            tallyWeight -= weightRemovedFromPreviousType;

            return Convert.ToInt32(Math.Max(tallyWeight, 0) * ((float)incident.voteWeight / 100f));
        }

        private bool TimerHasElapsed()
        {
            return true;
        }

        private VotingIncident previousVote = null;

        public StoryTellerVoteTracker voteTracker;

        public bool forced = false;
    }
}
