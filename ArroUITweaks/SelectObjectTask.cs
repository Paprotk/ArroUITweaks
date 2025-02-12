using System;
using System.Reflection;
using MonoPatcherLib;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Interfaces;
using Sims3.Gameplay.Objects.Vehicles;
using Sims3.SimIFace;
using Sims3.UI;

namespace Arro.UITweaks
{
    public class SelectObjectTask : UiMouseClickProcessorTask
    {
        // Reflection cache
        private static readonly Type sNraasSelectTaskType;
        private static readonly MethodInfo sOnSelectSimMethod;
        private static readonly MethodInfo sOnSelectVehicleMethod;

        static SelectObjectTask()
        {
            try
            {
                if (Main.selectorAssembly == null) return;
                
                sNraasSelectTaskType = Main.selectorAssembly.GetType("NRaas.SelectorSpace.Tasks.SelectTask");
                if (sNraasSelectTaskType == null) return;
                
                sOnSelectSimMethod = sNraasSelectTaskType.GetMethod("OnSelect",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(Sim), typeof(UIMouseEventArgs) },
                    null);

                sOnSelectVehicleMethod = sNraasSelectTaskType.GetMethod("OnSelect",
                    BindingFlags.NonPublic | BindingFlags.Instance,
                    null,
                    new[] { typeof(Vehicle), typeof(UIMouseEventArgs) },
                    null);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "InteractionMouseUp");
            }
        }

        [ReplaceMethod(typeof(Sims3.Gameplay.Tasks.SelectObjectTask), "ProcessClick")]
        public override void ProcessClick(ScenePickArgs eventArgs)
        {
            var instance = (Sims3.Gameplay.Tasks.SelectObjectTask)(this as object);
            
            if (eventArgs.mObjectType == ScenePickObjectType.None)
                return;

            if (instance.WasMouseDragged(eventArgs))
                return;

            try
            {
                HandleRightClickSelection(eventArgs, instance);
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "InteractionMouseUp");
            }

            HandleDefaultSelection(eventArgs);
        }

        private void HandleRightClickSelection(ScenePickArgs eventArgs, Sims3.Gameplay.Tasks.SelectObjectTask instance)
        {
            if (eventArgs.mMouseEvent.MouseKey != MouseKeys.kMouseRight) return;
            if ((eventArgs.mMouseEvent.Modifiers & Modifiers.kModifierMaskControl) != Modifiers.kModifierMaskNone) return;

            var objectGuid = new ObjectGuid(eventArgs.mObjectId);
            var proxy = Simulator.GetProxy(objectGuid);
            var target = proxy?.Target;

            if (target == null) return;

            switch (target)
            {
                case Vehicle vehicle when vehicle.Position != Vector3.OutOfWorld:
                    HandleVehicleSelection(vehicle, eventArgs);
                    break;
                
                case Sim sim when sim.Position != Vector3.OutOfWorld:
                    HandleSimSelection(sim, eventArgs, instance);
                    break;
            }
        }

        private void HandleVehicleSelection(Vehicle vehicle, ScenePickArgs eventArgs)
        {
            var driver = vehicle.Driver;
            Camera.FocusOnGivenPosition(driver.Position, 1f);
            CameraController.EnableObjectFollow(vehicle.ObjectId.Value, Vector3.Zero);
            
            if (sNraasSelectTaskType == null || sOnSelectVehicleMethod == null) return;

            try
            {
                var nrassInstance = Activator.CreateInstance(sNraasSelectTaskType, nonPublic: true);
                sOnSelectVehicleMethod.Invoke(nrassInstance, new object[] { vehicle, eventArgs.mMouseEvent });
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "InteractionMouseUp");
            }
        }

        private void HandleSimSelection(Sim sim, ScenePickArgs eventArgs, Sims3.Gameplay.Tasks.SelectObjectTask instance)
        {
            Camera.FocusOnGivenPosition(sim.Position, 1f);
            CameraController.EnableObjectFollow(sim.ObjectId.Value, Vector3.Zero);

            if (sNraasSelectTaskType == null || sOnSelectSimMethod == null) return;

            try
            {
                var nrassInstance = Activator.CreateInstance(sNraasSelectTaskType, nonPublic: true);
                sOnSelectSimMethod.Invoke(nrassInstance, new object[] { sim, eventArgs.mMouseEvent });
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "InteractionMouseUp");
            }
        }

        private void HandleDefaultSelection(ScenePickArgs eventArgs)
        {
            var handled = false;
            
            if (eventArgs.mObjectType == ScenePickObjectType.Object || 
                eventArgs.mObjectType == ScenePickObjectType.Sim)
            {
                var objectId = new ObjectGuid(eventArgs.mObjectId);
                var proxy = Simulator.GetProxy(objectId);
                
                if (proxy?.Target is IObjectUI objectUI)
                {
                    handled = objectUI.OnSelect(eventArgs.mMouseEvent);
                }
            }

            if (!handled)
            {
                CameraController.RequestLerpToTarget(eventArgs.mDisplayPos, 1.5f, false);
            }
        }
    }
}