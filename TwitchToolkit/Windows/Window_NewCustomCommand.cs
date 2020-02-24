﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace TwitchToolkit.Windows
{
    public class Window_NewCustomCommand : Window
    {
        public Window_NewCustomCommand()
        {
            this.doCloseButton = true;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Listing_Standard listing = new Listing_Standard();
            listing.Begin(inRect);

            defName = listing.TextEntryLabeled("Command Name", defName);

            if (listing.ButtonTextLabeled("New Command", "Create"))
            {
                TrySubmitNewCommand();
            }

            listing.Label(output);

            if (outputFrames > 0)
            {
                outputFrames--;
            }
            else
            {
                output = "";
            }

            listing.End();
        }

        void TrySubmitNewCommand()
        {
            if (defName == "")
            {
                NewOutputMessage("Name must be longer than 0 chars.");

                return;
            }

            string sanitizedDefName = string.Join("", defName.Split(' ')).ToLower();

            if (DefDatabase<Command>.AllDefs.Any(s => s.defName.ToLower() == sanitizedDefName))
            {
                NewOutputMessage("Command name is already taken");
                return;
            }

            Command newCommand = new Command();
            newCommand.defName = sanitizedDefName.CapitalizeFirst();
            newCommand.isCustomMessage = true;
            newCommand.label = sanitizedDefName;
            DefDatabase<Command>.Add(newCommand);

            if (ToolkitSettings.CustomCommandDefs == null)
            {
                ToolkitSettings.CustomCommandDefs = new List<string>();
            }

            ToolkitSettings.CustomCommandDefs.Add(newCommand.defName);

            Toolkit.Mod.WriteSettings();

            Window_CommandEditor window = new Window_CommandEditor(newCommand);
            Find.WindowStack.TryRemove(window.GetType());
            Find.WindowStack.Add(window);
            Close();
        }

        void NewOutputMessage(string message)
        {
            output = message;
            outputFrames = 240;
        }

        string defName = "";

        string output = "";

        int outputFrames = 0;
    }
}
