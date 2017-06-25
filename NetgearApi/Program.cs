using System;
using System.Linq;

namespace NetgearApi
{
    class Program
    {
        static void Main(string[] args)
        {
            do
            {
                // SessionID was found using Wireshark, but yours could be the same
                // https://github.com/balloob/pynetgear/blob/master/capture/device_scan.request
                var clients = NetgearApi.GetClients("A7D88AE69687E58D9A00");

                clients = clients.OrderBy(c => c.Type).ThenBy(c => c.Name);
                foreach (var client in clients)
                {
                    Console.WriteLine("{0, -30}\t{1}\t{2}\t{3}",
                        client.Name, client.IP, client.Expire, client.Type);
                }

                Console.WriteLine();
            }
            while (Console.ReadKey(true).Key != ConsoleKey.Escape);
        }
    }
}
