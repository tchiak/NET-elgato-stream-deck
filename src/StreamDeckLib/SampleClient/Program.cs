using System;
using System.Collections.Generic;
using System.Threading;
using StreamDeckLib;

namespace SampleClient
{
    internal class Program
    {
        private static bool _lightShow = false;
        private static List<string> _image_list = new List<string> {
            @"D:\OneDrive\Pictures\windows-start-icon.png",
            @"D:\OneDrive\Pictures\chase-icon.png",
            @"D:\OneDrive\Pictures\53-icon.png",
            @"D:\OneDrive\Pictures\postman-icon.png"
        };

        private static Random _rnd = new Random();

        private static StreamDeckDevice device = null;
        static void Main()
        {
            Console.WriteLine("Starting Program");
            Console.WriteLine("Press 'q' to stop listening for events");
            device = StreamDeckDevice.GetStreamDevice();
            device.OnKeyDown += DeviceOnDataReceived;
            
            device.StartListening();
            string option = null;
            do
            {
                option = Console.ReadLine();
                if(option.Equals("s"))
                    device.StopListening();
                if (option.Equals("e"))
                    device.StartListening();
                
            } while (!option.Equals("q"));
            Console.WriteLine("Stopping Listening");
            device.StopListening();
            Console.WriteLine("Press Enter To Continue");
            Console.ReadLine();
            device.Dispose();
        }

        private static void LightShow(StreamDeckDevice device )
        {
            device.WriteImage(@"D:\OneDrive\Pictures\windows-start-icon.png", 13).ConfigureAwait(false);
            /*  _lightShow = true;
            Random rnd = new Random();
            while (true)
            {

                device.WriteRGB(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256), 4);

                for (int i = 0; i < 16; i++)
                {
                    var success = device.WriteRGB(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256), i);
                    
                }
               // Thread.Sleep();
            }*/
        }
        private static void DeviceOnDataReceived(object sender, EventArgs e)
        {
            var keys = (KeyEventArgs) e;
            foreach (var key in keys.Keys) {
                device.WriteImage(_image_list[_rnd.Next(0, 4)], key).ConfigureAwait(false);
                //Console.WriteLine($"Key: {key} pressed ");
            }
        }
    }
}
