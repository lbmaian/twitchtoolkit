using Dwarves;
using RimWorld;
using TwitchToolkit;
using TwitchToolkit.Store;
using Verse;

namespace ToolkitLordOfTheRimsPatch
{
    public class IncidentHelper_AncientDwarvenStronghold : IncidentHelper
    {
        private void Log(string msg)
        {
            Verse.Log.Message($"<color=#6441AF>[LOTRToolkit]</color> {msg}");
        }

        public override bool IsPossible()
        {
            Log("Checking if possible...");
            worker = new Dwarves.IncidentWorker_AncientDwarvenStronghold();
            worker.def = IncidentDef.Named("LotRD_QuestAncientStronghold");

            Log($"Worker: {worker.def.ToString()}");

            Map map = Helper.AnyPlayerMap;

            if (map != null)
            {
                parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, map);

                int tileId = 0;
                bool foundTile = IncidentWorker_AncientDwarvenStronghold.TryFindNewSiteTile(out tileId, 8, 30, false, true, -1);
                Log($"Probably a good tile?: {foundTile} @ {tileId}");
                Log($"Any strongholds?: {IncidentWorker_AncientDwarvenStronghold.AnyExistingStrongholds()}");

                Log($"Firing with params: {parms.ToString()}");
                parms.forced = true;
                bool canFire =  worker.CanFireNow(parms);
                Log($"Can fire?: {canFire}");
                return canFire;
            }

            return false;
        }

        public override void TryExecute()
        {
            Log("Trying to execute...");
            bool success = worker.TryExecute(parms);

            Log($"Did execute? {success}");
        }

        private IncidentParms parms = null;
        private IncidentWorker worker = null;
    }
}
