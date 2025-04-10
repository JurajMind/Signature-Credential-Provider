﻿using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace Autentificator
{
    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        ///     This is self-installed service
        /// </summary>
        private static void Main(string[] args)
        {
            var SignatureAutheticator = new SignatureAuthenticator();
            if (Environment.UserInteractive)
            {
                var parameter = string.Concat(args);
                switch (parameter)
                {
                    case "--install":
                        ManagedInstallerClass.InstallHelper(new[] {Assembly.GetExecutingAssembly().Location});
                        break;
                    case "--uninstall":
                        ManagedInstallerClass.InstallHelper(new[]
                        {"/u", Assembly.GetExecutingAssembly().Location});
                        break;
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new SignatureAuthenticator()
                };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}