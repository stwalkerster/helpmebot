﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IPAddressExtensions.cs" company="Helpmebot Development Team">
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
//   Defines the IPAddressExtensions type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------
namespace Helpmebot.ExtensionMethods
{
    using System.IO;
    using System.Net;
    using System.Xml;

    using Castle.Core.Logging;

    using Helpmebot.Configuration.XmlSections.Interfaces;
    using Helpmebot.Model;
    using Helpmebot.Services.Interfaces;

    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    /// The ip address extensions.
    /// </summary>
    public static class IPAddressExtensions
    {
        #region Public Methods and Operators

        /// <summary>
        /// The get location.
        /// </summary>
        /// <param name="ip">
        /// The ip.
        /// </param>
        /// <returns>
        /// The <see cref="GeolocateResult"/>.
        /// </returns>
        public static GeolocateResult GetLocation(this IPAddress ip)
        {
            // FIXME: ServiceLocator - geolocationservice
            var geolocationService = ServiceLocator.Current.GetInstance<IGeolocationService>();
            return geolocationService.GetLocation(ip);
        }

        #endregion
    }
}