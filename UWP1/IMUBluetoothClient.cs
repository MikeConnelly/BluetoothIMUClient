using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace UWP1
{
    class IMUBluetoothClient
    {
        Guid AccelerationServiceUuid = BluetoothUuidHelper.FromShortId(0x180C);
        Guid VelocityServiceUuid = BluetoothUuidHelper.FromShortId(0x180D);
        Guid PositionServiceUuid = BluetoothUuidHelper.FromShortId(0x180E);

        Guid AccXCharUuid = BluetoothUuidHelper.FromShortId(0x2A10);
        Guid AccYCharUuid = BluetoothUuidHelper.FromShortId(0x2A11);
        Guid AccZCharUuid = BluetoothUuidHelper.FromShortId(0x2A12);
        Guid VelXCharUuid = BluetoothUuidHelper.FromShortId(0x2A13);
        Guid VelYCharUuid = BluetoothUuidHelper.FromShortId(0x2A14);
        Guid VelZCharUuid = BluetoothUuidHelper.FromShortId(0x2A15);
        Guid PosXCharUuid = BluetoothUuidHelper.FromShortId(0x2A16);
        Guid PosYCharUuid = BluetoothUuidHelper.FromShortId(0x2A17);
        Guid PosZCharUuid = BluetoothUuidHelper.FromShortId(0x2A18);

        GattDeviceService AccelerationService;
        GattDeviceService VelocityService;
        GattDeviceService PositionService;

        GattCharacteristic AccXChar;
        GattCharacteristic AccYChar;
        GattCharacteristic AccZChar;
        GattCharacteristic VelXChar;
        GattCharacteristic VelYChar;
        GattCharacteristic VelZChar;
        GattCharacteristic PosXChar;
        GattCharacteristic PosYChar;
        GattCharacteristic PosZChar;

        BluetoothLEAdvertisementWatcher NanoAdvertisementWatcher;

        public delegate void DeviceConnectedEventHandler(object sender, DeviceConnectedEventArgs e);
        public delegate void ServiceRetrievedEventHandler(object sender, EventArgs e);
        public delegate void CharacteristicRetrievedEventHandler(object sender, EventArgs e);
        public delegate void DataReceivedEventHandler(object sender, DataReceivedEventArgs e);

        public event DeviceConnectedEventHandler DeviceConnected;
        public event ServiceRetrievedEventHandler ServiceRetrieved;
        public event CharacteristicRetrievedEventHandler CharacteristicRetrieved;
        public event DataReceivedEventHandler DataReceived;

        public IMUBluetoothClient() {}

        public void StartWatcher()
        {
            NanoAdvertisementWatcher = new BluetoothLEAdvertisementWatcher();
            NanoAdvertisementWatcher.Received += OnAdvertisementReceived;
            NanoAdvertisementWatcher.Start();
        }

        public void StopWatcher()
        {
            NanoAdvertisementWatcher.Stop();
        }

        private async void OnAdvertisementReceived(BluetoothLEAdvertisementWatcher watcher, BluetoothLEAdvertisementReceivedEventArgs eventArgs)
        {
            Debug.Write("Ad ");
            string deviceName = eventArgs.Advertisement.LocalName;
            Debug.WriteLine(deviceName);

            if (deviceName == "Nano33BLE")
            {
                BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(eventArgs.BluetoothAddress);
                watcher.Stop();
                SetupConnection(device);
            }
        }

        private async void SetupConnection(BluetoothLEDevice device)
        {
            GattDeviceServicesResult result = await device.GetGattServicesAsync();
            if (result.Status == GattCommunicationStatus.Success)
            {
                DeviceConnected(this, null);

                var IMUServices = GetIMUServices(result);

                GattCharacteristicsResult accCharResult = await AccelerationService.GetCharacteristicsAsync();
                GattCharacteristicsResult velCharResult = await VelocityService.GetCharacteristicsAsync();
                GattCharacteristicsResult posCharResult = await PositionService.GetCharacteristicsAsync();

                if (accCharResult.Status == GattCommunicationStatus.Success)
                {
                    foreach (GattCharacteristic accChar in accCharResult.Characteristics)
                    {
                        if (accChar.Uuid.Equals(AccXCharUuid))
                        {
                            AccXChar = accChar;
                        }
                        else if (accChar.Uuid.Equals(AccYCharUuid))
                        {
                            AccYChar = accChar;
                        }
                        else if (accChar.Uuid.Equals(AccZCharUuid))
                        {
                            AccZChar = accChar;
                        }
                    }
                }

                if (velCharResult.Status == GattCommunicationStatus.Success)
                {
                    foreach (GattCharacteristic velChar in velCharResult.Characteristics)
                    {
                        if (velChar.Uuid.Equals(VelXCharUuid))
                        {
                            VelXChar = velChar;
                        }
                        else if (velChar.Uuid.Equals(VelYCharUuid))
                        {
                            VelYChar = velChar;
                        }
                        else if (velChar.Uuid.Equals(VelZCharUuid))
                        {
                            VelZChar = velChar;
                        }
                    }
                }

                if (posCharResult.Status == GattCommunicationStatus.Success)
                {
                    foreach (GattCharacteristic posChar in posCharResult.Characteristics)
                    {
                        if (posChar.Uuid.Equals(PosXCharUuid))
                        {
                            PosXChar = posChar;
                        }
                        else if (posChar.Uuid.Equals(PosYCharUuid))
                        {
                            PosYChar = posChar;
                        }
                        else if (posChar.Uuid.Equals(PosZCharUuid))
                        {
                            PosZChar = posChar;
                        }
                    }
                }

                List<GattCharacteristic> chars = new List<GattCharacteristic>()
                        {
                            AccXChar, AccYChar, AccZChar,
                            VelXChar, VelYChar, VelZChar,
                            PosXChar, PosYChar, PosZChar
                        };

                foreach (GattCharacteristic c in chars)
                {
                    GattCommunicationStatus status = await c.WriteClientCharacteristicConfigurationDescriptorAsync(
                            GattClientCharacteristicConfigurationDescriptorValue.Notify);
                    if (status == GattCommunicationStatus.Success)
                    {
                        c.ValueChanged += CharValueChangedSubscription;
                    }
                }
            }
        }

        private List<GattDeviceService> GetIMUServices(GattDeviceServicesResult result)
        {
            var IMUServices = new List<GattDeviceService>();
            var services = result.Services;

            foreach (GattDeviceService service in services)
            {
                if (service.Uuid.Equals(AccelerationServiceUuid))
                {
                    Debug.WriteLine("Acceleration Service Found");
                    ServiceRetrieved(this, null);

                    AccelerationService = service;
                    IMUServices.Add(service);
                }
                else if (service.Uuid.Equals(VelocityServiceUuid))
                {
                    Debug.WriteLine("Velocity Service Found");
                    ServiceRetrieved(this, null);

                    VelocityService = service;
                    IMUServices.Add(service);
                }
                else if (service.Uuid.Equals(PositionServiceUuid))
                {
                    Debug.WriteLine("Position Service Found");
                    ServiceRetrieved(this, null);

                    PositionService = service;
                    IMUServices.Add(service);
                }
            }
            return IMUServices;
        }

        private async void CharValueChangedSubscription(GattCharacteristic sender, GattValueChangedEventArgs args)
        {
            DataReceivedEventArgs e = new DataReceivedEventArgs();

            if (sender.Equals(AccXCharUuid))
            {
                e.FieldIndex = 1;    
            }
            else if (sender.Equals(AccYCharUuid))
            {
                e.FieldIndex = 2;
            }
            else if (sender.Equals(AccZCharUuid))
            {
                e.FieldIndex = 3;
            }
            else if (sender.Equals(VelXCharUuid))
            {
                e.FieldIndex = 4;
            }
            else if (sender.Equals(VelYCharUuid))
            {
                e.FieldIndex = 5;
            }
            else if (sender.Equals(VelZCharUuid))
            {
                e.FieldIndex = 6;
            }
            else if (sender.Equals(PosXCharUuid))
            {
                e.FieldIndex = 7;
            }
            else if (sender.Equals(PosYCharUuid))
            {
                e.FieldIndex = 8;
            }
            else if (sender.Equals(PosZCharUuid))
            {
                e.FieldIndex = 9;
            }

            var reader = DataReader.FromBuffer(args.CharacteristicValue);
            e.Value = reader.ReadSingle();
            DataReceived(this, e);
        }
    }
}
