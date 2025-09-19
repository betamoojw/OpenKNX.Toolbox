using OpenKNX.Toolbox.Lib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace OpenKNX.Toolbox.Models
{
    public class ReleaseModel
    {
        public string Name { get; set; } = string.Empty;
        public DateTime PublishedAt { get; set; }
        public string FileUrl { get; set; } = string.Empty;
        public bool IsPrerelease { get; set; } = false;
        public SemanticVersion? Version { get; set; } = null;
        public string VersionString { get; set; } = string.Empty;
        public bool IsLocalAvailable { get; set; } = false;

        public ReleaseModel(AppRelease release)
        {
            Name = release.Name;
            PublishedAt = release.PublishedAt;
            FileUrl = release.FileUrl;
            IsPrerelease = release.IsPrerelease;
            Version = release.Version;
            VersionString = Version?.ToString() ?? "Unbekannt";
        }
    }
}
