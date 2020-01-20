using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TwitchToolkit.Incidents;
using TwitchToolkit.Store;
using UnityEngine;
using Verse;

namespace TwitchToolkit.Windows
{
    public class Window_Trackers : Window
    {
        private Vector2 scrollPosition = Vector2.zero;

        public Window_Trackers()
        {
            this.doCloseButton = true;
            UpdateTrackerStats();
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect topBox = new Rect(0, 0, 300f, 28f);

            Widgets.Label(topBox, "Viewers: " + viewerCount);
            topBox.y += topBox.height;

            Widgets.Label(topBox, "Days per cooldown period: " + ToolkitSettings.EventCooldownInterval + " days");

            topBox.y += topBox.height;

            if (Widgets.ButtonText(topBox, "Cooldown Settings"))
            {
                SettingsWindow window = new SettingsWindow(Toolkit.Mod);
                Find.WindowStack.TryRemove(window.GetType());
                Find.WindowStack.Add(window);
                ToolkitSettings.currentTab = ToolkitSettings.SettingsTab.Cooldowns;
            }

            Rect karmaBox = new Rect(0, 120f, inRect.width / 2f, 28f);

            Widgets.Label(karmaBox, "Limit Events By Type:");
            Widgets.Checkbox(new Vector2(180f, karmaBox.y), ref ToolkitSettings.MaxEvents);
            karmaBox.y += karmaBox.height;

            // side one


            Rect sideOne = new Rect(0, karmaBox.y + 32f, 100f, 28f);
            Rect sideTwo = new Rect(sideOne)
            {
                x = 140f
            };

            Widgets.Label(sideOne, "Good");
            sideOne.y += sideOne.height;

            Widgets.Label(sideTwo, goodEventsInLog + "/" + goodEventsMax);
            bool goodBool = goodEventsMaxed;
            Widgets.Checkbox(new Vector2(sideTwo.x + 40f, sideTwo.y), ref goodBool);
            sideTwo.y += sideTwo.height;

            Widgets.Label(sideOne, "Bad");
            sideOne.y += sideOne.height;

            Widgets.Label(sideTwo, badEventsInLog + "/" + badEventsMax);
            bool badBool = badEventsMaxed;
            Widgets.Checkbox(new Vector2(sideTwo.x + 40f, sideTwo.y), ref badBool);
            sideTwo.y += sideTwo.height;

            Widgets.Label(sideOne, "Neutral");
            sideOne.y += sideOne.height;

            Widgets.Label(sideTwo, neutralEventsInLog + "/" + neutralEventsMax);
            bool neutralBool = neutralEventsMaxed;
            Widgets.Checkbox(new Vector2(sideTwo.x + 40f, sideTwo.y), ref neutralBool);
            sideTwo.y += sideTwo.height;

            Widgets.Label(sideOne, "Care Packages");
            sideOne.y += sideOne.height;

            Widgets.Label(sideTwo, carePackagesInLog + "/" + carePackagesMax);
            bool careBool = carePackagesMaxed;
            Widgets.Checkbox(new Vector2(sideTwo.x + 40f, sideTwo.y), ref careBool);
            sideTwo.y += sideTwo.height;

            float sideTwoWidth = ((inRect.width / 2f) - 200f);
            // SIDE TWO
            Rect eventBox = new Rect(sideTwoWidth, 120f, inRect.width / 2f, 28f);

            Widgets.Label(eventBox, "Limit Events By Event:");
            Widgets.Checkbox(new Vector2(eventBox.x + 180f, eventBox.y), ref ToolkitSettings.EventsHaveCooldowns);

            int validLoggedIncidents = storeIncidentsLogged.Count(x => x.Value >= 1);

            Rect outRect = new Rect(eventBox.x, eventBox.y + 32f, inRect.width - eventBox.x, inRect.height - 200f);
            Rect viewRect = new Rect(0, 0f, outRect.width - 20f, (validLoggedIncidents * 31f));

            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.BeginScrollView(outRect, ref scrollPosition, ref viewRect);

            foreach (KeyValuePair<StoreIncident, int> incidentPair in storeIncidentsLogged)
            {
                if (incidentPair.Value < 1) continue;

                bool maxed = storeIncidentMaxed[incidentPair.Key];
                StringBuilder label = new StringBuilder();
                label.AppendFormat("{0,50}", incidentPair.Key.LabelCap);
                label.AppendFormat(": {0,9} (Fired/Max) ", $"{incidentPair.Value}/{storeIncidentMax[incidentPair.Key]}");
                label.AppendFormat(" {0,3} days til next use", storeIncidentsDayTillUsuable[incidentPair.Key]);
                label.AppendFormat(" {0,-6}", maxed ? "MAXED" : "");

                Rect rectToDrawAt = listing.GetRect(28f);
                Widgets.Label(rectToDrawAt.LeftPart(0.9f),label.ToString());
                if (Widgets.ButtonText(rectToDrawAt.RightPart(0.1f), "Edit"))
                {
                    StoreIncidentEditor window = new StoreIncidentEditor(incidentPair.Key);
                    Find.WindowStack.TryRemove(window.GetType());
                    Find.WindowStack.Add(window);
                }

                listing.Gap(5);
            }
            

            listing.EndScrollView(ref viewRect);
            listing.End();

            cachedFramesCount++;

            if (cachedFramesCount >= 800)
            {
                UpdateTrackerStats();
                cachedFramesCount = 0;
            }
        }

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        void UpdateTrackerStats()
        {
            viewerCount = Viewers.jsonallviewers == null ? 0 : Viewers.ParseViewersFromJsonAndFindActiveViewers().Count;

            cooldownsByTypeEnabled = ToolkitSettings.MaxEvents;

            Store_Component component = Current.Game.GetComponent<Store_Component>();

            goodEventsInLog = component.KarmaTypesInLogOf(KarmaType.Good);
            badEventsInLog = component.KarmaTypesInLogOf(KarmaType.Bad);
            neutralEventsInLog = component.KarmaTypesInLogOf(KarmaType.Neutral);
            carePackagesInLog = component.IncidentsInLogOf(DefDatabase<StoreIncident>.GetNamed("Item").abbreviation);

            goodEventsMax = ToolkitSettings.MaxGoodEventsPerInterval;
            badEventsMax = ToolkitSettings.MaxBadEventsPerInterval;
            neutralEventsMax = ToolkitSettings.MaxNeutralEventsPerInterval;
            carePackagesMax = ToolkitSettings.MaxCarePackagesPerInterval;

            goodEventsMaxed = goodEventsInLog >= goodEventsMax;
            badEventsMaxed = badEventsInLog >= badEventsMax;
            neutralEventsMaxed = neutralEventsInLog >= neutralEventsMax;
            carePackagesMaxed = carePackagesInLog >= carePackagesMax;

            cooldownsByIncidentEnabled = ToolkitSettings.EventsHaveCooldowns;

            List<StoreIncident> storeIncidents = DefDatabase<StoreIncident>.AllDefs.ToList();

            storeIncidentsLogged = new Dictionary<StoreIncident, int>();
            storeIncidentMax = new Dictionary<StoreIncident, int>();
            storeIncidentMaxed = new Dictionary<StoreIncident, bool>();
            storeIncidentsDayTillUsuable = new Dictionary<StoreIncident, float>();

            foreach (StoreIncident incident in storeIncidents)
            {
                storeIncidentsLogged.Add(incident, component.IncidentsInLogOf(incident.abbreviation));
                storeIncidentMax.Add(incident, incident.eventCap);
                storeIncidentMaxed.Add(incident, storeIncidentsLogged[incident] >= incident.eventCap);

                if (storeIncidentsLogged[incident] >= incident.eventCap)
                {
                    storeIncidentsDayTillUsuable.Add(incident, component.DaysTillIncidentIsPurchaseable(incident));
                }
                else
                {
                    storeIncidentsDayTillUsuable.Add(incident, 0);
                }
                
            }

        }

        int cachedFramesCount = 0;

        int viewerCount;

        bool cooldownsByTypeEnabled;

        int goodEventsInLog;
        int badEventsInLog;
        int neutralEventsInLog;
        int carePackagesInLog;

        int goodEventsMax;
        int badEventsMax;
        int neutralEventsMax;
        int carePackagesMax;

        bool goodEventsMaxed;
        bool badEventsMaxed;
        bool neutralEventsMaxed;
        bool carePackagesMaxed;

        bool cooldownsByIncidentEnabled;

        Dictionary<StoreIncident, int> storeIncidentsLogged = new Dictionary<StoreIncident, int>();
        Dictionary<StoreIncident, int> storeIncidentMax = new Dictionary<StoreIncident, int>();
        Dictionary<StoreIncident, bool> storeIncidentMaxed = new Dictionary<StoreIncident, bool>();
        Dictionary<StoreIncident, float> storeIncidentsDayTillUsuable = new Dictionary<StoreIncident, float>();
        
    }
}
