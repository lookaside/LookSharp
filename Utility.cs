using System;
using System.Collections.Generic;
using EloBuddy;
using EloBuddy.SDK;
using EloBuddy.SDK.Events;
using EloBuddy.SDK.Menu;
using LookSharp.Utilities;

namespace LookSharp
{
    abstract class Utility
    {
        protected static AIHeroClient myHero { get { return Player.Instance; } }
        protected static Menu UtilityMenu;

        public static void Init()
        {
            UtilityMenu = MainMenu.AddMenu("Utility", "Utility");
            UtilityMenu.AddGroupLabel("Information");
            UtilityMenu.AddLabel("Made by Lookaside");
            UtilityMenu.AddLabel("Please upvote in forums!");

            new Structure();
        }
    }
}
