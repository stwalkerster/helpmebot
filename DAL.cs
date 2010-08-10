﻿/****************************************************************************
 *   This file is part of Helpmebot.                                        *
 *                                                                          *
 *   Helpmebot is free software: you can redistribute it and/or modify      *
 *   it under the terms of the GNU General Public License as published by   *
 *   the Free Software Foundation, either version 3 of the License, or      *
 *   (at your option) any later version.                                    *
 *                                                                          *
 *   Helpmebot is distributed in the hope that it will be useful,           *
 *   but WITHOUT ANY WARRANTY; without even the implied warranty of         *
 *   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the          *
 *   GNU General Public License for more details.                           *
 *                                                                          *
 *   You should have received a copy of the GNU General Public License      *
 *   along with Helpmebot.  If not, see <http://www.gnu.org/licenses/>.     *
 ****************************************************************************/

#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using MySql.Data.MySqlClient;

#endregion

namespace helpmebot6
{
    public class DAL
    {
        private static DAL _singleton;

        private readonly string _mySqlServer;
        private readonly string _mySqlUsername;
        private readonly string _mySqlPassword;
        private readonly string _mySqlSchema;
        private readonly uint _mySqlPort;

        private MySqlConnection _connection;

        public static DAL singleton()
        {
            return _singleton;
        }

        public static DAL singleton(string host, uint port, string username, string password, string schema)
        {
            return _singleton ??
                   ( _singleton =
                     new DAL( host, port, username, password, schema ) );
        }

        protected DAL(string host, uint port, string username, string password, string schema)
        {
            _mySqlPort = port;
            _mySqlPassword = password;
            _mySqlSchema = schema;
            _mySqlServer = host;
            _mySqlUsername = username;
        }

        public bool connect()
        {
            try
            {
                Logger.instance().addToLog("Opening database connection...", Logger.LogTypes.DAL);
                MySqlConnectionStringBuilder csb = new MySqlConnectionStringBuilder
                                                       {
                                                           Database =
                                                               this._mySqlSchema,
                                                           Password =
                                                               this.
                                                               _mySqlPassword,
                                                           Server =
                                                               this._mySqlServer,
                                                           UserID =
                                                               this.
                                                               _mySqlUsername,
                                                           Port =
                                                               this._mySqlPort
                                                       };

                _connection = new MySqlConnection(csb.ConnectionString);
                _connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                GlobalFunctions.errorLog(ex);
                return false;
            }
        }

        #region internals

        private void executeNonQuery(ref MySqlCommand cmd)
        {
            Logger.instance().addToLog("Locking access to DAL...", Logger.LogTypes.DalLock);
            lock (this)
            {
                Logger.instance().addToLog("Executing (non)query: " + cmd.CommandText, Logger.LogTypes.DAL);
                try
                {
                    runConnectionTest();
                    //MySqlTransaction transact = _connection.BeginTransaction( System.Data.IsolationLevel.RepeatableRead );
                    cmd.Connection = _connection;
                    cmd.ExecuteNonQuery();
                    //transact.Commit( );
                }
                catch (MySqlException ex)
                {
                    GlobalFunctions.errorLog(ex);
                }
                catch (Exception ex)
                {
                    GlobalFunctions.errorLog(ex);
                }
                Logger.instance().addToLog("Done executing (non)query: " + cmd.CommandText, Logger.LogTypes.DAL);
            }
            Logger.instance().addToLog("DAL Lock released.", Logger.LogTypes.DalLock);
        }

        private MySqlDataReader executeReaderQuery(string query)
        {

            MySqlDataReader result = null;

            Logger.instance().addToLog("Locking access to DAL...", Logger.LogTypes.DalLock);
            lock (this)
            {
                Logger.instance().addToLog("Executing (reader)query: " + query, Logger.LogTypes.DAL);

                try
                {
                    runConnectionTest();

                    MySqlCommand cmd = new MySqlCommand(query)
                                           { Connection = this._connection };
                    result = cmd.ExecuteReader();
                    Logger.instance().addToLog("Done executing (reader)query: " + query, Logger.LogTypes.DAL);

                    return result;
                }
                catch (Exception ex)
                {
                    Logger.instance().addToLog("Problem executing (reader)query: " + query, Logger.LogTypes.DAL);
                    GlobalFunctions.errorLog(ex);
                }
            }
            Logger.instance().addToLog("DAL Lock released.", Logger.LogTypes.DalLock);
            return result;
        }

