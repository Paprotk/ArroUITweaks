using System;
using System.Collections.Generic;
using System.Reflection;
using MonoPatcherLib;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using Sims3.UI;
using OneShotFunctionTask = Sims3.Gameplay.OneShotFunctionTask;

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
            Commands.sGameCommands.Register("FocusOnVenue", "Focuson venuelol.",
                Commands.CommandType.General, (Main.VenueCheck));
            Commands.sGameCommands.Register("SendStrayToActiveLot", "Sends a stray pet to the active lot.",
                Commands.CommandType.Cheat, (StrayTooltipPatch.SendStrayToActiveLot));
            CheckForMods();
            
        }

        private static int VenueCheck(object[] parameters)
        {
            Simulator.AddObject(new OneShotFunctionTask(() =>
                        {
                            VenueCollector.Initialize();
                            VenueCollector.ShowSearchDialog();
                        }, StopWatch.TickStyles.Seconds, 0.1f));
           
           return 1;
        }

        public static void OnWorldLoadFinished(object sender, EventArgs e)
        {
            if (selectorAssembly == null) return;
            Simulator.AddObject(new OneShotFunctionTask(() =>
            {
                Sims3.Gameplay.Tasks.SelectObjectTask.Shutdown(); 
                Sims3.Gameplay.Tasks.SelectObjectTask.sSelectObjectTask = new Sims3.Gameplay.Tasks.SelectObjectTask();
                Simulator.AddObject(Sims3.Gameplay.Tasks.SelectObjectTask.sSelectObjectTask);
                
            }, StopWatch.TickStyles.Seconds, 20f));
            
        }
        private static void CheckForMods()
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            Assembly[] assems = currentDomain.GetAssemblies();
            foreach (Assembly assembly in assems)
            {
                if (assembly.GetName().Name == "NRaasSelector")
                {
                    selectorAssembly = assembly;
                    break;
                }
            }
        }
        public static Assembly selectorAssembly;

        public static class TinyUIFixForTS3Integration
        {
            public delegate float FloatGetter();

            public static FloatGetter getUIScale = () => 1f;
        }
    }
}