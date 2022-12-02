using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Plugin.Geolocator;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XFBackgroundLocationSample
{
    public partial class MainPage : ContentPage
    {
        INotificationManager notificationManager;
        Location goalLocation = new Location(latitude: 32.02069, longitude: 34.763419999999996);
        bool arrivedDestination = false;

        public MainPage()
        {
            InitializeComponent();

            notificationManager = DependencyService.Get<INotificationManager>();
            notificationManager.NotificationReceived += (sender, eventArgs) =>
            {
                var evtData = (NotificationEventArgs)eventArgs;
                ShowNotification(evtData.Title, evtData.Message);
            };


            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Subscribe<LocationMessage>(this, "Location", message => {
                    Device.BeginInvokeOnMainThread(() => {
                        try
                        {
                            locationLabel.Text += $"{Environment.NewLine}{message.Latitude}, {message.Longitude}, {DateTime.Now.ToLongTimeString()}!!! Lin";
                            Console.WriteLine($"{message.Latitude}, {message.Longitude}, {DateTime.Now.ToLongTimeString()}!!!");

                            if (!arrivedDestination && message.Latitude == goalLocation.Latitude && message.Longitude == goalLocation.Longitude)
                            {
                                locationLabel.Text += "You've arrived your desitnation!\n";
                                notificationManager.SendNotification("Destination arrived!", "You're arrived your destionation");
                                Console.WriteLine("You've arrived your destination!");
                                arrivedDestination = true;
                            }
                        }
                        catch (Exception ex) {
                            Console.Write("Failed in MessagingCenter.Subscribe<LocationMessage>: " + ex.Message);
                        }
                    });
                });

                MessagingCenter.Subscribe<StopServiceMessage>(this, "ServiceStopped", message => {
                    Device.BeginInvokeOnMainThread(() => {
                        try
                        {
                            locationLabel.Text = "Location Service has been stopped!";
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Failed in MessagingCenter.Subscribe<StopServiceMessage>: " + ex.Message);
                        }
                    });
                });

                MessagingCenter.Subscribe<LocationErrorMessage>(this, "LocationError", message => {
                    Device.BeginInvokeOnMainThread(() => {
                        try
                        {
                            locationLabel.Text = "There was an error updating location!";
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Failed in MessagingCenter.Subscribe<LocationErrorMessage>: " + ex.Message);
                        }
                    });
                });

                MessagingCenter.Subscribe<LocationArrivedMessage>(this, "LocationArrived", message => {
                    Device.BeginInvokeOnMainThread(() => {
                        try
                        {
                            locationLabel.Text = "You've arrived your destination!";
                        }
                        catch (Exception ex)
                        {
                            Console.Write("Failed in MessagingCenter.Subscribe<LocationArrivedMessage>: " + ex.Message);
                        }
                    });
                });

                if (Preferences.Get("LocationServiceRunning", false) == true)
                {
                    StartService();
                }
            }
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            var permission = await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.LocationAlways>();

            Console.WriteLine("Button " + sender + " clicked!");
            Console.Write("Permissions:" + permission);

            if (permission == Xamarin.Essentials.PermissionStatus.Denied)
            {
                // TODO Let the user know they need to accept
                Console.WriteLine("Permission denied.");
                return;
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                Console.WriteLine("Running on IOS.");
                if (CrossGeolocator.Current.IsListening)
                {
                    await CrossGeolocator.Current.StopListeningAsync();
                    CrossGeolocator.Current.PositionChanged -= Current_PositionChanged;

                    return;
                }

                await CrossGeolocator.Current.StartListeningAsync(TimeSpan.FromSeconds(1), 10, false, new Plugin.Geolocator.Abstractions.ListenerSettings
                {
                    ActivityType = Plugin.Geolocator.Abstractions.ActivityType.AutomotiveNavigation,
                    AllowBackgroundUpdates = true,
                    DeferLocationUpdates = false,
                    DeferralDistanceMeters = 10,
                    DeferralTime = TimeSpan.FromSeconds(5),
                    ListenForSignificantChanges = true,
                    PauseLocationUpdatesAutomatically = true
                });

                CrossGeolocator.Current.PositionChanged += Current_PositionChanged;
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                Console.WriteLine("Running on Android.");
                if (Preferences.Get("LocationServiceRunning", false) == false)
                {
                    Console.WriteLine("Start service on Android.");
                    StartService();
                }
                else
                {
                    Console.WriteLine("Stop service on Android.");
                    StopService();
                }
            }
        }

        private void StartService()
        {
            var startServiceMessage = new StartServiceMessage();
            Console.WriteLine("Created StartServiceMessage.");

            try
            {
                MessagingCenter.Send(startServiceMessage, "ServiceStarted");
                Preferences.Set("LocationServiceRunning", true);

                locationLabel.Text = "Location Service has been started!\n";
                locationLabel.Text += $"Goal destination: {goalLocation.Latitude},{goalLocation.Longitude}";
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void StopService()
        {
            var stopServiceMessage = new StopServiceMessage();
            MessagingCenter.Send(stopServiceMessage, "ServiceStopped");
            Preferences.Set("LocationServiceRunning", false);
        }

        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            locationLabel.Text += $"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}{Environment.NewLine}";
            Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}!!!!!!");

            if(e.Position.Latitude.Equals(goalLocation.Latitude) && e.Position.Longitude.Equals(goalLocation.Longitude))
            {
                notificationManager.SendNotification("Destination arrived!", "You're arrived your destionation");
                Console.WriteLine("You've arrived your destination!");
            }
        }

        void ShowNotification(string title, string message)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                var msg = new Label()
                {
                    Text = $"Notification Received:\nTitle: {title}\nMessage: {message}"
                };
                stackLayout.Children.Add(msg);
            });
        }
    }
}
