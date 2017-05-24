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
            device.OnDataReceived += DeviceOnDataReceived;
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

        private static void DeviceOnDataReceived(object sender, EventArgs e)
        {
            var eventArgs = (DataReceivedEventArgs) e;
            var keyEventData = new byte[15];
            Buffer.BlockCopy(eventArgs.Data, 1, keyEventData, 0, 15);
            
            var keysDownList = new List<int>(15);

            int count = 1;
            for (int i = 5; i >= 0; i--) {
                if (keyEventData[i] == 1) {
                    keysDownList.Add(count);
                }
                count++;
            }
            count = 5;
            for (int i = 9; i >=5; i--)
            {
                if (keyEventData[i] == 1)
                {
                    keysDownList.Add(i);
                }
                count++;
            }

            count = 11;
            for (int i = 14; i >= 9; i--)
            {
                
                if (keyEventData[i] == 1)
                {
                    keysDownList.Add(count);
                }
                count++;
            }
            foreach (var key in keysDownList) {
                Console.Write(key + ", ");
            }
            Console.WriteLine(BitConverter.ToString(eventArgs.Data));
            
        }
    }
}
