using Sims3.Gameplay.UI;
using Sims3.Gameplay.Socializing;
using System.Collections.Generic;
using MonoPatcherLib;
using Sims3.Gameplay;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.ActorSystems;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Services;
using Sims3.SimIFace;
using Sims3.UI.CAS;
using Sims3.UI.Hud;

[TypePatch(typeof(HudModel))]
public class HudModelPatch
{
    public Dictionary<IMiniSimDescription, IRelationship> CurrentRelationships
		{
			get
			{
                var instance = (HudModel)(this as object);
				Dictionary<IMiniSimDescription, IRelationship> dictionary = new Dictionary<IMiniSimDescription, IRelationship>();
				if (GameStates.IsTravelling)
				{
					return dictionary;
				}
				if (instance.mSavedCurrentSim != null && PlumbBob.SelectedActor != null)
				{
					bool flag = GameUtils.IsInstalled(ProductVersion.EP5);
					foreach (Relationship relationship in instance.mSavedCurrentSim.SocialComponent.Relationships)
					{
						SimDescription otherSimDescription = relationship.GetOtherSimDescription(instance.mSavedCurrentSim.SimDescription);
						if (otherSimDescription != null && otherSimDescription.IsValidDescription && (otherSimDescription.Household == null || !otherSimDescription.Household.IsPreviousTravelerHousehold) && !otherSimDescription.IsBonehilda)
						{
							if (otherSimDescription.CASGenealogy.IsAlive())
							{
								Service createdByService = otherSimDescription.CreatedByService;
								bool flag2 = false;
								if (createdByService != null)
								{
									ServiceType serviceType = createdByService.ServiceType;
									if (serviceType == ServiceType.GrimReaper)
									{
										flag2 = true;
									}
								}
								OccultGenie occultGenie = null;
								if (otherSimDescription.CreatedSim != null && otherSimDescription.CreatedSim.OccultManager != null)
								{
									occultGenie = (otherSimDescription.CreatedSim.OccultManager.GetOccultType(OccultTypes.Genie) as OccultGenie);
								}
								if (flag2 || otherSimDescription.IsTombMummy)
								{
									Sim createdSim = otherSimDescription.CreatedSim;
									if (createdSim != null && createdSim.LotCurrent == instance.mSavedCurrentSim.LotCurrent)
									{
										dictionary.Add(relationship.GetOtherSimDescription(instance.mSavedCurrentSim.SimDescription), new HudModel.UIRelationship(relationship, instance.mSavedCurrentSim));
									}
								}
								else if (!otherSimDescription.IsWildAnimal && (!otherSimDescription.IsGenie || occultGenie != null))
								{
									dictionary.Add(relationship.GetOtherSimDescription(instance.mSavedCurrentSim.SimDescription), new HudModel.UIRelationship(relationship, instance.mSavedCurrentSim));
								}
							}
							else if (otherSimDescription.IsGhost)
							{
								Sim createdSim2 = otherSimDescription.CreatedSim;
								if (createdSim2 != null && (createdSim2.Household != null))
								{
									dictionary.Add(relationship.GetOtherSimDescription(instance.mSavedCurrentSim.SimDescription), new HudModel.UIRelationship(relationship, instance.mSavedCurrentSim));
								}
							}
						}
					}
					MiniSimDescription miniSimDescription = MiniSimDescription.Find(instance.mSavedCurrentSim.SimDescription.SimDescriptionId);
					if (miniSimDescription != null)
					{
						foreach (MiniRelationship miniRelationship in miniSimDescription.MiniRelationships)
						{
							MiniSimDescription miniSimDescription2 = MiniSimDescription.Find(miniRelationship.SimDescriptionId);
							if ((flag || miniSimDescription2 != null) && (flag || !miniSimDescription2.IsPet) && miniSimDescription2 != null && miniSimDescription2.IsContactable && (miniSimDescription2.mDeathStyle == SimDescription.DeathType.None || miniSimDescription2.IsPlayableGhost) && !miniSimDescription2.Instantiated)
							{
								dictionary.Add(miniSimDescription2, new HudModel.UIRelationship(miniSimDescription2, miniRelationship));
							}
						}
					}
					if (!GameUtils.IsOnVacation())
					{
						List<GameObjectRelationship> gameObjectRelationships = instance.mSavedCurrentSim.SimDescription.GameObjectRelationships;
						if (gameObjectRelationships != null)
						{
							bool isFemale = instance.mSavedCurrentSim.IsFemale;
							bool isHuman = instance.mSavedCurrentSim.IsHuman;
							foreach (GameObjectRelationship gameObjectRelationship in gameObjectRelationships)
							{
								dictionary.Add(gameObjectRelationship.GameObjectDescription, new HudModel.UIRelationship(gameObjectRelationship, isFemale, isHuman));
							}
						}
					}
				}
				return dictionary;
			}
		}
}