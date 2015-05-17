﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Uncurl.cs" company="Helpmebot Development Team">
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
//   Uncurl command to set the bot's hedgehog status to false.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace helpmebot6.Commands
{
    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities.Response;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// Uncurl command to set the bot's hedgehog status to false.
    /// </summary>
    /// <remarks>This is a fun command, but because FunCommand checks hedgehog is false, that base class can't be used.</remarks>
    [CommandInvocation("uncurl")]
    [CommandFlag(Helpmebot.Model.Flag.LegacySuperuser)]
    public class Uncurl : GenericCommand
    {
        /// <summary>
        /// Initialises a new instance of the <see cref="Uncurl"/> class.
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
        public Uncurl(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        /// <summary>
        /// The execute command.
        /// </summary>
        /// <returns>
        /// The <see cref="CommandResponseHandler"/>.
        /// </returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            LegacyConfig.Singleton()["hedgehog", this.Channel] = "false";
            return new CommandResponseHandler(this.CommandServiceHelper.MessageService.RetrieveMessage(Messages.Done, this.Channel, null));
        }
    }
}
