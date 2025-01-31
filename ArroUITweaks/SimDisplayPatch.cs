using MonoPatcherLib;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.Hud;

namespace Arro.UITweaks
{
    public class SimDisplayPatch
    {
        [ReplaceMethod(typeof(SimDisplay), "OnLunarCycleUpdate")]
        public void OnLunarCycleUpdate(uint moonPhase)
        {
            var instance = (SimDisplay)(this as object);
            if (moonPhase != instance.mLastMoonPhase)
            {
                string image = "MoonPhase" + moonPhase;
                instance.mLunarCycleIcon.SetImage(image);
                
                // Set the tooltip callback
                instance.mLunarCycleIcon.CreateTooltipCallbackFunction = CreateLunarTooltip;
                
                instance.mLunarCycleChangeWindow.Visible = true;
                Simulator.AddObject(new OneShotFunctionTask(instance.OnMoonChangeInFinish, StopWatch.TickStyles.Seconds, 0.4f));
                instance.mLastMoonPhase = moonPhase;
            }
        }

        public Tooltip CreateLunarTooltip(Vector2 mousePosition, WindowBase parent, ref Vector2 tooltipPosition)
        {
            return new LunarTooltip();
        }
    }
}