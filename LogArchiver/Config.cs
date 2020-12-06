using System.Collections.Generic;
using System.ComponentModel;
using Exiled.API.Interfaces;

namespace LogArchiver
{
    public class Config : IConfig
    {
        [Description("Whether or not this plugin is enabled.")]
        public bool IsEnabled { get; set; }
        
        [Description("Whether or not debug messages should be shown.")]
        public bool Debug { get; set; }

        [Description("How many log files can exist being archived.")]
        public int FileLimit { get; set; } = 50;

        [Description("How many archives may exist before the OLDEST ones will get automatically deleted.")]
        public int ArchiveLimit { get; set; } = 10;
        
        [Description("The location your server stores log files. This is a list, and can include multiple directories. Be sure to use full directory paths.")]
        public List<string> LogLocations { get; set; } = new List<string>
        {
            "/home/scp/scp_server/logs/",
            "/home/scp/scp_server/servers/server1/logs/",
            "/home/scp/.config/SCP Secret Laboratory/ServerLogs/",
        };
    }
}