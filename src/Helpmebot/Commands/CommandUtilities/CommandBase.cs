﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CommandBase.cs" company="Helpmebot Development Team">
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
//   Defines the CommandBase type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CommandUtilities
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;

    using Castle.Core.Internal;
    using Castle.Core.Logging;

    using Helpmebot.Attributes;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Exceptions;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The command base.
    /// </summary>
    public abstract class CommandBase : ICommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="CommandBase"/> class.
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
        protected CommandBase(
            string commandSource, 
            IUser user, 
            IEnumerable<string> arguments, 
            IUserFlagService userFlagService, 
            ILogger logger, 
            IMessageService messageService, 
            IAccessLogService accessLogService, 
            IIrcClient client, 
            ISession databaseSession)
        {
            this.DatabaseSession = databaseSession;
            this.Client = client;
            this.AccessLogService = accessLogService;
            this.MessageService = messageService;
            this.Logger = logger;
            this.CommandSource = commandSource;
            this.User = user;
            this.Arguments = arguments;
            this.UserFlagService = userFlagService;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the arguments to the command.
        /// </summary>
        public IEnumerable<string> Arguments { get; private set; }

        /// <summary>
        /// Gets the client.
        /// </summary>
        public IIrcClient Client { get; private set; }

        /// <summary>
        /// Gets the source (where the command was triggered).
        /// </summary>
        public string CommandSource { get; private set; }

        /// <summary>
        /// Gets the database session.
        /// </summary>
        public ISession DatabaseSession { get; private set; }

        /// <summary>
        /// Gets the flag required to execute.
        /// </summary>
        public string Flag
        {
            get
            {
                return this.GetType().GetAttribute<CommandFlagAttribute>().Flag;
            }
        }

        /// <summary>
        /// Gets or sets the redirection target.
        /// </summary>
        public IEnumerable<string> RedirectionTarget { get; set; }

        /// <summary>
        /// Gets the user who triggered the command.
        /// </summary>
        public IUser User { get; private set; }

        /// <summary>
        /// Gets or sets the original arguments.
        /// </summary>
        public IEnumerable<string> OriginalArguments { get; set; }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the access log service.
        /// </summary>
        protected IAccessLogService AccessLogService { get; private set; }

        /// <summary>
        /// Gets the logger.
        /// </summary>
        protected ILogger Logger { get; private set; }

        /// <summary>
        /// Gets the message service.
        /// </summary>
        protected IMessageService MessageService { get; private set; }

        /// <summary>
        /// Gets or sets the user flag service.
        /// </summary>
        protected IUserFlagService UserFlagService { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Returns true if the command can be executed.
        /// </summary>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public virtual bool CanExecute()
        {
            return this.UserFlagService.GetFlagsForUser(this.User).Contains(this.Flag);
        }

        /// <summary>
        /// The run.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        public IEnumerable<CommandResponse> Run()
        {
            // TODO: check what should be in a transaction, and what should not - autonomous transactions?
            this.DatabaseSession.FlushMode = FlushMode.Commit;
            ITransaction transaction = this.DatabaseSession.BeginTransaction(IsolationLevel.ReadCommitted);

            if (this.CanExecute())
            {
                this.AccessLogService.Success(this.User, this.GetType(), this.Arguments, this.CommandSource);

                try
                {
                    var commandResponses = this.Execute() ?? new List<CommandResponse>();
                    var completedResponses = this.OnCompleted() ?? new List<CommandResponse>();

                    if (transaction.IsActive)
                    {
                        transaction.Commit();
                    }

                    return commandResponses.Concat(completedResponses);
                }
                catch (CommandExecutionException e)
                {
                    this.Logger.Info("Command encountered an issue during execution.", e);

                    if (transaction.IsActive)
                    {
                        transaction.Rollback();
                    }

                    return new List<CommandResponse>
                               {
                                   new CommandResponse
                                       {
                                           Destination =
                                               CommandResponseDestination.Default, 
                                           Message = e.Message
                                       }
                               };
                }
                catch (ADOException e)
                {
                    this.Logger.Error("Command encountered an issue during execution.", e);

                    if (transaction.IsActive)
                    {
                        transaction.Rollback();
                    }

                    return new List<CommandResponse>
                               {
                                   new CommandResponse
                                       {
                                           Destination =
                                               CommandResponseDestination.Default, 
                                           Message = e.Message
                                       }
                               };
                }
            }

            this.AccessLogService.Failure(this.User, this.GetType(), this.Arguments, this.CommandSource);

            this.Logger.InfoFormat("Access denied for user {0}", this.User);

            IEnumerable<CommandResponse> accessDeniedResponses = this.OnAccessDenied() ?? new List<CommandResponse>();

            if (transaction.IsActive)
            {
                transaction.Commit();
            }

            return accessDeniedResponses;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The execute.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected abstract IEnumerable<CommandResponse> Execute();

        /// <summary>
        /// The help.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandResponse> Help()
        {
            var response = new CommandResponse
                               {
                                   Destination = CommandResponseDestination.PrivateMessage, 
                                   Message =
                                       this.MessageService.RetrieveMessage(
                                           Messages.NoHelpAvailable, 
                                           this.CommandSource, 
                                           null)
                               };

            return response.ToEnumerable();
        }

        /// <summary>
        /// The on access denied.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandResponse> OnAccessDenied()
        {
            var response = new CommandResponse
                               {
                                   Destination = CommandResponseDestination.PrivateMessage, 
                                   Message =
                                       this.MessageService.RetrieveMessage(
                                           Messages.OnAccessDenied, 
                                           this.CommandSource, 
                                           null)
                               };

            return response.ToEnumerable();
        }

        /// <summary>
        /// The on completed.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        protected virtual IEnumerable<CommandResponse> OnCompleted()
        {
            return null;
        }

        #endregion
    }
}