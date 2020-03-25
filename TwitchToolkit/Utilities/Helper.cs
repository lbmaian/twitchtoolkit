﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RimWorld;
using TwitchToolkit.Incidents;
using TwitchToolkit.Utilities;
using UnityEngine;
using Verse;
using TSIncidents = TwitchToolkit.Incidents;

namespace TwitchToolkit
{
    public static class Helper
    {
        public static string LogFile = Path.Combine(Path.Combine(SaveHelper.dataPath, "Logs"),
            $"{DateTime.Now.Month}_{DateTime.Now.Day}_toolkit_log.txt");

        private static bool _infestationPossible = false;
        public static string _state = null;
        public static List<string> playerMessages = new List<string>();

        public static void Reset()
        {
            _infestationPossible = false;
        }

        public static string ReplacePlaceholder(
          string quote,
          string colonist = null,
          string colonists = null,
          string gender = null,
          string stat = null,
          string skill = null,
          string material = null,
          string item = null,
          string animal = null,
          string from = null,
          string to = null,
          string amount = null,
          string mod = null,
          string viewer = null,
          string newbalance = null,
          string karma = null,
          string first = null,
          string second = null,
          string third = null)
        {
            quote = quote.Replace("{colonist}", colonist ?? "");
            quote = quote.Replace("{colonists}", colonists ?? "");
            quote = quote.Replace("{gender}", gender ?? "");
            quote = quote.Replace("{stat}", stat ?? "");
            quote = quote.Replace("{skill}", skill ?? "");
            quote = quote.Replace("{material}", material ?? "");
            quote = quote.Replace("{item}", item ?? "");
            quote = quote.Replace("{animal}", animal ?? "");
            quote = quote.Replace("{from}", from ?? "");
            quote = quote.Replace("{to}", to ?? "");
            quote = quote.Replace("{amount}", amount ?? "");
            quote = quote.Replace("{mod}", mod ?? "");
            quote = quote.Replace("{viewer}", viewer ?? "");
            quote = quote.Replace("{newbalance}", newbalance ?? "");
            quote = quote.Replace("{karma}", karma ?? "");
            quote = quote.Replace("{first}", first ?? "");
            quote = quote.Replace("{second}", second ?? "");
            quote = quote.Replace("{third}", third ?? "");
            return quote;
        }

        public static Map AnyPlayerMap
        {
            get
            {
                if (Current.Game == null || Current.Game.Maps == null)
                    return null;
                else
                {
                    List<Map> maps = Current.Game.Maps.Where(s => s.IsPlayerHome).ToList();
                    maps.Shuffle();

                    return maps[0];
                }
            }
        }

        public static bool ModActive
        {
            get
            {
                return
                  Helper.AnyPlayerMap != null &&
                  Current.Game.storyteller != null &&
                  Current.Game.storyteller.def != null &&
                  Current.Game.storyteller.def.defName != null;
            }
        }

        private static string[] defaultColors = { "FF0000", "0000FF", "008000", "008000", "FF7F50", "9ACD32", "FF4500", "2E8B57", "DAA520", "D2691E", "5F9EA0", "1E90FF", "FF69B4", "8A2BE2", "8A2BE2" };

        public static string GetRandomColorCode()
        {
            return defaultColors[Verse.Rand.Range(0, defaultColors.Length - 1)];
        }

