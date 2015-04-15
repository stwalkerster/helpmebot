﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TestCommand.cs" company="Helpmebot Development Team">
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
//   The test command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands
{
    using System.Collections.Generic;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The test command.
    /// </summary>
    [CommandFlag(Model.Flag.Debug)]
    [CommandInvocation("testcommand")]
    public class TestCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="TestCommand"/> class. 
        /// </summary>
        /// <param name="commandSource">
        /// The command source.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="userFlagService">
        /// The user Flag Service.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="messageService">
        /// The message Service.
        /// </param>
        /// <param name="accessLogService">
        /// The access Log Service.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <param name="session">
        /// The session.
        /// </param>
        public TestCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            IUserFlagService userFlagService, 
            ILogger logger, 
            IMessageService messageService, 
            IAccessLogService accessLogService, 
            IIrcClient client, 
            ISession session)
            : base(
                commandSource, 
                user, 
                arguments, 
                userFlagService, 
                logger, 
                messageService, 
                accessLogService, 
                client, 
                session)
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected override IEnumerable<CommandResponse> Execute()
        {
            return new List<CommandResponse>
                       {
                           new CommandResponse
                               {
                                   Destination = CommandResponseDestination.Default, 
                                   Message = "ohai. test command 1 here."
                               }
                       };
        }

        #endregion
    }
}