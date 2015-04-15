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
    using System;
    using System.Collections.Generic;

    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using NHibernate;

    /// <summary>
    /// The user flag service.
    /// </summary>
    public class UserFlagService : IUserFlagService
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="UserFlagService"/> class.
        /// </summary>
        /// <param name="databaseSession">
        /// The database Session.
        /// </param>
        public UserFlagService(ISession databaseSession)
        {
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

            return new List<string> { "D", "A" };
            throw new NotImplementedException();
        }

        #endregion
    }
}