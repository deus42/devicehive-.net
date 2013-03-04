﻿using System.Collections.Generic;

namespace DeviceHive.WebSockets.Host
{
    internal class ApplicationCollection
    {
        private readonly object _lock = new object();

        private readonly Dictionary<string, Application> _applicationsByHost = new Dictionary<string, Application>();


        public void Add(Application app)
        {
            lock (_lock)
                _applicationsByHost.Add(app.Host.ToLower(), app);
        }

        public bool Remove(string host)
        {
            lock (_lock)
                return _applicationsByHost.Remove(host);
        }

        public Application GetApplicationByHost(string host)
        {
            lock (_lock)
            {
                Application app;
                return _applicationsByHost.TryGetValue(host.ToLower(), out app) ? app : null;
            }
        }

        public IEnumerable<Application> GetAllApplications()
        {
            lock (_lock)
                return _applicationsByHost.Values;
        }
    }
}