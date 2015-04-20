// --------------------------------------------------------------------------------------------------------------------
// <copyright file="HelpMessage.cs" company="Helpmebot Development Team">
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
//   The help message.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Commands.CommandUtilities.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// The help message.
    /// </summary>
    public class HelpMessage
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="HelpMessage"/> class. 
        /// </summary>
        /// <param name="commandName">
        /// The command Name.
        /// </param>
        /// <param name="syntax">
        /// The syntax.
        /// </param>
        /// <param name="text">
        /// The text.
        /// </param>
        public HelpMessage(string commandName, string syntax, string text)
        {
            this.CommandName = commandName;
            this.Syntax = syntax;
            this.Text = text;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the command name.
        /// </summary>
        public string CommandName { get; private set; }

        /// <summary>
        /// Gets the syntax.
        /// </summary>
        public string Syntax { get; private set; }

        /// <summary>
        /// Gets the text.
        /// </summary>
        public string Text { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The to command responses.
        /// </summary>
        /// <param name="commandTrigger">
        /// The command Trigger.
        /// </param>
        /// <returns>
        /// The <see cref="IEnumerable{CommandResponse}"/>.
        /// </returns>
        public IEnumerable<CommandResponse> ToCommandResponses(string commandTrigger)
        {
            var initialLine = string.Format("{2}{0} {1}", this.CommandName, this.Syntax, commandTrigger);
            var secondLine = string.Format("   {0}", this.Text);
            return new List<CommandResponse>
                       {
                           new CommandResponse
                               {
                                   Message = initialLine, 
                                   Destination =
                                       CommandResponseDestination.PrivateMessage
                               }, 
                           new CommandResponse
                               {
                                   Message = secondLine, 
                                   Destination =
                                       CommandResponseDestination.PrivateMessage
                               }, 
                       };
        }

        #endregion
    }
}