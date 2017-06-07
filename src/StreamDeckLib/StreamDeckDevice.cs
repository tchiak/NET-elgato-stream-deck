using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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

        private static int _packetSize = 8218;

        private readonly HidDevice _streamHidDevice;
        private bool _isListening;
        private bool _stopThread;

        private Thread _dataReceivedThread;

        /// <summary>
        /// Creates a new StreamDeckDevice instance for interacting with an
        /// active Stream Deck
        /// </summary>
        /// <param name="device">device to control</param>
        private StreamDeckDevice(IHidDevice device)
        {
            if (device != null)
            {
                this._streamHidDevice = (HidDevice) device;
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
                
                while(!this._stopThread)
                {
                    if (!this._isListening)
                    {
                        this._streamHidDevice.Read();
                    }
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
            this._stopThread = true;

            if(this._streamHidDevice.IsOpen)
                this._streamHidDevice.CloseDevice();
        }

        private void KeyPressProccessor(object sender, EventArgs e)
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

        private const int PagePacketSize = 8191;
        private const int NumFirstPagePixels = 2583;
        private const int NumSecondPagePixels = 2601;
        private const int IconSize = 72;
        private const int NumTotalPixels = NumFirstPagePixels + NumSecondPagePixels;
        private static readonly byte[] WriteHeader = new byte[]
        {
            0x02, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00,
            0x00
        };

        private async Task<bool> WriteImage(byte[] image)
        {
            return false;
        }

        public bool WriteRGB(int r, int g, int b, int key)
        {
            Bitmap bmp = new Bitmap(72, 72, PixelFormat.Format32bppRgb);
            var color = Color.FromArgb(r,g,b);

            for (int i = 0; i < 72; i++)
            {
                for (int j = 0; j < 72; j++)
                {
                    bmp.SetPixel(i, j, color );
                }
            }

            bmp = bmp.Clone(new Rectangle(0, 0, 72, 72), PixelFormat.Format24bppRgb);
    
            byte[] page = bmp.ToByteArray(ImageFormat.Bmp);

            var page1 = new byte[_packetSize];
            Buffer.BlockCopy(WriteHeader, 0, page1, 0, WriteHeader.Length);

            var page2 = new byte[_packetSize];
            Buffer.BlockCopy(WriteHeader, 0, page2, 0, WriteHeader.Length);

            Buffer.BlockCopy(page, 0, page1, WriteHeader.Length, _packetSize - WriteHeader.Length);
            Buffer.BlockCopy(page, _packetSize-WriteHeader.Length, page2, WriteHeader.Length, page.Length - _packetSize);

            var check1 = WritePage1(page1, key);

            var check2 = WritePage2(page2, key);

            return true;
        }

        private bool WritePage1(byte[] page, int key)
        {
            Buffer.SetByte(page, 5, Convert.ToByte(key));
            Buffer.SetByte(page, 2, 0x01);
            return _streamHidDevice.Write(page);

        }

        private bool WritePage2(byte[] page, int key)
        {
            Buffer.SetByte(page, 5, Convert.ToByte(key));
            Buffer.SetByte(page, 2, 0x02);
            Buffer.SetByte(page, 4, 0x01);
            return _streamHidDevice.Write(page);
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


    public static class ImageExtensions
    {
        public static byte[] ToByteArray(this Image image, ImageFormat format)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                image.Save(ms, format);
                return ms.ToArray();
            }
        }
    }


}
