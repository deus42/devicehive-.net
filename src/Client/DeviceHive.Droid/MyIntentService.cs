using System;
using Android.App;
using Android.Content;
using Android.OS;
using Notification = Android.App.Notification;

namespace DeviceHive.Droid
{
    [Service]
    public class MyIntentService : IntentService
    {
        static PowerManager.WakeLock sWakeLock;
        static object LOCK = new object();

        internal static void RunIntentInService(Context context, Intent intent)
        {
            lock (LOCK)
            {
                if (sWakeLock == null)
                {
                    // This is called from BroadcastReceiver, there is no init.
                    var pm = PowerManager.FromContext(context);
                    sWakeLock = pm.NewWakeLock(
                    WakeLockFlags.Partial, "My WakeLock Tag");
                }
            }

            sWakeLock.Acquire();
            intent.SetClass(context, typeof(MyIntentService));
            context.StartService(intent);
        }

        protected override void OnHandleIntent(Intent intent)
        {
            try
            {
                Context context = this.ApplicationContext;
                string action = intent.Action;

                if (action.Equals("com.google.android.c2dm.intent.REGISTRATION"))
                {
                    HandleRegistration(context, intent);
                }
                else if (action.Equals("com.google.android.c2dm.intent.RECEIVE"))
                {
                    HandleMessage(context, intent);
                }
            }
            finally
            {
                lock (LOCK)
                {
                    //Sanity check for null as this is a public method
                    if (sWakeLock != null)
                        sWakeLock.Release();
                }
            }
        }

        private void HandleRegistration(Context context, Intent intent)
        {
            string registrationID = intent.GetStringExtra("registration_id");
            string error = intent.GetStringExtra("error");
            if (registrationID != null)
            {
                Application.SaveString("registrationID", registrationID);
            }
        }

        private void HandleMessage(Context context, Intent intent)
        {
            foreach (var key in intent.Extras.KeySet())
                Console.WriteLine("Key: {0}, Value: {1}", key, intent.Extras.GetString(key));

            if (intent != null && intent.Extras != null)
            {
                var message = intent.Extras.GetString("notification");
                CreateNotification("Push Received", message);
            }
        }

        private void CreateNotification(string title, string desc)
        {
            var notificationManager = GetSystemService(Context.NotificationService)
                                      as NotificationManager;
            var uiIntent = new Intent(this, typeof(MainActivity));
            var notification = new Notification(Resource.Drawable.Icon, title);
            notification.Flags = NotificationFlags.AutoCancel;
            notification.Defaults = NotificationDefaults.Sound;
            notification.SetLatestEventInfo(this, title, desc,
                    PendingIntent.GetActivity(this, 0, uiIntent, 0));
            notificationManager.Notify(1, notification);
        }
    }
}