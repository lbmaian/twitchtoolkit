﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TwitchToolkit.Incidents
{
    public abstract class IncidentWorker_Raid : IncidentWorker_PawnsArrive
    {
        public static TwitchToolkit _mod = LoadedModManager.GetMod<TwitchToolkit>();

        protected abstract bool TryResolveRaidFaction(IncidentParms parms);

        protected abstract void ResolveRaidStrategy(IncidentParms parms, PawnGroupKindDef groupKind);

        protected abstract string GetLetterLabel(IncidentParms parms);

        protected abstract string GetLetterText(IncidentParms parms, List<Pawn> pawns);

        protected abstract LetterDef GetLetterDef();

        protected abstract string GetRelatedPawnsInfoLetterText(IncidentParms parms);

        protected abstract void ResolveRaidPoints(IncidentParms parms);

        protected virtual void ResolveRaidArriveMode(IncidentParms parms)
        {
            if (parms.raidArrivalMode != null)
            {
                return;
            }
            if (parms.raidArrivalModeForQuickMilitaryAid)
            {
                if (!(from d in DefDatabase<PawnsArrivalModeDef>.AllDefs
                      where d.forQuickMilitaryAid
                      select d).Any((PawnsArrivalModeDef d) => d.Worker.GetSelectionWeight(parms) > 0f))
                {
                    parms.raidArrivalMode = ((Rand.Value >= 0.6f) ? PawnsArrivalModeDefOf.CenterDrop : PawnsArrivalModeDefOf.EdgeDrop);
                    return;
                }
            }
            if (!(from x in parms.raidStrategy.arriveModes
                  where x.Worker.CanUseWith(parms)
                  select x).TryRandomElementByWeight((PawnsArrivalModeDef x) => x.Worker.GetSelectionWeight(parms), out parms.raidArrivalMode))
            {
                Log.Error("Could not resolve arrival mode for raid. Defaulting to EdgeWalkIn. parms=" + parms, false);
                parms.raidArrivalMode = PawnsArrivalModeDefOf.EdgeWalkIn;
            }
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        {
            this.ResolveRaidPoints(parms);
            if (this.disableEvent) return false;
            if (!this.TryResolveRaidFaction(parms))
            {
                return false;
            }
            PawnGroupKindDef combat = PawnGroupKindDefOf.Combat;
            this.ResolveRaidStrategy(parms, combat);
            this.ResolveRaidArriveMode(parms);
            if (!parms.raidArrivalMode.Worker.TryResolveRaidSpawnCenter(parms))
            {
                return false;
            }
            parms.points = IncidentWorker_Raid.AdjustedRaidPoints(parms.points, parms.raidArrivalMode, parms.raidStrategy, parms.faction, combat);
            PawnGroupMakerParms defaultPawnGroupMakerParms = IncidentParmsUtility.GetDefaultPawnGroupMakerParms(combat, parms, false);
            List<Pawn> list = PawnGroupMakerUtility.GeneratePawns(defaultPawnGroupMakerParms, true).ToList<Pawn>();
            List<string> viewernames = Viewers.ParseViewersFromJsonAndFindActiveViewers();
            if (list.Count > 0 && viewernames != null)
            {
                int count = 0;
                int totalviewers = viewernames.Count();
                foreach (Pawn pawn in list)
                {
                    if (count == list.Count() || viewernames.NullOrEmpty() || pawn.RaceProps.IsMechanoid)
                    {
                        continue;
                    }
                    int thisviewer = Verse.Rand.Range(0, viewernames.Count());
                    NameTriple name = pawn.Name as NameTriple;
                    NameTriple newname = new NameTriple(name.First, viewernames[thisviewer], name.Last);
                    if (!newname.UsedThisGame || ToolkitSettings.RepeatViewerNames)
                    {
                        pawn.Name = newname;
                    }

                    viewernames.RemoveAt(thisviewer);
                    count++;
                }
            }
            if (list.Count == 0)
            {
                Log.Error("Got no pawns spawning raid from parms " + parms, false);
                return false;
            }
            parms.raidArrivalMode.Worker.Arrive(list, parms);
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Points = " + parms.points.ToString("F0"));
            foreach (Pawn current in list)
            {
                string str = (current.equipment == null || current.equipment.Primary == null) ? "unarmed" : current.equipment.Primary.LabelCap;
                stringBuilder.AppendLine(current.KindLabel + " - " + str);
            }
            string letterLabel = this.GetLetterLabel(parms);
            string letterText = this.GetLetterText(parms, list);
            PawnRelationUtility.Notify_PawnsSeenByPlayer_Letter(list, ref letterLabel, ref letterText, this.GetRelatedPawnsInfoLetterText(parms), true, true);
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
                    if (list3[i] != list4)
                    {
                        if (list3[i].Any<Pawn>())
                        {
                            list2.Add(list3[i][0]);
                        }
                    }
                }
            }
            else if (list.Any<Pawn>())
            {
                list2.Add(list[0]);
            }

            Find.LetterStack.ReceiveLetter(letterLabel, letterText, this.GetLetterDef(), list2, parms.faction, stringBuilder.ToString());
            parms.raidStrategy.Worker.MakeLords(parms, list);
            return true;
        }

        public static float AdjustedRaidPoints(float points, PawnsArrivalModeDef raidArrivalMode, RaidStrategyDef raidStrategy, Faction faction, PawnGroupKindDef groupKind)
        {
            if (raidArrivalMode.pointsFactorCurve != null)
            {
                points *= raidArrivalMode.pointsFactorCurve.Evaluate(points);
            }
            if (raidStrategy.pointsFactorCurve != null)
            {
                points *= raidStrategy.pointsFactorCurve.Evaluate(points);
            }
            points = Mathf.Max(points, raidStrategy.Worker.MinimumPoints(faction, groupKind) * 1.05f);
            return points;
        }

        public void DoTable_RaidFactionSampled()
        {
            int ticksGame = Find.TickManager.TicksGame;
            Find.TickManager.DebugSetTicksGame(36000000);
            List<TableDataGetter<Faction>> list = new List<TableDataGetter<Faction>>();
            list.Add(new TableDataGetter<Faction>("name", (Faction f) => f.Name));
            foreach (float points in Dialog_DebugActionsMenu.PointsOptions(false))
            {
                Dictionary<Faction, int> factionCount = new Dictionary<Faction, int>();
                foreach (Faction current in Find.FactionManager.AllFactions)
                {
                    factionCount.Add(current, 0);
                }
                for (int i = 0; i < 500; i++)
                {
                    IncidentParms incidentParms = new IncidentParms();
                    incidentParms.target = Find.CurrentMap;
                    incidentParms.points = points;
                    if (this.TryResolveRaidFaction(incidentParms))
                    {
                        Dictionary<Faction, int> factionCount2;
                        Faction faction;
                        (factionCount2 = factionCount)[faction = incidentParms.faction] = factionCount2[faction] + 1;
                    }
                }
                list.Add(new TableDataGetter<Faction>(points.ToString("F0"), delegate (Faction str)
                {
                    int num = factionCount[str];
                    return ((float)num / 500f).ToStringPercent();
                }));
            }
            Find.TickManager.DebugSetTicksGame(ticksGame);
            DebugTables.MakeTablesDialog<Faction>(Find.FactionManager.AllFactions, list.ToArray());
        }

        public void DoTable_RaidStrategySampled(Faction fac)
        {
            int ticksGame = Find.TickManager.TicksGame;
            Find.TickManager.DebugSetTicksGame(36000000);
            List<TableDataGetter<RaidStrategyDef>> list = new List<TableDataGetter<RaidStrategyDef>>();
            list.Add(new TableDataGetter<RaidStrategyDef>("defName", (RaidStrategyDef d) => d.defName));
            foreach (float points in Dialog_DebugActionsMenu.PointsOptions(false))
            {
                Dictionary<RaidStrategyDef, int> strats = new Dictionary<RaidStrategyDef, int>();
                foreach (RaidStrategyDef current in DefDatabase<RaidStrategyDef>.AllDefs)
                {
                    strats.Add(current, 0);
                }
                for (int i = 0; i < 500; i++)
                {
                    IncidentParms incidentParms = new IncidentParms();
                    incidentParms.target = Find.CurrentMap;
                    incidentParms.points = points;
                    incidentParms.faction = fac;
                    if (this.TryResolveRaidFaction(incidentParms))
                    {
                        this.ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
                        if (incidentParms.raidStrategy != null)
                        {
                            Dictionary<RaidStrategyDef, int> strats2;
                            RaidStrategyDef raidStrategy;
                            (strats2 = strats)[raidStrategy = incidentParms.raidStrategy] = strats2[raidStrategy] + 1;
                        }
                    }
                }
                list.Add(new TableDataGetter<RaidStrategyDef>(points.ToString("F0"), delegate (RaidStrategyDef str)
                {
                    int num = strats[str];
                    return ((float)num / 500f).ToStringPercent();
                }));
            }
            Find.TickManager.DebugSetTicksGame(ticksGame);
            DebugTables.MakeTablesDialog<RaidStrategyDef>(DefDatabase<RaidStrategyDef>.AllDefs, list.ToArray());
        }

        public void DoTable_RaidArrivalModeSampled(Faction fac)
        {
            int ticksGame = Find.TickManager.TicksGame;
            Find.TickManager.DebugSetTicksGame(36000000);
            List<TableDataGetter<PawnsArrivalModeDef>> list = new List<TableDataGetter<PawnsArrivalModeDef>>();
            list.Add(new TableDataGetter<PawnsArrivalModeDef>("mode", (PawnsArrivalModeDef f) => f.defName));
            foreach (float points in Dialog_DebugActionsMenu.PointsOptions(false))
            {
                Dictionary<PawnsArrivalModeDef, int> modeCount = new Dictionary<PawnsArrivalModeDef, int>();
                foreach (PawnsArrivalModeDef current in DefDatabase<PawnsArrivalModeDef>.AllDefs)
                {
                    modeCount.Add(current, 0);
                }
                for (int i = 0; i < 500; i++)
                {
                    IncidentParms incidentParms = new IncidentParms();
                    incidentParms.target = Find.CurrentMap;
                    incidentParms.points = points;
                    incidentParms.faction = fac;
                    if (this.TryResolveRaidFaction(incidentParms))
                    {
                        this.ResolveRaidStrategy(incidentParms, PawnGroupKindDefOf.Combat);
                        this.ResolveRaidArriveMode(incidentParms);
                        Dictionary<PawnsArrivalModeDef, int> modeCount2;
                        PawnsArrivalModeDef raidArrivalMode;
                        (modeCount2 = modeCount)[raidArrivalMode = incidentParms.raidArrivalMode] = modeCount2[raidArrivalMode] + 1;
                    }
                }
                list.Add(new TableDataGetter<PawnsArrivalModeDef>(points.ToString("F0"), delegate (PawnsArrivalModeDef str)
                {
                    int num = modeCount[str];
                    return ((float)num / 500f).ToStringPercent();
                }));
            }
            Find.TickManager.DebugSetTicksGame(ticksGame);
            DebugTables.MakeTablesDialog<PawnsArrivalModeDef>(DefDatabase<PawnsArrivalModeDef>.AllDefs, list.ToArray());
        }

        public bool disableEvent = false;
    }
}