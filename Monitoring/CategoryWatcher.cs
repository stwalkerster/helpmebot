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
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Collections;


namespace helpmebot6.Monitoring
{
    public class CategoryWatcher
    {

        string _site;
        string _category;
        string _username;
        string _password;
        string _key;

        Thread watcherThread;

        int _sleepTime = 180;


        public delegate void CategoryHasItemsEventHook( DotNetWikiBot.PageList items, string keyword);
        public event CategoryHasItemsEventHook CategoryHasItemsEvent;

        DotNetWikiBot.Site mw_instance;

        public CategoryWatcher( string Category, string Key )
        {
            // look up site id
            string baseWiki = Configuration.Singleton( ).retrieveGlobalStringOption( "baseWiki" );
            _site = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_mainpage` FROM `site` WHERE `site_id` = " + baseWiki + ";" );
            _category = Category;
            _username = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_username` FROM `site` WHERE `site_id` = " + baseWiki + ";" );
            _password = DAL.Singleton( ).ExecuteScalarQuery( "SELECT `site_password` FROM `site` WHERE `site_id` = " + baseWiki + ";" );
            _key = Key;


            mw_instance = new DotNetWikiBot.Site( _site , _username , _password );

            watcherThread = new Thread( new ThreadStart( this.watcherThreadMethod ) );
            watcherThread.Start( );
           
        }

        private void watcherThreadMethod( )
        {
            try
            {
                while ( true )
                {
                    Thread.Sleep( this.SleepTime * 1000 );
                    DotNetWikiBot.PageList categoryResults = this.doCategoryCheck( );
                    if ( categoryResults.Count() > 0)
                    {
                        CategoryHasItemsEvent( categoryResults, _key);
                    }
                }
            }
            catch ( ThreadAbortException ex )
            {
                GlobalFunctions.ErrorLog( ex, System.Reflection.MethodInfo.GetCurrentMethod( ) );
            }
        }

        public DotNetWikiBot.PageList doCategoryCheck( )
        {
            
            DotNetWikiBot.PageList list = new DotNetWikiBot.PageList(mw_instance);
            list.FillAllFromCategory(_category);

            return list;
        }

        public void Stop()
        {
            GlobalFunctions.Log("Stopping Watcher Thread for " + _category + " ...");
            watcherThread.Abort();
        }

        /// <summary>
        /// The time to sleep, in seconds.
        /// </summary>
        public int SleepTime
        {
            get
            {
                return _sleepTime;
            }
            set
            {
                _sleepTime = value;
            }
        }

        public override string ToString( )
        {
            return _key;
        }

        
    }
}
