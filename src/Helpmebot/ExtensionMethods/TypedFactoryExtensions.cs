// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TypedFactoryExtensions.cs" company="Helpmebot Development Team">
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
//   The typed factoy extensions.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.ExtensionMethods
{
    using System;
    using System.Collections.Generic;

    using Helpmebot.Commands.Interfaces;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.TypedFactories;

    /// <summary>
    /// The typed factory extensions.
    /// </summary>
    public static class TypedFactoryExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The create.
        /// </summary>
        /// <param name="factory">
        /// The factory.
        /// </param>
        /// <param name="commandType">
        /// The command type.
        /// </param>
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
        /// </param>
        /// <returns>
        /// The <see cref="object"/>.
        /// </returns>
        public static ICommand CreateType(this ICommandTypedFactory factory, Type commandType, string commandSource, IUser user, IEnumerable<string> arguments, IIrcClient client)
        {
            return
                (ICommand)
                typeof(ICommandTypedFactory).GetMethod("Create")
                    .MakeGenericMethod(commandType)
                    .Invoke(factory, new object[] { commandSource, user, arguments, client });
        }

        #endregion
    }
}