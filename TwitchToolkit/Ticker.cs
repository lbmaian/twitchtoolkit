﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using RimWorld;
using TwitchToolkit.Store;
using TwitchToolkit.Votes;
using UnityEngine;
using Verse;

namespace TwitchToolkit
{
    public class Ticker : Thing
    {
        public Timer timer = null;

        public static long LastIRCPong = 0;

        public static Queue<FiringIncident> FiringIncidents = new Queue<FiringIncident>();
        public static Queue<VoteEvent> VoteEvents = new Queue<VoteEvent>();
        public static Queue<IncidentWorker> Incidents = new Queue<IncidentWorker>();
        public static Queue<IncidentHelper> IncidentHelpers = new Queue<IncidentHelper>();
        public static Queue<IncidentHelperVariables> IncidentHelperVariables = new Queue<IncidentHelperVariables>();

        public bool CreatedByController { get; internal set; }

        static Thread _registerThread;
        static Game _game;
        static TwitchToolkit _mod = Toolkit.Mod;

        static Ticker _instance;
        public static Ticker Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Ticker();
                }
                return _instance;
            }
        }

        public Ticker()
        {
            def = new ThingDef { tickerType = TickerType.Normal, isSaveable = false };
            _registerThread = new Thread(Register);
            _registerThread.Start();
            lastEvent = DateTime.Now;
            LastIRCPong = DateTime.Now.ToFileTime();
        }

        void Register()
        {
            while (true)
            {
                try
                {
                    if (_game != Current.Game)
                    {
                        if (_game != null)
                        {
                            _game.tickManager.DeRegisterAllTickabilityFor(this);
                            _game = null;
                        }

                        _game = Current.Game;
                        if (_game != null)
                        {
                            _game = Current.Game;
                            _game.tickManager.RegisterAllTickabilityFor(this);
                            Toolkit.Mod.RegisterTicker();
                        }
                    }

                    //if (_map != Helper.AnyPlayerMap)
                    //{
                    //    _map = Helper.AnyPlayerMap;
                    //    _mod.Reset();
                    //}
                }
                catch (Exception ex)
                {
                    Helper.Log("Exception: " + ex.Message + "\n" + ex.StackTrace);
                }
                finally
                {
                    Thread.Sleep(1000);
                }
            }
        }

        int[] _baseTimes = { 20, 60, 120, 180, 999999 };
        private int _lastMinute = -1;
        private int _lastCoinReward = -1;

        private void DebugLog(Viewer viewer, string cmd, string msg)
        {
            string finalCmd = cmd ?? "NONE";
            string username = viewer?.username ?? "UNKNOWN";
            DebugLog(username, finalCmd, msg);
        }

        private void DebugLog(string msg)
        {
            DebugLog("UNKNOWN", "NONE", msg);
        }

        private void DebugLog(string viewer, string cmd, string msg)
        {
            Helper.Log($"({viewer}->{cmd}): {msg}");
        }

        public override void Tick()
        {
            try
            {
                if (_game == null || _mod == null)
                {
                    return;
                }

                _mod.Tick();
                var minutes = (int)(_game.Info.RealPlayTimeInteracting / 60f);
                double getTime = (double)Time.time / 60f;
                int time = Convert.ToInt32(Math.Truncate(getTime));

                if (IncidentHelpers.Any())
                {
                    try
                    {
                        while (IncidentHelpers.Any())
                        {
                            var incidentHelper = IncidentHelpers.Dequeue();
                            if (incidentHelper != null)
                            {
                                if (!(incidentHelper is VotingHelper))
                                {
                                    Purchase_Handler.QueuePlayerMessage(incidentHelper.Viewer, incidentHelper.message);
                                }

                                string defName = incidentHelper.storeIncident != null ? incidentHelper.storeIncident.defName : incidentHelper.ToString();
                                DebugLog(incidentHelper.Viewer, incidentHelper.message, $"Trying to execute IH {defName}");
                                incidentHelper.TryExecute();
                            }
                            else
                            {
                                DebugLog("Incident helper dequeued was null! Firing nothing");
                            }
                        }

                        Helper.playerMessages = new List<string>();
                    }
                    catch (Exception e)
                    {
                        Helper.Log($"Exception trying to process incident helper: {e.Message}");
                        Helper.Log(e.ToString());
                    }
                }

                if (IncidentHelperVariables.Any())
                {
                    while (IncidentHelperVariables.Any())
                    {
                        var incidentHelper = IncidentHelperVariables.Dequeue();
                        if (incidentHelper != null)
                        {
                            string defName = incidentHelper.storeIncident != null ? incidentHelper.storeIncident.defName : incidentHelper.ToString();

                            Purchase_Handler.QueuePlayerMessage(incidentHelper.Viewer, incidentHelper.message, incidentHelper.storeIncident.variables);
                            DebugLog(incidentHelper.Viewer, incidentHelper.message, $"Trying to execute IHV {defName}");
                            incidentHelper.TryExecute();
                            if (Purchase_Handler.viewerNamesDoingVariableCommands.Contains(incidentHelper.Viewer.username))
                            {
                                Purchase_Handler.viewerNamesDoingVariableCommands.Remove(incidentHelper.Viewer.username);
                            }
                        }
                        else
                        {
                            DebugLog("Incident variable helper dequeued was null! Firing nothing");
                        }
                    }

                    Helper.playerMessages = new List<string>();
                }

                if (Incidents.Count > 0)
                {
                    var incident = Incidents.Dequeue();
                    IncidentParms incidentParms = new IncidentParms();
                    incidentParms.target = Helper.AnyPlayerMap;
                    DebugLog($"Trying to execute Incident {incident.def.defName}");
                    incident.TryExecute(incidentParms);
                }

                if (FiringIncidents.Count > 0)
                {
                    DebugLog($"Firing {FiringIncidents.First().def.defName}");
                    var incident = FiringIncidents.Dequeue();
                    incident.def.Worker.TryExecute(incident.parms);
                }

                try
                {
                    VoteHandler.CheckForQueuedVotes();
                }
                catch (Exception e)
                {
                    Helper.ErrorLog($"Failed to check for queued votes! {e.Message}");
                }

                if (_lastCoinReward < 0)
                {
                    _lastCoinReward = time;
                }
                else if (ToolkitSettings.EarningCoins && ((time - _lastCoinReward) >= ToolkitSettings.CoinInterval) && Viewers.jsonallviewers != null)
                {
                    _lastCoinReward = time;
                    Viewers.AwardViewersCoins();
                }
                if (_lastMinute < 0)
                {
                    _lastMinute = time;
                }
                else if (_lastMinute < time)
                {
                    _lastMinute = time;
                    Toolkit.JobManager.CheckAllJobs();
                    Viewers.RefreshViewers();
                }
            }
            catch (Exception ex)
            {
                Helper.Log($"Exception: {ex.Message}");
                Helper.Log(ex.ToString());
            }
        }

        public static DateTime lastEvent;
    }
}
