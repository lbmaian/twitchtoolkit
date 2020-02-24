﻿using System;
using System.Text;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;
using TwitchToolkit.IRC;
using Verse;

namespace TwitchToolkit
{
    public class Command : Def
    {

        public void RunCommand(IRCMessage message)
        {
            if (command == null)
            {
                throw new Exception("Command is null");
            }

            CommandDriver driver = (CommandDriver)Activator.CreateInstance(commandDriver);
            driver.command = this;
            driver.RunCommand(message);
        }

        public string Label
        {
            get
            {
                if (label != null && label != "")
                {
                    return label;
                }

                return defName;
            }
        }

        public string command = null;

        public bool enabled = true;

        public bool shouldBeInSeparateRoom = false;

        public Type commandDriver = typeof(CommandDriver);

        public bool requiresMod = false;

        public bool requiresAdmin = false;

        public string outputMessage = "";

        public bool isCustomMessage = false;
    }

    public class CommandDriver
    {
        public Command command = null;

        public virtual void RunCommand(IRCMessage message)
        {
            Helper.Log("filtering command");

            string output = FilterTags(message, command.outputMessage);

            Helper.Log("command filtered");

            Toolkit.client.SendMessage(output);
        }

        public string FilterTags(IRCMessage message, string input)
        {
            Helper.Log("starting filter");

            Viewer viewer = Viewers.GetViewer(message.User);

            StringBuilder output = new StringBuilder(input);
            output.Replace("{username}", viewer.username);
            output.Replace("{balance}", viewer.GetViewerCoins().ToString());
            output.Replace("{karma}", viewer.GetViewerKarma().ToString());
            output.Replace("{purchaselist}", ToolkitSettings.CustomPricingSheetLink);
            output.Replace("{coin-reward}", ToolkitSettings.CoinAmount.ToString());

            output.Replace("\n", "");

            Helper.Log("starting regex");

            Regex regex = new Regex(@"\[(.*?)\]");

            MatchCollection matches = regex.Matches(output.ToString());

            foreach (Match match in matches)
            {
                Helper.Log("found match " + match.Value);
                string code = match.Value;
                code = code.Replace("[", "");
                code = code.Replace("]", "");

                //Regex doubleReg = new Regex("double\\<(.*?)\\>");

                //foreach (Match innerMatch in doubleReg.Matches(match.Value.ToString()))
                //{
                //    Helper.Log("found match " + innerMatch.Value);

                //    string innerCode = innerMatch.Value;
                //    innerCode = innerCode.Replace("double<", "");
                //    innerCode = innerCode.Replace(">", "");

                //    Helper.Log("executing double " + innerCode);

                //    output.Replace(innerMatch.Value, MoonSharpDouble(code).ToString());
                //}

                // Helper.Log("finished inner code");

                output.Replace(match.Value, MoonSharpString(code));
            }

            return output.ToString();
        }

        public string MoonSharpString(string function)
        {
            string script = @function;

            DynValue res = Script.RunString(script);
            return res.String;
        }

        public double MoonSharpDouble(string function)
        {
            string script = @function;

            DynValue res = Script.RunString(script);
            return res.Number;
        }
    }

    //public static class LUATools
    //{
    //    public static double Double(string code)
    //    {
    //        try
    //        {
    //            string script = code;

    //            DynValue res = Script.RunString(script);

    //            return res.Number;
    //        }
    //        catch (Exception e)
    //        {
    //            Log.Error(e.Message);

    //            return 0d;
    //        }
    //    }
    //}

    //public class LUAActivator
    //{
    //    public static object CreateInstance(string type)
    //    {
    //        Type classType = Type.GetType(type);
    //        object classObject = (object)Activator.CreateInstance(classType);

    //        return classObject;
    //    }
    //}
}
