﻿using System;
using System.IO;
using TwitchToolkit.Utilities;

namespace TwitchToolkit.Store
{
    public static class Store_Logger
    {
        public static string DataPath = Path.Combine(SaveHelper.dataPath, "Logs");
        public static string LogFile = Path.Combine(DataPath, (DateTime.Now.Month + "_" + DateTime.Now.Day + "_log.txt"));

        public static void LogString(string line)
        {
            if (!Directory.Exists(DataPath))
                Directory.CreateDirectory(DataPath);

            if (!File.Exists(LogFile))
            {
                try
                {
                    using (StreamWriter writer = File.CreateText(LogFile))
                    {
                        writer.WriteLine("TwitchToolkit - Log - " + DateTime.Now.ToLongDateString());
                    }
                }
                catch (Exception e)
                {
                    Helper.Log(e.Message);
                }
            }

            try
            {
                using (StreamWriter writer = File.AppendText(LogFile))
                {
                    writer.WriteLine($"[{DateTime.UtcNow.ToString("mm:HH:ss.ffff")}] {line}");
                }
            }
            catch (Exception e)
            {
                Helper.Log(e.Message);
            }
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
