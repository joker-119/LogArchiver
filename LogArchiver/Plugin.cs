using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Exiled.API.Features;
using MapEvents = Exiled.Events.Handlers.Map;
using PlayerEvents = Exiled.Events.Handlers.Player;
using Scp049Events = Exiled.Events.Handlers.Scp049;
using Scp079Events = Exiled.Events.Handlers.Scp079;
using Scp096Events = Exiled.Events.Handlers.Scp096;
using Scp106Events = Exiled.Events.Handlers.Scp106;
using Scp914Events = Exiled.Events.Handlers.Scp914;
using ServerEvents = Exiled.Events.Handlers.Server;
using WarheadEvents = Exiled.Events.Handlers.Warhead;

namespace LogArchiver
{
    public class Plugin : Plugin<Config>
    {
        public override string Author { get; } = "Galaxy119";
        public override string Name { get; } = "LogArchiver";
        public override string Prefix { get; } = "LogArchiver";
        public override Version Version { get; } = new Version(1, 0, 0);
        public override Version RequiredExiledVersion { get; } = new Version(2, 1, 18);

        public Methods Methods { get; private set; }
        public EventHandlers EventHandlers { get; private set; }

        internal Dictionary<string, List<Tuple<string, DateTime>>> ExistingArchives;

        public override void OnEnabled()
        {
            EventHandlers = new EventHandlers(this);
            Methods = new Methods(this);

            Log.Debug("Scanning for existing archives..", Config.Debug);
            ExistingArchives = Methods.FindExistingArchives();

            Log.Info($"Checking log files for given directories. If this is the first time you've used this plugin, this may take a few minutes.");
            Methods.CheckLogFiles();

            base.OnEnabled();
        }

        public override void OnDisabled()
        {
            EventHandlers = null;
            Methods = null;

            base.OnDisabled();
        }
    }
}