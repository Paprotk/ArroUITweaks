using MonoPatcherLib;
using Sims3.SimIFace;
using Sims3.UI;
using Sims3.UI.CAS;
using Sims3.UI.Hud;

namespace Arro.RecoverNotification
{
    public class NavigationPatch
    {
        [ReplaceMethod(typeof(Navigation), "OnSimAgeChanged")]
        public void OnSimAgeChanged(ObjectGuid objGuid)
		{
            var instance = (Navigation)(this as object);
			SimInfo currentSimInfo = instance.mHudModel.GetCurrentSimInfo();
			if (currentSimInfo != null && currentSimInfo.mGuid == objGuid)
			{
				ISimDescription simDescription = instance.mHudModel.GetSimDescription();
				if (simDescription != null && (simDescription.ToddlerOrBelow || simDescription.IsPet))
                {
                    instance.mInfoStateButtons[1].Visible = false;//hide career
                    instance.mInfoStateButtons[6].Visible = false;//hide opportunities
                    instance.mInfoStateButtons[2].Position = new Vector2(102f, -29f);//move skills into career position
                    instance.mInfoStateButtons[5].Position = new Vector2(145f, -29f);//move inventory into skills position
                    instance.mInfoStateButtons[3].Position = new Vector2(188f, -29f);//move rewards into inventory position
                    instance.mInfoStateButtons[7].Position = new Vector2(231, -29f);//move motives into opportunities position
                    
					if (HudController.Instance.IsInfoStateActive(InfoState.Opportunities))
					{
						instance.mInfoStateButtons[6].Selected = false;
						HudController.SetInfoState(InfoState.Simology);
					}
					if (simDescription.IsPet)
					{
						instance.mInfoStateButtons[1].Enabled = false;
						instance.mInfoStateButtons[1].TooltipText = Responder.Instance.LocalizationModel.LocalizeString("Ui/Tooltip/HUD/Navigation:PetCareerDisallowed", new object[0]);
						if (HudController.Instance.IsInfoStateActive(InfoState.Career))
						{
							instance.mInfoStateButtons[1].Selected = false;
							HudController.SetInfoState(InfoState.Simology);
						}
						instance.mInfoStateButtons[0].TooltipText = instance.mPetologyTooltipText;
						instance.mInfoStateButtons[6].TooltipText = instance.mOpportunityPetTooltipText;
					}
					else
					{
						instance.mInfoStateButtons[1].Enabled = true;
						instance.mInfoStateButtons[1].TooltipText = instance.GetCareerTooltipString();
						instance.mInfoStateButtons[0].TooltipText = instance.mSimologyTooltipText;
						instance.mInfoStateButtons[6].TooltipText = instance.mOpportunitySimTooltipText;
					}
					instance.mInfoStateButtons[2].Enabled = true;
					instance.mInfoStateButtons[2].TooltipText = instance.mSkillsTooltipText;
					return;
				}
				if (simDescription.IsEP11Bot)
				{
					instance.mInfoStateButtons[6].Enabled = instance.mHudModel.RobotOpportunitiesEnabled;
					if (!instance.mInfoStateButtons[6].Enabled && HudController.Instance.IsInfoStateActive(InfoState.Opportunities))
					{
						instance.mInfoStateButtons[6].Selected = false;
						HudController.SetInfoState(InfoState.Simology);
					}
					instance.mInfoStateButtons[6].TooltipText = !instance.mInfoStateButtons[6].Enabled ? instance.mOpportunityRobotLockedTooltipText : instance.mOpportunitySimTooltipText;
					instance.mInfoStateButtons[1].Enabled = instance.mHudModel.RobotCareerEnabled;
					instance.mInfoStateButtons[1].TooltipText = Responder.Instance.LocalizationModel.LocalizeString("Ui/Tooltip/HUD/Navigation:Career", new object[0]);
					if (!instance.mInfoStateButtons[1].Enabled && HudController.Instance.IsInfoStateActive(InfoState.Career))
					{
						instance.mInfoStateButtons[1].Selected = false;
						HudController.SetInfoState(InfoState.Simology);
					}
					instance.mInfoStateButtons[2].Enabled = instance.mHudModel.RobotSkillsEnabled;
					if (!instance.mInfoStateButtons[2].Enabled && HudController.Instance.IsInfoStateActive(InfoState.Skills))
					{
						instance.mInfoStateButtons[2].Selected = false;
						HudController.SetInfoState(InfoState.Simology);
					}
					if (instance.mInfoStateButtons[2].Enabled)
					{
						instance.mInfoStateButtons[2].TooltipText = instance.mSkillsTooltipText;
						return;
					}
					instance.mInfoStateButtons[2].TooltipText = instance.mSkillsRobotLockedTooltipText;
                }
				else
				{
                    instance.mInfoStateButtons[1].Visible = true;//show career
                    instance.mInfoStateButtons[6].Visible = true;//show opportunities
                    instance.mInfoStateButtons[2].Position = new Vector2(145f, -29f);//move skills into original position
                    instance.mInfoStateButtons[5].Position = new Vector2(188f, -29f);//move inventory into original position
                    instance.mInfoStateButtons[3].Position = new Vector2(274f, -29f);//move rewards into original position
                    instance.mInfoStateButtons[7].Position = new Vector2(317f, -29f);//move motives into original position
					instance.mInfoStateButtons[6].Enabled = true;
					instance.mInfoStateButtons[6].TooltipText = instance.mOpportunitySimTooltipText;
					instance.mInfoStateButtons[1].Enabled = true;
					instance.mInfoStateButtons[1].TooltipText = instance.GetCareerTooltipString();
					instance.mInfoStateButtons[0].TooltipText = instance.mSimologyTooltipText;
					instance.mInfoStateButtons[2].Enabled = true;
					instance.mInfoStateButtons[2].TooltipText = instance.mSkillsTooltipText;
				}
			}
		}
    }
}