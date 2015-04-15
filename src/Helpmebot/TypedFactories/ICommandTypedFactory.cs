﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICommandTypedFactory.cs" company="Helpmebot Development Team">
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
//   The CommandTypedFactory interface.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.TypedFactories
{
    using System.Collections.Generic;

    using Castle.MicroKernel.ModelBuilder.Descriptors;

    using Helpmebot.Commands.Interfaces;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model.Interfaces;

    /// <summary>
    /// The CommandTypedFactory interface.
    /// </summary>
    public interface ICommandTypedFactory
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get.
        /// </summary>
        /// <param name="commandSource">
        /// The command Source.
        /// </param>
        /// <param name="user">
        /// The user.
        /// </param>
        /// <param name="arguments">
        /// The arguments.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>5
        /// <typeparam name="T">
        /// The command type
        /// </typeparam>
        /// <returns>
        /// The <see cref="ICommand"/>.
        /// </returns>
        T Create<T>(string commandSource, IUser user, IEnumerable<string> arguments, IIrcClient client)
            where T : ICommand;

        /// <summary>
        /// The release.
        /// </summary>
        /// <param name="command">
        /// The command.
        /// </param>
        void Release(ICommand command);

        #endregion
    }
}