        #endregion

        public long insert(string table, params string[] values)
        {
            string query = "INSERT INTO `" + sanitise(table) + "` VALUES (";
            foreach (string item in values)
            {
                if (item != string.Empty)
                {
                    query += " \"" + sanitise(item) + "\",";
                }
                else
                {
                    query += "null,";
                }
            }

            query = query.TrimEnd(',');
            query += " );";

            MySqlCommand cmd = new MySqlCommand(query);
            this.executeNonQuery(ref cmd);
            return cmd.LastInsertedId;
        }

        public void delete(string table, int limit, params WhereConds[] conditions)
        {

            string query = "DELETE FROM `" + sanitise(table) + "`";
            for (int i = 0; i < conditions.Length; i++)
            {
                if (i == 0)
                    query += " WHERE ";
                else
                    query += " AND ";

                query += conditions[i].ToString();
            }

            if (limit > 0)
                query += " LIMIT " + limit;

            query += ";";
            MySqlCommand deleteCommand = new MySqlCommand(query);
            this.executeNonQuery(ref deleteCommand);
        }

        public void update(string table, Dictionary<string, string> items, int limit, params WhereConds[] conditions)
        {
            if (items.Count < 1)
                return;

            string query = "UPDATE `" + sanitise(table) + "` SET ";

            foreach (KeyValuePair<string, string> col in items)
            {
                query += "`" + sanitise(col.Key) + "` = \"" + sanitise(col.Value) + "\", ";
            }

            query = query.TrimEnd(',', ' ');

            for (int i = 0; i < conditions.Length; i++)
            {
                if (i == 0)
                    query += " WHERE ";
                else
                    query += " AND ";

                query += conditions[i].ToString();
            }

            if (limit > 0)
                query += " LIMIT " + limit;

            query += ";";

            MySqlCommand updateCommand = new MySqlCommand(query);
            this.executeNonQuery(ref updateCommand);
        }

        public ArrayList executeSelect(Select query)
        {
            MySqlDataReader dr = this.executeReaderQuery(query.ToString());

            ArrayList resultSet = new ArrayList();
            if (dr != null)
            {
                while (dr.Read())
                {
                    object[] row = new object[dr.FieldCount];
                    dr.GetValues(row);
                    resultSet.Add(row);
                }
                dr.Close();
            }
            return resultSet;
        }

        public string executeScalarSelect(Select query)
        {
            ArrayList al = executeSelect(query);
            return al.Count > 0 ? (((object[]) al[0])[0]).ToString() : "";
        }

        private void runConnectionTest()
        {
            // ok, first let's assume the connection is dead.
            bool connectionOk = false;

            // first time through, skip the connection attempt
            bool firstTime = true;

            int sleepTime = 1000;

            while (!connectionOk)
            {
                if (!firstTime)
                {
                    Logger.instance().addToLog("Reconnecting to database....", Logger.LogTypes.Error);

                    this.connect();

                    Thread.Sleep(sleepTime);

                    sleepTime = (int) (sleepTime*1.5) > int.MaxValue ? sleepTime : (int) (sleepTime*1.5);
                }

                connectionOk = _connection.Ping();

                firstTime = false;
            }
        }

