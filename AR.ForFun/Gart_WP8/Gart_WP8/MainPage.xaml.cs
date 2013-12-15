using GART;
using GART.BaseControls;
using GART.Controls;
using GART.Data;
using Microsoft.Phone.Controls;
using System;
using System.Collections.ObjectModel;
using System.Device.Location;

namespace Gart_WP8
{
    public partial class MainPage : PhoneApplicationPage
    {
        private ObservableCollection<ARItem> locationsTvrda;
        public MainPage()
        {
            InitializeComponent();
            locationsTvrda = new ObservableCollection<ARItem>();

            GetData();
            this.Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            var currentLocation = ardisplay.Location;
            Random rand = new Random();
            for (int i = 0; i < 5; i++)
            {

                GeoCoordinate offset = new GeoCoordinate()
                {
                    Latitude = currentLocation.Latitude + ((double)rand.Next(-90, 90)) / 100000,
                    Longitude = currentLocation.Longitude + ((double)rand.Next(-90, 90)) / 100000
                };

                locationsTvrda.Add(new CityPlace()
                {
                    GeoLocation = offset,
                    Content = "Lorem ipsum " + i,
                    Description = "Quisque tincidunt lorem vitae porta gravida. Nullam vitae suscipit risus, sit amet sagittis arcu. In elementum lacinia turpis, vel commodo eros consequat nec. Nam vitae tristique metus, a sagittis nibh. Etiam id purus vel elit egestas semper ac ac ipsum. Etiam id commodo ligula, quis ultrices nibh. "
                });
            }
        }

        private void GetData()
        {
            locationsTvrda.Add(new CityPlace()
            {
                GeoLocation = new GeoCoordinate(45.560533, 18.695746),
                Content = "Kugin spomenik",
                Description = "Spomenik Presvetom Trojstvu - sa svecima Sv. Sebastionom, Sv.Rokom, Sv. Rozalijom i Sv. Katarinom - podignut je 1729. do 1730. godine u središtu Tvrđe kao zavjetni spomenik da se Bog smiluje i da odvrati kugu koja je harala u Osijeku i u cijeloj Slavoniji"
            });

            ardisplay.ARItems = locationsTvrda;
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            ardisplay.Filters = new ObservableCollection<LiveFilter>
            {
                LiveFilter.Grayscale,
                LiveFilter.Lomo,
                LiveFilter.Cartoon,
                LiveFilter.Paint,
                LiveFilter.AutoLevels,
                LiveFilter.AutoEnhance,
                LiveFilter.WhiteboardEnhancement
            };

            ardisplay.StartServices();
            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {
            ardisplay.StopServices();
            base.OnNavigatedFrom(e);
        }
        private void next_Click(object sender, EventArgs e)
        {
            ardisplay.TakeNextFilter();
        }

        private void world_on_off_Click(object sender, EventArgs e)
        {
            UIHelper.ToggleVisibility(worldView);
        }

        private void mapButton_Click(object sender, EventArgs e)
        {
            UIHelper.ToggleVisibility(overheadMap);
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);

            ControlOrientation orientation = ControlOrientation.Default;

            switch (e.Orientation)
            {
                case PageOrientation.LandscapeLeft:
                    orientation = ControlOrientation.Clockwise270Degrees;
                    break;
                case PageOrientation.LandscapeRight:
                    orientation = ControlOrientation.Clockwise90Degrees;
                    break;
            }

            ardisplay.Orientation = orientation;
        }
    }
}