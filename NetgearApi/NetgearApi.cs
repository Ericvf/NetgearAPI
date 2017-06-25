using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace NetgearApi
{
    public class NetgearApi
    {
        public class TcpClient
        {
            public string Expire { get; set; }
            public string IP { get; set; }
            public string Mac { get; set; }
            public string Name { get; set; }
            public string Type { get; set; }
            public string Strength { get; set; }
        }

        public static string Authenticate(string username, string password)
        {
            var message = $@"
                <SOAP-ENV:Envelope xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
                    <SOAP-ENV:Header>
                        <SessionID xsi:type='xsd:string' xmlns:xsi='http://www.w3.org/1999/XMLSchema-instance'>E6A88AE69687E58D9A00</SessionID>
                    </SOAP-ENV:Header>
                    <SOAP-ENV:Body>
                        <Authenticate>
                            <NewPassword xsi:type='xsd:string' xmlns:xsi='http://www.w3.org/1999/XMLSchema-instance'>${password}</NewPassword>
                            <NewUsername xsi:type='xsd:string' xmlns:xsi='http://www.w3.org/1999/XMLSchema-instance'>${username}</NewUsername>
                        </Authenticate>
                    </SOAP-ENV:Body>
                </SOAP-ENV:Envelope>";
            
            var action = "urn:NETGEAR-ROUTER:service:ParentalControl:1#Authenticate";
            var response = Request(action, message);

            return response;
        }

        public static IEnumerable<TcpClient> GetClients(string sessionId)
        {
            var message = $@"
                <SOAP-ENV:Envelope xmlns:SOAPSDK1='http://www.w3.org/2001/XMLSchema' xmlns:SOAPSDK2='http://www.w3.org/2001/XMLSchema-instance' xmlns:SOAPSDK3='http://schemas.xmlsoap.org/soap/encoding/' xmlns:SOAP-ENV='http://schemas.xmlsoap.org/soap/envelope/'>
                    <SOAP-ENV:Header>
                        <SessionID>${sessionId}</SessionID>
                    </SOAP-ENV:Header>
                    <SOAP-ENV:Body>
                        <M1:GetAttachDevice xmlns:M1='urn:NETGEAR-ROUTER:service:DeviceInfo:1'></M1:GetAttachDevice>
                    </SOAP-ENV:Body>
                </SOAP-ENV:Envelope>";

            var action = "urn:NETGEAR-ROUTER:service:DeviceInfo:1#GetAttachDevice";
            var response = Request(action, message);

            var document = XDocument.Parse(response);

            return from node in document.Descendants("NewAttachDevice")
                   from lines in node.Value.Split('@').Skip(1)
                   let fields = lines.Split(';')
                   select new TcpClient
                   {
                       IP = fields[1],
                       Name = fields[2],
                       Mac = fields[3],
                       Type = fields[4],
                       Strength = fields[5]
                   };
        }

        private static string Request(string action, string message)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("SOAPAction", action);
                client.Headers.Add("User-Agent", "SOAP Toolkit 3.0");

                var encoding = Encoding.UTF8;
                var output = client.UploadData("http://routerlogin.net:5000/soap/server_sa/", "POST", encoding.GetBytes(message));

                return encoding.GetString(output);
            }
        }
    }
}
