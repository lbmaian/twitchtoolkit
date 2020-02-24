﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RimWorld;
using Verse;
using Verse.AI;

namespace TwitchToolkit.Incidents
{
    public class IncidentWorker_CallForAid : IncidentWorker_Raid
    {
        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            IEnumerable<IAttackTarget> targetsHostileToColony = map.attackTargetsCache.TargetsHostileToColony;
            if (target0 == null)
            {
                target0 = new Func<IAttackTarget, bool>(GenHostility.IsActiveThreatToPlayer);
            }
            IEnumerable<Faction> source = (from p in targetsHostileToColony.Where(target0)
                                           select ((Thing)p).Faction).Distinct<Faction>();
            return base.FactionCanBeGroupSource(f, map, desperate) && !f.def.hidden && f.PlayerRelationKind == FactionRelationKind.Ally && (!source.Any<Faction>() || source.Any((Faction hf) => hf.HostileTo(f)));
        }

        protected override bool CanFireNowSub(IncidentParms parms)
        {
            if (!base.CanFireNowSub(parms))
            {
                return false;
            }
            Map map = (Map)parms.target;
            IEnumerable<IAttackTarget> targetsHostileToColony = map.attackTargetsCache.TargetsHostileToColony;
            if (target1 == null)
            {
                target1 = new Func<IAttackTarget, bool>(GenHostility.IsActiveThreatToPlayer);
            }
            return targetsHostileToColony.Where(target1).Sum(delegate (IAttackTarget p)
            {
                Pawn pawn = p as Pawn;
                if (pawn != null)
                {
                    return pawn.kindDef.combatPower;
                }
                return 0f;
            }) > 120f;
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (parms.faction != null)
            {
                return true;
            }
            if (!base.CandidateFactions(map, false).Any<Faction>())
            {
                return false;
            }
            parms.faction = base.CandidateFactions(map, false).RandomElementByWeight((Faction fac) => (float)fac.PlayerGoodwill + 120.000008f);
            return true;
        }

        protected override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelFriendly;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = string.Format(parms.raidArrivalMode.textFriendly, parms.faction.def.pawnsPlural, parms.faction.Name);
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextFriendly;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                text += "\n\n";
                text += "FriendlyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
            }

            return text;
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.PositiveEvent;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidFriendly".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }

        [CompilerGenerated]
        private static Func<IAttackTarget, bool> target0;

        [CompilerGenerated]
        private static Func<IAttackTarget, bool> target1;
    }
}
