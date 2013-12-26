﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProtectedCommand.cs" company="Helpmebot Development Team">
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
//   Defines the ProtectedCommand type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using System;
    using System.Linq;

    using Helpmebot;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    /// The protected command.
    /// </summary>
    public abstract class ProtectedCommand : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="ProtectedCommand"/> class.
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
        protected ProtectedCommand(LegacyUser source, string channel, string[] args, IMessageService messageService)
            : base(source, channel, args, messageService)
        {
        }

        /// <summary>
        /// The really run command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ReallyRunCommand()
        {
            if (
                !AccessLog.instance()
                     .save(new AccessLog.AccessLogEntry(this.Source, GetType(), true, this.Channel, this.Arguments, this.AccessLevel)))
            {
                var errorResponse = new CommandResponseHandler();
                errorResponse.respond("Error adding to access log - command aborted.", CommandResponseDestination.ChannelDebug);
                string message = this.MessageService.RetrieveMessage(
                    "AccessDeniedAccessListFailure",
                    this.Channel,
                    null);
                errorResponse.respond(message, CommandResponseDestination.Default);
                return errorResponse;
            }

            Log.Info("Starting command execution...");
            CommandResponseHandler crh;

            try
            {
                crh = this.Arguments.Contains("@confirm") ? this.ExecuteCommand() : this.NotConfirmed();
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message, ex);
                crh = new CommandResponseHandler(ex.Message);
            }

            Log.Info("Command execution complete.");
            return crh;
        }

        /// <summary>
        /// The not confirmed.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected abstract CommandResponseHandler NotConfirmed();
    }
}