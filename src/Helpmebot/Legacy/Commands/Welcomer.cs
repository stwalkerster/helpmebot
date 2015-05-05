﻿// --------------------------------------------------------------------------------------------------------------------
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
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Attributes;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    using NHibernate.Linq;

    /// <summary>
    /// Controls the newbie welcomer
    /// </summary>
    [CommandInvocation("welcomer")]
    [CommandFlag(Helpmebot.Model.Flag.Protected)]
    public class Welcomer : GenericCommand
    {
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
        public Welcomer(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            var response = new CommandResponseHandler();

            if (this.Arguments.Length == 0)
            {
                response.Respond(this.CommandServiceHelper.MessageService.NotEnoughParameters(this.Channel, "Welcomer", 1, 0));
                return response;
            }
            
            List<string> argumentsList = this.Arguments.ToList();
            var mode = argumentsList.PopFromFront();

            this.DatabaseSession.BeginTransaction(IsolationLevel.RepeatableRead);

            switch (mode.ToLower())
            {
                case "enable":
                case "disable":
                    response.Respond(
                        this.CommandServiceHelper.MessageService.RetrieveMessage("Welcomer-ObsoleteOption", this.Channel, new[] { mode }),
                        CommandResponseDestination.PrivateMessage);
                    break;
                case "add":
                    this.AddMode(argumentsList, response);
                    break;
                case "del":
                case "delete":
                case "remove":
                    this.DeleteMode(argumentsList, response);
                    break;
                case "list":
                    this.ListMode(response);
                    break;
            }
            
            return response;
        }

        /// <summary>
        /// The delete mode.
        /// </summary>
        /// <param name="argumentsList">
        /// The arguments list.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        private void DeleteMode(List<string> argumentsList, CommandResponseHandler response)
        {
            try
            {
                this.Logger.Debug("Getting list of welcomeusers ready for deletion!");

                var exception = false;

                if (argumentsList[0] == "@ignore")
                {
                    exception = true;
                    argumentsList.RemoveAt(0);
                }

                var implode = argumentsList.Implode();

                var welcomeUsers =
                    this.DatabaseSession.QueryOver<WelcomeUser>()
                        .Where(x => x.Exception == exception && x.Host == implode && x.Channel == this.Channel)
                        .List();

                this.Logger.Debug("Got list of WelcomeUsers, proceeding to Delete...");

                welcomeUsers.ForEach(this.DatabaseSession.Delete);

                this.Logger.Debug("All done, cleaning up and sending message to IRC");

                response.Respond(this.CommandServiceHelper.MessageService.Done(this.Channel));

                this.DatabaseSession.Transaction.Commit();
            }
            catch (Exception e)
            {
                this.Logger.Error("Error occurred during addition of welcome mask.", e);
                response.Respond(e.Message);
                this.DatabaseSession.Transaction.Rollback();
            }
        }

        /// <summary>
        /// The add mode.
        /// </summary>
        /// <param name="argumentsList">
        /// The arguments list.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        private void AddMode(List<string> argumentsList, CommandResponseHandler response)
        {
            try
            {
                var exception = false;

                if (argumentsList[0] == "@ignore")
                {
                    exception = true;
                    argumentsList.RemoveAt(0);
                }

                var welcomeUser = new WelcomeUser
                                      {
                                          Nick = ".*",
                                          User = ".*",
                                          Host = argumentsList.Implode(),
                                          Channel = this.Channel,
                                          Exception = exception
                                      };

                this.DatabaseSession.Save(welcomeUser);

                response.Respond(this.CommandServiceHelper.MessageService.Done(this.Channel));

                this.DatabaseSession.Transaction.Commit();
            }
            catch (Exception e)
            {
                this.Logger.Error("Error occurred during addition of welcome mask.", e);
                response.Respond(e.Message);

                this.DatabaseSession.Transaction.Rollback();
            }
        }

        /// <summary>
        /// The list mode.
        /// </summary>
        /// <param name="response">
        /// The response.
        /// </param>
        private void ListMode(CommandResponseHandler response)
        {
            var welcomeForChannel =
                this.DatabaseSession.QueryOver<WelcomeUser>().Where(x => x.Channel == this.Channel).List();
            welcomeForChannel.ForEach(x => response.Respond(x.ToString()));
        }
    }
}
