using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plugin.Geolocator;
using Xamarin.Forms;

namespace XFBackgroundLocationSample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        async void Button_Clicked(System.Object sender, System.EventArgs e)
        {
            var permission = await Xamarin.Essentials.Permissions.RequestAsync<Xamarin.Essentials.Permissions.LocationAlways>();

            if (permission == Xamarin.Essentials.PermissionStatus.Denied)
            {
                // TODO Let the user know they need to accept
                return;
            }

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

        private void Current_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            locationLabel.Text += $"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}{Environment.NewLine}";

            Console.WriteLine($"{e.Position.Latitude}, {e.Position.Longitude}, {e.Position.Timestamp.TimeOfDay}");
        }
    }
}
