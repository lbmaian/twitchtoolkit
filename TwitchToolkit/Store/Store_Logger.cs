using System;
using System.IO;
using TwitchToolkit.Utilities;

namespace TwitchToolkit.Store
{
    public static class Store_Logger
    {
        public static string LogFile = Path.Combine(Path.Combine(SaveHelper.dataPath, "Logs"),
            $"{DateTime.Now.Month}_{DateTime.Now.Day}_toolkit_store_log.txt");

        public static void LogString(string line)
        {
            Helper.FileLog(LogFile, $"[{DateTime.UtcNow.ToString("mm:HH:ss.ffff")}] {line}");
        }

        public static void LogPurchase(string username, string command)
        {
            LogString($"Purchase {username}: {command}");
        }

        public static void LogKarmaChange(string username, int oldKarma, int newKarma)
        {
            LogString($"{username}'s karma went from {oldKarma} to {newKarma}");
        }

        public static void LogGiveCoins(string username, string giftee, int amount)
        {
            LogString($"{username} gave viewer {giftee} {amount} coins");
        }

        public static void LogGiftCoins(string username, string giftee, int amount)
        {
            LogString($"{username} gifted viewer {giftee} {amount} coins");
        }
    }
}
