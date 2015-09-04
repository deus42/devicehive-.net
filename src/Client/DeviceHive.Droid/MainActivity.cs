using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Widget;
using DeviceHive.Client;

namespace DeviceHive.Droid
{
    [Activity(Label = "DeviceHive.Android", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private const string LedCode = "LED";   // LED equipment code

        private DeviceHiveClient _client;
        private Device _selectedDevice;
        private List<Device> _devices;
        private ISubscription _subscription;

        private string serverUrl = "http://192.168.164.85/DeviceHive.API";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            ShowMain();
        }

        private void ShowMain()
        {
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            var button = FindViewById<Button>(Resource.Id.InitializeButton);
            button.Click += Initialize;

            var deviceListButton = FindViewById<Button>(Resource.Id.SelectDeviceButton);
            deviceListButton.Click += ShowDeviceList;

        }

        private void ShowMainEventHandler(object sender, EventArgs e)
        {
            ShowMain();
        }

        private async void ShowDeviceList(object sender, EventArgs e)
        {
            DisplayInfo("Listing available devices...");
            try
            {
                _devices = await _client.GetDevicesAsync();
            }
            catch (Exception ex)
            {
                DisplayInfo(ex);
            }
            if (!_devices.Any())
            {
                DisplayInfo("No devices are available!\n");
            }
            else
            {
                var index = 0;
                _devices.ForEach(device => { DisplayInfo($"({ '1' + index++}) {device.Name}"); });
                SetContentView(Resource.Layout.DeviceList);

                var deviceList = FindViewById<ListView>(Resource.Id.deviceListView);
                var devices = _devices.Select(device => device.Name).ToArray();
                deviceList.Adapter = new ArrayAdapter(ApplicationContext, global::Android.Resource.Layout.SimpleListItem1, devices);
                deviceList.ItemClick += OnListItemClick;
            }
        }

        protected async void OnListItemClick(object sender, EventArgs eventArgs)
        {
            var view = (AdapterView.ItemClickEventArgs)eventArgs;
            _selectedDevice = _devices.Find(d => d.Name == ((TextView)view.View).Text);
            ShowMain();
            if (_selectedDevice != null)
            {
                _subscription = await _client.AddNotificationSubscriptionAsync(new[] { _selectedDevice.Id }, null, HandleNotification);
                DisplayInfo("Selected: " + _selectedDevice.Name);
            }
        }

        private async void Initialize(object sender, EventArgs eventArgs)
        {
            await VirtualLedClientTask();
        }

        async Task VirtualLedClientTask()
        {
            // create a DeviceHiveClient object used to communicate with the DeviceHive service
            var connectionInfo = new DeviceHiveConnectionInfo(serverUrl, "dhadmin", "dhadmin_#911");

            // create a DeviceHiveClient object used to communicate with the DeviceHive service
            _client = new DeviceHiveClient(connectionInfo);

            // show usage
            DisplayInfo("Use the following steps");
            DisplayInfo("Apply Initialization token - to get access to device");
            DisplayInfo("Select Device from the list of available devices");
            DisplayInfo("Then you will see device notifications");
            DisplayInfo("TODO: unselect device");

        }

        protected override void OnStop()
        {
            base.OnStop();
            RemoveSubscription();
        }

        private async void RemoveSubscription()
        {
            // unsubscribe from notifications
            if (_subscription != null)
                await _client.RemoveSubscriptionAsync(_subscription);

            // dispose the client
            _client?.Dispose();
        }

        private void DisplayInfo(Exception exception)
        {
            DisplayInfo($"Exception caught: {exception.Message}");
        }

        private void DisplayInfo(string info)
        {
            var textView = FindViewById<TextView>(Resource.Id.textView1);
            if (textView != null)
            {
                RunOnUiThread(() => textView.Text = textView.Text.Insert(0, $"{info}\n"));
            }
        }

        private void HandleNotification(DeviceNotification deviceNotification)
        {
            // get the notification object
            var notification = deviceNotification.Notification;

            if (notification.Name == "equipment" && notification.GetParameter<string>("equipment") == LedCode)
            {
                // output the current LED state
                DisplayInfo($"Device sent LED state change notification, new state: {notification.GetParameter<int>("state")}");
            }
            else if (notification.Name == "$device-update" && notification.GetParameter<string>("status") != null)
            {
                DisplayInfo($"Device changed the status, new status: {notification.GetParameter<string>("status")}");
            }

        }
    }
}

