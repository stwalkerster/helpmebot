// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Welcomer.cs" company="Helpmebot Development Team">
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
//   Controls the newbie welcomer
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System.Collections.Generic;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Attributes;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Repositories.Interfaces;

    using NHibernate.Criterion;
    using NHibernate.Linq;

    /// <summary>
    /// Controls the newbie welcomer
    /// </summary>
    [CommandInvocation("welcomer")]
    [CommandFlag(Helpmebot.Model.Flag.LegacyAdvanced)]
    public class Welcomer : GenericCommand
    {
        /// <summary>
        /// The repository.
        /// </summary>
        private readonly IWelcomeUserRepository repository;

        /// <summary>
        /// Initialises a new instance of the <see cref="Welcomer"/> class.
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
        /// <param name="welcomeUserRepository">
        /// The welcome User Repository.
        /// </param>
        public Welcomer(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper, IWelcomeUserRepository welcomeUserRepository)
            : base(source, channel, args, commandServiceHelper)
        {
            this.repository = welcomeUserRepository;
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var response = new CommandResponseHandler();

            var messageService = this.CommandServiceHelper.MessageService;
            if (this.Arguments.Length == 0)
            {
                response.Respond(messageService.NotEnoughParameters(this.Channel, "Welcomer", 1, 0));
                return response;
            }

            List<string> argumentsList = this.Arguments.ToList();
            var mode = argumentsList.PopFromFront();

            switch (mode.ToLower())
            {
                case "enable":
                case "disable":
                    response.Respond(
                        messageService.RetrieveMessage("Welcomer-ObsoleteOption", this.Channel, new[] { mode }),
                        CommandResponseDestination.PrivateMessage);
                    break;
                case "add":
                    var welcomeUser = new WelcomeUser
                                          {
                                              Nick = ".*",
                                              User = ".*",
                                              Host = string.Join(" ", argumentsList.ToArray()),
                                              Channel = this.Channel,
                                              Exception = false
                                          };
                    this.repository.Save(welcomeUser);

                    response.Respond(messageService.Done(this.Channel));
                    break;
                case "del":
                case "Delete":
                case "remove":

                    this.Logger.Debug("Getting list of welcomeusers ready for deletion!");

                    // TODO: move to repository.
                    var criteria = Restrictions.And(
                        Restrictions.Eq("Host", string.Join(" ", argumentsList.ToArray())),
                        Restrictions.Eq("Channel", this.Channel));

                    var welcomeUsers = this.repository.Get(criteria);

                    this.Logger.Debug("Got list of WelcomeUsers, proceeding to Delete...");

                    this.repository.Delete(welcomeUsers);

                    this.Logger.Debug("All done, cleaning up and sending message to IRC");

                    response.Respond(messageService.Done(this.Channel));
                    break;
                case "list":
                    var welcomeForChannel = this.repository.GetWelcomeForChannel(this.Channel);
                    welcomeForChannel.ForEach(x => response.Respond(x.Host));
                    break;
            }
            
            return response;
        }
    }
}
