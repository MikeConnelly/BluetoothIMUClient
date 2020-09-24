using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using System.Diagnostics;
using Windows.UI.Core;

namespace UWP1
{
    /// <summary>
    /// BLE Test
    /// </summary>
    public sealed partial class MainPage : Page
    {
        IMUBluetoothClient Client;

        public MainPage()
        {
            this.InitializeComponent();
            Client = new IMUBluetoothClient();
            Client.DeviceConnected += ClientDeviceConnected;
            Client.ServiceRetrieved += ClientServiceRetrieved;
            Client.CharacteristicRetrieved += ClientCharacteristicRetrieved;
            Client.DataReceived += ClientDataRecieved;

            ScanButton.IsEnabled = true;
            StopButton.IsEnabled = false;
        }

        private async void StartWatcher(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ScanButton.IsEnabled = false;
                StopButton.IsEnabled = true;
                Status.Text = "Searching for device...";
            });

            Debug.WriteLine("begin scan");
            Client.StartWatcher();
        }

        private async void StopWatcher(object sender, RoutedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ScanButton.IsEnabled = true;
                StopButton.IsEnabled = false;
            });

            Debug.WriteLine("stop scan");
            Status.Text = "Status: Not Connected";
            Client.StopWatcher();
        }

        private async void ClientDeviceConnected(object sender, DeviceConnectedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Status.Text = "Connected. Retrieving IMU Service...";
            });
        }

        private async void ClientServiceRetrieved(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Status.Text = "Connected. Subscribing to Updates...";
            });
        }

        private async void ClientCharacteristicRetrieved(object sender, EventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                Status.Text = "Connected.";
            });
        }

        private async void ClientDataRecieved(object sender, DataReceivedEventArgs e)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                switch (e.FieldIndex)
                {
                    case 1:
                        AX.Text = String.Format("Acc X: {0}m/s", e.Value.ToString("0.00"));
                        break;
                    case 2:
                        AY.Text = String.Format("Acc Y: {0}m/s", e.Value.ToString("0.00"));
                        break;
                    case 3:
                        AZ.Text = String.Format("Acc Z: {0}m/s", e.Value.ToString("0.00"));
                        break;
                    case 4:
                        VX.Text = String.Format("Vel X: {0}m/s", e.Value.ToString("0.00"));
                        break;
                    case 5:
                        VY.Text = String.Format("Vel Y: {0}m/s", e.Value.ToString("0.00"));
                        break;
                    case 6:
                        VZ.Text = String.Format("Vel Z: {0}m/s", e.Value.ToString("0.00"));
                        break;
                    case 7:
                        PX.Text = String.Format("Pos X: {0}m", e.Value.ToString("0.00"));
                        break;
                    case 8:
                        PY.Text = String.Format("Pos Y: {0}m", e.Value.ToString("0.00"));
                        break;
                    case 9:
                        PZ.Text = String.Format("Pos Z: {0}m", e.Value.ToString("0.00"));
                        break;
                }
            });
        }
    }
}
