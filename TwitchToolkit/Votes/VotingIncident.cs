using System;
using RimWorld;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.Votes
{
    public class VotingIncident : Def
    {
        public int weight;

        public int voteWeight = 100;

        public Storyteller storyteller;

        public EventType eventType;

        public EventCategory eventCategory;

        public Type votingHelper = typeof(IncidentHelper);

        public VotingHelper helper = null;

        public VotingHelper Helper
        {
            get
            {
                if (helper == null)
                {
                    Log.Warning("Casting " + label);
                    helper = VotingIncidentMaker.makeVotingHelper(this);
                }

                return helper;
            }
        }
    }

    public abstract class VotingHelper : IncidentHelper
    {
        public IIncidentTarget target;

        public override bool IsPossible()
        {
            return true;
        }
    }

    public static class VotingIncidentMaker
    {
        public static VotingHelper makeVotingHelper(VotingIncident def)
        {
            try
            {
                return (VotingHelper)Activator.CreateInstance(def.votingHelper);
            }
            catch (Exception e)
            {
                Helper.ErrorLog($"{nameof(VotingIncidentMaker.makeVotingHelper)} could not create instance of {def.votingHelper}: " + e.Message);
                throw;
            }
        }
    }

    public enum EventType
    {
        Bad = 1,
        Good = 2,
        Neutral = 4
    }

    public enum EventCategory
    {
        Animal = 8,
        Colonist = 4,
        Drop = 128,
        Environment = 2,
        Foreigner = 256,
        Disease = 512,
        Hazard = 32,
        Invasion = 1,
        Mind = 64,
        Weather = 16
    }

    public enum Storyteller
    {
        ToryTalker,
        SpartanBot,
        UristBot,
        HodlBot
    }
}
