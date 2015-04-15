﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FlagGroupCommand.cs" company="Helpmebot Development Team">
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
//   The flag group command.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.AccessControl
{
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.CommandUtilities;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The flag group command.
    /// </summary>
    [CommandFlag(Model.Flag.Access)]
    [CommandInvocation("flaggroup")]
    public class FlagGroupCommand : CommandBase
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="FlagGroupCommand"/> class.
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
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        public FlagGroupCommand(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            IUserFlagService userFlagService, 
            ILogger logger, 
            IMessageService messageService, 
            IAccessLogService accessLogService, 
            IIrcClient client, 
            ISession databaseSession)
            : base(
                commandSource, 
                user, 
                arguments, 
                userFlagService, 
                logger, 
                messageService, 
                accessLogService, 
                client, 
                databaseSession)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The parse flags.
        /// </summary>
        /// <param name="flags">
        /// The flags.
        /// </param>
        /// <param name="flagGroup">
        /// The group.
        /// </param>
        public static void ParseFlags(string flags, FlagGroup flagGroup)
        {
            bool? adding = null;
            bool replaced = false;

            if (flagGroup.Flags == null)
            {
                flagGroup.Flags = new List<FlagGroupAssoc>();
            }

            foreach (var f in flags)
            {
                var flag = f.ToString(CultureInfo.InvariantCulture);

                if (flag == "+")
                {
                    adding = true;
                    continue;
                }

                if (flag == "-")
                {
                    adding = false;
                    continue;
                }

                if (adding == null)
                {
                    if (!replaced)
                    {
                        flagGroup.Flags.Clear();
                        replaced = true;
                    }

                    flagGroup.Flags.Add(new FlagGroupAssoc { Flag = flag });
                    continue;
                }

                if (adding.GetValueOrDefault(false))
                {
                    if (!flagGroup.Flags.Select(x => x.Flag).Contains(flag))
                    {
                        flagGroup.Flags.Add(new FlagGroupAssoc { Flag = flag });
                    }
                }
                else
                {
                    if (flagGroup.Flags.Select(x => x.Flag).Contains(flag))
                    {
                        flagGroup.Flags.Remove(flagGroup.Flags.First(x => x.Flag == flag));
                    }
                }
            }
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
            if (!this.Arguments.Any())
            {
                return this.ListGroups();
            }

            if (this.Arguments.Count() < 2)
            {
                throw new ArgumentCountException(2, this.Arguments.Count());
            }

            var mode = this.Arguments.ElementAt(0).ToLowerInvariant();
            var group = this.Arguments.ElementAt(1).ToLowerInvariant();

            if (mode == "set" || mode == "setup")
            {
                return this.SetupGroup(group);
            }

            if (mode == "del" || mode == "delete")
            {
                return this.DeleteGroup(group);
            }

            throw new CommandInvocationException();
        }

        /// <summary>
        /// The delete group.
        /// </summary>
        /// <param name="flagGroup">
        /// The flag group.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> DeleteGroup(string flagGroup)
        {
            var responses = new List<CommandResponse>();

            var existing = this.DatabaseSession.QueryOver<FlagGroup>().Where(x => x.Name == flagGroup).List();

            existing.Apply(item => responses.Add(new CommandResponse { Message = item.ToString() }));
            existing.Apply(this.DatabaseSession.Delete);

            return responses;
        }

        /// <summary>
        /// The list groups.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> ListGroups()
        {
            return this.DatabaseSession.QueryOver<FlagGroup>()
                .List()
                .Aggregate(
                    new List<CommandResponse>(), 
                    (list, x) =>
                        {
                            list.Add(new CommandResponse { Message = x.ToString() });
                            return list;
                        });
        }

        /// <summary>
        /// The setup group.
        /// </summary>
        /// <param name="flagGroup">
        /// The flag group.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        private IEnumerable<CommandResponse> SetupGroup(string flagGroup)
        {
            if (this.Arguments.Count() < 3)
            {
                throw new ArgumentCountException(3, this.Arguments.Count());
            }

            var flags = this.Arguments.ElementAt(2).ToUpperInvariant();

            var group = this.DatabaseSession.QueryOver<FlagGroup>().Where(x => x.Name == flagGroup).SingleOrDefault()
                        ?? new FlagGroup { Flags = new List<FlagGroupAssoc>() };

            group.Name = flagGroup;

            ParseFlags(flags, group);

            foreach (var flag in group.Flags)
            {
                flag.FlagGroup = group;
            }

            this.DatabaseSession.SaveOrUpdate(group);
            return new CommandResponse { Message = group.ToString() }.ToEnumerable();
        }

        #endregion
    }
}