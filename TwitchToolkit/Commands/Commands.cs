﻿using System;
using System.Collections.Generic;
using System.Linq;
using TwitchToolkit.IRC;
using Verse;

namespace TwitchToolkit
{
    public static class CommandsHandler
    {
        public static void CheckCommand(IRCMessage msg)
        {

            if (msg == null)
            {
                return;
            }

            if (msg.Message == null)
            {
                return;
            }

            string message = msg.Message;
            string user = msg.User;
            if (message.Split(' ')[0] == "/w")
            {
                List<string> messagewhisper = message.Split(' ').ToList();
                messagewhisper.RemoveAt(0);
                message = string.Join(" ", messagewhisper.ToArray());
                Helper.Log("Whispered command: " + message);
            }

            Viewer viewer = Viewers.GetViewer(user);
            viewer.last_seen = DateTime.Now;

            if (viewer.IsBanned)
            {
                return;
            }

            Command commandDef = DefDatabase<Command>.AllDefs.ToList().Find(s => msg.Message.StartsWith("!" + s.command));

            if (commandDef != null)
            {
                bool runCommand = true;

                if (commandDef.requiresMod && (!viewer.mod && viewer.username.ToLower() != ToolkitSettings.Channel.ToLower()))
                {
                    runCommand = false;
                }

                if (commandDef.requiresAdmin && msg.User.ToLower() != ToolkitSettings.Channel.ToLower())
                {
                    runCommand = false;
                }

                if (!commandDef.enabled)
                {
                    runCommand = false;
                }

                if (commandDef.shouldBeInSeparateRoom && !AllowCommand(msg))
                {
                    runCommand = false;
                }

                if (runCommand)
                {
                    commandDef.RunCommand(msg);
                }

            }

            List<TwitchInterfaceBase> modExtensions = Current.Game.components.OfType<TwitchInterfaceBase>().ToList();

            if (modExtensions == null)
            {
                return;
            }

            foreach (TwitchInterfaceBase parser in modExtensions)
            {
                parser.ParseCommand(msg);
            }
        }

        public static bool AllowCommand(IRCMessage msg)
        {
            if (!ToolkitSettings.UseSeparateChatRoom && (msg.Whisper || ToolkitSettings.AllowBothChatRooms || msg.Channel == "#" + ToolkitSettings.Channel.ToLower())) return true;
            if (msg.Channel == "#chatrooms:" + ToolkitSettings.ChannelID + ":" + ToolkitSettings.ChatroomUUID) return true;
            if (ToolkitSettings.AllowBothChatRooms && ToolkitSettings.UseSeparateChatRoom || (msg.Whisper)) return true;
            return false;
        }

        public static bool SendToChatroom(IRCMessage msg)
        {
            if (msg.Whisper && ToolkitSettings.WhispersGoToChatRoom)
            {
                return true;
            }
            else if (msg.Whisper)
            {
                return false;
            }

            if (msg.Channel == "#" + ToolkitSettings.Channel.ToLower()) return false;
            if (ToolkitSettings.UseSeparateChatRoom && !ToolkitSettings.AllowBothChatRooms) return true;
            if (msg.Channel == "#chatrooms:" + ToolkitSettings.ChannelID + ":" + ToolkitSettings.ChatroomUUID && ToolkitSettings.UseSeparateChatRoom) return true;
            return false;
        }

        static DateTime modsCommandCooldown = DateTime.MinValue;
        static DateTime aliveCommandCooldown = DateTime.MinValue;
    }
}
