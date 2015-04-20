// --------------------------------------------------------------------------------------------------------------------
// <copyright file="UserFlagService.cs" company="Helpmebot Development Team">
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
//   Defines the UserFlagService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Services
{
    using System.Collections.Generic;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;
    using NHibernate.Linq;

    /// <summary>
    /// The user flag service.
    /// </summary>
    public class UserFlagService : IUserFlagService
    {
        #region Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The flag group users.
        /// </summary>
        private IList<FlagGroupUser> flagGroupUsers;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="UserFlagService"/> class.
        /// </summary>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public UserFlagService(ISession databaseSession, ILogger logger)
        {
            this.logger = logger;
            this.DatabaseSession = databaseSession;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the database session.
        /// </summary>
        public ISession DatabaseSession { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The get flag groups for user.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{FlagGroup}"/>.
        /// </returns>
        public virtual IEnumerable<FlagGroup> GetFlagGroupsForUser(IUser user)
        {
            if (this.flagGroupUsers == null)
            {
                this.flagGroupUsers = this.DatabaseSession.QueryOver<FlagGroupUser>().List();
            }

            return from flagGroupUser in this.flagGroupUsers
                   where
                       flagGroupUser.AccountRegex.Match(user.Account).Success
                       && flagGroupUser.NicknameRegex.Match(user.Nickname).Success
                       && flagGroupUser.UsernameRegex.Match(user.Username).Success
                       && flagGroupUser.HostnameRegex.Match(user.Hostname).Success
                   select flagGroupUser.FlagGroup;
        }

        /// <summary>
        /// The get flags for user.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{String}"/>.
        /// </returns>
        public IEnumerable<string> GetFlagsForUser(IUser user)
        {
            var flagGroups = this.GetFlagGroupsForUser(user).ToList();

            this.logger.DebugFormat("Retrieved {0} flag groups for user {1}", flagGroups.Count, user);

            var flags = new HashSet<string>();

            foreach (var flag in flagGroups.Where(x => !x.DenyGroup))
            {
                flag.Flags.Select(x => x.Flag).ForEach(x => flags.Add(x));
            }

            this.logger.DebugFormat("Flag groups contain these allow flags: {0}", flags.Implode());

            foreach (var flag in flagGroups.Where(x => x.DenyGroup))
            {
                flag.Flags.Select(x => x.Flag).ForEach(x => flags.Remove(x));
            }

            this.logger.InfoFormat("Found flags for user {1}: {0}", flags.Implode(), user);

            return flags;
        }

        /// <summary>
        /// The invalidate cache.
        /// </summary>
        public void InvalidateCache()
        {
            this.flagGroupUsers = null;
        }

        #endregion
    }
}