        public void executeProcedure(string name, params string[] args)
        {
            MySqlCommand cmd = new MySqlCommand
                                   {
                                       CommandType = CommandType.StoredProcedure,
                                       CommandText = name
                                   };

            foreach (string item in args)
            {
                cmd.Parameters.Add(new MySqlParameter(item, MySqlDbType.Int16));
            }

            cmd.Connection = _connection;

            runConnectionTest();

            cmd.ExecuteNonQuery();
        }

// ReSharper disable InconsistentNaming
        public void proc_HMB_UPDATE_BINARYSTORE(byte[] raw, string desc)
// ReSharper restore InconsistentNaming
        {
            lock (this)
            {
                runConnectionTest();

                MySqlCommand cmd = new MySqlCommand
                                       {
                                           Connection = this._connection,
                                           CommandType =
                                               CommandType.StoredProcedure,
                                           CommandText =
                                               "HMB_UPDATE_BINARYSTORE"
                                       };
                cmd.Parameters.Add("@raw", MySqlDbType.Blob).Value = raw;
                cmd.Parameters.Add("@desc", MySqlDbType.VarChar).Value = desc;

                cmd.ExecuteNonQuery();
            }
        }

// ReSharper disable InconsistentNaming
        public string proc_HMB_GET_LOCAL_OPTION(string option, string channel)
// ReSharper restore InconsistentNaming
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand
                                       {
                                           Connection = this._connection,
                                           CommandType =
                                               CommandType.StoredProcedure,
                                           CommandText = "HMB_GET_LOCAL_OPTION"
                                       };

                cmd.Parameters.AddWithValue("@optionName", option);
                cmd.Parameters["@optionName"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@channel", channel);
                cmd.Parameters["@channel"].Direction = ParameterDirection.Input;

                cmd.Parameters.AddWithValue("@optionValue", "");
                cmd.Parameters["@optionValue"].Direction = ParameterDirection.Output;
                lock (this)
                {
                    runConnectionTest();
                    cmd.ExecuteNonQuery();
                }
                return (string) cmd.Parameters["@optionValue"].Value;
            }
            catch (FormatException ex)
            {
                GlobalFunctions.errorLog(ex);
                Logger.instance().addToLog(option + "@" + channel, Logger.LogTypes.Error);
                throw;
            }
        }


        /// <summary>
        ///   Class encapsulating a SELECT statement
        /// </summary>
        public class Select
        {
            private bool _shallIEscapeSelects = true;

            private readonly string[] _fields;
            private string _from;
            private readonly LinkedList<Join> _joins = new LinkedList<Join>();
            private readonly LinkedList<WhereConds> _wheres;
            private readonly LinkedList<string> _groups;
            private readonly LinkedList<Order> _orders;
            private readonly LinkedList<WhereConds> _havings;
            private int _limit;
            private int _offset;

            public Select(params string[] fields)
            {
                this._fields = fields;
                this._from = string.Empty;
                this._limit = this._offset = 0;
                this._joins = new LinkedList<Join>();
                this._wheres = new LinkedList<WhereConds>();
                this._groups = new LinkedList<string>();
                this._orders = new LinkedList<Order>();
                this._havings = new LinkedList<WhereConds>();
            }

            public void escapeSelects(bool escape)
            {
                this._shallIEscapeSelects = escape;
            }

            public void setFrom(string from)
            {
                this._from = from;
            }

            public void addJoin(string table, JoinTypes joinType, WhereConds conditions)
            {
                this._joins.AddLast(new Join(joinType, table, conditions));
            }

            public void addWhere(WhereConds conditions)
            {
                this._wheres.AddLast(conditions);
            }

            public void addGroup(string field)
            {
                this._groups.AddLast(field);
            }

            public void addOrder(Order order)
            {
                this._orders.AddLast(order);
            }

            public void addHaving(WhereConds conditions)
            {
                this._havings.AddLast(conditions);
            }

            public void addLimit(int limit, int offset)
            {
                this._limit = limit;
                this._offset = offset;
            }

