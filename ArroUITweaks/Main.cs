using System;
using MonoPatcherLib;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
namespace Arro.UITweaks
{
    [Plugin]
    public class Main
    {
        public Main()
        {
            World.sOnStartupAppEventHandler = (EventHandler)Delegate.Combine(World.sOnStartupAppEventHandler,
                new EventHandler(Main.OnStartupApp));
            World.sOnWorldLoadFinishedEventHandler = (EventHandler)Delegate.Combine(World.sOnWorldLoadFinishedEventHandler,
                new EventHandler(Main.OnWorldLoadFinished));
        }
        
        private static void OnStartupApp(object sender, EventArgs e)
        {
            Commands.sGameCommands.Register("RecoverNotification", "Recovers the last deleted notification.",
                Commands.CommandType.General, (RecoverNotification.RecoverLastDeletedNotification));
            Commands.sGameCommands.Register("SendStrayToActiveLot", "Sends a stray pet to the active lot.",
                Commands.CommandType.Cheat, (StrayTooltipPatch.SendStrayToActiveLot));
            
        }
        public static void OnWorldLoadFinished(object sender, EventArgs e)
        {
        }
        public static class TinyUIFixForTS3Integration
        {
            public delegate float FloatGetter();

            public static FloatGetter getUIScale = () => 1f;
        }
    }
}