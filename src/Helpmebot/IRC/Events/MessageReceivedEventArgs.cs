﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MessageReceivedEventArgs.cs" company="Helpmebot Development Team">
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
//   Defines the MessageReceivedEventArgs type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.IRC.Events
{
    using System;

    using Helpmebot.IRC.Interfaces;
    using Helpmebot.IRC.Messages;

    /// <summary>
    /// The message received event args.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        #region Fields

        /// <summary>
        /// The message.
        /// </summary>
        private readonly IMessage message;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="MessageReceivedEventArgs"/> class.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        public MessageReceivedEventArgs(IMessage message, IIrcClient client)
        {
            this.Client = client;
            this.message = message;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the client.
        /// </summary>
        public IIrcClient Client { get; private set; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        public IMessage Message
        {
            get
            {
                return this.message;
            }
        }

        #endregion
    }
}