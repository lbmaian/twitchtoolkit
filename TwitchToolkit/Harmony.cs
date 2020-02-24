﻿using System;
using Harmony;
using RimWorld;
using TwitchToolkit.IRC;
using TwitchToolkit.Storytellers.StorytellerPackWindows;
using TwitchToolkit.Utilities;
using UnityEngine;
using Verse;

namespace TwitchToolkit
{
    [StaticConstructorOnStartup]
    static class HarmonyPatches
    {
        public static TwitchToolkit _mod = LoadedModManager.GetMod<TwitchToolkit>();
        private static readonly Type patchType = typeof(HarmonyPatches);

        static HarmonyPatches()
        {
            SaveHelper.LoadListOfViewers();

            HarmonyInstance harmony = HarmonyInstance.Create("com.github.harmony.rimworld.mod.twitchtoolkit");

            harmony.Patch(
                    original: AccessTools.Method(
                            type: typeof(GameDataSaveLoader),
                            name: "SaveGame"),
                    postfix: new HarmonyMethod(typeof(HarmonyPatches).GetMethod("SaveGame_Postfix"))
                );

            harmony.Patch(
                    original: AccessTools.Method(
                        type: typeof(LetterMaker),
                        name: "MakeLetter",
                        parameters: new[] { typeof(string), typeof(string), typeof(LetterDef) }),
                    prefix: new HarmonyMethod(type: patchType, name: nameof(AddLastPlayerMessagePrefix))
                );

            harmony.Patch(
                original: AccessTools.Method(
                        type: typeof(StorytellerUI),
                        name: "DrawStorytellerSelectionInterface"),
                    postfix: new HarmonyMethod(type: patchType, name: nameof(DrawCustomStorytellerInterface)
                )
            );

            harmony.Patch(
                    original: AccessTools.Method(
                        type: typeof(GameDataSaveLoader),
                        name: "LoadGame",
                        parameters: new[] { typeof(string) }),
                    postfix: new HarmonyMethod(type: patchType, name: nameof(NewTwitchConnection))
                );

            //StringBuilder json = new StringBuilder();

            //foreach (ModMetaData modMeta in ModsConfig.ActiveModsInLoadOrder)
            //{
            //    json.Append(modMeta.GetPublishedFileId() + ", ");
            //}

            //Helper.Log(json.ToString());
        }

        public static void SaveGame_Postfix()
        {
            var mod = LoadedModManager.GetMod<TwitchToolkit>();
            SaveHelper.SaveAllModData();
        }

        public static void AddLastPlayerMessagePrefix(string label, ref string text, LetterDef def)
        {
            if (Helper.playerMessages.Count > 0)
            {
                string msg = Helper.playerMessages[0];
                if (text != "")
                {
                    text += msg;
                }
                Helper.playerMessages.RemoveAt(0);
            }
        }

        public static void DrawCustomStorytellerInterface(Rect rect, ref StorytellerDef chosenStoryteller, ref DifficultyDef difficulty, Listing_Standard infoListing)
        {
            //if (chosenStoryteller.defName != "StorytellerPacks") return;

            Rect storytellerPacksButton = new Rect(140f, rect.height - 50f, 190f, 38f);

            if (Widgets.ButtonText(storytellerPacksButton, "Storyteller Packs"))
            {
                Window_StorytellerPacks window = new Window_StorytellerPacks();
                Find.WindowStack.TryRemove(window.GetType());
                Find.WindowStack.Add(window);
            }
        }

        static void NewTwitchConnection()
        {
            if (Toolkit.client != null)
            {
                Toolkit.client.Disconnect();
            }

            Helper.Log("== Creating new chat client connection ==");

            if (ToolkitSettings.AutoConnect)
                ToolkitIRC.NewInstance();
        }
    }
}
