using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using SimpleJSON;
using TwitchToolkit.Store;
using TwitchToolkit.Utilities;
using Verse;

namespace TwitchToolkit
{
    public static class Viewers
    {
        public static WebClient webClient = new WebClient();
        public static object twitchViewersUpdateLockObj = new object();
        public static string jsonallviewers;
        public static List<Viewer> All = new List<Viewer>();

        static Viewers()
        {
            ServicePointManager.ServerCertificateValidationCallback += (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) => { return true; };
            webClient.DownloadStringCompleted += SaveUsernamesFromJsonResponse;
        }

        public static void AwardViewersCoins(int setamount = 0)
        {
            Dictionary<string, ViewerGroup> viewerNameToGroup = ParseViewersAndGroupsFromJsonAndFindActiveViewers();
            if (viewerNameToGroup != null)
            {
                Helper.Log($"Awarding coins to {viewerNameToGroup.Count} users");
                foreach (KeyValuePair<string, ViewerGroup> pair in viewerNameToGroup)
                {
                    Viewer viewer = GetViewer(pair);

                    if (viewer.IsBanned)
                    {
                        continue;
                    }

                    if (setamount > 0)
                    {
                        viewer.GiveViewerCoins(setamount);
                    }
                    else
                    {
                        int baseCoins = ToolkitSettings.CoinAmount;
                        float baseMultiplier = (float)viewer.GetViewerKarma() / 100f;

                        if (viewer.IsSub)
                        {
                            baseCoins += ToolkitSettings.SubscriberExtraCoins;
                            baseMultiplier *= ToolkitSettings.SubscriberCoinMultiplier;
                        }
                        else if (viewer.IsVIP)
                        {
                            baseCoins += ToolkitSettings.VIPExtraCoins;
                            baseMultiplier *= ToolkitSettings.VIPCoinMultiplier;
                        }
                        else if (viewer.mod)
                        {
                            baseCoins += ToolkitSettings.ModExtraCoins;
                            baseMultiplier *= ToolkitSettings.ModCoinMultiplier;
                        }

                        // check if viewer is active in chat
                        int minutesSinceViewerWasActive = TimeHelper.MinutesElapsed(viewer.last_seen);

                        if (ToolkitSettings.ChatReqsForCoins)
                        {
                            if (minutesSinceViewerWasActive > ToolkitSettings.TimeBeforeHalfCoins)
                            {
                                baseMultiplier *= 0.5f;
                            }

                            if (minutesSinceViewerWasActive > ToolkitSettings.TimeBeforeNoCoins)
                            {
                                baseMultiplier *= 0f;
                            }
                        }

                        double coinsToReward = (double)baseCoins * baseMultiplier;

                        Store_Logger.LogString($"{viewer.username} gets {baseCoins} * {baseMultiplier} coins, total {(int)Math.Ceiling(coinsToReward)}");

                        viewer.GiveViewerCoins((int)Math.Ceiling(coinsToReward));
                    }
                }
            }
            else
            {
                Helper.Log("No users to set coins on.");
            }
        }

        public static void GiveAllViewersCoins(int amount, List<Viewer> viewers = null)
        {
            if (viewers != null)
            {
                foreach (Viewer viewer in viewers)
                {
                    viewer.GiveViewerCoins(amount);
                }

                return;
            }

            Dictionary<string, ViewerGroup> viewerNameToGroup = ParseViewersAndGroupsFromJsonAndFindActiveViewers();
            if (viewerNameToGroup != null)
            {
                foreach (KeyValuePair<string, ViewerGroup> pair in viewerNameToGroup)
                {
                    Viewer viewer = Viewers.GetViewer(pair);
                    if (viewer != null && viewer.GetViewerKarma() > 1)
                    {
                        viewer.GiveViewerCoins(amount);
                    }
                }
            }
        }

        public static void SetAllViewersCoins(int amount, List<Viewer> viewers = null)
        {
            if (viewers != null)
            {
                foreach (Viewer viewer in viewers)
                {
                    viewer.SetViewerCoins(amount);
                }

                return;
            }

            if (All != null)
            {
                foreach (Viewer viewer in All)
                {
                    if (viewer != null)
                    {
                        viewer.SetViewerCoins(amount);
                    }
                }
            }
        }

        public static void GiveAllViewersKarma(int amount, List<Viewer> viewers = null)
        {
            if (viewers != null)
            {
                foreach (Viewer viewer in viewers)
                {
                    viewer.SetViewerKarma(Math.Min(ToolkitSettings.KarmaCap, viewer.GetViewerKarma() + amount));
                }

                return;
            }

            Dictionary<string, ViewerGroup> viewerNameToGroup = ParseViewersAndGroupsFromJsonAndFindActiveViewers();
            if (viewerNameToGroup != null)
            {
                foreach (KeyValuePair<string, ViewerGroup> pair in viewerNameToGroup)
                {
                    Viewer viewer = Viewers.GetViewer(pair);
                    if (viewer != null && viewer.GetViewerKarma() > 1)
                    {
                        viewer.SetViewerKarma(Math.Min(ToolkitSettings.KarmaCap, viewer.GetViewerKarma() + amount));
                    }
                }
            }
        }

