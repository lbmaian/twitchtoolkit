﻿using System;
using TwitchToolkit.IRC;
using TwitchToolkit.Store;
using Verse;

namespace TwitchToolkit.Commands.ModCommands
{
    public class RefreshViewers : CommandDriver
    {
        public override void RunCommand(IRCMessage message)
        {
            Viewers.RefreshViewers();

            Toolkit.client.SendMessage($"@{message.User} viewers have been refreshed.");
        }
    }

    public class KarmaRound : CommandDriver
    {
        public override void RunCommand(IRCMessage message)
        {
            Viewers.AwardViewersCoins();

            Toolkit.client.SendMessage($"@{message.User} rewarding all active viewers coins.");
        }
    }

    public class GiveAllCoins : CommandDriver
    {
        public override void RunCommand(IRCMessage message)
        {
            try
            {
                string[] command = message.Message.Split(' ');

                if (command.Length < 2)
                {
                    return;
                }

                bool isNumeric = int.TryParse(command[1], out int amount);

                if (isNumeric)
                {
                    foreach (Viewer vwr in Viewers.All)
                    {
                        vwr.GiveViewerCoins(amount);
                    }

                    Toolkit.client.SendMessage($"@{message.User} " + Helper.ReplacePlaceholder("TwitchToolkitGiveAllCoins".Translate(), amount: amount.ToString()), CommandsHandler.SendToChatroom(message));
                }
            }
            catch (InvalidCastException e)
            {
                Helper.Log("Give All Coins Syntax Error " + e.Message);
            }
        }
    }

    public class GiveCoins : CommandDriver
    {
        public override void RunCommand(IRCMessage message)
        {
            try
            {
                string[] command = message.Message.Split(' ');

                if (command.Length < 3)
                {
                    return;
                }

                string receiver = command[1].Replace("@", "");

                if (message.User.ToLower() != ToolkitSettings.Channel.ToLower() && receiver.ToLower() == message.User.ToLower())
                {
                    Toolkit.client.SendMessage($"@{message.User} " + "TwitchToolkitModCannotGiveCoins".Translate());
                    return;
                }

                int amount;
                bool isNumeric = int.TryParse(command[2], out amount);
                if (isNumeric)
                {
                    Viewer giftee = Viewers.GetViewer(receiver);

                    Helper.Log($"Giving viewer {giftee.username} {amount} coins");
                    giftee.GiveViewerCoins(amount);
                    Toolkit.client.SendMessage($"@{message.User} " + Helper.ReplacePlaceholder("TwitchToolkitGivingCoins".Translate(), viewer: giftee.username, amount: amount.ToString(), newbalance: giftee.coins.ToString()), CommandsHandler.SendToChatroom(message));
                    Store_Logger.LogGiveCoins(message.User, giftee.username, amount);
                }
            }
            catch (InvalidCastException e)
            {
                Helper.Log("Invalid Give Viewer Coins Command " + e.Message);
            }
        }
    }

    public class CheckUser : CommandDriver
    {
        public override void RunCommand(IRCMessage message)
        {
            try
            {
                string[] command = message.Message.Split(' ');

                if (command.Length < 2)
                {
                    return;
                }

                string target = command[1].Replace("@", "");

                Viewer targeted = Viewers.GetViewer(target);
                Toolkit.client.SendMessage($"@{message.User} " + Helper.ReplacePlaceholder("TwitchToolkitCheckUser".Translate(), viewer: targeted.username, amount: targeted.coins.ToString(), karma: targeted.GetViewerKarma().ToString()), CommandsHandler.SendToChatroom(message));

            }
            catch (InvalidCastException e)
            {
                Helper.Log("Invalid Check User Command " + e.Message);
            }
        }
    }

    public class SetKarma : CommandDriver
    {
        public override void RunCommand(IRCMessage message)
        {
            try
            {
                string[] command = message.Message.Split(' ');

                if (command.Length < 3)
                {
                    return;
                }

                string target = command[1].Replace("@", "");
                int amount;
                bool isNumeric = int.TryParse(command[2], out amount);
                if (isNumeric)
                {
                    Viewer targeted = Viewers.GetViewer(target);
                    targeted.SetViewerKarma(amount);
                    Toolkit.client.SendMessage($"@{message.User}" + Helper.ReplacePlaceholder("TwitchToolkitSetKarma".Translate(), viewer: targeted.username, karma: amount.ToString()), CommandsHandler.SendToChatroom(message));
                }
            }
            catch (InvalidCastException e)
            {
                Helper.Log("Invalid Check User Command " + e.Message);
            }
        }
    }

    public class ToggleCoins : CommandDriver
    {
        public override void RunCommand(IRCMessage message)
        {
            if (ToolkitSettings.EarningCoins)
            {
                ToolkitSettings.EarningCoins = false;
                Toolkit.client.SendMessage($"@{message.User} " + "TwitchToolkitEarningCoinsMessage".Translate() + " " + "TwitchToolkitOff".Translate(), CommandsHandler.SendToChatroom(message));
            }
            else
            {
                ToolkitSettings.EarningCoins = true;
                Toolkit.client.SendMessage($"@{message.User} " + "TwitchToolkitEarningCoinsMessage".Translate() + " " + "TwitchToolkitOn".Translate(), CommandsHandler.SendToChatroom(message));
            }
        }
    }
}
