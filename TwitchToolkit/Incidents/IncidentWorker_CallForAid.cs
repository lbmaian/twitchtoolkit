using RimWorld;
using RimWorld.Planet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Verse;
using Verse.AI;

namespace TwitchToolkit.Incidents
{
    public class IncidentWorker_CallForAid : IncidentWorker_RaidFriendly
    {
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            // IncidentWorker_PawnsArrive.CanFireNowSub
            Map map = (Map)parms.target;
            if (parms.faction == null)
            {
                return CandidateFactions(map).Any();
            }
            return true;
        }

        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = true)
        {
            // Note: desperate is ignored
            return !f.IsPlayer && !f.defeated && !f.temporary && !f.Hidden && f.PlayerRelationKind >= FactionRelationKind.Neutral;
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.ResolveRaidPoints(parms);
            if (!this.TryResolveRaidFaction(parms))
            {
                return false;
            }
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            this.ResolveRaidStrategy(parms, combat);
            this.ResolveRaidArriveMode(parms);
            parms.raidStrategy.Worker.TryGenerateThreats(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            float points = parms.points;
            // Only functional change compared to IncidentWorker_RaidFriendly.TryExecuteWorker:
            parms.points = IncidentWorker_Raid.AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
            List<Pawn> list = parms.raidStrategy.Worker.SpawnThreats(parms);
            if (list == null)
            {
                list = PawnGroupMakerUtility.GeneratePawns(IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms, false), true).ToList<Pawn>();
                if (list.Count == 0)
                {
                    Log.Error("Got no pawns spawning raid from parms " + parms, false);
                    return false;
                }
                parms.raidArrivalMode.Worker.Arrive(list, parms);
            }
            GenerateRaidLoot(parms, points, list);
            TaggedString baseLetterLabel = this.GetLetterLabel(parms);
            TaggedString baseLetterText = this.GetLetterText(parms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref baseLetterLabel, ref baseLetterText, this.GetRelatedPawnsInfoLetterText(parms), true, true);
            List<TargetInfo> list2 = new List<TargetInfo>();
            if (parms.pawnGroups != null)
            {
                List<List<Pawn>> list3 = IncidentParmsUtility.SplitIntoGroups(list, parms.pawnGroups);
                List<Pawn> list4 = list3.MaxBy((List<Pawn> x) => x.Count);
                if (list4.Any<Pawn>())
                {
                    list2.Add(list4[0]);
                }
                for (int i = 0; i < list3.Count; i++)
                {
                    if (list3[i] != list4 && list3[i].Any<Pawn>())
                    {
                        list2.Add(list3[i][0]);
                    }
                }
            }
            else if (list.Any<Pawn>())
            {
                foreach (Pawn t in list)
                {
                    list2.Add(t);
                }
            }
            base.SendStandardLetter(baseLetterLabel, baseLetterText, this.GetLetterDef(), parms, list2, Array.Empty<NamedArgument>());
            parms.raidStrategy.Worker.MakeLords(parms, list);
            LessonAutoActivator.TeachOpportunity(ConceptDefOf.EquippingWeapons, OpportunityType.Critical);
            if (!PlayerKnowledgeDatabase.IsComplete(ConceptDefOf.ShieldBelts))
            {
                for (int j = 0; j < list.Count; j++)
                {
                    if (list[j].apparel.WornApparel.Any((Apparel ap) => ap is ShieldBelt))
                    {
                        LessonAutoActivator.TeachOpportunity(ConceptDefOf.ShieldBelts, OpportunityType.Critical);
                        break;
                    }
                }
            }
            return true;
        }
    }
}
