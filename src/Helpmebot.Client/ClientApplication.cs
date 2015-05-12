// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ClientApplication.cs" company="Helpmebot Development Team">
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
//   The client application.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Helpmebot.Client
{
    using System;
    using System.Data;

    using Castle.Core.Logging;

    using Helpmebot.ExtensionMethods;
    using Helpmebot.Model;

    using NHibernate;

    /// <summary>
    /// The client application.
    /// </summary>
    public class ClientApplication : IApplication
    {
        #region Fields

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly ILogger logger;

        /// <summary>
        /// The database session factory.
        /// </summary>
        private readonly ISessionFactory databaseSessionFactory;

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initialises a new instance of the <see cref="ClientApplication"/> class.
        /// </summary>
        /// <param name="logger">
        /// The logger.
        /// </param>
        /// <param name="databaseSessionFactory">
        /// The database session Factory.
        /// </param>
        public ClientApplication(ILogger logger, ISessionFactory databaseSessionFactory)
        {
            this.logger = logger;
            this.databaseSessionFactory = databaseSessionFactory;
        }

        #endregion

        /// <summary>
        /// The run.
        /// </summary>
        public void Run()
        {
            Console.Clear();
            while (true)
            {
                Console.WriteLine("Helpmebot Client");
                Console.WriteLine();
                Console.WriteLine("Please choose an option:");
                Console.WriteLine("   0: Exit");
                Console.WriteLine("   1: Recreate permissions");
                Console.WriteLine();
                Console.Write("Choice [0]: ");
                var readLine = Console.ReadLine();

                if (string.IsNullOrEmpty(readLine) || readLine == "0")
                {
                    break;
                }

                if (readLine == "1")
                {
                    this.RecreatePermissions();
                }

                Console.Clear();
            }
        }

        /// <summary>
        /// The recreate permissions.
        /// </summary>
        private void RecreatePermissions()
        {
            this.logger.InfoFormat("Opening database connection, and starting transaction");
            var database = this.databaseSessionFactory.OpenSession();
            var transaction = database.BeginTransaction(IsolationLevel.RepeatableRead);

            try
            {
                this.logger.Info("Clearing old flag groups and associations...");

                var deleteFlagGroups = database.CreateSQLQuery("DELETE FROM flaggroup");
                deleteFlagGroups.ExecuteUpdate();

                this.logger.Info("Creating new groups");

                var owner = new FlagGroup { IsProtected = true, Name = "Owner" };
                var ownerFlags = new[] { "D", "O", "A", "P", "S", "B", "C" };
                ownerFlags.Apply(x => owner.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = owner }));
                this.logger.InfoFormat("Creating {0}", owner);
                database.Save(owner);

                var superuser = new FlagGroup { IsProtected = true, Name = "LegacySuperuser" };
                var superuserFlags = new[] { "A", "P", "S", "B", "C" };
                superuserFlags.Apply(x => superuser.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = superuser }));
                this.logger.InfoFormat("Creating {0}", superuser);
                database.Save(superuser);

                var advanced = new FlagGroup { IsProtected = true, Name = "LegacyAdvanced" };
                var advancedFlags = new[] { "P", "B", "C" };
                advancedFlags.Apply(x => advanced.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = advanced })); 
                this.logger.InfoFormat("Creating {0}", advanced);
                database.Save(advanced);

                var normal = new FlagGroup { IsProtected = true, Name = "LegacyNormal" };
                var normalFlags = new[] { "B" };
                normalFlags.Apply(x => normal.Flags.Add(new FlagGroupAssoc { Flag = x, FlagGroup = normal })); 
                this.logger.InfoFormat("Creating {0}", normal);
                database.Save(normal);

                var stwalkerster = new FlagGroupUser
                                       {
                                           Account = "stwalkerster",
                                           Username = "*",
                                           Nickname = "*",
                                           Hostname = "*",
                                           FlagGroup = owner,
                                           Protected = true
                                       };
                this.logger.InfoFormat("Creating user {0}", stwalkerster);
                database.Save(stwalkerster);

                this.logger.InfoFormat("Importing old users");

                var sqlQuery = database.CreateSQLQuery(
                    @"select 
                      replace(user_nickname, '%', '*') nickname
                    , replace(user_username, '%', '*') username
                    , replace(user_hostname, '%', '*') hostname
                    , '*' account
                    , CASE user_accesslevel
	                    when 'Superuser' then (select id from flaggroup where name = 'LegacySuperuser')
                        when 'Advanced' then (select id from flaggroup where name = 'LegacyAdvanced')
	                    when 'Normal' then (select id from flaggroup where name = 'LegacyNormal')
	                    when 'Developer' then (select id from flaggroup where name = 'Owner')
                      end flaggroup_id
                    , 0 protected
                    , 0 id
                    from user");

                var groupUsers = sqlQuery.AddEntity(typeof(FlagGroupUser)).List<FlagGroupUser>();
                groupUsers.Apply(x => database.Save(x));

                this.logger.InfoFormat("Committing transaction.");

                transaction.Commit();
                database.Close();
            }
            catch (Exception e)
            {
                transaction.Rollback();
                this.logger.Error(e.Message, e);
                database.Close();
            }

            this.logger.Info("Done. Press a key to continue.");
            Console.ReadLine();
        }
    }
}