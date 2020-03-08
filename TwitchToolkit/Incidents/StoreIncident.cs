using System;
using RimWorld;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.Incidents
{
    public class StoreIncident : Def
    {
        public string abbreviation;

        public int cost;

        public int eventCap;

        public Type incidentHelper = typeof(IncidentHelper);

        public KarmaType karmaType;

        public int variables = 0;
    }

    public class StoreIncidentSimple : StoreIncident
    {
    }

    public class StoreIncidentVariables : StoreIncident
    {
        public void RegisterCustomSettings()
        {
            if (settings == null)
                settings = StoreIncidentMaker.MakeIncidentVariablesSettings(this);
        }

        public int minPointsToFire = 0;

        public int maxWager = 0;

        public string syntax = null;

        public new Type incidentHelper = typeof(IncidentHelperVariables);

        public bool customSettings = false;

        public Type customSettingsHelper = typeof(IncidentHelperVariablesSettings);

        public IncidentHelperVariablesSettings settings = null;
    }

    public static class StoreIncidentMaker
    {
        public static IncidentHelper MakeIncident(StoreIncidentSimple def)
        {
            try
            {
                IncidentHelper helper = (IncidentHelper)Activator.CreateInstance(def.incidentHelper);
                helper.storeIncident = def;
                return helper;
            }
            catch (Exception e)
            {
                Helper.ErrorLog($"{nameof(StoreIncidentMaker.MakeIncident)} could not create instance of {def.incidentHelper}: " + e.Message);
                throw;
            }
        }

        public static IncidentHelperVariables MakeIncidentVariables(StoreIncidentVariables def)
        {
            try
            {
                IncidentHelperVariables helper = (IncidentHelperVariables)Activator.CreateInstance(def.incidentHelper);
                helper.storeIncident = def;
                return helper;
            }
            catch (Exception e)
            {
                Helper.ErrorLog($"{nameof(StoreIncidentMaker.MakeIncidentVariables)} could not create instance of {def.incidentHelper}: " + e.Message);
                throw;
            }
        }

        public static IncidentHelperVariablesSettings MakeIncidentVariablesSettings(StoreIncidentVariables def)
        {
            if (!def.customSettings)
                return null;

            try
            {
                return (IncidentHelperVariablesSettings)Activator.CreateInstance(def.customSettingsHelper);
            }
            catch (Exception e)
            {
                Helper.ErrorLog($"{nameof(StoreIncidentMaker.MakeIncidentVariablesSettings)} could not create instance of {def.customSettingsHelper}: " + e.Message);
                throw;
            }
        }
    }

    public abstract class IncidentHelperVariablesSettings
    {
        public abstract void ExposeData();

        public abstract void EditSettings();
    }

    [DefOf]
    public class StoreIncidentDefOf
    {
        public static StoreIncidentVariables Item;
    }
}
