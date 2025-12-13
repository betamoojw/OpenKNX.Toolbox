using Kaenx.Konnect.Addresses;
using Kaenx.Konnect.Telegram.IP.DIB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Models
{
    public class DeviceConnectionModel
    {
        public IPEndPoint RemoteEndpoint { get; set; }
        public UnicastAddress RemoteAddress { get; set; }
        public int Version { get; set; } = 0;
        public string FriendlyName { get; set; }

        public bool IsRoutingDevice { get; set; } = false;

        public DeviceConnectionModel(IPEndPoint remoteEndpoint, UnicastAddress unicastAddress, int version, string friendlyName, bool isRouting = false)
        {
            RemoteEndpoint = remoteEndpoint;
            RemoteAddress = unicastAddress;
            Version = version;
            IsRoutingDevice = isRouting;
            FriendlyName = friendlyName;
        }

        public override string ToString()
        {
            return $"{(IsRoutingDevice ? "Routing  " : "Tunneling")} v{Version} -> { RemoteEndpoint,-20} ({RemoteAddress,-9}) [{FriendlyName}]";
        }
    }
}
