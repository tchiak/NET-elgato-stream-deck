using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamDeckLib;

namespace SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting Program");
            Console.WriteLine("Press 'q' to stop listening for events");
            var device = StreamDeckDevice.GetStreamDevice();
            device.DataReceived += Device_DataReceived;
            device.StartListening();
            string option = null;
            do
            {
                option = Console.ReadLine();
            } while (!option.Equals("q"));
            Console.WriteLine("Stopping Listening");
            device.StopListening();
            Console.WriteLine("Press Enter To Continue");
            Console.ReadLine();
        }

        private static void Device_DataReceived(object sender, EventArgs e)
        {
            var eventArgs = (DataReceivedEventArgs) e;
            Console.WriteLine(BitConverter.ToString(eventArgs.Data));
        }
    }
}
