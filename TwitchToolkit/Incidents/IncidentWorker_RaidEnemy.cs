﻿using System;
using System.Collections.Generic;
using System.Linq;
using Verse;
using RimWorld;
using HarmonyLib;
using System.Reflection;

namespace TwitchToolkit.Incidents
{
    public class IncidentWorker_RaidEnemy : IncidentWorker_Raid
    {
        protected override bool FactionCanBeGroupSource(Faction f, Map map, bool desperate = false)
        {
            return base.FactionCanBeGroupSource(f, map, desperate) && f.HostileTo(Faction.OfPlayer) && (desperate || (float)GenDate.DaysPassed >= f.def.earliestRaidDays);
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            if (!base.TryExecuteWorker(parms))
            {
                return false;
            }
            Find.TickManager.slower.SignalForceNormalSpeedShort();
            Find.StoryWatcher.statsRecord.numRaidsEnemy++;
            return true;
        }

        protected override bool TryResolveRaidFaction(IncidentParms parms)
        {
            Map map = (Map)parms.target;
            if (parms.faction != null)
            {
                return true;
            }
            float num = parms.points;
            if (num <= 0f)
            {
                num = 999999f;
            }
            return PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, map, false), true, true, true, true) || PawnGroupMakerUtility.TryGetRandomFactionForCombatPawnGroup(num, out parms.faction, (Faction f) => this.FactionCanBeGroupSource(f, map, true), true, true, true, true);
        }

        protected override void ResolveRaidPoints(IncidentParms parms)
        {
            if (parms.points <= 0f)
            {
                Log.Error("RaidEnemy is resolving raid points. They should always be set before initiating the incident.", false);
                parms.points = StorytellerUtility.DefaultThreatPointsNow(parms.target);
            }
        }

        protected override void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind)
        {
            if (parms.raidStrategy != null)
            {
                return;
            }
            Map map = (Map)parms.target;
            if (!(from d in DefDatabase<RaidStrategyDef>.AllDefs
                  where d.Worker.CanUseWith(parms, groupKind) && (parms.raidArrivalMode != null || (d.arriveModes != null && d.arriveModes.Any((PawnsArrivalModeDef x) => x.Worker.CanUseWith(parms))))
                  select d).TryRandomElementByWeight((RaidStrategyDef d) => d.Worker.SelectionWeight(map, parms.points), out parms.raidStrategy))
            {
                Log.Error(string.Concat("No raid stategy found, defaulting to ImmediateAttack. Faction=", parms.faction.def.defName, ", points=", parms.points, ", groupKind=", groupKind, ", parms=", parms));
                parms.raidStrategy = RaidStrategyDefOf.ImmediateAttack;
            }
        }

        protected override string GetLetterLabel(IncidentParms parms)
        {
            return parms.raidStrategy.letterLabelEnemy + ": " + parms.faction.Name;
        }

        protected override string GetLetterText(IncidentParms parms, List<Pawn> pawns)
        {
            string text = string.Format(parms.raidArrivalMode.textEnemy, parms.faction.def.pawnsPlural, parms.faction.Name.ApplyTag(parms.faction)).CapitalizeFirst();
            text += "\n\n";
            text += parms.raidStrategy.arrivalTextEnemy;
            Pawn pawn = pawns.Find((Pawn x) => x.Faction.leader == x);
            if (pawn != null)
            {
                text += "\n\n";
                text += "EnemyRaidLeaderPresent".Translate(pawn.Faction.def.pawnsPlural, pawn.LabelShort, pawn.Named("LEADER"));
            }

            return text;
        }

        protected override LetterDef GetLetterDef()
        {
            return LetterDefOf.ThreatBig;
        }

        protected override string GetRelatedPawnsInfoLetterText(IncidentParms parms)
        {
            return "LetterRelatedPawnsRaidEnemy".Translate(Faction.OfPlayer.def.pawnsPlural, parms.faction.def.pawnsPlural);
        }

        protected override void GenerateRaidLoot(IncidentParms parms, float raidLootPoints, List<Pawn> pawns)
        {
            if (parms.faction.def.raidLootMaker != null && pawns.Any())
            {
                raidLootPoints *= Find.Storyteller.difficultyValues.EffectiveRaidLootPointsFactor;
                float num = parms.faction.def.raidLootValueFromPointsCurve.Evaluate(raidLootPoints);
                if (parms.raidStrategy != null)
                {
                    num *= parms.raidStrategy.raidLootValueFactor;
                }
                ThingSetMakerParams parms2 = default(ThingSetMakerParams);
                parms2.totalMarketValueRange = new FloatRange(num, num);
                parms2.makingFaction = parms.faction;
                List<Thing> loot = parms.faction.def.raidLootMaker.root.Generate(parms2);
                distributeLoot.Invoke(Activator.CreateInstance(raidLootDistributorType, parms, pawns, loot), Array.Empty<object>());
            }
        }

        private static readonly Type raidLootDistributorType = GenTypes.GetTypeInAnyAssembly("RimWorld.RaidLootDistributor");
        private static readonly MethodInfo distributeLoot = AccessTools.Method(raidLootDistributorType, "DistributeLoot");
    }
}