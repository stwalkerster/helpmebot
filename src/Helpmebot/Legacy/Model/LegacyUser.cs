﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="LegacyUser.cs" company="Helpmebot Development Team">
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
namespace Helpmebot.Legacy.Model
{
    using System;
    using System.Linq;

    using Castle.Core.Logging;

    using Helpmebot.Model;
    using Helpmebot.Model.Interfaces;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     The user.
    /// </summary>
    public class LegacyUser : ILegacyUser
    {
        #region Fields

        /// <summary>
        ///     The _access level.
        /// </summary>
        private UserRights accessLevel;

        /// <summary>
        ///     The retrieved access level.
        /// </summary>
        private bool retrievedAccessLevel;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initialises a new instance of the <see cref="LegacyUser" /> class.
        /// </summary>
        public LegacyUser()
        {
            // FIXME: ServiceLocator - legacydatabase
            this.Log = ServiceLocator.Current.GetInstance<ILogger>();
        }

        #endregion

        #region Enums

        /// <summary>
        ///     The user rights.
        /// </summary>
        public enum UserRights
        {
            /// <summary>
            ///     The developer.
            /// </summary>
            Developer = 3, 

            /// <summary>
            ///     The super user.
            /// </summary>
            Superuser = 2, 

            /// <summary>
            ///     The advanced.
            /// </summary>
            Advanced = 1, 

            /// <summary>
            ///     The normal.
            /// </summary>
            Normal = 0, 

            /// <summary>
            ///     The semi-ignored.
            /// </summary>
            Semiignored = -1, 

            /// <summary>
            ///     The ignored.
            /// </summary>
            Ignored = -2
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets the access level.
        /// </summary>
        /// <value>The access level.</value>
        public UserRights AccessLevel
        {
            get
            {
                try
                {
                    if (this.retrievedAccessLevel == false)
                    {
                        // TODO: servicelocator (userflagservice)
                        var userFlagService = ServiceLocator.Current.GetInstance<IUserFlagService>();
                        var flagsForUser = userFlagService.GetFlagsForUser(this).ToList();

                        if (flagsForUser.Contains(Flag.LegacyDeveloper))
                        {
                            this.accessLevel = UserRights.Developer;
                            this.retrievedAccessLevel = true;
                            return this.accessLevel;
                        }

                        if (flagsForUser.Contains(Flag.LegacySuperuser))
                        {
                            this.accessLevel = UserRights.Superuser;
                            this.retrievedAccessLevel = true;
                            return this.accessLevel;
                        }

                        if (flagsForUser.Contains(Flag.LegacyAdvanced))
                        {
                            this.accessLevel = UserRights.Advanced;
                            this.retrievedAccessLevel = true;
                            return this.accessLevel;
                        }

                        this.accessLevel = UserRights.Normal;
                        this.retrievedAccessLevel = true;
                    }

                    return this.accessLevel;
                }
                catch (Exception ex)
                {
                    this.Log.Error(ex.Message, ex);
                }

                return UserRights.Normal;
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        ///     Gets or sets the account.
        /// </summary>
        public string Account { get; set; }

        /// <summary>
        ///     Gets or sets the hostname.
        /// </summary>
        /// <value>The hostname.</value>
        public string Hostname { get; set; }

        /// <summary>
        ///     Gets or sets the Castle.Windsor Logger
        /// </summary>
        public ILogger Log { get; set; }

        /// <summary>
        ///     Gets the network.
        /// </summary>
        /// <value>The network.</value>
        public uint Network { get; private set; }

        /// <summary>
        ///     Gets or sets the nickname.
        /// </summary>
        /// <value>The nickname.</value>
        public string Nickname { get; set; }

        /// <summary>
        ///     Gets or sets the username.
        /// </summary>
        /// <value>The username.</value>
        public string Username { get; set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The new from other user.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The <see cref="LegacyUser"/>.
        /// </returns>
        public static LegacyUser NewFromOtherUser(IUser source)
        {
            if (source.GetType() == typeof(LegacyUser))
            {
                return (LegacyUser)source;
            }

            if (source.GetType().GetInterfaces().Contains(typeof(ILegacyUser)))
            {
                return (LegacyUser)source;
            }

            return NewFromString(string.Format("{0}!{1}@{2}", source.Nickname, source.Username, source.Hostname));
        }

        /// <summary>
        /// New from string.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <returns>
        /// The legacy user
        /// </returns>
        public static LegacyUser NewFromString(string source)
        {
            return NewFromString(source, 0);
        }

        /// <summary>
        /// New user from string.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="network">
        /// The network.
        /// </param>
        /// <returns>
        /// The legacy user
        /// </returns>
        public static LegacyUser NewFromString(string source, uint network)
        {
            string user, host;
            string nick = user = host = null;
            try
            {
                if (source.Contains("@") && source.Contains("!"))
                {
                    char[] splitSeparators = { '!', '@' };
                    string[] sourceSegment = source.Split(splitSeparators, 3);
                    nick = sourceSegment[0];
                    user = sourceSegment[1];
                    host = sourceSegment[2];
                }
                else if (source.Contains("@"))
                {
                    char[] splitSeparators = { '@' };
                    string[] sourceSegment = source.Split(splitSeparators, 2);
                    nick = sourceSegment[0];
                    host = sourceSegment[1];
                }
                else
                {
                    nick = source;
                }
            }
            catch (IndexOutOfRangeException ex)
            {
                ServiceLocator.Current.GetInstance<ILogger>().Error(ex.Message, ex);
            }

            if (nick == null)
            {
                return null;
            }

            var ret = new LegacyUser { Hostname = host, Nickname = nick, Username = user, Network = network };
            return ret;
        }

        /// <summary>
        /// The new from string with access level.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="accessLevel">
        /// The access level.
        /// </param>
        /// <returns>
        /// The <see cref="LegacyUser"/>.
        /// </returns>
        public static LegacyUser NewFromStringWithAccessLevel(string source, UserRights accessLevel)
        {
            return NewFromStringWithAccessLevel(source, 0, accessLevel);
        }

        /// <summary>
        /// The new from string with access level.
        /// </summary>
        /// <param name="source">
        /// The source.
        /// </param>
        /// <param name="network">
        /// The network.
        /// </param>
        /// <param name="accessLevel">
        /// The access level.
        /// </param>
        /// <returns>
        /// The <see cref="LegacyUser"/>.
        /// </returns>
        public static LegacyUser NewFromStringWithAccessLevel(string source, uint network, UserRights accessLevel)
        {
            LegacyUser u = NewFromString(source, network);
            if (u == null)
            {
                return null;
            }

            u.accessLevel = accessLevel;
            return u;
        }

        /// <summary>
        ///     Recompiles the source string
        /// </summary>
        /// <returns>nick!user@host, OR nick@host, OR nick</returns>
        public override string ToString()
        {
            string endResult = string.Empty;

            if (this.Nickname != null)
            {
                endResult = this.Nickname;
            }

            if (this.Username != null)
            {
                endResult += "!" + this.Username;
            }

            if (this.Hostname != null)
            {
                endResult += "@" + this.Hostname;
            }

            return endResult;
        }

        #endregion
    }
}