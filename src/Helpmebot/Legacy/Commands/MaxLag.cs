﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MaxLag.cs" company="Helpmebot Development Team">
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
//   Returns the maximum replication lag on the wiki
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Xml;

    using Helpmebot;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Model;
    using Helpmebot.Repositories.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///   Returns the maximum replication lag on the wiki
    /// </summary>
    internal class Maxlag : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Maxlag"/> class.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <param name="commandServiceHelper">
        /// The message Service.
        /// </param>
        public Maxlag(LegacyUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Gets the maximum replication lag between the Wikimedia Foundation MySQL database cluster for the base wiki of the channel.
        /// </summary>
        /// <param name="channel">The channel.</param>
        /// <returns>The maximum replication lag</returns>
        public static string GetMaxLag(string channel)
        {
            // look up site id
            string baseWiki = LegacyConfig.Singleton()["baseWiki", channel];
             
            // get api
            // FIXME: ServiceLocator
            var mediaWikiSiteRepository = ServiceLocator.Current.GetInstance<IMediaWikiSiteRepository>();
            MediaWikiSite mediaWikiSite = mediaWikiSiteRepository.GetById(int.Parse(baseWiki));

            // TODO: use Linq-to-XML
            var mlreader =
                new XmlTextReader(HttpRequest.Get(mediaWikiSite.Api + "?action=query&meta=siteinfo&siprop=dbrepllag&format=xml"));
            do
            {
                mlreader.Read();
            }
            while (mlreader.Name != "db");

            string lag = mlreader.GetAttribute("lag");

            return lag;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>The response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] messageParameters = { this.Source.Nickname, GetMaxLag(this.Channel) };
            string message = this.CommandServiceHelper.MessageService.RetrieveMessage("cmdMaxLag", this.Channel, messageParameters);
            return new CommandResponseHandler(message);
        }
    }
}
