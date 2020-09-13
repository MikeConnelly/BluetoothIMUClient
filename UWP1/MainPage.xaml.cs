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
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using System.Diagnostics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UWP1
{
    /// <summary>
    /// BLE Test
    /// </summary>
    public sealed partial class MainPage : Page
    {

        Guid ServiceUuid = BluetoothUuidHelper.FromShortId(0x180C);
        Guid CharacteristicUuid = BluetoothUuidHelper.FromShortId(0x2A56);
        GattCharacteristic readChar;

        public MainPage()
        {
            this.InitializeComponent();
        }

        public void StartWatcher()
        {
            /*
            BluetoothLEAdvertisementPublisher publisher = new BluetoothLEAdvertisementPublisher();

            // Add custom data to the advertisement
            var manufacturerData = new BluetoothLEManufacturerData();
            manufacturerData.CompanyId = 0xFFFE;

            var writer = new DataWriter();
            writer.WriteString("Hello World");

            // Make sure that the buffer length can fit within an advertisement payload (~20 bytes). 
            // Otherwise you will get an exception.
            manufacturerData.Data = writer.DetachBuffer();

            // Add the manufacturer data to the advertisement publisher:
            publisher.Advertisement.ManufacturerData.Add(manufacturerData);

            publisher.Start();*/

            BluetoothLEAdvertisementWatcher watcher = new BluetoothLEAdvertisementWatcher();
            watcher.Received += OnAdvertisementReceived;
            watcher.Start();
            Debug.WriteLine("test");
        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            Debug.Write("Ad ");
            string deviceName = eventArgs.Advertisement.LocalName;
            Debug.WriteLine(deviceName);
            
            if (deviceName == "Nano33BLE")
            {
                watcher.Stop();
                BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
                SetupDeviceIO(device);
            }
        }

        private async void SetupDeviceIO(BluetoothLEDevice device)
        {
            GattDeviceServicesResult result = await device.GetGattServicesAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                GattDeviceService service = GetService(result);

                GattCharacteristicsResult charResult = await service.GetCharacteristicsAsync();
                if (charResult.Status == GattCommunicationStatus.Success)
                {
                    GattCharacteristic characteristic = GetCharacteristic(charResult);
                    this.readChar = characteristic;
                }
            }
        }

        private GattDeviceService GetService(GattDeviceServicesResult result)
        {
            var services = result.Services;

            foreach (GattDeviceService service in services)
            {
                if (service.Uuid.Equals(ServiceUuid))
                {
                    Debug.WriteLine("Subscription found");
                    return service;
                }
            }
            return null;
        }

        private GattCharacteristic GetCharacteristic(GattCharacteristicsResult result)
        {
            var characteristics = result.Characteristics;

            foreach(GattCharacteristic characteristic in characteristics)
            {
                if (characteristic.Uuid.Equals(CharacteristicUuid))
                {
                    Debug.WriteLine("Char found");
                    return characteristic;
                }
            }
            return null;
        }
    }
}
