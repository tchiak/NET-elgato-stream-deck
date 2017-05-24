using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Threading;
using System.Xml.Schema;
using HidLibrary;

namespace StreamDeckLib
{
    public class StreamDeckDevice
    {
        private static readonly string _manufacturer = ConfigurationManager.AppSettings["manufacturerId"];
        private static readonly string _deviceName = ConfigurationManager.AppSettings["deviceId"];

        private readonly HidDevice StreamHidDevice;
        private bool IsListening = false;

        private StreamDeckDevice()
        {
            var device = StreamDeckDevice.SearchForActiveStreamDeck();
            if (device != null)
            {
                this.StreamHidDevice = (HidDevice)device;
            }
        }

        public void StartListening()
        {
            Task.Factory.StartNew(() =>
            {
                do
                {
                    var deviceData = this.StreamHidDevice.Read(10);
                    if (deviceData.Status == HidDeviceData.ReadStatus.Success)
                    {
                        OnDataReceived(new DataReceivedEventArgs {Data = deviceData.Data});
                    }
                } while (IsListening);
            });
        }

        public event EventHandler DataReceived;

        protected virtual void OnDataReceived(DataReceivedEventArgs e)
        {
            var handler = DataReceived;
            handler?.Invoke(this, e);
        }

        private static IHidDevice SearchForActiveStreamDeck()
        {

            string manufacturer = null;
            string product = null;

            foreach (var device in new HidEnumerator().Enumerate())
            {
                byte[] manufacturerBytes;
                if (device.ReadManufacturer(out manufacturerBytes))
                {
                    manufacturer = manufacturerBytes.ToUTF8String();
                }
                byte[] productBytes;
                if (device.ReadProduct(out productBytes))
                {
                    product = productBytes.ToUTF8String();
                }

                if (manufacturer != null && manufacturer.Equals(StreamDeckDevice._manufacturer)
                    && product != null && product.Equals(StreamDeckDevice._deviceName))
                {
                    return device;
                }
            }

            return null;

        }

        public static StreamDeckDevice GetStreamDevice()
        {
            return new StreamDeckDevice();
        }

    }


    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }


}
