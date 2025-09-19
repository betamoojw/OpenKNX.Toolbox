using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Models
{
    public class DeviceModel
    {
        public string Name { get; set; } = string.Empty;
        public string FirmwareName { get; set; } = string.Empty;
        public string FirmwareVersionString { get; set; } = string.Empty;
        public SemanticVersion? FirmwareVersion { get; set; }
        public string IP { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string AppId { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public bool Configured { get; set; } = false;
        public int Port { get; set; } = 0;
        public string SerialNumber { get; set; } = string.Empty;
        public SemanticVersion? UpdateVersion { get; set; } = null;
        public string UpdateVersionString { get; set; } = string.Empty;
        public bool IsIP { get; set; } = false;
    }
}
