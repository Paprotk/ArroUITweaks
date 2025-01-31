using System.Collections.Generic;
using Sims3.SimIFace;
using Sims3.UI;

namespace Arro.RecoverNotification
{
    public class LunarTooltip : Tooltip
    {
        public LunarTooltip() : base("SeasonTooltip", 1) //Use the same layout as the season tooltip
        {
            this.mNameText = (this.mTooltipWindow.GetChildByID(99484672U, true) as Text);
            this.mTempText = (this.mTooltipWindow.GetChildByID(117147912U, true) as Text);
            this.mNextText = (this.mTooltipWindow.GetChildByID(223489328U, true) as Text);
            this.mAutoSizer = (this.mTooltipWindow.GetChildByID(99484679U, true) as AutosizeBaseController);
            this.Update();
            Responder.Instance.SimMinuteChanged -= this.Update;
        }

        public override void Dispose()
        {
            Responder.Instance.SimMinuteChanged -= this.Update;
            base.Dispose();
        }
        
        public void Update()
        {
            var lunarPhase = World.GetLunarPhase();
            string entryKey = "UI/LunarCycle:MoonString" + (lunarPhase + 1U);
            this.mNameText.Caption = Responder.Instance.LocalizationModel.LocalizeString(entryKey, new object[0]);
            this.mTempText.Caption = null;
            var daysUntilFullMoon = GetDaysUntilFullMoon();
            if (daysUntilFullMoon > 1)
            {
                this.mNextText.Caption = Responder.Instance.LocalizationModel.LocalizeString("UI/LunarCycle:NextFullMoon", new object[]
                {
                    daysUntilFullMoon
                });
            }
            if (daysUntilFullMoon == 1)
            {
                string fullMoon = Responder.Instance.LocalizationModel.LocalizeString("UI/LunarCycle:MoonString1", new object[0]);
                string tomorrow = GetTomorrowString();
                this.mNextText.Caption = fullMoon + " " + tomorrow.ToLower();
            }
            else //Lunar disabled or full moon
            {
                this.mNextText.Caption = null;
            }
            this.mAutoSizer.AutosizeFields();
        }
        private int GetDaysUntilFullMoon()
        {
            IOptionsModel optionsModel = Responder.Instance.OptionsModel;
            int currentLunarPhase = World.GetLunarPhase();

            DeviceConfig.GetOption(optionsModel.LunarCycleLengthKey, out var lunarCycleLength);
            DeviceConfig.GetOption(optionsModel.EnableLunarCycleKey, out var isLunarCycleEnabled);

            if (isLunarCycleEnabled == 0)
            {
                return 0;
            }

            // Define lookup tables for days until full moon based on lunar cycle length
            var daysUntilFullMoonLookup = new Dictionary<uint, Dictionary<int, int>>
            {
                { 1, new Dictionary<int, int> { { 4, 1 }, { 0, 0 } } }, // 2-day cycle
                { 2, new Dictionary<int, int> { { 4, 2 }, { 3, 1 }, { 0, 0 }, { 6, 3 } } }, // 4-day cycle
                { 3, new Dictionary<int, int> { { 4, 3 }, { 1, 2 }, { 3, 1 }, { 0, 0 }, { 5, 5 }, { 7, 4 } } }, // 6-day cycle
                { 4, new Dictionary<int, int> { { 4, 4 }, { 1, 3 }, { 2, 2 }, { 3, 1 }, { 0, 0 }, { 5, 7 }, { 6, 6 }, { 7, 5 } } }, // 8-day cycle
                { 5, new Dictionary<int, int> { { 4, 5 }, { 1, 4 }, { 2, 3 }, { 3, 1 }, { 0, 0 }, { 5, 9 }, { 6, 8 }, { 7, 6 } } } // 10-day cycle
            };
            
            if (daysUntilFullMoonLookup.TryGetValue(lunarCycleLength, out var phaseLookup) && phaseLookup.TryGetValue(currentLunarPhase, out var daysUntilFullMoon))
            {
                return daysUntilFullMoon;
            }

            return 0;
        }

        public string GetTomorrowString()
        {
            var locale = StringTable.GetLocale();
            switch (locale)
            {
                case "zh-cn": return "明天";
                case "zh-tw": return "明天";
                case "cs-cz": return "Zítra";
                case "da-dk": return "I morgen";
                case "nl-nl": return "Morgen";
                case "en-gb": return "Tomorrow";
                case "en-us": return "Tomorrow";
                case "fi-fi": return "Huomenna";
                case "fr-fr": return "Demain";
                case "de-de": return "Morgen";
                case "el-gr": return "Αύριο";
                case "hu-hu": return "Holnap";
                case "it-it": return "Domani";
                case "ja-jp": return "明日";
                case "ko-kr": return "내일";
                case "no-no": return "I morgen";
                case "pl-pl": return "Jutro";
                case "pt-br": return "Amanhã";
                case "pt-pt": return "Amanhã";
                case "ru-ru": return "Завтра";
                case "es-es": return "Mañana";
                case "es-mx": return "Mañana";
                case "sv-se": return "Imorgon";
                case "th-th": return "พรุ่งนี้";
            }
            return "tomorrow string";
        }
        public readonly Text mNameText;

        public readonly Text mTempText;

        public readonly Text mNextText;

        public readonly AutosizeBaseController mAutoSizer;
    }
}