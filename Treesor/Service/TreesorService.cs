using Microsoft.Owin.Hosting;
using NLog;
using NLog.Fluent;
using System;

namespace Treesor.Service
{
    internal class TreesorService
    {
        private static readonly Logger log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args)
        {
            string baseAddress = "http://localhost:9002/";

            using (WebApp.Start<StartupConfiguration>(url: baseAddress))
            {
                log.Info().Message($"Server started at {baseAddress}").Write();

                Console.ReadLine();
            }
        }
    }
}