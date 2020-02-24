﻿using System;
using RimWorld;
using Verse;

namespace TwitchToolkit.Storytellers
{
    public class StorytellerPack : Def
    {
        public StorytellerComp StorytellerComp
        {
            get
            {
                try
                {
                    if (storytellerComp == null)
                    {
                        storytellerCompProps = (StorytellerCompProperties)Activator.CreateInstance(this.storytellerCompPropertiesType);
                        storytellerCompProps.ResolveReferences(Current.Game.storyteller.def);
                        storytellerComp = (StorytellerComp)Activator.CreateInstance(storytellerCompProps.compClass);
                        storytellerComp.props = storytellerCompProps;
                    }
                }
                catch (Exception e)
                {
                    Helper.ErrorLog(e.Message);
                }

                Helper.Log("storyteller comp loaded " + storytellerComp.GetType());

                return storytellerComp;
            }
        }

        public bool enabled = false;

        public int minDaysBetweenEvents = 1;

        public Type storytellerCompPropertiesType = typeof(StorytellerCompProperties);

        public StorytellerComp storytellerComp = null;

        public StorytellerCompProperties storytellerCompProps = null;
    }
}
