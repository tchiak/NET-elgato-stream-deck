using System;
using System.Collections.Generic;
using System.Threading;
using HidLibrary;

namespace StreamDeckLib
{
    /// <summary>
    /// A control wrapper that provides abstracted functionalities for interacting
    /// with a single active Stream Deck Hardware.
    /// </summary>
    public class StreamDeckDevice : IDisposable
    {
        private static string _manufacturer = "Elgato Systems";
        private static string _deviceName = "Stream Deck";

        private readonly HidDevice _streamHidDevice;
        private bool _isListening;

        private Thread _dataReceivedThread;

        private byte[] previousKeyEvent;

        /// <summary>
        /// Creates a new StreamDeckDevice instance for interacting with an
        /// active Stream Deck
        /// </summary>
        /// <param name="device">device to control</param>
        private StreamDeckDevice(IHidDevice device)
        {
            if (device != null)
            {
                this._streamHidDevice = (HidDevice)device;
            }
        }

        /// <summary>
        /// Starts listening for event data from the stream deck.
        /// This will raise the OnDataReceivedHandler event when data is returned.
        /// </summary>
        /// <returns>true if listening started successfully, false if no handlers are registered.</returns>
        public bool StartListening()
        {
            if (OnDataReceived == null)
            {
                OnDataReceived += KeyPressProccessor;
            }
            this._isListening = true;

            _dataReceivedThread= new Thread(() =>
            {
                
                while(this._isListening)
                {
                    var deviceData = this._streamHidDevice.Read();
                    if (this._isListening && deviceData.Status == HidDeviceData.ReadStatus.Success)
                    {
                        OnDataReceivedHandler(new DataReceivedEventArgs {Data = deviceData.Data});
                    }
                }
                
            });
            _dataReceivedThread.Start();
            return true;
        }

        /// <summary>
        /// Stops listening for data from the stream deck
        /// </summary>
        public void StopListening()
        {
            this._isListening = false;
            OnDataReceived -= KeyPressProccessor;
            return;
        }

        /// <summary>
        /// Events related to data received
        /// </summary>
        public event EventHandler OnDataReceived;

        public event EventHandler OnKeyDown;

        protected virtual void OnKeyDownHandler(KeyEventArgs e)
        {
            var handler = OnKeyDown;
            handler?.Invoke(this, e);
        }

        /// <summary>
        /// Handles invoking the OnDataReceived event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDataReceivedHandler(DataReceivedEventArgs e)
        {
            var handler = OnDataReceived;
            handler?.Invoke(this, e);
        }
        
        /// <summary>
        /// Searches for an active connected Stream Deck
        /// </summary>
        /// <returns>The device if found, null otherwise</returns>
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

        /// <summary>
        /// Returns an instance of StreamDeckDevice 
        /// </summary>
        /// <returns>null if no active Stream Deck Device is detected, else StreamDeckDevice Instance</returns>
        public static StreamDeckDevice GetStreamDevice()
        {
            var device = StreamDeckDevice.SearchForActiveStreamDeck();
            return device == null ? null : new StreamDeckDevice(device);
        }

        /// <summary>
        /// Performs clean up of the device
        /// </summary>
        public void Dispose()
        {
            this._isListening = false;
            if(this._dataReceivedThread.ThreadState != ThreadState.Stopped)
                this._dataReceivedThread.Abort();
            if(this._streamHidDevice.IsOpen)
                this._streamHidDevice.CloseDevice();
        }

        public void KeyPressProccessor(object sender, EventArgs e)
        {
            var keyEventData = ((DataReceivedEventArgs)e).Data;
            var keysDownList = new List<int>(15);
            var count = 1;
            for (var i = 5; i > 0; i--)
            {
                if (keyEventData[i] == 1)
                {
                    keysDownList.Add(count);
                }
                count++;
            }
            for (var i = 10; i >= 6; i--)
            {
                if (keyEventData[i] == 1)
                {
                    keysDownList.Add(count);
                }
                count++;
            }
            for (var i = 15; i > 10; i--)
            {

                if (keyEventData[i] == 1)
                {
                    keysDownList.Add(count);
                }
                count++;
            }

            var args = new KeyEventArgs
            {
                Keys = keysDownList
            };

            OnKeyDownHandler(args);
        }
    }


    public class DataReceivedEventArgs : EventArgs
    {
        public byte[] Data { get; set; }
    }

    public class KeyEventArgs : EventArgs
    {
        public List<int> Keys { get; set; }
    }


}
