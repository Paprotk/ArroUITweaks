using System;
using System.Collections.Generic;
using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.Interactions;
using Sims3.UI;
using Sims3.UI.Hud;
using Sims3.SimIFace;
using InteractionQueue = Sims3.Gameplay.ActorSystems.InteractionQueue;

namespace Arro.UITweaks
{
    public class QueueMove
    {
        [ReplaceMethod(typeof(InteractionQueueItem), "InteractionMouseUp")]
        public void InteractionMouseUp(WindowBase sender, UIMouseEventArgs eventArgs)
        {
            var instance = (InteractionQueueItem)(this as object);
            if (eventArgs.MouseKey == MouseKeys.kMouseRight)
            {
                if (instance.InteractionId != 0UL && Responder.Instance.HudModel.IsInteractionCancellableByPlayer(instance.InteractionId))
                {
                    Audio.StartSound("ui_primary_button");
                    Sim activeSim = Sim.ActiveActor;
                    if (activeSim != null)
                    {
                        InteractionQueue interactionQueue = activeSim.InteractionQueue;
                        if (interactionQueue != null)
                        {
                            int currentIndex = -1;
                            for (int i = 0; i < interactionQueue.Count; i++)
                            {
                                if (interactionQueue.mInteractionList[i].Id == instance.InteractionId)
                                {
                                    currentIndex = i;
                                    break;
                                }
                            }
                            
                            if (currentIndex != -1 && currentIndex < interactionQueue.Count - 1 && currentIndex > 0)
                            {
                                try
                                {
                                    InteractionInstance interactionToMove = interactionQueue.mInteractionList[currentIndex];
                                    interactionQueue.RemoveInteraction(currentIndex, false);
                                    // Insert it at the new index (currentIndex + 1)
                                    interactionQueue.InsertInteraction(interactionToMove, currentIndex + 1);
                                }
                                catch (Exception ex)
                                {
                                    ExceptionHandler.HandleException(ex, "InteractionMouseUp");
                                }
                                
                            }
                        }
                    }
                }
            }
            if (eventArgs.MouseKey == MouseKeys.kMouseLeft)
            {
                if (instance.InteractionId != 0UL)
                {
                    if (Responder.Instance.HudModel.IsInteractionCancellableByPlayer(instance.InteractionId))
                    {
                        if (!instance.mCancelImageWin.Visible)
                        {
                            Audio.StartSound("ui_queue_delete");
                        }
                        List<IInteractionInstance> interactionList = Responder.Instance.HudModel.GetInteractionList();
                        foreach (IInteractionInstance interactionInstance in interactionList)
                        {
                            if (interactionInstance.Id == instance.InteractionId)
                            {
                                if (interactionInstance.Autonomous)
                                {
                                    Responder.Instance.HudModel.OnAutonomousInteractionCancelledFromUI();
                                }
                                break;
                            }
                        }
                        Responder.Instance.HudModel.CancelInteraction(instance.InteractionId);
                    }
                }
                else
                {
                    if (instance.mInteractionStyle == InteractionQueueItem.InteractionStyle.Relationship)
                    {
                        Responder.Instance.HudModel.CancelSocializing();
                        return;
                    }
                    Responder.Instance.HudModel.CancelPosture();
                }
            }
        }
    }
}