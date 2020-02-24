using Verse;

namespace TwitchToolkit.IRC
{
    public abstract class TwitchInterfaceBase : GameComponent
    {
        public abstract void ParseCommand(IRCMessage msg);
    }
}
