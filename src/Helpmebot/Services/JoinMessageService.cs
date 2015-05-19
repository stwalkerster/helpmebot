// --------------------------------------------------------------------------------------------------------------------
// <copyright file="JoinMessageService.cs" company="Helpmebot Development Team">
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
//   Defines the JoinMessageService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot.IRC.Events;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The join message service.
    /// </summary>
    public class JoinMessageService : IJoinMessageService
    {
        #region Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The message service.
        /// </summary>
        private readonly IMessageService messageService;

        /// <summary>
        /// The session.
        /// </summary>
        private readonly ISession session;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="JoinMessageService"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public JoinMessageService(ILogger logger, IMessageService messageService, ISession session)
        {
            this.logger = logger;
            this.messageService = messageService;
            this.session = session;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get exceptions.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IList{WelcomeUser}"/>.
        /// </returns>
        public virtual IList<WelcomeUser> GetExceptions(string channel)
        {
            var exceptions =
                this.session.QueryOver<WelcomeUser>().Where(x => x.Channel == channel && x.Exception).List();
            return exceptions;
        }

        /// <summary>
        /// The get welcome users.
        /// </summary>
        /// <param name="channel">
        /// The channel.
        /// </param>
        /// <returns>
        /// The <see cref="IList{WelcomeUser}"/>.
        /// </returns>
        public virtual IList<WelcomeUser> GetWelcomeUsers(string channel)
        {
            var users =
                this.session.QueryOver<WelcomeUser>().Where(x => x.Channel == channel && x.Exception == false).List();
            return users;
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
        public void WelcomeNewbieOnJoinEvent(object sender, JoinEventArgs e)
        {
            try
            {
                // status
                bool match = false;

                this.logger.DebugFormat("Searching for welcome matches for {0} in {1}...", e.User, e.Channel);

                var users = this.GetWelcomeUsers(e.Channel);

                if (users.Any())
                {
                    foreach (var welcomeUser in users)
                    {
                        Match nick = new Regex(welcomeUser.Nick).Match(e.User.Nickname);
                        Match user = new Regex(welcomeUser.User).Match(e.User.Username);
                        Match host = new Regex(welcomeUser.Host).Match(e.User.Hostname);

                        if (nick.Success && user.Success && host.Success)
                        {
                            this.logger.DebugFormat(
                                "Found a match for {0} in {1} with {2}", 
                                e.User, 
                                e.Channel, 
                                welcomeUser);
                            match = true;
                            break;
                        }
                    }
                }

                if (!match)
                {
                    this.logger.InfoFormat("No welcome matches found for {0} in {1}.", e.User, e.Channel);
                    return;
                }

                this.logger.DebugFormat("Searching for exception matches for {0} in {1}...", e.User, e.Channel);

                var exceptions = this.GetExceptions(e.Channel);

                if (exceptions.Any())
                {
                    foreach (var welcomeUser in exceptions)
                    {
                        Match nick = new Regex(welcomeUser.Nick).Match(e.User.Nickname);
                        Match user = new Regex(welcomeUser.User).Match(e.User.Username);
                        Match host = new Regex(welcomeUser.Host).Match(e.User.Hostname);

                        if (nick.Success && user.Success && host.Success)
                        {
                            this.logger.DebugFormat(
                                "Found an exception match for {0} in {1} with {2}", 
                                e.User, 
                                e.Channel, 
                                welcomeUser);

                            return;
                        }
                    }
                }

                this.logger.InfoFormat("Welcoming {0} into {1}...", e.User, e.Channel);

                var welcomeMessage = this.messageService.RetrieveMessage(
                    "WelcomeMessage", 
                    e.Channel, 
                    new[] { e.User.Nickname, e.Channel });

                e.Client.SendMessage(e.Channel, welcomeMessage);
            }
            catch (Exception exception)
            {
                this.logger.Error("Exception encountered in WelcomeNewbieOnJoinEvent", exception);
            }
        }

        #endregion
    }
}