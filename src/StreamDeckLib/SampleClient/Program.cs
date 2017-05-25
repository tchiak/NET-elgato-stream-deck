using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDeckLib;
using DataReceivedEventArgs = StreamDeckLib.DataReceivedEventArgs;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Program");
            Console.WriteLine("Press 'q' to stop listening for events");
            var device = StreamDeckDevice.GetStreamDevice();
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
