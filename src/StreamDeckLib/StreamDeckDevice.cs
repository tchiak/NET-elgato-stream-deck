using System;
using System.Threading;
using HidLibrary;

namespace StreamDeckLib
{
    public class StreamDeckDevice : IDisposable
    {
        private static readonly string _manufacturer = "Elgato Systems";
        private static readonly string _deviceName = "Stream Deck";

        private readonly HidDevice _streamHidDevice;
        private bool _isListening = false;

        private Thread dataReceivedThread;

        private StreamDeckDevice()
        {
            var device = StreamDeckDevice.SearchForActiveStreamDeck();
            if (device != null)
            {
                this._streamHidDevice = (HidDevice)device;
            }
        }

        public bool StartListening()
        {
            if (DataReceived == null)
            {
                return false;
            }
            this._isListening = true;

            dataReceivedThread= new Thread(() =>
            {
                
                while(this._isListening)
                {
                    var deviceData = this._streamHidDevice.Read();
                    if (this._isListening && deviceData.Status == HidDeviceData.ReadStatus.Success)
                    {
                        OnDataReceived(new DataReceivedEventArgs {Data = deviceData.Data});
                    }
                }
                
            });
            dataReceivedThread.Start();
            return true;
        }

        public void StopListening()
        {
            this._isListening = false;
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
                    manufacturer = manufacturerBytes.ToUTF16String().Trim();
                }
                byte[] productBytes;
                if (device.ReadProduct(out productBytes))
                {
                    product = productBytes.ToUTF16String().Trim();
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

        public void Dispose()
        {
            this._isListening = false;
            if (this.dataReceivedThread.IsAlive)
            {
                this.dataReceivedThread.Abort();
            }
            if(this._streamHidDevice.IsOpen)
                this._streamHidDevice.CloseDevice();
        }
    }


    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }


}
