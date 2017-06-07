using System;
using System.Threading;
using StreamDeckLib;

namespace SampleClient
{
    internal class Program
    {
        private static bool _lightShow = false;

        static void Main()
        {
            Console.WriteLine("Starting Program");
            Console.WriteLine("Press 'q' to stop listening for events");
            var device = StreamDeckDevice.GetStreamDevice();
            device.OnKeyDown += DeviceOnDataReceived;
            LightShow(device);
            /*
            device.StartListening();
            string option = null;
            do
            {
                option = Console.ReadLine();
                if(option.Equals("s"))
                    device.StopListening();
                if (option.Equals("e"))
                    device.StartListening();
                if (option.Equals("l"))
                    LightShow(device);

            } while (!option.Equals("q"));
            Console.WriteLine("Stopping Listening");
            device.StopListening();
            Console.WriteLine("Press Enter To Continue");
            Console.ReadLine();
            */
            device.Dispose();
        }

        private static void LightShow(StreamDeckDevice device )
        {
            _lightShow = true;
            Random rnd = new Random();
            while (true)
            {

                device.WriteRGB(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256), 4);

                for (int i = 0; i < 16; i++)
                {
                    var success = device.WriteRGB(rnd.Next(0, 256), rnd.Next(0, 256), rnd.Next(0, 256), i);
                    
                }
               // Thread.Sleep();
            }
        }
        private static void DeviceOnDataReceived(object sender, EventArgs e)
        {
            var keys = (KeyEventArgs) e;
            foreach (var key in keys.Keys)
            {
                Console.WriteLine($"Key: {key} pressed ");
            }
        }
    }
}
