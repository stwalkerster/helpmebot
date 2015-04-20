﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAccessLogService.cs" company="Helpmebot Development Team">
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
//   Defines the IAccessLogService type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Services.Interfaces
{
    using System;
    using System.Collections.Generic;

    using Helpmebot.Legacy.Model;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The AccessLogService interface.
    /// </summary>
    public interface IAccessLogService
    {
        /// <summary>
        /// The success.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        void Success(IUser user, Type command, IEnumerable<string> arguments, string destination);

        /// <summary>
        /// The failure.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        void Failure(IUser user, Type command, IEnumerable<string> arguments, string destination);

        /// <summary>
        /// The save legacy access log entry.
        /// </summary>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="class">
        /// The command.
        /// </param>
        /// <param name="allowed">
        /// The execution allowed.
        /// </param>
        /// <param name="channel">
        /// The destination.
        /// </param>
        /// <param name="parameters">
        /// The arguments.
        /// </param>
        /// <param name="requiredAccessLevel">
        /// The required access level.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        [Obsolete("Legacy compatability layer")]
        bool SaveLegacyAccessLogEntry(
            ILegacyUser user,
            Type @class,
            bool allowed,
            string channel,
            string[] parameters,
            LegacyUser.UserRights requiredAccessLevel);
    }
}
