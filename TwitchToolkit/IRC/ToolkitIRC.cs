﻿using System;
using TwitchToolkit.Store;
using TwitchToolkit.Utilities;
using TwitchToolkit.Votes;
using Verse;

namespace TwitchToolkit.IRC
{
    public class ToolkitIRC
    {
        static ToolkitIRC instance = null;

        public static ToolkitIRC Instance
        {
            get
            {
                if (instance == null)
                {
                    NewInstance();
                }

                return instance;
            }

            private set
            {
                instance = value;

                Toolkit.client = instance;
            }
        }

        public static void NewInstance()
        {
            if (!Helper.ModActive || Current.Game == null)
            {
                return;
            }

            if (instance != null)
            {
                Helper.Log("Previous instance exists, trying to destroy");
                instance.Disconnect();
                instance = null;
            }

            Instance = new ToolkitIRC();
        }

        public ToolkitIRC()
        {
            Connect();
        }

        private void Connect()
        {
            StripUsernameAndChannel();

            Helper.Log("creating new connection");

            client = new IRCClient(_ircHost, _ircPort, ToolkitSettings.Username, ToolkitSettings.OAuth, ToolkitSettings.Channel.ToLower());
            client.OnPrivMsg += OnPrivMsg;
            client.Connect();
        }

        public void Disconnect()
        {
            if (client != null)
            {
                Helper.Log("Disconnecting client");
                client.Dispose();
            }
        }

        public void Reconnect()
        {
            client.Reconnect();
        }

        void OnPrivMsg(IRCMessage message)
        {
            Store_Logger.LogString(message.Message);
            Store_Logger.LogString($"connected: {Toolkit.client.client.Connected} - {DateTime.Now.ToShortTimeString()}");

            //if (activeChatWindow != null && !message.Message.StartsWith("!") && message.User != ToolkitSettings.Username)
            //{
            //    if ((_voteActive && !int.TryParse(message.Message[0].ToString(), out int result)) || !_voteActive)
            //    {
            //        activeChatWindow.AddMessage(
            //            message.Message,
            //            message.User,
            //            (message.Parameters.ContainsKey("color")) ? message.Parameters["color"].Remove(0, 1) : Viewer.GetViewerColorCode(message.User)
            //        );
            //    }

            //}
            Store_Logger.LogString("Checking command");

            if (Helper.ModActive) CommandsHandler.CheckCommand(message);

            Store_Logger.LogString("Checking if is vote");

            if (VoteHandler.voteActive && int.TryParse(message.Message[0].ToString(), out int voteKey)) VoteHandler.currentVote.RecordVote(Viewers.GetViewer(message.User).id, voteKey - 1);
        }

        public string[] MessageLog
        {
            get
            {
                if (client == null)
                    return new string[] { };
                return client.MessageLog;
            }
        }

        public void SendMessage(string message, bool v = false)
        {
            Helper.Log($"message: {message} bool: {v}");

            if (client == null)
            {
                Helper.ErrorLog("Client null");
                return;
            }

            if (!client.Connected)
            {
                Helper.ErrorLog("Internal client is disconnected");
            }

            client.SendMessage(message, v);
        }

        public bool Connected
        {
            get
            {
                return client.Connected;
            }
        }

        private void StripUsernameAndChannel()
        {
            ToolkitSettings.Channel = ToolkitSettings.Channel.Replace("https://www.twitch.tv/", "");
            ToolkitSettings.Channel = ToolkitSettings.Channel.Replace("www.twitch.tv/", "");
            ToolkitSettings.Channel = ToolkitSettings.Channel.Replace("twitch.tv/", "");

            ToolkitSettings.Username = ToolkitSettings.Username.Replace("https://www.twitch.tv/", "");
            ToolkitSettings.Username = ToolkitSettings.Username.Replace("www.twitch.tv/", "");
            ToolkitSettings.Username = ToolkitSettings.Username.Replace("twitch.tv/", "");
        }

        public ChatWindow activeChatWindow = null;
        private IRCClient client = null;

        static string _ircHost = "irc.twitch.tv";
        static short _ircPort = 443;
    }

}
