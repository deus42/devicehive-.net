﻿using System.ServiceProcess;

namespace DeviceHive.WebSockets.API.Service
{
    internal class WindowsService : ServiceBase
    {
        private readonly HostServiceImpl _impl;

        public WindowsService(HostServiceImpl impl)
        {            
            _impl = impl;

            ServiceName = "DeviceHive.WebSockets";
        }

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);
            _impl.Start();
        }

        protected override void OnStop()
        {
            base.OnStop();
            _impl.Stop();
        }
    }
}