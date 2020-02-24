﻿using RimWorld;
using Verse;

namespace TwitchToolkit.Incidents
{
    public class IncidentWorker_PsychicEmanatorShipPartCrash : IncidentWorker_ShipPartCrash
    {

        public IncidentWorker_PsychicEmanatorShipPartCrash(string quote) : base(quote) { }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            var map = (Map)parms.target;
            if (map.gameConditionManager.ConditionIsActive(GameConditionDefOf.PsychicDrone))
            {
                return false;
            }
            return base.CanFireNowSub(parms);
        }
    }
}