            public override string ToString()
            {
                string query = "SELECT ";
                bool firstField = true;
                foreach (string  f in this._fields)
                {
                    if (!firstField)
                        query += ", ";

                    string fok = MySqlHelper.EscapeString(f);
                    if (! this._shallIEscapeSelects)
                        fok = f;

                    firstField = false;

                    query += fok;
                }

                if (this._from != string.Empty)
                {
                    query += " FROM " + "`" + MySqlHelper.EscapeString(this._from) + "`";
                }

                if (this._joins.Count != 0)
                {
                    foreach (Join item in this._joins)
                    {
                        switch (item.joinType)
                        {
                            case JoinTypes.Inner:
                                query += " INNER JOIN ";
                                break;
                            case JoinTypes.Left:
                                query += " LEFT OUTER JOIN ";
                                break;
                            case JoinTypes.Right:
                                query += " RIGHT OUTER JOIN ";
                                break;
                            case JoinTypes.FullOuter:
                                query += " FULL OUTER JOIN ";
                                break;
                            default:
                                break;
                        }

                        query += "`" + MySqlHelper.EscapeString(item.table) + "`";

                        query += " ON " + item.joinConditions;
                    }
                }

                if (this._wheres.Count > 0)
                {
                    query += " WHERE ";

                    bool first = true;

                    foreach (WhereConds w in this._wheres)
                    {
                        if (!first)
                            query += " AND ";
                        first = false;
                        query += w.ToString();
                    }
                }
                if (this._groups.Count != 0)
                {
                    query += " GROUP BY ";
                    bool first = true;
                    foreach (string group in this._groups)
                    {
                        if (!first)
                            query += ", ";
                        first = false;
                        query += MySqlHelper.EscapeString(group);
                    }
                }
                if (this._orders.Count > 0)
                {
                    query += " ORDER BY ";

                    bool first = true;
                    foreach (Order order in this._orders)
                    {
                        if (!first)
                            query += ", ";
                        first = false;
                        query += order.ToString();
                    }
                }
                if (this._havings.Count > 0)
                {
                    query += " HAVING ";

                    bool first = true;

                    foreach (WhereConds w in this._havings)
                    {
                        if (!first)
                            query += " AND ";
                        first = false;
                        query += w.ToString();
                    }
                }

                if (this._limit != 0)
                    query += " LIMIT " + this._limit;

                if (this._offset != 0)
                    query += " OFFSET " + this._offset;

                query += ";";
                return query;
            }

            public struct Order
            {
                public Order(string column, bool asc)
                {
                    this._column = column;
                    this._asc = asc;
                    this._escape = true;
                }

                public Order(string column, bool asc, bool escape)
                {
                    this._column = column;
                    this._asc = asc;
                    this._escape = escape;
                }

                private readonly string _column;
                private readonly bool _asc;
                private readonly bool _escape;

                public override string ToString()
                {
                    return "`" + (this._escape ? MySqlHelper.EscapeString(this._column) : this._column) + "` " + (this._asc ? "ASC" : "DESC");
                }
            }

            private struct Join
            {
                public readonly JoinTypes joinType;
                public readonly string table;
                public readonly WhereConds joinConditions;

                public Join(JoinTypes type, string table, WhereConds conditions)
                {
                    joinType = type;
                    this.table = table;
                    joinConditions = conditions;
                }
            }

            public enum JoinTypes
            {
                Inner,
                Left,
                Right,
                FullOuter
            }
        }

        public struct WhereConds
        {
            private readonly bool _quoteA;
            private readonly bool _quoteB;
            private readonly string _a;
            private readonly string _b;
            private readonly string _comparer;

            public WhereConds(bool aNeedsQuoting, string a, string comparer, bool bNeedsQuoting, string b)
            {
                this._quoteA = aNeedsQuoting;
                this._quoteB = bNeedsQuoting;
                this._a = a;
                this._b = b;
                this._comparer = comparer;
            }

            public WhereConds(string column, string value)
            {
                this._quoteA = false;
                this._quoteB = true;
                this._a = column;
                this._b = value;
                this._comparer = "=";
            }

            public WhereConds(string column, int value)
            {
                this._quoteA = false;
                this._quoteB = true;
                this._a = column;
                this._b = value.ToString();
                this._comparer = "=";
            }

            public override string ToString()
            {
                string actualA = (this._quoteA ? "\"" : "") + MySqlHelper.EscapeString(this._a) + (this._quoteA ? "\"" : "");
                string actualB = (this._quoteB ? "\"" : "") + MySqlHelper.EscapeString(this._b) + (this._quoteB ? "\"" : "");
                string actualComp = MySqlHelper.EscapeString(this._comparer);
                return actualA + " " + actualComp + " " + actualB;
            }
        }

        private static string sanitise(string rawData)
        {
            return MySqlHelper.EscapeString(rawData);
        }
    }
}