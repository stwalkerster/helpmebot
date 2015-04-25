﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Age.cs" company="Helpmebot Development Team">
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
// --------------------------------------------------------------------------------------------------------------------
namespace helpmebot6.Commands
{
    using System;
    using System.Globalization;

    using Helpmebot;
    using Helpmebot.Attributes;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    /// <summary>
    ///     Returns the age of a wikipedian
    /// </summary>
    [CommandInvocation("age")]
    [CommandFlag(Helpmebot.Model.Flag.LegacyNormal)]
    public class Age : GenericCommand
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="Age"/> class.
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
        public Age(IUser source, string channel, string[] args, ICommandServiceHelper commandServiceHelper)
            : base(source, channel, args, commandServiceHelper)
        {
        }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Gets the wikipedian age.
        /// </summary>
        /// <param name="userName">
        /// Name of the user.
        /// </param>
        /// <param name="channel">
        /// The channel the command is requested in. (Retrieves the relevant base wiki)
        /// </param>
        /// <returns>
        /// timespan of the age
        /// </returns>
        public static TimeSpan GetWikipedianAge(string userName, string channel)
        {
            DateTime regdate = Registration.GetRegistrationDate(userName, channel);
            TimeSpan age = DateTime.Now.Subtract(regdate);
            if (regdate.Equals(new DateTime(0001, 1, 1)))
            {
                age = new TimeSpan(0);
            }

            return age;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Actual command logic
        /// </summary>
        /// <returns>the response</returns>
        protected override CommandResponseHandler ExecuteCommand()
        {
            string userName;
            if (this.Arguments.Length > 0 && this.Arguments[0] != string.Empty)
            {
                userName = string.Join(" ", this.Arguments);
            }
            else
            {
                userName = this.Source.Nickname;
            }

            TimeSpan time = GetWikipedianAge(userName, this.Channel);
            string message;
            IMessageService messageService = this.CommandServiceHelper.MessageService;
            if (time.Equals(new TimeSpan(0)))
            {
                string[] messageParameters = { userName };
                message = messageService.RetrieveMessage("noSuchUser", this.Channel, messageParameters);
            }
            else
            {
                string[] messageParameters =
                    {
                        userName, (time.Days / 365).ToString(CultureInfo.InvariantCulture),
                        (time.Days % 365).ToString(CultureInfo.InvariantCulture),
                        time.Hours.ToString(CultureInfo.InvariantCulture),
                        time.Minutes.ToString(CultureInfo.InvariantCulture),
                        time.Seconds.ToString(CultureInfo.InvariantCulture)
                    };
                message = messageService.RetrieveMessage("cmdAge", this.Channel, messageParameters);
            }

            return new CommandResponseHandler(message);
        }

        #endregion
    }
}