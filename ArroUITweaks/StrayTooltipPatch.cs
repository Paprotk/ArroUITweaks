using System.Collections.Generic;
using MonoPatcherLib;
using Sims3.Gameplay.Actors;
using Sims3.Gameplay.CAS;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.PetSystems;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;
using Responder = Sims3.Gameplay.UI.Responder;

namespace Arro.RecoverNotification
{
    public class StrayTooltipPatch : Sim
    {
        [ReplaceMethod(typeof(GoToVirtualHome.GoToVirtualHomeInternal), "GreyedOutTooltipText")]
        public string GreyedOutTooltipText(Sim simA, Sim simB)
        {
            if (WildHorses.IsWildHorse(simB.mSimDescription))
            {
                string wildHorse = Responder.Instance.LocalizationModel.LocalizeString("Gameplay/Actors/Sim/WildHorse:Tooltip", new object[0]);
                string goingBackToItsPlace = GetGoingBackToItsPlace();
                return $"{wildHorse}{goingBackToItsPlace}";
            }
            string entryKey = "Gameplay/Socializing/SocialInteractionA:CannotSocializeWhileGoingHome";
            return Localization.LocalizeString(simB.IsFemale, entryKey, new object[]
            {
                simB.SimDescription
            });
        }
        [ReplaceMethod(typeof(GoToVirtualHome), "GreyedOutTooltipText")]
        public string GreyedOutTooltipText1(Sim simA, Sim simB)
        {
            if (simB.mSimDescription.IsStray)
            {
                string stray = GetStrayInfo(simB);
                string goingBackToItsPlace = GetGoingBackToItsPlace();
                return $"{stray}{goingBackToItsPlace}";
            }
            string entryKey = "Gameplay/Socializing/SocialInteractionA:CannotSocializeWhileGoingHome";
            return Localization.LocalizeString(simB.IsFemale, entryKey, new object[]
            {
                simB.SimDescription
            });
        }
        public string GetStrayInfo(Sim simB)
        {
            string text2 = "Gameplay/Actors/Sim/StrayPet:Tooltip";
            string result;
            if (Localization.GetLocalizeSpeciesString(ref text2, out result, new object[]
                {
                    simB.mSimDescription
                }))
            {
                return result;
            }
            return "null";
        }

        public string GetGoingBackToItsPlace()
        {
            var locale = StringTable.GetLocale();
            switch (locale)
            {
                case "zh-cn": return " 回到它待的地方"; // Back to its place
                case "zh-tw": return " 回到它待的地方";  
                case "cs-cz": return " se vrací na své místo"; // Back to its place
                case "da-dk": return " går tilbage til sit sted"; // Back to its place
                case "nl-nl": return " gaat terug naar zijn plek"; // Back to its place
                case "en-gb": return " is going back to its place"; 
                case "en-us": return " is going back to its place";
                case "fi-fi": return " menee takaisin sinne, missä se on"; // Back to where it is
                case "fr-fr": return " retourne là où il est habituellement"; // Returns to where it usually is
                case "de-de": return " geht zurück an seinen Platz"; // Goes back to its place
                case "el-gr": return " επιστρέφει εκεί που ανήκει"; // Returns to where it belongs
                case "hu-hu": return " visszamegy oda, ahol van"; // Back to where it is
                case "it-it": return " torna al suo posto"; // Goes back to its place
                case "ja-jp": return " 自分のいる場所に戻る"; // Returns to where it stays
                case "ko-kr": return " 자신이 있는 곳으로 돌아간다"; // Goes back to where it stays
                case "no-no": return " går tilbake til sitt sted"; // Goes back to its place
                case "pl-pl": return " wraca do siebie"; // Returns to its place
                case "pt-br": return " está voltando para o seu lugar"; // Going back to its place
                case "pt-pt": return " está a voltar para o seu lugar"; // Going back to its place
                case "ru-ru": return " возвращается туда, где ему место"; // Goes back to where it belongs
                case "es-es": return " vuelve a su lugar habitual"; // Returns to its usual place
                case "es-mx": return " vuelve a su lugar habitual";
                case "sv-se": return " går tillbaka till sin plats"; // Back to its place
                case "th-th": return " กำลังกลับไปยังที่ของมัน"; // Back to its place
            }
            return " is going back to its place";
        }
        public static int SendStrayToActiveLot(object[] parameters)
        {
            Lot currentLot = LotManager.ActiveLot;
            if (currentLot == null)
            {
                currentLot = Household.ActiveHousehold?.LotHome;
                if (currentLot == null) return 0;
            }
            
            List<SimDescription> availableStrays = StrayPets.GetStrayPets(true);
            if (availableStrays.Count == 0)
            {
                string message = "No available strays";
                StyledNotification.Show(new StyledNotification.Format(message, 
                    StyledNotification.NotificationStyle.kGameMessagePositive));
                return 0;
            }
            
            SimDescription selectedStray = RandomUtil.GetRandomObjectFromList(availableStrays);
            float visitLength = 10000f;
        
            StrayPets.SendStrayToLot(selectedStray, currentLot, visitLength);
            
            StrayPets.sLastActiveHouseholdVisit = SimClock.CurrentTime();
            return 1;
        }
    }
}