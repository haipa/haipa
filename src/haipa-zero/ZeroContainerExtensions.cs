﻿using Haipa.IdentityDb;
using Haipa.IdentityDb.Sqlite;
using Haipa.Modules.Api;
using Haipa.Modules.Controller;
using Haipa.Modules.Hosting;
using Haipa.Modules.Identity;
using Haipa.Modules.VmHostAgent;
using Haipa.Rebus;
using Haipa.StateDb;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Rebus.Persistence.InMem;
using Rebus.Transport.InMem;
using SimpleInjector;
using IdentityDbContext = Haipa.IdentityDb.IdentityDbContext;

namespace Haipa.Runtime.Zero
{
    internal static class ZeroContainerExtensions
    {
        public static void Bootstrap(this Container container, string[] args)
        {
            container.HostModule<ApiModule>();
            container.HostModule<IdentityModule>();
            container.HostModule<VmHostAgentModule>();
            container.HostModule<ControllerModule>();

            container
                .HostAspNetCore((path) =>
                {
                    return WebHost.CreateDefaultBuilder(args)
                        .UseHttpSys()
                        .UseUrls($"http://localhost:62189/{path}")
                        .ConfigureLogging(lc => lc.SetMinimumLevel(LogLevel.Debug));
                });

            container
                .UseInMemoryBus()
                .UseInMemoryDb();
        }

        public static Container UseInMemoryBus(this Container container)
        {
            container.RegisterInstance(new InMemNetwork(true));
            container.RegisterInstance(new InMemorySubscriberStore());
            container.Register<IRebusTransportConfigurer, InMemoryTransportConfigurer>();
            container.Register<IRebusSagasConfigurer, InMemorySagasConfigurer>();
            container.Register<IRebusSubscriptionConfigurer, InMemorySubscriptionConfigurer>();
            container.Register<IRebusTimeoutConfigurer, InMemoryTimeoutConfigurer>(); return container;
        }

        public static Container UseInMemoryDb(this Container container)
        {
            container.RegisterInstance(new InMemoryDatabaseRoot());
            container.Register<StateDb.IDbContextConfigurer<StateStoreContext>, InMemoryStateStoreContextConfigurer>();
            container.Register<IdentityDb.IDbContextConfigurer<IdentityDbContext>, SqliteDbContextConfigurer<IdentityDbContext>>();

            return container;
        }
    }
}
