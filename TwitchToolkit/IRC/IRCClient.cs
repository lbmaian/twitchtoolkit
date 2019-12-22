using System;
using System.Net.Sockets;
using System.Net.Security;
using System.Text;
using System.Collections.Generic;
using System.Threading;
using Verse;
using TwitchToolkit.Store;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace TwitchToolkit.IRC
{
    public delegate void OnPrivMsg(IRCMessage message);
    

    public class IRCClient : IDisposable
    {
        private const string IRC_ENDOFMOTD_CODE = "376";

        public event OnPrivMsg OnPrivMsg;
        public event OnPrivMsg OnUnkwnMsg;

        string _host;
        short _port;
        string _user;
        string _pass;
        string _channel;

        bool _ping;
        byte[] _buffer;
        IRCParser _parser;
        Socket _socket;
        bool _socketReady;
        NetworkStream _networkStream;
        SslStream _sslStream;
        object sslLock = new object();

        Queue<string> _messageQueue;
        int _messageInterval = 2;
        Thread _messageThread;
        Thread _pingThread;
        Thread _readThread;

        ConcurrentCircularBuffer<string> _ircMessages = new ConcurrentCircularBuffer<string>(10);

        public IRCClient(string host, short port, string user, string pass, string channel)
        {
            _socketReady = false;
            _host = host;
            _port = port;
            _user = user.ToLower();

            if (!pass.StartsWith("oauth:", StringComparison.InvariantCultureIgnoreCase))
                _pass = "oauth:" + pass;
            else
                _pass = pass;

            _channel = channel.ToLower();
            _messageQueue = new Queue<string>();
            _buffer = new byte[8192];
            _parser = new IRCParser();
            _messageThread = new Thread(MessageThread);
            _messageThread.Start();
            _pingThread = new Thread(PingThread);
            _pingThread.Start();
            _readThread = new Thread(ReadThread);
            _readThread.Start();
        }

        public void Dispose()
        {
            Disconnect();
            try
            {
                _messageThread.Abort();
                _pingThread.Abort();
                _readThread.Abort();
            }
            catch (Exception e)
            {
                Helper.ErrorLog($"Couldnt abort threads? {e.ToString()}");
            }
        }

        public string[] MessageLog
        {
            get
            {
                return _ircMessages.Read();
            }
        }

        private void ParseMessage(IRCMessage message)
        {
            Ticker.LastIRCPong = DateTime.Now.ToFileTime();

            _ircMessages.Put(message.Cmd + " " + message.Args);

            switch (message.Cmd)
            {
                case "USERSTATE":
                    if (message.Parameters.ContainsKey("mod") && message.Parameters["mod"] == "1")
                    {
                        _messageInterval = 1;
                    }
                    else
                    {
                        _messageInterval = 2;
                    }
                    break;
                case "PING":
                    Send("PONG\n");
                    break;
                case IRC_ENDOFMOTD_CODE:
                    if (ToolkitSettings.UseSeparateChatRoom && ToolkitSettings.ChatroomUUID != "" && ToolkitSettings.ChannelID != "")
                    {
                        Send(
                            "CAP REQ :twitch.tv/membership\n" +
                            "CAP REQ :twitch.tv/tags\n" +
                            "CAP REQ :twitch.tv/commands\n" +
                            "JOIN #" + _channel + "\n" +
                            "JOIN #chatrooms:" + ToolkitSettings.ChannelID + ":" + ToolkitSettings.ChatroomUUID + "\n"
                            );
                    }
                    else
                    {
                        Send(
                            "CAP REQ :twitch.tv/membership\n" +
                            "CAP REQ :twitch.tv/tags\n" +
                            "CAP REQ :twitch.tv/commands\n" +
                            "JOIN #" + _channel + "\n"
                            );
                    }

                    _socketReady = true;
                    break;
                case "PRIVMSG":
                    if (!ToolkitSettings.WhisperCmdsOnly)
                    {
                        Helper.Log($"Received private message {message.Message}");
                        OnPrivMsg?.Invoke(message);
                    }
                    break;
                case "WHISPER":
                    if (ToolkitSettings.WhisperCmdsAllowed)
                    {
                        Helper.Log($"Received private whisper {message.Message}");
                        message.Whisper = true;
                        OnPrivMsg?.Invoke(message);
                    }
                    break;
                case "PONG":
                    break;
                default:
                    OnUnkwnMsg?.Invoke(message);
                    break;
            }
        }

        private void ReadThread()
        {
            while (true)
            {
                try
                {
                    Thread.Sleep(10);
                    lock (sslLock)
                    {
                        if (_sslStream == null)
                        {
                            continue;
                        }

                        _sslStream.BeginRead(_buffer, 0, _buffer.Length, new AsyncCallback(OnRead), null);
                    }
                }
                catch (IOException e)
                {
                    Helper.ErrorLog($"Socket exception: {e.ToString()}");
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Helper.ErrorLog($"Prevented read thread death! {e.Message}");
                }
            }
        }

        private void OnRead(IAsyncResult asyncResult)
        {
            try
            {
                int read = _sslStream.EndRead(asyncResult);
                _parser.Parse(_buffer, read, ParseMessage);
            }
            catch (Exception e)
            {
                Helper.ErrorLog($"Failed to read and parse message! {e.Message}");
            }
        }

        AutoResetEvent _messageHandle = new AutoResetEvent(false);
        private void MessageThread()
        {
            while (true)
            {
                try
                {
                    _messageHandle.WaitOne(_messageInterval * 1000);

                    if (_socketReady == false || _messageQueue.Count == 0)
                    {
                        continue;
                    }

                    while (_messageQueue.Count > 0)
                    {
                        string message = _messageQueue.Peek();
                        bool success = Send(message);
                        if (!success)
                        {
                            Helper.ErrorLog($"Failed to send message: {message}");
                            break;
                        }
                        _messageQueue.Dequeue();
                    }
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Helper.ErrorLog($"Prevented message thread death! {e.Message}");
                }
            }
        }

        private void PingThread()
        {
            while (true)
            {
                try
                {
                    if (_ping && _socketReady == true && _socket != null)
                    {
                        Send("PING\n");
                    }

                    Thread.Sleep(60000);
                }
                catch (ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Helper.ErrorLog($"Prevented ping thread death! {e.Message}");
                }
            }
        }

        public void Connect()
        {
            if (_socket != null)
            {
                Helper.ErrorLog("Can't connect, socket is not empty");
                Disconnect();
            }
            Helper.Log("Preparing socket");
            _ping = true;
            _socketReady = false;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.BeginConnect(_host, _port, new AsyncCallback(OnConnect), null);

            Helper.Log("Finished socket connection");
        }

        private void OnConnect(IAsyncResult asyncResult)
        {
            if (_socket != null)
            {
                _socket.EndConnect(asyncResult);
            }

            if (_socket == null || _socket.Connected == false)
            {
                Helper.ErrorLog("Socket did not connect! Calling connect again...");
                Connect();
            }
            else
            {
                _networkStream = new NetworkStream(_socket);
                lock (sslLock)
                {
                    _sslStream = new SslStream(_networkStream, false, (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyError) => { return true; });
                    _sslStream.AuthenticateAsClient(_host);
                }
                Send("PASS " + _pass + "\nNICK " + _user + "\n");
            }
        }

        public bool Connected
        {
            get
            {
                if (_socket == null)
                {
                    return false;
                }

                return _socket.Connected;
            }
        }

        public void Disconnect()
        {
            if (!Connected) { return; }

            _ircMessages.Clear();

            _ping = false;
            _socketReady = false;

            if (_socket != null)
            {
                try
                {
                    Helper.Log("closing socket");
                    _socket.Close();
                    Helper.Log("socket should be closed");
                }
                catch (Exception e)
                {
                    Helper.ErrorLog($"Failed to close IRC socket! {e.Message}");
                }
            }
        }

        public void Reconnect()
        {
            ToolkitIRC.NewInstance();
        }

        public void SendMessage(string message, bool botchannel = false)
        {
            if (ToolkitSettings.UseSeparateChatRoom && ToolkitSettings.ChatroomUUID != "" && ToolkitSettings.ChannelID != "" && botchannel)
            {
                _messageQueue.Enqueue("PRIVMSG #chatrooms:" + ToolkitSettings.ChannelID + ":" + ToolkitSettings.ChatroomUUID + " :" + message + "\n");
            }
            else
            {
                _messageQueue.Enqueue("PRIVMSG #" + _channel + " :" + message + "\n");
            }
            _messageHandle.Set();
        }

        bool Send(string message)
        {
            try
            {
                Encoding encoding = Helper.LanguageEncoding();
                byte[] _data = Encoding.UTF8.GetBytes(message);
                lock (sslLock)
                {
                    _sslStream.BeginWrite(_data, 0, _data.Length, new AsyncCallback(OnSend), null);
                }
            }
            catch (Exception e)
            {
                Helper.ErrorLog($"IRC send '{message}' failed! {e.Message}. Calling Connect....");
                Connect();
                return false;
            }

            return true;
        }

        private void OnSend(IAsyncResult asyncResult)
        {
            _sslStream.EndWrite(asyncResult);
        }


    }
}
