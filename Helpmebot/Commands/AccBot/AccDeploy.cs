﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Xml.XPath;

namespace helpmebot6.Commands
{
    class Accdeploy : GenericCommand
    {
        #region Overrides of GenericCommand

        /// <summary>
        /// Actual command logic
        /// </summary>
        /// <param name="source">The user who triggered the command.</param>
        /// <param name="channel">The channel the command was triggered in.</param>
        /// <param name="args">The arguments to the command.</param>
        /// <returns></returns>
        protected override CommandResponseHandler execute(User source, string channel, string[] args)
        {
            string revision;

            if (args.Length > 0 && args[0] != "")
            {
                revision = string.Join(" ", args);
            }
            else
            {
                throw new ArgumentException();
            }

            revision = HttpUtility.UrlEncode(revision);

            string apiDeployPassword = Configuration.singleton()["accDeployPassword"];

            string key = md5(md5(revision) + apiDeployPassword);

            HttpRequest.get("http://toolserver.org/~acc/api.php?action=deploy&r=" + revision + "&k=" + key);

  
            
        }

        #endregion

        string md5(string s)
        {
            return BitConverter.ToString(
                System.Security.Cryptography.MD5.Create().ComputeHash(
                    new System.Text.UTF8Encoding().GetBytes(s)
                )
            ).Replace("-", "").ToLower();
        }
    }
}
