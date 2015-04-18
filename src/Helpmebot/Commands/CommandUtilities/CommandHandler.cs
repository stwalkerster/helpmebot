﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandHandler.cs" company="Helpmebot Development Team">
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
//   The command handler.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CommandUtilities
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;

    using Castle.Core.Internal;
    using Castle.Core.Logging;

    using Helpmebot.Commands.Interfaces;
    using Helpmebot.IRC.Events;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;
    using Helpmebot.IRC.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The command handler.
    /// </summary>
    public class CommandHandler : ICommandHandler
    {
        #region Fields

        /// <summary>
        /// The command parser.
        /// </summary>
        private readonly ICommandParser commandParser;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="commandParser">
        /// The command parser.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public CommandHandler(ICommandParser commandParser, ILogger logger)
        {
            this.commandParser = commandParser;
            this.logger = logger;
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Called on new messages received by the IRC client
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.Message.Command != "PRIVMSG")
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(this.ProcessMessageAsync, e);
        }

        /// <summary>
        /// The process message async.
        /// </summary>
        /// <param name="state">
        /// The state.
        /// </param>
        private void ProcessMessageAsync(object state)
        {
            var eventArgs = (MessageReceivedEventArgs)state;

            var parameters = eventArgs.Message.Parameters.ToList();
            IIrcClient client = eventArgs.Client;

            string message = parameters[1];

            var commandMessage = this.commandParser.ParseCommandMessage(message, client.Nickname);

            var command = this.commandParser.GetCommand(
                commandMessage,
                IrcUser.FromPrefix(eventArgs.Message.Prefix),
                parameters[0],
                client);

            if (command == null)
            {
                return;
            }

            try
            {
                IEnumerable<CommandResponse> commandResponses = command.Run();
                commandResponses.ForEach(
                    x =>
                        {
                            if (commandMessage.OverrideSilence)
                            {
                            }

                            string destination;

                            switch (x.Destination)
                            {
                                case CommandResponseDestination.ChannelDebug:
                                    destination = "##helpmebot"; // FIXME: Use configuration?
                                    break;
                                case CommandResponseDestination.PrivateMessage:
                                    destination = command.User.Nickname;
                                    break;
                                case CommandResponseDestination.Default:
                                    destination = command.CommandSource;
                                    break;
                                default:
                                    destination = command.CommandSource;
                                    this.logger.Warn("Command response has an unknown destination!");
                                    break;
                            }

                            client.Send(new PrivateMessage(destination, x.Message));
                        });
            }
            finally
            {
                this.commandParser.Release(command);
            }
        }

        #endregion
    }
}