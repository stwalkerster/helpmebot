﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyCommandParser.cs" company="Helpmebot Development Team">
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
//   A command parser
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.Legacy
{
    using System;
    using System.Linq;
    using System.Text.RegularExpressions;

    using Castle.Core.Logging;

    using Helpmebot.Commands.CommandUtilities.Models;
    using Helpmebot.Commands.Interfaces;
    using Helpmebot.ExtensionMethods;
    using Helpmebot.IRC.Interfaces;
    using Helpmebot.Legacy.Configuration;
    using Helpmebot.Legacy.Model;
    using Helpmebot.Services.Interfaces;

    using helpmebot6.Commands;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     A command parser
    /// </summary>
    public class LegacyCommandParser
    {
        #region Constants

        /// <summary>
        ///     The allowed command name chars.
        /// </summary>
        private const string AllowedCommandNameChars = "0-9a-z-_";

        #endregion

        #region Fields

        /// <summary>
        /// The command service helper.
        /// </summary>
        private readonly ICommandServiceHelper commandServiceHelper;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="LegacyCommandParser"/> class.
        /// </summary>
        /// <param name="commandServiceHelper">
        /// The command Service Helper.
        /// </param>
        /// <param name="logger">
        /// The logger.
        /// </param>
        public LegacyCommandParser(ICommandServiceHelper commandServiceHelper, ILogger logger)
        {
            this.commandServiceHelper = commandServiceHelper;
            this.Log = logger;

            this.OverrideBotSilence = false;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        ///     Gets or sets a value indicating whether [override bot silence].
        /// </summary>
        /// <value><c>true</c> if [override bot silence]; otherwise, <c>false</c>.</value>
        public bool OverrideBotSilence { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// Finds the redirection.
        /// </summary>
        /// <param name="args">
        /// The args.
        /// </param>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        public static string FindRedirection(ref string[] args)
        {
            var commandParser = ServiceLocator.Current.GetInstance<ICommandParser>();
            RedirectionResult redirectionResult = commandParser.ParseRedirection(args);

            args = redirectionResult.Arguments.ToArray();
            return redirectionResult.Target.Implode();
        }

        /// <summary>
        /// Handles the command.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="command">
        /// The command.
        /// </param>
        /// <param name="args">
        /// The args.
        /// </param>
        public void HandleCommand(LegacyUser source, string destination, string command, string[] args)
        {
            this.Log.Debug("Handling recieved message...");

            // user is null (!)
            if (source == null)
            {
                this.Log.Debug("Ignoring message from null user.");
                return;
            }

            // if on ignore list, ignore!
            if (source.AccessLevel == LegacyUser.UserRights.Ignored)
            {
                this.Log.Debug("Ignoring message from ignored user.");
                return;
            }

            // flip destination over if required
            if (destination == this.commandServiceHelper.Client.Nickname)
            {
                destination = source.Nickname;
            }

            /* 
             * Check for a valid command
             * search for a class that can handle this command.
             */

            // Create a new object which holds the type of the command handler, if it exists.
            // if the command handler doesn't exist, then this won't be set to a value
            Type commandHandler =
                Type.GetType(
                    "helpmebot6.Commands." + command.Substring(0, 1).ToUpper() + command.Substring(1).ToLower());

            // check the type exists
            if (commandHandler != null)
            {
                string directedTo = FindRedirection(ref args);

                // create a new instance of the commandhandler.
                // cast to genericcommand (which holds all the required methods to run the command)
                // run the command.
                CommandResponseHandler response =
                    ((GenericCommand)
                     Activator.CreateInstance(commandHandler, source, destination, args, this.commandServiceHelper))
                        .RunCommand();
                this.HandleCommandResponseHandler(source, destination, directedTo, response);
            }
        }

        /// <summary>
        /// Tests against recognised message formats
        /// </summary>
        /// <param name="message">
        /// the message received
        /// </param>
        /// <param name="overrideSilence">
        /// ref: whether this message format overrides any imposed silence
        /// </param>
        /// <param name="client">
        /// The client.
        /// </param>
        /// <returns>
        /// true if the message is in a recognised format
        /// </returns>
        /// <remarks>
        /// Allowed formats:
        ///     !command
        ///     !helpmebot command
        ///     Helpmebot: command
        ///     Helpmebot command
        ///     Helpmebot, command
        ///     Helpmebot&gt; command
        /// </remarks>
        public bool IsRecognisedMessage(ref string message, ref bool overrideSilence, IIrcClient client)
        {
            return ParseRawLineForMessage(
                ref message, 
                client.Nickname, 
                this.commandServiceHelper.ConfigurationHelper.CoreConfiguration.CommandTrigger);
        }

        #endregion

        #region Methods

        /// <summary>
        /// The parse raw line for message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <param name="nickname">
        /// The nickname.
        /// </param>
        /// <param name="trigger">
        /// The trigger.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        private static bool ParseRawLineForMessage(ref string message, string nickname, string trigger)
        {
            var validCommand =
                new Regex(
                    @"^(?:" + trigger + @"(?:(?<botname>" + nickname.ToLower() + @") )?(?<cmd>["
                    + AllowedCommandNameChars + "]+)|(?<botname>" + nickname.ToLower() + @")[ ,>:](?: )?(?<cmd>["
                    + AllowedCommandNameChars + "]+))(?: )?(?<args>.*?)(?:\r)?$");

            Match m = validCommand.Match(message);

            if (m.Length > 0)
            {
                message = m.Groups["cmd"].Value
                          + (m.Groups["args"].Length > 0 ? " " + m.Groups["args"].Value : string.Empty);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Handles the command response handler.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="destination">
        /// The destination.
        /// </param>
        /// <param name="directedTo">
        /// The directed to.
        /// </param>
        /// <param name="response">
        /// The response.
        /// </param>
        private void HandleCommandResponseHandler(
            LegacyUser source, 
            string destination, 
            string directedTo, 
            CommandResponseHandler response)
        {
            if (response != null)
            {
                foreach (CommandResponse item in response.GetResponses())
                {
                    string message = item.Message;

                    if (directedTo != string.Empty)
                    {
                        message = directedTo + ": " + message;
                    }

                    var irc1 = this.commandServiceHelper.Client;
                    switch (item.Destination)
                    {
                        case CommandResponseDestination.Default:
                            if (this.OverrideBotSilence || LegacyConfig.Singleton()["silence", destination] != "true")
                            {
                                irc1.SendMessage(destination, message);
                            }

                            break;
                        case CommandResponseDestination.ChannelDebug:
                            irc1.SendMessage(
                                this.commandServiceHelper.ConfigurationHelper.CoreConfiguration.DebugChannel, 
                                message);
                            break;
                        case CommandResponseDestination.PrivateMessage:
                            irc1.SendMessage(source.Nickname, message);
                            break;
                    }
                }
            }
        }

        #endregion
    }
}