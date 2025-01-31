using System.Collections.Generic;
using MonoPatcherLib;
using Sims3.UI;

namespace Arro.UITweaks
{
    public class RecoverNotification
    {
        private static  DeletedNotification mLastDeletedNotification;

        [ReplaceMethod(typeof(NotificationManager), "Remove")]
		public void Remove(Notification notification)
		{
			var notificationManager = (NotificationManager)(this as object);
			RecoverNotification.mLastDeletedNotification = new DeletedNotification
			{
				Notification = notification,
				Category = notificationManager.mCurrentCategory
			};
			foreach (KeyValuePair<NotificationManager.TNSCategory, List<Notification>> keyValuePair in notificationManager.mNotifications)
			{
				List<Notification> value = keyValuePair.Value;
				int num = value.IndexOf(notification);
				bool flag = num >= 0;
				if (flag)
				{
					value.Remove(notification);
					notificationManager.mNewPostNotifications.Remove(notification);
					bool flag2 = value.Count == 0;
					if (flag2)
					{
						notificationManager.mTabs[keyValuePair.Key].Enabled = false;
						notificationManager.mTabs[keyValuePair.Key].Selected = false;
					}
					bool flag3 = keyValuePair.Key == notificationManager.mCurrentCategory;
					if (flag3)
					{
						notification.Hide();
						bool flag4 = num <= notificationManager.mCurrentNotification;
						if (flag4)
						{
							bool flag5 = num < notificationManager.mCurrentNotification || num == value.Count;
							if (flag5)
							{
								bool flag6 = num == notificationManager.mCurrentNotification && notificationManager.mCurrentNotification > 0;
								if (flag6)
								{
									value[notificationManager.mCurrentNotification - 1].Show();
								}
								notificationManager.mCurrentNotification--;
							}
							else
							{
								bool flag7 = notificationManager.mCurrentNotification >= 0;
								if (flag7)
								{
									value[notificationManager.mCurrentNotification].Show();
								}
							}
						}
					}
				}
			}
			bool flag8 = !notificationManager.mTabs[notificationManager.mCurrentCategory].Enabled;
			if (flag8)
			{
				bool flag9 = false;
				foreach (KeyValuePair<NotificationManager.TNSCategory, Button> keyValuePair2 in notificationManager.mTabs)
				{
					bool enabled = keyValuePair2.Value.Enabled;
					if (enabled)
					{
						keyValuePair2.Value.Selected = true;
						notificationManager.mCurrentCategory = keyValuePair2.Key;
						notificationManager.mCurrentNotification = notificationManager.mNotifications[notificationManager.mCurrentCategory].Count - 1;
						notificationManager.mNotifications[notificationManager.mCurrentCategory][notificationManager.mCurrentNotification].Show();
						notificationManager.UpdatePageInfo();
						flag9 = true;
						break;
					}
				}
				bool flag10 = !flag9;
				if (flag10)
				{
					notificationManager.Visible = false;
				}
			}
		}
        // Recover the last deleted notification
        public static int RecoverLastDeletedNotification(object[] parameters)
        {
            bool flag = RecoverNotification.mLastDeletedNotification != null;
            if (flag)
            {
                NotificationManager instance = NotificationManager.Instance;
                instance.Add(RecoverNotification.mLastDeletedNotification.Notification, RecoverNotification.mLastDeletedNotification.Category);
                instance.Show();
                RecoverNotification.mLastDeletedNotification = null;
            }

            return 1;
        }
    }

    public class DeletedNotification
    {
        public Notification Notification;
        public NotificationManager.TNSCategory Category;
    }
}