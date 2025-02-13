using System;
using System.Collections.Generic;
using Sims3.Gameplay.Abstracts;
using Sims3.Gameplay.Core;
using Sims3.Gameplay.Utilities;
using Sims3.SimIFace;
using Sims3.UI;

namespace Arro.UITweaks
{
    public class VenueInfo
    {
        public Vector3 Position { get; set; }
        public string LocalizedName { get; set; }
        public RabbitHoleType Type { get; set; }
    }

    public static class VenueCollector
    {
        private static List<VenueInfo> _cachedVenues = new List<VenueInfo>();

        public static void Initialize()
        {
            // Cache all venues with localized names on startup
            _cachedVenues = GetAllVenuesWithLocalizedNames();
        }

        private static List<VenueInfo> GetAllVenuesWithLocalizedNames()
        {
            List<VenueInfo> venues = new List<VenueInfo>();

            foreach (RabbitHoleType type in Enum.GetValues(typeof(RabbitHoleType)))
            {
                if (type == RabbitHoleType.None || type == RabbitHoleType.Any)
                    continue;

                // Get localized name using the pattern Gameplay/Objects/RabbitHoles/{TypeName}:{TypeName}
                string localizationKey = $"Gameplay/Objects/RabbitHoles/{type}:{type}";
                string localizedName = Localization.LocalizeString(localizationKey, new object[0]);

                List<RabbitHole> rabbitHoles = RabbitHole.GetRabbitHolesOfType(type);
                foreach (RabbitHole hole in rabbitHoles)
                {
                    if (hole.LotCurrent != null)
                    {
                        venues.Add(new VenueInfo
                        {
                            Position = hole.LotCurrent.Position,
                            LocalizedName = localizedName,
                            Type = type
                        });
                    }
                }
            }

            return venues;
        }

        public static void ShowSearchDialog()
        {
            try
            {
                string searchTerm = StringInputDialog.Show(
                    "Search for venues",
                    "Enter query",
                    "", 
                    false
                );

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    SearchAndFocusVenue(searchTerm);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "ShowSearchDialog");
            }
        }

        private static void SearchAndFocusVenue(string searchTerm)
        {
            foreach (VenueInfo venue in _cachedVenues)
            {
                // Case-insensitive contains check
                if (venue.LocalizedName.IndexOf(searchTerm, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    FocusCameraOnPosition(venue.Position);
                    // StyledNotification.Show(new StyledNotification.Format(
                    //     Localization.LocalizeString("Ui/Caption/GameEntry:FoundVenueNotification", new object[] { venue.LocalizedName }),
                    //     StyledNotification.NotificationStyle.kGameMessagePositive
                    // ));
                    return;
                }
            }

            StyledNotification.Show(new StyledNotification.Format("No venues with that name", 
                    StyledNotification.NotificationStyle.kGameMessagePositive));
        }

        private static void FocusCameraOnPosition(Vector3 position)
        {
            try
            {
                ICameraModel cameraModel = Responder.Instance.CameraModel;
                if (cameraModel != null)
                {
                    // Focus camera on position with default zoom
                    cameraModel.FocusOnGivenPosition(position, 1f);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler.HandleException(ex, "FocusCameraOnPosition");
            }
        }
    }
}