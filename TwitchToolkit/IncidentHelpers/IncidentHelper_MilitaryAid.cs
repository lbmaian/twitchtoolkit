using System.Linq;
using RimWorld;
using TwitchToolkit.Incidents;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.IncidentHelpers.MilitaryAid
{
    public class CallForAid : IncidentHelper
    {
        public override bool IsPossible()
        {
            return TryFindAlly() != null;
        }

        public override void TryExecute()
        {
            var incident = new IncidentWorker_CallForAid();

            var tryAlly = TryFindAlly();

            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = Helper.AnyPlayerMap;
            incidentParms.faction = tryAlly;
            if (tryAlly.def.techLevel >= TechLevel.Industrial)
                incidentParms.raidArrivalModeForQuickMilitaryAid = true;
            incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
            tryAlly.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
            incident.TryExecute(incidentParms);
        }

        private Faction TryFindAlly()
        {
            FactionManager manager = Find.FactionManager;

            Faction tryAlly = manager.RandomAlliedFaction(false, false, true, TechLevel.Industrial);

            if (tryAlly == null)
            {
                (from x in manager.AllFactions
                 where !x.IsPlayer && !x.def.hidden && !x.defeated && x.def.techLevel >= TechLevel.Industrial && x.PlayerRelationKind == FactionRelationKind.Neutral
                 select x).TryRandomElement(out tryAlly);
            }

            if (tryAlly == null)
            {
                (from x in manager.AllFactions
                 where !x.IsPlayer && !x.def.hidden && !x.defeated && x.PlayerRelationKind == FactionRelationKind.Neutral
                 select x).TryRandomElement(out tryAlly);
            }

            return tryAlly;
        }
    }
}
