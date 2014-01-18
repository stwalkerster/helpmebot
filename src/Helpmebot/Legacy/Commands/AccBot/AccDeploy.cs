﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AccDeploy.cs" company="Helpmebot Development Team">
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
//   Defines the Accdeploy type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.IO;
    using System.Web;

    using Helpmebot;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    using HttpRequest = Helpmebot.HttpRequest;

    /// <summary>
    /// The deploy ACC command.
    /// </summary>
    internal class Accdeploy : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Accdeploy"/> class.
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
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        public Accdeploy(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string[] args = this.Arguments;

            Helpmebot6.irc.IrcPrivmsg(this.Channel, this.MessageService.RetrieveMessage("DeployInProgress", this.Channel, null));

            string revision;

            bool showUrl = false;

            if (args[0].ToLower() == "@url")
            {
                showUrl = true;
                GlobalFunctions.popFromFront(ref args);
            }

            if (args.Length > 0 && args[0] != string.Empty)
            {
                revision = string.Join(" ", args);
            }
            else
            {
                throw new ArgumentException();
            }
            
            string apiDeployPassword = LegacyConfig.Singleton()["accDeployPassword"];

            string key = this.EncodeMD5(this.EncodeMD5(revision) + apiDeployPassword);

            revision = HttpUtility.UrlEncode(revision);

            string requestUri = "http://accounts-dev.wmflabs.org/deploy/deploy.php?r=" + revision + "&k=" + key;

            var r = new StreamReader(HttpRequest.get(requestUri, 1000 * 30 /* 30 sec timeout */));

            var crh = new CommandResponseHandler();
            if (showUrl)
            {
                crh.respond(requestUri, CommandResponseDestination.PrivateMessage);
            }

            foreach (var x in r.ReadToEnd().Split('\n', '\r'))
            {
                crh.respond(x);
            }

            return crh;
        }

        #endregion

        /// <summary>
        /// The md 5.
        /// </summary>
        /// <param name="s">
        /// The s.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        private string EncodeMD5(string s)
        {
            return
                BitConverter.ToString(
                    System.Security.Cryptography.MD5.Create().ComputeHash(new System.Text.UTF8Encoding().GetBytes(s)))
                    .Replace("-", string.Empty)
                    .ToLower();
        }
    }
}
