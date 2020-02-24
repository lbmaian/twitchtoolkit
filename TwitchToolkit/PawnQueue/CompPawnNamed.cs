using Verse;

namespace TwitchToolkit.PawnQueue
{
    public class CompPawnNamed : ThingComp
    {
        public CompProperties_PawnNamed PropsName
        {
            get
            {
                return (CompProperties_PawnNamed)this.props;
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<bool>(ref this.PropsName.isNamed, "isNamed", false, false);
        }
    }
}