        public static void TakeAllViewersKarma(int amount, List<Viewer> viewers = null)
        {
            if (viewers != null)
            {
                foreach (Viewer viewer in viewers)
                {
                    viewer.SetViewerKarma(Math.Max(0, viewer.GetViewerKarma() - amount));
                }

                return;
            }

            if (All != null)
            {
                foreach (Viewer viewer in All)
                {
                    if (viewer != null)
                    {
                        viewer.SetViewerKarma(Math.Max(0, viewer.GetViewerKarma() - amount));
                    }
                }
            }
        }

        public static void SetAllViewersKarma(int amount, List<Viewer> viewers = null)
        {
            if (viewers != null)
            {
                foreach (Viewer viewer in viewers)
                {
                    viewer.SetViewerKarma(amount);
                }

                return;
            }

            if (All != null)
            {
                foreach (Viewer viewer in All)
                {
                    if (viewer != null)
                    {
                        viewer.SetViewerKarma(amount);
                    }
                }
            }
        }

        private enum ViewerGroup
        {
            //broadcaster,
            moderators,
            staff,
            admins,
            global_mods,
            viewers,
            vips,
            // Note: Can't distinguish subscribers.
        };

        public static List<string> ParseViewersFromJsonAndFindActiveViewers()
        {
            return ParseViewersAndGroupsFromJsonAndFindActiveViewers().Keys.ToList();
        }

        private static Dictionary<string, ViewerGroup> ParseViewersAndGroupsFromJsonAndFindActiveViewers()
        {
            Dictionary<string, ViewerGroup> viewerNameToGroup = new Dictionary<string, ViewerGroup>();

            string json;
            JSONNode parsed;
            lock (twitchViewersUpdateLockObj)
            {
                json = jsonallviewers;

                if (json.NullOrEmpty())
                {
                    return null;
                }

                parsed = JSON.Parse(json);
            }
            foreach (ViewerGroup viewerGroup in Enum.GetValues(typeof(ViewerGroup)))
            {
                JSONArray jsonGroup = parsed["chatters"][Enum.GetName(typeof(ViewerGroup), viewerGroup)].AsArray;
                foreach (JSONNode jsonUsername in jsonGroup)
                {
                    string username = jsonUsername.ToString();
                    username = username.Remove(0, 1);
                    username = username.Remove(username.Length - 1, 1);
                    viewerNameToGroup.Add(username, viewerGroup);
                }
            }

            // for bigger streams, the chatter api can get buggy. Therefore we add viewers active in chat within last 30 minutes just in case.

            foreach (Viewer viewer in All.Where(s => s.last_seen != null && TimeHelper.MinutesElapsed(s.last_seen) <= ToolkitSettings.TimeBeforeHalfCoins))
            {
                string username = viewer.username;
                if (!viewerNameToGroup.ContainsKey(username))
                {
                    Helper.Log("Viewer " + username + " added to active viewers through chat participation but not in chatter list.");
                    viewerNameToGroup.Add(username, viewer.mod ? ViewerGroup.moderators : viewer.IsVIP ? ViewerGroup.vips : ViewerGroup.viewers);
                }
            }

            Helper.Log($"Active viewers updated. {viewerNameToGroup.Count} known viewers");

            return viewerNameToGroup;
        }

        public static void SaveUsernamesFromJsonResponse(object sender, DownloadStringCompletedEventArgs response)
        {
            if (response.Error != null)
            {
                Helper.Log($"Getting viewers failed! {response.Error.ToString()}");
                return;
            }
            if (response.Cancelled)
            {
                Helper.Log("Request to get viewers from twitch was cancelled! Viewers may be out of date");
                return;
            }

            Helper.Log($"Updating viwers... {response.Result.Length} bytes");

            lock (twitchViewersUpdateLockObj)
            {
                jsonallviewers = response.Result;
            }

            Helper.Log($"Viewers updated from twitch api. {response.Result.Length} bytes");

            return;
        }

        public static void ResetViewers()
        {
            All = new List<Viewer>();
        }

        private static Viewer GetViewer(KeyValuePair<string, ViewerGroup> pair)
        {
            Viewer viewer = GetViewer(pair.Key);
            switch (pair.Value)
            {
                case ViewerGroup.moderators:
                    viewer.mod = true;
                    break;
                case ViewerGroup.vips:
                    viewer.vip = true;
                    break;
            }
            return viewer;
        }

        public static Viewer GetViewer(string user)
        {
            Viewer viewer = All.Find(x => x.username == user.ToLower());
            if (viewer == null)
            {
                viewer = new Viewer(user);
                viewer.SetViewerCoins((int)ToolkitSettings.StartingBalance);
                viewer.karma = ToolkitSettings.StartingKarma;
            }
            return viewer;
        }

        public static Viewer GetViewerById(int id)
        {
            return All.Find(s => s.id == id);
        }

        public static void RefreshViewers()
        {
            Uri uri = new Uri($"https://tmi.twitch.tv/group/user/{ToolkitSettings.Channel.ToLower()}/chatters");
            Helper.Log($"Retrieving viewers from twich api...");
            webClient.DownloadStringAsync(uri);
        }

        public static void ResetViewersCoins()
        {
            foreach (Viewer viewer in All) viewer.coins = (int)ToolkitSettings.StartingBalance;
        }

        public static void ResetViewersKarma()
        {
            foreach (Viewer viewer in All) viewer.karma = (int)ToolkitSettings.StartingKarma;
        }
    }
}
