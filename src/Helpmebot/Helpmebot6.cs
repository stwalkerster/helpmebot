// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Helpmebot6.cs" company="Helpmebot Development Team">
//   Helpmebot is free software: you can redistribute it and/or modify
//   it under the terms of the GNU General Public License as published by
//   the Free Software Foundation, either version 3 of the License, or
//   (at your option) any later version.
//   
//   Helpmebot is distributed in the hope that it will be useful,
//   but WITHOUT ANY WARRANTY; without even the implied warranty of
//   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//   GNU General Public License for more details.
//   
//   You should have received a copy of the GNU General Public License
//   along with Helpmebot.  If not, see http://www.gnu.org/licenses/ .
// </copyright>
// <summary>
//   Helpmebot main class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot
{
    using System;

    using Castle.Core.Logging;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using Castle.Windsor.Installer;

    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Configuration;
    using Helpmebot.IRC;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Database;
    using Helpmebot.Model;
    using Helpmebot.Monitoring;
    using Helpmebot.Repositories.Interfaces;
    using Helpmebot.Services.Interfaces;
    using Helpmebot.Startup;
    using Helpmebot.Threading;

    using helpmebot6.Commands;

    using Microsoft.Practices.ServiceLocation;

#if PERFCOUNTER
    using System.Security;
    using Castle.MicroKernel.Releasers;
    using Castle.Windsor.Diagnostics;
#endif

    /// <summary>
    /// Helpmebot main class
    /// </summary>
    public class Helpmebot6
    {
        /// <summary>
        /// The start-up time.
        /// </summary>
        public static readonly DateTime StartupTime = DateTime.Now;
        
        /// <summary>
        /// The new IRC client.
        /// </summary>
        private static IrcClient newIrc;

        /// <summary>
        /// The container.
        /// </summary>
        private static IWindsorContainer container;

        /// <summary>
        /// The DB access layer.
        /// </summary>
        private static ILegacyDatabase dbal;

        /// <summary>
        /// The join message service.
        /// </summary>
        /// <para>
        /// This is the replacement for the newbiewelcomer
        /// </para>
        private static IJoinMessageService joinMessageService;

        /// <summary>
        /// The command handler.
        /// </summary>
        private static ICommandHandler commandHandler;

        /// <summary>
        /// Gets or sets the Castle.Windsor Logger
        /// </summary>
        public static ILogger Log { get; set; }

        /// <summary>
        /// The stop.
        /// </summary>
        public static void Stop()
        {
            ThreadList.GetInstance().Stop();
            container.Dispose();
        }

        /// <summary>
        /// The main.
        /// </summary>
        private static void Main()
        {
            BootstrapContainer();

            Log = ServiceLocator.Current.GetInstance<ILogger>();
            Log.Info("Initialising Helpmebot...");

            InitialiseBot();
        }

        /// <summary>
        /// The bootstrap container.
        /// </summary>
        private static void BootstrapContainer()
        {
            container = new WindsorContainer();

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));

            container.Install(FromAssembly.This(new WindsorBootstrap()));

#if PERFCOUNTER
            // setup castle windsor performance counters if possible
            try
            {
                var diagnostic = LifecycledComponentsReleasePolicy.GetTrackedComponentsDiagnostic(container.Kernel);
                var counter =
                    LifecycledComponentsReleasePolicy.GetTrackedComponentsPerformanceCounter(
                        new PerformanceMetricsFactory());
                container.Kernel.ReleasePolicy = new LifecycledComponentsReleasePolicy(diagnostic, counter);
            }
            catch (SecurityException ex)
            {
                ServiceLocator.Current.GetInstance<ILogger>().Warn("Unable to set up performance counter.");
            }
#endif
        }

        /// <summary>
        /// The initialise bot.
        /// </summary>
        private static void InitialiseBot()
        {
            dbal = container.Resolve<ILegacyDatabase>();

            if (!dbal.Connect())
            {
                // can't Connect to database, DIE
                return;
            }

            LegacyConfig.Singleton();

            var configurationHelper = container.Resolve<IConfigurationHelper>();

            INetworkClient networkClient;
            if (configurationHelper.IrcConfiguration.Ssl)
            {
                networkClient = new SslNetworkClient(
                    configurationHelper.IrcConfiguration.Hostname,
                    configurationHelper.IrcConfiguration.Port,
                    container.Resolve<ILogger>().CreateChildLogger("NetworkClient"));
            }
            else
            {
                networkClient = new NetworkClient(
                    configurationHelper.IrcConfiguration.Hostname,
                    configurationHelper.IrcConfiguration.Port,
                    container.Resolve<ILogger>().CreateChildLogger("NetworkClient"));
            }
            
            newIrc =
                new IrcClient(
                    networkClient,
                    container.Resolve<ILogger>().CreateChildLogger("IrcClient"),
                    configurationHelper.IrcConfiguration,
                    configurationHelper.PrivateConfiguration.IrcPassword);

            JoinChannels();
            
            // TODO: remove me!
            container.Register(Component.For<IIrcClient>().Instance(newIrc));

            joinMessageService = container.Resolve<IJoinMessageService>();
            commandHandler = container.Resolve<ICommandHandler>();
            
            SetupEvents();

            // initialise the deferred installers.
            container.Install(FromAssembly.This(new DeferredWindsorBootstrap()));

            WatcherController.Instance();
        }

        /// <summary>
        /// The setup events.
        /// </summary>
        private static void SetupEvents()
        {
            newIrc.JoinReceivedEvent += WelcomeNewbieOnJoinEvent;

            newIrc.JoinReceivedEvent += NotifyOnJoinEvent;

            newIrc.ReceivedMessage += commandHandler.OnMessageReceived;

            newIrc.InviteReceivedEvent += IrcInviteEvent;

            // FIXME: ServiceLocator - WLS
            var wls = ServiceLocator.Current.GetInstance<IWikiLinkService>();
            newIrc.ReceivedMessage += wls.ParseIncomingMessage;
        }

        /// <summary>
        /// The IRC invite event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void IrcInviteEvent(object sender, InviteEventArgs e)
        {
            // FIXME: servicelocator (commandparser)
            var commandParser = ServiceLocator.Current.GetInstance<ICommandParser>();

            commandParser.GetCommand(
                new CommandMessage { CommandName = "join", ArgumentList = e.Channel },
                e.User,
                e.Channel,
                e.Client).Run();
        }

        /// <summary>
        /// The welcome newbie on join event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void WelcomeNewbieOnJoinEvent(object sender, JoinEventArgs e)
        {
            try
            {
                joinMessageService.Welcome(e.User, e.Channel);
            }
            catch (Exception exception)
            {
                Log.Error("Exception encountered in WelcomeNewbieOnJoinEvent", exception);
            }
        }

        /// <summary>
        /// The notify on join event.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void NotifyOnJoinEvent(object sender, JoinEventArgs e)
        {
            try
            {
                // FIXME: ServiceLocator - commandparser
                var commandParser = ServiceLocator.Current.GetInstance<ICommandParser>();

                var command = commandParser.GetCommand(
                    new CommandMessage { CommandName = "notify" },
                    e.User,
                    e.Channel,
                    e.Client) as Notify;

                if (command != null)
                {
                    command.NotifyJoin(e.User, e.Channel);
                }
            }
            catch (Exception exception)
            {
                Log.Error("Exception encountered in NotifyOnJoinEvent", exception);
            }
        }

        /// <summary>
        /// The join channels.
        /// </summary>
        private static void JoinChannels()
        {
            // FIXME: ServiceLocator - channelrepo
            var channelRepository = ServiceLocator.Current.GetInstance<IChannelRepository>();
            
            foreach (var channel in channelRepository.GetEnabled())
            {
                newIrc.JoinChannel(channel.Name);
            }
        }
    }
}
