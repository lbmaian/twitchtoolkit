using RimWorld;
using Verse;

namespace TwitchToolkit
{
    public class StorytellerCompProperties_CustomCategoryMTB : StorytellerCompProperties
    {
        public StorytellerCompProperties_CustomCategoryMTB() => compClass = typeof(StorytellerComp_CustomCategoryMTB);

        public float mtbDays = -1f;

        public SimpleCurve mtbDaysFactorByDaysPassedCurve;

        public IncidentCategoryDef category;
    }
}
