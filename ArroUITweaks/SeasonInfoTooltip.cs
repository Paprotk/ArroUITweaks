using MonoPatcherLib;
using Sims3.Gameplay.Seasons;
using Sims3.SimIFace.Enums;
using Sims3.UI;
using Sims3.UI.Hud;

namespace Arro.RecoverNotification
{
    public class SeasonInfoTooltip
    {
        [ReplaceMethod(typeof(SeasonTooltip), "Update")]
        public void Update()
        {
            var instance = (SeasonTooltip)(this as object);
            IHudModel hudModel = Responder.Instance.HudModel;
            
            instance.mNameText.Caption = hudModel.GetSeasonName(hudModel.GetCurrentSeason(), false);
            
            string tempString = hudModel.GetTemperatureString();
            string weatherString = GetLocalizedWeatherString();
            
            instance.mTempText.Caption = $"{weatherString}  {tempString}";
            
            instance.mNextText.Caption = hudModel.GetSeasonDaysLeftString();
            instance.mAutoSizer.AutosizeFields();
        }

        private string GetLocalizedWeatherString()
        {
            // Get current weather from SeasonsManager
            Weather currentWeather = SeasonsManager.CurrentWeather;
            switch (currentWeather)
            {
                case Weather.Fog:
                    return Responder.Instance.LocalizationModel.LocalizeString("Gameplay/Seasons/Weather:Fog", new object[0]);
                case Weather.Sunny:
                    return Responder.Instance.LocalizationModel.LocalizeString("Gameplay/Seasons/Weather:Sunny", new object[0]);
                case Weather.Hail:
                    return Responder.Instance.LocalizationModel.LocalizeString("Gameplay/Seasons/Weather:Hail", new object[0]);
                case Weather.Rain:
                    return Responder.Instance.LocalizationModel.LocalizeString("Gameplay/Seasons/Weather:Rain", new object[0]);
                case Weather.Snow:
                    return Responder.Instance.LocalizationModel.LocalizeString("Gameplay/Seasons/Weather:Snow", new object[0]);
                default:
                    return "unknown weather";
            }
        }
    }
}