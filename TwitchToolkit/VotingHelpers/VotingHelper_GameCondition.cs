using RimWorld;
using TwitchToolkit.Votes;

namespace TwitchToolkit.VotingHelpers
{
    public class VotingHelper_GameCondition : VotingHelper
    {
        public override bool IsPossible()
        {
            worker = new IncidentWorker_MakeGameCondition();
            worker.def = IncidentDef.Named(incidentDefName);

            parms = StorytellerUtility.DefaultParmsNow(IncidentCategoryDefOf.Misc, target);

            return worker.CanFireNow(parms);
        }

        public override void TryExecute()
        {
            worker.TryExecute(parms);
        }

        public string incidentDefName;
        private IncidentWorker worker;
        private IncidentParms parms;
    }
}