        public static void FileLog(string logFile, string line)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFile));

            if (!File.Exists(logFile))
            {
                try
                {
                    using (StreamWriter writer = File.CreateText(logFile))
                    {
                        writer.WriteLine("TwitchToolkit - Log - " + DateTime.Now.ToLongDateString());
                    }
                }
                catch (Exception e)
                {
                    Verse.Log.Message(e.Message);
                }
            }

            try
            {
                using (StreamWriter writer = File.AppendText(logFile))
                {
                    writer.WriteLine(line);
                }
            }
            catch (Exception e)
            {
                Verse.Log.Message(e.Message);
            }
        }

        public static void Log(string message)
        {
            Log(message, "<color=#6441A4>[Toolkit]</color> ");
        }

        public static void Log(string message, string prefix)
        {
            message = $"[{DateTime.UtcNow.ToString("mm:HH:ss.ffff")}] {message}";
            Verse.Log.Message(prefix + message);
            FileLog(LogFile, message);
        }

        public static void ErrorLog(string message)
        {
            ErrorLog(message, "[Toolkit] ");
        }

        public static void ErrorLog(string message, string prefix)
        {
            message = $"[{DateTime.UtcNow.ToString("mm:HH:ss.ffff")}] ERROR {message}";
            Verse.Log.Error(prefix + message);
            FileLog(LogFile, message);
        }

        public static void LogPaste(string message)
        {
            Verse.Log.Message(message);
            FileLog(LogFile, message);
        }

        public static void Vote(string message, LetterDef type)
        {
            if (message == null)
            {
                message = "";
            }

            Current.Game.letterStack.ReceiveLetter("TwitchStoriesVote".Translate(), message, type);
        }

        public static void Vote(string message, LetterDef type, Thing thing)
        {
            if (message == null)
            {
                message = "";
            }

            Current.Game.letterStack.ReceiveLetter("TwitchStoriesVote".Translate(), message, type, new TargetInfo(thing));
        }

        public static void Vote(string message, LetterDef type, List<Thing> things)
        {
            if (message == null)
            {
                message = "";
            }

            Find.LetterStack.ReceiveLetter("TwitchStoriesVote".Translate(), message, type, new LookTargets(things));
        }

        public static void Vote(string message, LetterDef type, List<Pawn> pawns)
        {
            if (message == null)
            {
                message = "";
            }

            Find.LetterStack.ReceiveLetter("TwitchStoriesVote".Translate(), message, type, new LookTargets(pawns));
        }

        public static void Vote(string message, LetterDef type, IntVec3 vec)
        {
            if (message == null)
            {
                message = "";
            }

            Current.Game.letterStack.ReceiveLetter("TwitchStoriesVote".Translate(), message, type, new TargetInfo(vec, Helper.AnyPlayerMap, false));
        }

        public static void CarePackage(string message, LetterDef type, IntVec3 vec)
        {
            if (message == null)
            {
                message = "";
            }

            Current.Game.letterStack.ReceiveLetter("TwitchToolkitCarePackage".Translate(), message, type, new TargetInfo(vec, Helper.AnyPlayerMap, false));
        }
        /*
        public static void Mushroom(string gender)
        {
          var hook = new CaptainHook.CaptainHook(typeof(GenderUtility), "GetLabel", typeof(Helper), "GetLabel");
          hook.Install();
          Pawn p = GetColonists(0).First();
          p.gender = (Gender)4;

          Vote("new mushroom", LetterDefOf.PositiveEvent, p);
        }

        public static string GetLabel(this Gender gender, bool animal = false)
        {
          if (gender == Gender.None)
          {
            return "NoneLower".Translate();
          }

          if (gender == Gender.Male)
          {
            return (!animal) ? "Male".Translate() : "MaleAnimal".Translate();
          }

          if (gender == (Gender)4)
          {
            return "Mushroom";
          }

          if (gender != Gender.Female)
          {
            throw new ArgumentException();
          }

          return (!animal) ? "Female".Translate() : "FemaleAnimal".Translate();
        }

        public static void PlaySound(string name)
        {
          SoundDef sound = SoundDefOf.TinyBell;
          Verse.Sound.SoundInfo info = new Verse.Sound.SoundInfo();
          info.pitchFactor = 1f;
          info.volumeFactor = 100f;
          for (int i = 0; i < 3; i++)
          {
            Verse.Sound.SoundStarter.PlayOneShot(sound, info);
            System.Threading.Thread.Sleep(100);
          }
        }
    */
        static IEnumerable<Pawn> GetColonists(float percentColony)
        {
            var pawns = new List<Pawn>();
            foreach (Map map in Current.Game.Maps)
            {
                foreach (Pawn pawn in map.mapPawns.AllPawns)
                {
                    if (!pawn.IsColonist || pawn.Dead || pawn.Destroyed || pawn.Discarded)
                    {
                        continue;
                    }

                    pawns.Add(pawn);
                }
            }

            pawns.Shuffle();
            var i = Math.Max(1, (int)(pawns.Count * percentColony));
            while (i-- > 0)
            {
                yield return pawns[i];
            }
        }

        static IEnumerable<Pawn> GetAnimals(float percentAnimals)
        {
            var pawns = new List<Pawn>();
            foreach (Pawn pawn in Helper.AnyPlayerMap.mapPawns.AllPawns)
            {
                if (!pawn.AnimalOrWildMan() || pawn.IsWildMan() || pawn.Dead || pawn.Destroyed || pawn.Discarded)
                {
                    continue;
                }

                pawns.Add(pawn);
            }

            pawns.Shuffle();
            var i = Math.Max(1, (int)(pawns.Count * percentAnimals));
            while (i-- > 0)
            {
                yield return pawns[i];
            }
        }

        public static bool GetRandomVec3(ThingDef thing, Map map, out IntVec3 vec, int contract = 0)
        {
            return CellFinderLoose.TryFindSkyfallerCell(thing, map, out vec, contract, map.Center, 99999, true, false, false, true, true, false, null);
        }

        #region Weather

        public static void Weather(string quote, WeatherDef weather, LetterDef type)
        {
            Helper.AnyPlayerMap.weatherManager.TransitionTo(weather);
            Vote(quote, type);
        }

        public static void WeatherClear(string quote)
        {
            Weather(quote, WeatherDefOf.Clear, LetterDefOf.NeutralEvent);
        }

        public static void WeatherRain(string quote)
        {
            Weather(quote, WeatherDef.Named("Rain"), LetterDefOf.NeutralEvent);
        }

        public static void WeatherFoggyRain(string quote)
        {
            Weather(quote, WeatherDef.Named("FoggyRain"), LetterDefOf.NeutralEvent);
        }

        public static void WeatherRainyThunderstorm(string quote)
        {
            Weather(quote, WeatherDef.Named("RainyThunderstorm"), LetterDefOf.NeutralEvent);
        }

        public static void WeatherDryThunderstorm(string quote)
        {
            Weather(quote, WeatherDef.Named("DryThunderstorm"), LetterDefOf.NeutralEvent);
        }

        public static void WeatherSnowGentle(string quote)
        {
            Weather(quote, WeatherDef.Named("SnowGentle"), LetterDefOf.NeutralEvent);
        }

        public static void WeatherSnowHard(string quote)
        {
            Weather(quote, WeatherDef.Named("SnowHard"), LetterDefOf.NeutralEvent);
        }

        public static void WeatherFog(string quote)
        {
            Weather(quote, WeatherDef.Named("Fog"), LetterDefOf.NeutralEvent);
        }

        public static void GameCondition(string quote, GameConditionDef condition, int ticks, LetterDef type)
        {
            Helper.AnyPlayerMap.GameConditionManager.RegisterCondition(GameConditionMaker.MakeCondition(condition, ticks));
            Vote(quote, type);
        }

        public static void WeatherFlashstorm(string quote, int ticks = 5 * 60 * 60)
        {
            GameCondition(quote, GameConditionDef.Named("Flashstorm"), ticks, LetterDefOf.NeutralEvent);
        }

        public static bool WeatherHeatWavePossible(int ticks = 5 * 60 * 60)
        {
            if (AnyPlayerMap.mapTemperature.OutdoorTemp < 20)
            {
                return false;
            }

            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDef.Named("HeatWave");
            return incident.CanFireNow(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WeatherHeatWave(string quote, int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDef.Named("HeatWave");

            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool WeatherColdSnapPossible(int ticks = 5 * 60 * 60)
        {
            if (AnyPlayerMap.mapTemperature.OutdoorTemp > 10)
            {
                return false;
            }

            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDef.Named("ColdSnap");
            return incident.CanFireNow(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WeatherColdSnap(string quote, int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDef.Named("ColdSnap");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool WeatherSolarFlarePossible(int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDefOf.SolarFlare;
            return incident.CanFireNow(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WeatherSolarFlare(string quote, int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDefOf.SolarFlare;
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool WeatherToxicFalloutPossible(int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDefOf.ToxicFallout;
            return incident.CanFireNow(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WeatherToxicFallout(string quote, int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDefOf.ToxicFallout;
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool WeatherVolcanicWinterPossible(int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDef.Named("VolcanicWinter");
            return incident.CanFireNow(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WeatherVolcanicWinter(string quote, int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDef.Named("VolcanicWinter");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WeatherEclipse(string quote, int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_MakeGameCondition();
            incident.def = IncidentDefOf.Eclipse;
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap,
            });
        }

        public static bool WeatherAuroraPossible()
        {
            var incident = new RimWorld.IncidentWorker_Aurora();
            incident.def = IncidentDef.Named("Aurora");
            return incident.CanFireNow(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WeatherAurora(string quote, int ticks = 5 * 60 * 60)
        {
            var incident = new RimWorld.IncidentWorker_Aurora();
            incident.def = IncidentDef.Named("Aurora");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        #endregion

        #region Invasion

        public static bool RaidPossible(float points, PawnsArrivalModeDef arrival, RaidStrategyDef strategy = null, Faction faction = null)
        {
            var raidEnemy = new TSIncidents.IncidentWorker_RaidEnemy();
            raidEnemy.def = IncidentDefOf.RaidEnemy;
            return raidEnemy.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = Math.Max(StorytellerUtility.DefaultThreatPointsNow(Helper.AnyPlayerMap), points),
                raidArrivalMode = arrival,
                raidStrategy = strategy == null ? RaidStrategyDefOf.ImmediateAttack : strategy,
                faction = faction
            });
        }

        public static void Raid(string quote, float points, PawnsArrivalModeDef arrival, RaidStrategyDef strategy = null, Faction faction = null)
        {
            if (points < 0) return;
            var raidEnemy = new TSIncidents.IncidentWorker_RaidEnemy();
            raidEnemy.def = IncidentDefOf.RaidEnemy;
            raidEnemy.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = Math.Max(StorytellerUtility.DefaultThreatPointsNow(Helper.AnyPlayerMap), points),
                raidArrivalMode = arrival,
                raidStrategy = strategy == null ? RaidStrategyDefOf.ImmediateAttack : strategy,
                faction = faction
            });
        }

        public static bool SiegePossible(float points, PawnsArrivalModeDef arrival)
        {
            return RaidPossible(points, arrival, DefDatabase<RaidStrategyDef>.GetNamed("Siege", true));
        }

        public static void Siege(string quote, float points, PawnsArrivalModeDef arrival)
        {
            Raid(quote, points, arrival, DefDatabase<RaidStrategyDef>.GetNamed("Siege", true), FactionUtility.DefaultFactionFrom(FactionDef.Named("OutlanderRough")));
        }

        public static bool MechanoidRaidPossible(float points, PawnsArrivalModeDef arrival)
        {
            return RaidPossible(points, arrival, RaidStrategyDefOf.ImmediateAttack, Faction.OfMechanoids);
        }

        public static void MechanoidRaid(string quote, float points, PawnsArrivalModeDef arrival)
        {
            Raid(quote, points, arrival, RaidStrategyDefOf.ImmediateAttack, Faction.OfMechanoids);
        }

        public static void CheckInfestation()
        {
            if (_infestationPossible == true)
            {
                return;
            }

            var incidentParms = new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = StorytellerUtility.DefaultSiteThreatPointsNow(),
                forced = true
            };

            _infestationPossible = new RimWorld.IncidentWorker_Infestation().CanFireNow(incidentParms, true);
        }

        public static bool InfestationPossible()
        {
            return _infestationPossible;
        }

        public static void Infestation(string quote, float points = 0)
        {
            var incidentParms = new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = points <= 0 ? StorytellerUtility.DefaultSiteThreatPointsNow() : points,
                forced = true
            };

            var incident = new RimWorld.IncidentWorker_Infestation();
            if (!incident.CanFireNow(incidentParms, true))
            {
                return;
            }

            incident.def = IncidentDef.Named("Infestation");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(incidentParms);
        }

        #endregion

        #region Diseases
        static void Disease(string quote, HediffDef disease, float percentColony)
        {
            List<Pawn> pawns = GetColonists(percentColony).ToList();
            string names = "";
            foreach (Pawn pawn in pawns)
            {
                pawn.health.AddHediff(disease);
                names += pawn.Name.ToString() + "\n";
            }

            quote = ReplacePlaceholder(quote, colonist: pawns.Last().Name.ToString(), colonists: names);

            Vote(quote, LetterDefOf.NegativeEvent, pawns);
        }

        public static void DiseasePlague(string quote, float percentColony = 0)
        {
            Disease(quote, HediffDefOf.Plague, percentColony);
        }

        public static void DiseaseFlu(string quote, float percentColony = 0)
        {
            Disease(quote, HediffDefOf.Flu, percentColony);
        }

        public static void DiseaseInfection(string quote, float percentColony = 0)
        {
            var label = "";
            var text = "";
            List<Pawn> pawns = GetColonists(percentColony).ToList();
            foreach (Pawn pawn in pawns)
            {
                BodyPartRecord part = pawn.health.hediffSet.GetRandomNotMissingPart(new DamageDef());
                Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.WoundInfection, pawn, part);
                label = "LetterLabelNewDisease".Translate() + " (" + hediff.def.label + ")";
                var d = hediff.def.CompProps<HediffCompProperties_Discoverable>();
                d.sendLetterWhenDiscovered = false;
                pawn.health.hediffSet.hediffs.Add(hediff);

                if (pawns.Count <= 1)
                {
                    text = "NewPartDisease".Translate(pawn.Named("PAWN"), part.Label, pawn.LabelDefinite(), hediff.def.label).AdjustedFor(pawn, "PAWN").CapitalizeFirst();
                }

            }

            if (pawns.Count > 1)
            {
                text = "TwitchStoriesDescription18".Translate();
            }

            if (quote != null)
            {
                text += "\n\n";
                text += quote;
            }

            if (pawns.Count > 1)
            {
                Current.Game.letterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawns, null, null);
            }
            else
            {
                Current.Game.letterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawns[0], null, null);
            }
        }

        public static void DiseaseMalaria(string quote, float percentColony = 0)
        {
            Disease(quote, HediffDefOf.Malaria, percentColony);
        }

        public static void DiseaseGutWorms(string quote, float percentColony = 0)
        {
            Disease(quote, HediffDef.Named("GutWorms"), percentColony);
        }

        public static void DiseaseMuscleParasites(string quote, float percentColony = 0)
        {
            Disease(quote, HediffDef.Named("MuscleParasites"), percentColony);
        }

        public static void DiseaseCryptosleepSickness(string quote, float percentColony = 0)
        {
            Disease(quote, HediffDefOf.CryptosleepSickness, percentColony);
        }

        public static void DiseaseSleepingSickness(string quote, float percentColony = 0)
        {
            //var sleepingSicknes = HediffDef.Named("SleepingSickness");
            //sleepingSicknes.CompProps<HediffCompProperties_TendDuration>().severityPerDayTended = -0.25;
            Disease(quote, HediffDef.Named("SleepingSickness"), percentColony);
        }

        public static void DiseaseBlindness(string quote)
        {
            List<Pawn> pawns = GetColonists(1).ToList();
            for (int i = 0; i < pawns.Count - 1; i++)
            {
                Pawn pawn = pawns[i];
                var eyes = pawn.health.hediffSet.GetNotMissingParts(BodyPartHeight.Undefined, BodyPartDepth.Undefined, null, null).FirstOrDefault((BodyPartRecord x) => x.def == BodyPartDefOf.Eye);
                pawns[i].health.AddHediff(HediffDefOf.Blindness, eyes);
            }

            Vote(quote, LetterDefOf.NegativeEvent);
        }

        public static void DiseaseHeartAttack(string quote)
        {
            foreach (Pawn pawn in GetColonists(0))
            {
                pawn.health.AddHediff(HediffDef.Named("HeartAttack"));
                quote = ReplacePlaceholder(quote, colonist: pawn.Name.ToString());
                break;
            }

            Vote(quote, LetterDefOf.NegativeEvent);
        }

        #endregion

        public static bool BlightPossible()
        {
            var incident = new RimWorld.IncidentWorker_CropBlight();
            incident.def = new IncidentDef();
            incident.def.tale = null;
            incident.def.category = new IncidentCategoryDef();
            incident.def.category.tale = null;
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void Blight(string quote = null)
        {
            var incident = new RimWorld.IncidentWorker_CropBlight();
            incident.def = new IncidentDef();
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.def.tale = null;
            incident.def.category = new IncidentCategoryDef();
            incident.def.category.tale = null;
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool RefugeeChasedPossible()
        {
            var incident = new RimWorld.IncidentWorker_RefugeeChased();
            incident.def = IncidentDef.Named("RefugeeChased");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void RefugeeChased(string quote)
        {
            var incident = new RimWorld.IncidentWorker_RefugeeChased();
            incident.def = IncidentDef.Named("RefugeeChased");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool AnimalTamePossible()
        {
            var incident = new RimWorld.IncidentWorker_SelfTame();
            incident.def.tale = null;
            incident.def.category = new IncidentCategoryDef();
            incident.def.category.tale = null;
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            }, true);
        }

        public static void AnimalTame(string quote = null)
        {
            var incident = new RimWorld.IncidentWorker_SelfTame();
            incident.def = new IncidentDef();
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.def.tale = null;
            incident.def.category = new IncidentCategoryDef();
            incident.def.category.tale = null;
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void VomitRain(string quote, int count)
        {
            for (int i = 0; i < count; i++)
            {
                Vomit(Helper.AnyPlayerMap);
            }

            Vote(quote, LetterDefOf.NeutralEvent);
        }

        public static void Vomit(Map map)
        {
            var vomitDef = ThingDef.Named("DropPodIncoming");
            vomitDef.label = "vomit (incoming)";
            vomitDef.graphicData.texPath = "Things/Filth/PoolSoft";

            var vomit = new Filth();
            vomit.def = ThingDefOf.Filth_Vomit;

            IntVec3 intVec;
            if (!GetRandomVec3(vomitDef, map, out intVec))
            {
                return;
            }

            SkyfallerMaker.SpawnSkyfaller(vomitDef, new List<Thing> { vomit }, intVec, map);
        }

        public static bool WandererPossible()
        {
            var incident = new RimWorld.IncidentWorker_WandererJoin();
            incident.def = IncidentDefOf.WandererJoin;
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void Wanderer(string quote)
        {
            var incident = new RimWorld.IncidentWorker_WandererJoin();
            incident.def = IncidentDefOf.WandererJoin;
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void MilitaryAid(string quote)
        {
            var incident = new TSIncidents.IncidentWorker_CallForAid();

            FactionManager manager = Find.FactionManager;

            Faction ofPlayer = Faction.OfPlayer;

            Faction tryAlly = manager.RandomAlliedFaction(false, false, true, TechLevel.Industrial);

            if (tryAlly == null)
            {
                (from x in manager.AllFactions
                 where !x.IsPlayer && (false || !x.def.hidden) && (false || !x.defeated) && (true || x.def.humanlikeFaction) && (x.def.techLevel >= TechLevel.Industrial) && x.PlayerRelationKind == FactionRelationKind.Neutral
                 select x).TryRandomElement(out tryAlly);
            }

            IncidentParms incidentParms = new IncidentParms();
            incidentParms.target = Helper.AnyPlayerMap;
            incidentParms.faction = tryAlly;
            incidentParms.raidArrivalModeForQuickMilitaryAid = true;
            incidentParms.points = DiplomacyTuning.RequestedMilitaryAidPointsRange.RandomInRange;
            tryAlly.lastMilitaryAidRequestTick = Find.TickManager.TicksGame;
            incident.TryExecute(incidentParms);
        }

        public static bool CargoPodPossible()
        {
            var incident = new RimWorld.IncidentWorker_ResourcePodCrash();
            incident.def = IncidentDefOf.ShipChunkDrop;
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void CargoPod(string quote)
        {
            var incident = new RimWorld.IncidentWorker_ResourcePodCrash();
            incident.def = IncidentDefOf.ShipChunkDrop;
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void CargoPodFrenzy(string quote)
        {
            var incident = new TSIncidents.IncidentWorker_ResourcePodFrenzy(quote);
            incident.def = IncidentDefOf.ShipChunkDrop;
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool TransportPodPossible()
        {
            var incident = new RimWorld.IncidentWorker_TransportPodCrash();
            incident.def = IncidentDefOf.ShipChunkDrop;
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void TransportPod(string quote)
        {
            var incident = new RimWorld.IncidentWorker_TransportPodCrash();
            incident.def = IncidentDefOf.ShipChunkDrop;
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool TraderPossible()
        {
            var incident = new RimWorld.IncidentWorker_TraderCaravanArrival();
            incident.def = IncidentDefOf.TraderCaravanArrival;
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void Trader(string quote)
        {
            var incident = new RimWorld.IncidentWorker_TraderCaravanArrival();
            incident.def = IncidentDefOf.TraderCaravanArrival;
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool TraderShipPossible()
        {
            var incident = new RimWorld.IncidentWorker_OrbitalTraderArrival();
            incident.def = IncidentDefOf.OrbitalTraderArrival;
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void TraderShip(string quote)
        {
            var incident = new RimWorld.IncidentWorker_OrbitalTraderArrival();
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.def = IncidentDefOf.OrbitalTraderArrival;
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void GenderSwap(string quote)
        {
            foreach (Pawn pawn in GetColonists(0))
            {
                pawn.gender = pawn.gender == Gender.Female ? Gender.Male : Gender.Female;

                quote = ReplacePlaceholder(quote, colonist: pawn.Name.ToString(), gender: pawn.gender.ToString());

                Vote(quote, LetterDefOf.NeutralEvent, pawn);
                break;
            }
        }

        public static void Inspiration(string quote)
        {
            List<string> inspirations = new List<string>() { "Inspired_Creativity", "Inspired_Recruitment", "Inspired_Surgery", "Inspired_Trade" };
            inspirations.Shuffle();
            foreach (Pawn pawn in GetColonists(1))
            {
                foreach (string inspiration in inspirations)
                {
                    InspirationDef inspirationDef = DefDatabase<InspirationDef>.GetNamed(inspiration);
                    var letter = inspirationDef.beginLetter;
                    if (quote != null)
                    {
                        inspirationDef.beginLetter += "\n\n";
                        inspirationDef.beginLetter += ReplacePlaceholder(quote, colonist: pawn.Name.ToString(), skill: inspirationDef.label);
                    }
                    var inspired = pawn.mindState.inspirationHandler.TryStartInspiration(inspirationDef);
                    inspirationDef.beginLetter = letter;
                    if (inspired)
                    {
                        return;
                    }
                }
            }

            // TODO: FIX
            //IncreaseRandomSkill(Events.GetQuote(55, "Inspired by chat"));
        }

        public static void Meteorite(string quote)
        {
            var incident = new RimWorld.IncidentWorker_MeteoriteImpact();
            incident.def = IncidentDef.Named("MeteoriteImpact");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void MeteoriteShower(string quote, int count, bool mayHitColony = false)
        {
            List<Thing> meteorites = new List<Thing>();
            for (int i = 0; i < count; i++)
            {
                meteorites.Add(MeteoriteSpawn(Helper.AnyPlayerMap, mayHitColony));
            }

            Vote(quote, LetterDefOf.PositiveEvent, meteorites);
        }

        static bool TryFindMeteoriteCell(out IntVec3 cell, Map map)
        {
            int maxMineables = ThingSetMaker_Meteorite.MineablesCountRange.max;
            return CellFinderLoose.TryFindSkyfallerCell(ThingDefOf.MeteoriteIncoming, map, out cell, 10, default(IntVec3), -1, true, false, false, false, true, true, delegate (IntVec3 x)
            {
                int num = Mathf.CeilToInt(Mathf.Sqrt(maxMineables)) + 2;
                var cellRect = CellRect.CenteredOn(x, num, num);
                int num2 = 0;
                var iterator = cellRect.GetIterator();
                while (!iterator.Done())
                {
                    if (iterator.Current.InBounds(map) && iterator.Current.Standable(map))
                    {
                        num2++;
                    }
                    iterator.MoveNext();
                }
                return num2 >= maxMineables;
            });
        }

        public static Thing MeteoriteSpawn(Map map, bool mayHitColony)
        {
            IntVec3 intVec;
            if (mayHitColony)
            {
                if (!GetRandomVec3(ThingDefOf.MeteoriteIncoming, map, out intVec, 2))
                {
                    return null;
                }
            }
            else if (!TryFindMeteoriteCell(out intVec, map))
            {
                return null;
            }

            var list = ThingSetMakerDefOf.Meteorite.root.Generate();
            SkyfallerMaker.SpawnSkyfaller(ThingDefOf.MeteoriteIncoming, list, intVec, map);

            return list.Last();
        }

        public static void Tornado(string quote, int count = 1)
        {
            Map map = Helper.AnyPlayerMap;

            for (int i = 0; i < count; i++)
            {
                IntVec3 loc;
                if (!GetRandomVec3(ThingDefOf.Tornado, map, out loc, 30))
                {
                    return;
                }
                ThingDef tornado = ThingDef.Named("Tornado");
                GenSpawn.Spawn(tornado, loc, map);
            }

            Vote(quote, LetterDefOf.NegativeEvent);
        }

        public static bool ManhunterPackPossible(float points = 0)
        {
            var incident = new RimWorld.IncidentWorker_ManhunterPack();
            incident.def = IncidentDef.Named("ManhunterPack");
            return incident.CanFireNow(new IncidentParms
            {
                points = points <= 0 ? StorytellerUtility.DefaultSiteThreatPointsNow() : points,
                target = Helper.AnyPlayerMap
            });
        }

        public static void ManhunterPack(string quote, float points = 0)
        {
            var incident = new RimWorld.IncidentWorker_ManhunterPack();
            incident.def = IncidentDef.Named("ManhunterPack");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                points = points <= 0 ? StorytellerUtility.DefaultSiteThreatPointsNow() : points,
                target = Helper.AnyPlayerMap
            });
        }

        public static bool PsychicWavePossible(float points = 0)
        {
            var incident = new RimWorld.IncidentWorker_AnimalInsanityMass();
            incident.def = IncidentDef.Named("AnimalInsanityMass");
            return incident.CanFireNow(new IncidentParms
            {
                points = points <= 0 ? StorytellerUtility.DefaultSiteThreatPointsNow() : points,
                target = Helper.AnyPlayerMap
            });
        }

        public static void PsychicWave(string quote, float points = 0)
        {
            var incident = new RimWorld.IncidentWorker_AnimalInsanityMass();
            incident.def = IncidentDef.Named("AnimalInsanityMass");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                points = points <= 0 ? StorytellerUtility.DefaultSiteThreatPointsNow() : points,
                target = Helper.AnyPlayerMap
            });
        }

        public static bool ManInBlackPossible()
        {
            var incident = new RimWorld.IncidentWorker_WandererJoin();
            incident.def = IncidentDef.Named("StrangerInBlackJoin");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void ManInBlack(string quote)
        {
            var incident = new RimWorld.IncidentWorker_WandererJoin();
            incident.def = IncidentDef.Named("StrangerInBlackJoin");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool MadAnimalPossible()
        {
            var incident = new RimWorld.IncidentWorker_AnimalInsanitySingle();
            incident.def = IncidentDef.Named("AnimalInsanitySingle");
            return incident.CanFireNow(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void MadAnimal(string quote)
        {
            var incident = new RimWorld.IncidentWorker_AnimalInsanitySingle();
            incident.def = IncidentDef.Named("AnimalInsanitySingle");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms()
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool ShipPartPsychicPossible()
        {
            var incident = new IncidentWorker_PsychicEmanatorShipPartCrash(null);
            incident.def = IncidentDef.Named("PsychicEmanatorShipPartCrash");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = StorytellerUtility.DefaultSiteThreatPointsNow()
            });
        }

        public static void ShipPartPsychic(string quote)
        {
            var incident = new IncidentWorker_PsychicEmanatorShipPartCrash(quote);
            incident.def = IncidentDef.Named("PsychicEmanatorShipPartCrash");
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = StorytellerUtility.DefaultSiteThreatPointsNow()
            });
        }

        public static bool ShipPartPoisonPossible()
        {
            var incident = new TSIncidents.IncidentWorker_PoisonShipPartCrash(null);
            incident.def = IncidentDef.Named("PoisonShipPartCrash");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = StorytellerUtility.DefaultSiteThreatPointsNow()
            });
        }

        public static void ShipPartPoison(string quote)
        {
            var incident = new TSIncidents.IncidentWorker_PoisonShipPartCrash(quote);
            incident.def = IncidentDef.Named("PoisonShipPartCrash");
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap,
                points = StorytellerUtility.DefaultSiteThreatPointsNow()
            });
        }

        public static bool PsychicDronePossible()
        {
            var incident = new RimWorld.IncidentWorker_PsychicDrone();
            incident.def = IncidentDef.Named("PsychicDrone");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void PsychicDrone(string quote)
        {
            var incident = new RimWorld.IncidentWorker_PsychicDrone();
            incident.def = IncidentDef.Named("PsychicDrone");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool AmbrosiaSproutPossible()
        {
            var incident = new RimWorld.IncidentWorker_AmbrosiaSprout();
            incident.def = IncidentDef.Named("AmbrosiaSprout");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void AmbrosiaSprout(string quote)
        {
            var incident = new RimWorld.IncidentWorker_AmbrosiaSprout();
            incident.def = IncidentDef.Named("AmbrosiaSprout");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool HerdMigrationPossible()
        {
            var incident = new RimWorld.IncidentWorker_HerdMigration();
            incident.def = IncidentDef.Named("HerdMigration");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void HerdMigration(string quote)
        {
            var incident = new RimWorld.IncidentWorker_HerdMigration();
            incident.def = IncidentDef.Named("HerdMigration");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool AnimalsWanderInPossible()
        {
            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn("TwitchStoriesLetterLabelAnimalsWanderIn", null, false, Verse.Rand.Range(4, 11));
            incident.def = IncidentDef.Named("HerdMigration");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void AnimalsWanderIn(string quote)
        {
            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn("TwitchStoriesLetterLabelAnimalsWanderIn", null, false, Verse.Rand.Range(4, 11));
            incident.def = IncidentDef.Named("HerdMigration");
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool PsychicSoothePossible()
        {
            var incident = new RimWorld.IncidentWorker_PsychicSoothe();
            incident.def = IncidentDef.Named("PsychicSoothe");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void PsychicSoothe(string quote)
        {
            var incident = new RimWorld.IncidentWorker_PsychicSoothe();
            incident.def = IncidentDef.Named("PsychicSoothe");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool PredatorsPossible()
        {
            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn("TwitchStoriesLetterLabelPredators", PawnKindDef.Named("Bear_Grizzly"), false, 1, true);
            incident.def = IncidentDef.Named("HerdMigration");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void Predators(string quote)
        {
            string[] animals = { "Bear_Grizzly", "Bear_Polar", "Rhinoceros", "Elephant", "Megasloth", "Thrumbo" };
            string animal = animals[Verse.Rand.Range(0, animals.Length)];

            ThingDef def = ThingDef.Named(animal);
            float averagePower = 0;
            if (def != null && def.race != null)
            {
                foreach (Tool t in def.tools)
                {
                    averagePower += t.power;
                }
                averagePower = averagePower / def.tools.Count;
            }

            float animalCount = 2.5f;
            if (averagePower > 18)
            {
                animalCount = 2.0f;
            }

            animalCount = animalCount * GetColonists(1).Count();

            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn("TwitchStoriesLetterLabelPredators", PawnKindDef.Named(animal), false, (int)animalCount, true);
            incident.def = IncidentDef.Named("HerdMigration");
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool WildHumanPossible()
        {
            var incident = new RimWorld.IncidentWorker_WildManWandersIn();
            incident.def = IncidentDef.Named("WildManWandersIn");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void WildHuman(string quote)
        {
            var incident = new RimWorld.IncidentWorker_WildManWandersIn();
            incident.def = IncidentDef.Named("WildManWandersIn");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });

            Vote(quote, LetterDefOf.PositiveEvent);
        }

        public static bool ThrumbosPossible()
        {
            var incident = new RimWorld.IncidentWorker_ThrumboPasses();
            incident.def = IncidentDef.Named("ThrumboPasses");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void Thrumbos(string quote)
        {
            var incident = new RimWorld.IncidentWorker_ThrumboPasses();
            incident.def = IncidentDef.Named("ThrumboPasses");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool ShipChunkDropPossible()
        {
            var incident = new RimWorld.IncidentWorker_ShipChunkDrop();
            incident.def = IncidentDef.Named("ShipChunkDrop");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void ShipChunkDrop(string quote)
        {
            var incident = new RimWorld.IncidentWorker_ShipChunkDrop();
            incident.def = IncidentDef.Named("ShipChunkDrop");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool TravelerPossible()
        {
            var incident = new RimWorld.IncidentWorker_TravelerGroup();
            incident.def = IncidentDef.Named("TravelerGroup");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void Traveler(string quote)
        {
            var incident = new RimWorld.IncidentWorker_TravelerGroup();
            incident.def = IncidentDef.Named("TravelerGroup");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool VisitorPossible()
        {
            var incident = new RimWorld.IncidentWorker_VisitorGroup();
            incident.def = IncidentDef.Named("VisitorGroup");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void Visitor(string quote)
        {
            var incident = new RimWorld.IncidentWorker_VisitorGroup();
            incident.def = IncidentDef.Named("VisitorGroup");
            if (quote != null)
            {
                _state = quote;
                Helper.Log("state set to " + _state);
            }
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void IncreaseRandomSkill(string quote)
        {
            foreach (Pawn pawn in GetColonists(0))
            {
                if (pawn.skills.skills.Count <= 0)
                {
                    continue;
                }

                pawn.skills.skills.Shuffle();

                for (int i = 0; i < pawn.skills.skills.Count; i++)
                {
                    SkillRecord skill = pawn.skills.skills[i];
                    if (skill.Level >= 20)
                    {
                        continue;
                    }

                    var before = skill.Level;
                    var amount = Verse.Rand.Range(1, 5);
                    skill.Level += amount;

                    quote = ReplacePlaceholder(quote, colonist: pawn.Name.ToString(), skill: skill.def.skillLabel, from: before.ToString(), to: skill.Level.ToString(), amount: amount.ToString());

                    Vote(quote, LetterDefOf.PositiveEvent, pawn);

                    return;
                }
            }
        }

        public static bool FarmAnimalsPossible()
        {
            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn(null, null, true, 1, false, true);
            incident.def = IncidentDef.Named("FarmAnimalsWanderIn");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void FarmAnimals(string quote)
        {
            int num = Verse.Rand.Range(2, 6);
            quote = ReplacePlaceholder(quote, amount: num.ToString());
            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn(null, null, true, num, false, true);
            incident.def = IncidentDef.Named("FarmAnimalsWanderIn");
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool YorkshireTerrierPossible()
        {
            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn(null, PawnKindDef.Named("YorkshireTerrier"), true, Verse.Rand.Range(3, 8), false, true);
            incident.def = IncidentDef.Named("FarmAnimalsWanderIn");
            return incident.CanFireNow(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static void YorkshireTerrier(string quote)
        {
            var incident = new TSIncidents.IncidentWorker_SpecificAnimalsWanderIn(null, PawnKindDef.Named("YorkshireTerrier"), true, Verse.Rand.Range(3, 8), false, true);
            incident.def = IncidentDef.Named("FarmAnimalsWanderIn");
            incident.TryExecute(new IncidentParms
            {
                target = Helper.AnyPlayerMap
            });
        }

        public static bool PartyPossible()
        {
            Pawn pawn = PartyUtility.FindRandomPartyOrganizer(Faction.OfPlayer, Helper.AnyPlayerMap);
            if (pawn == null)
            {
                return false;
            }

            IntVec3 intVec;
            if (!RCellFinder.TryFindPartySpot(pawn, out intVec))
            {
                return false;
            }

            return true;
        }

        public static void Party(string quote)
        {
            Pawn pawn = PartyUtility.FindRandomPartyOrganizer(Faction.OfPlayer, Helper.AnyPlayerMap);
            if (pawn == null)
            {
                return;
            }
            IntVec3 intVec;
            if (!RCellFinder.TryFindPartySpot(pawn, out intVec))
            {
                return;
            }
            Verse.AI.Group.LordMaker.MakeNewLord(pawn.Faction, new LordJob_Joinable_Party(intVec, pawn), Helper.AnyPlayerMap, null);
            var text = "LetterNewParty".Translate(pawn.LabelShort, pawn);

            if (quote != null)
            {
                text += "\n\n";
                quote = ReplacePlaceholder(quote, colonist: pawn.Name.ToString());
                text += quote;
            }

            Find.LetterStack.ReceiveLetter("LetterLabelNewParty".Translate(), text, LetterDefOf.PositiveEvent, new TargetInfo(intVec, Helper.AnyPlayerMap, false), null, null);
        }

        public static bool MentalBreak(string quote, MentalBreakDef breakDef, float percentColony = 0)
        {
            bool b = false;
            foreach (Pawn pawn in GetColonists(percentColony))
            {
                if (breakDef.Worker.TryStart(pawn, quote, true))
                {
                    b = true;
                }
            }

            return b;
        }

        public static void Berserk(string quote)
        {
            MentalBreakDef breakDef = DefDatabase<MentalBreakDef>.GetNamed("Berserk", true);
            var beginLetter = breakDef.Worker.def.mentalState.beginLetter;
            breakDef.Worker.def.mentalState.beginLetter = null;

            MentalBreak(null, breakDef, 1);

            breakDef.Worker.def.mentalState.beginLetter = beginLetter;

            Vote(quote, LetterDefOf.ThreatBig);
        }

        public static void MentalBreak(string quote, int severity = 0)
        {
            string[][] mentalBreaks = {
                new string[]{"Binging_Food","Wander_OwnRoom","InsultingSpree","TargetedInsultingSpree","Wander_Sad"},
                new string[]{"CorpseObsession","Wander_Psychotic","SadisticRage","Binging_DrugMajor","Tantrum","TargetedTantrum"},
                new string[]{"Berserk","Catatonic","FireStartingSpree","GiveUpExit","Binging_DrugExtreme","Jailbreaker","MurderousRage","RunWild","Slaughterer"}
              };

            int i = 0;
            for (; i < 100; i++)
            {
                string mentalBreak = mentalBreaks[severity][Verse.Rand.Range(0, mentalBreaks[severity].Length)];
                MentalBreakDef breakDef = DefDatabase<MentalBreakDef>.GetNamed(mentalBreak, true);
                if (MentalBreak(quote, breakDef) == true)
                {
                    break;
                }
            }
        }

        public static void Fire(string quote)
        {
            foreach (Thing thing in Helper.AnyPlayerMap.spawnedThings)
            {
                thing.TryAttachFire(5);
            }

            Vote(quote, LetterDefOf.NegativeEvent);
        }

        public static void FireColony(string quote, float percentColony)
        {
            foreach (Pawn pawn in GetColonists(percentColony))
            {
                pawn.TryAttachFire(50);
            }

            Vote(quote, LetterDefOf.NegativeEvent);
        }

        public static void Luciferium(string quote, float percentColony)
        {
            foreach (Pawn pawn in GetColonists(percentColony))
            {
                if (pawn.health.hediffSet.HasHediff(HediffDef.Named("LuciferiumAddiction")))
                {
                    Thing luciferium = new Thing();
                    luciferium.def = ThingDef.Named("Luciferium");
                    pawn.inventory.TryAddItemNotForSale(luciferium);
                }
                else
                {
                    pawn.health.AddHediff(HediffDef.Named("LuciferiumHigh"));
                    pawn.health.AddHediff(HediffDef.Named("LuciferiumAddiction"));
                }
            }

            Vote(quote, LetterDefOf.NegativeEvent);
        }

        public static IntVec3 Rain(ThingDef thingDef, Thing thing)
        {
            IntVec3 intVec;
            if (!GetRandomVec3(thingDef, Helper.AnyPlayerMap, out intVec, 5))
            {
                return intVec;
            }

            Map any = Helper.AnyPlayerMap;
            intVec = DropCellFinder.TradeDropSpot(any);
            TradeUtility.SpawnDropPod(intVec, any, thing);

            return intVec;
        }

        public static IntVec3 Rain(ThingDef thingDef, MinifiedThing thing)
        {
            IntVec3 intVec;
            if (!GetRandomVec3(thingDef, Helper.AnyPlayerMap, out intVec, 5))
            {
                return intVec;
            }

            //SkyfallerMaker.SpawnSkyfaller(thingDef, new List<Thing> { thing }, intVec, Helper.AnyPlayerMap);
            //DropPodUtility.DropThingsNear(intVec, Helper.AnyPlayerMap, new List<Thing> { thing }, 110, false, true, true);
            Map any = Helper.AnyPlayerMap;
            intVec = DropCellFinder.TradeDropSpot(any);
            TradeUtility.SpawnDropPod(intVec, any, thing);

            return intVec;
        }

        public static void Gold(string quote, int min, int max)
        {
            var goldDef = ThingDef.Named("DropPodIncoming");
            var goldThing = new Thing();
            goldThing.def = ThingDefOf.Gold;
            goldThing.stackCount = Verse.Rand.Range(min, max) * GetColonists(1).Count();

            IntVec3 vec = Rain(goldDef, goldThing);

            Vote(quote, LetterDefOf.PositiveEvent, vec);
        }

        public static void Hostile(float percentColony)
        {
            foreach (Pawn pawn in GetColonists(percentColony))
            {

            }
        }

        public static void LogPredators()
        {
            foreach (ThingDef def in DefDatabase<ThingDef>.AllDefs)
            {
                if (def != null && def.race != null)
                {
                    float averagePower = 0;
                    foreach (Tool t in def.tools)
                    {
                        averagePower += t.power;
                    }
                    averagePower = averagePower / def.tools.Count;
                    //Log(def.defName + " -  " + def.race.baseHealthScale + " - " + averagePower);
                }
            }

        }

        public static void Cannibal(string quote)
        {
            foreach (Pawn pawn in GetColonists(1))
            {
                if (pawn.story.traits.HasTrait(TraitDefOf.Cannibal))
                {
                    continue;
                }

                Trait cannibalTrait = new Trait(TraitDefOf.Cannibal, 0, true);
                pawn.story.traits.GainTrait(cannibalTrait);

                quote = ReplacePlaceholder(quote, colonist: pawn.Name.ToString());
                Vote(quote, LetterDefOf.NegativeEvent, pawn);

                return;
            }
        }

        #region Needs testing
        public static void ForeverAlone()
        {
            foreach (Pawn pawn in GetColonists(1f))
            {
                var traitSet = new TraitSet(pawn);
                var trait = new Trait(TraitDefOf.Gay, 0, true);
                traitSet.GainTrait(trait);
                trait = new Trait(TraitDefOf.DislikesMen, 0, true);
                traitSet.GainTrait(trait);
            }
        }

        public static void Test()
        {
            foreach (Pawn pawn in GetColonists(1f))
            {
            }
        }

        public static void VomitParty(float percentColony)
        {
            foreach (Pawn colonist in GetColonists(percentColony))
            {

            }
        }

        public static void Pregnant(string quote, float percentColony = 0)
        {
            Disease(quote, HediffDefOf.Pregnant, percentColony);
        }

        #endregion


        public static Encoding LanguageEncoding()
        {
            string lang = Prefs.LangFolderName.ToLower();
            Encoding encoding;
            switch (lang)
            {
                case "deutsch":
                    encoding = System.Text.Encoding.GetEncoding(850);
                    break;
                default:
                    encoding = Encoding.UTF8;
                    break;
            }
            return encoding;
        }
    }
}
