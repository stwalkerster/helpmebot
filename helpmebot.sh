#!/bin/bash

#### THIS IS THE REAL CONTROL FILE!!!

COPY="hmb6"

cd /home/stwalkerster/$COPY

startbot() 
{
	HMBLOG="bin/log/"`date +%Y-%m-%d--%H-%M`
	echo "Starting bot with logfile $HMBLOG"
	mono bin/Debug/helpmebot6.exe &> $HMBLOG &
}

stopbot()
{
	echo "Stopping bot..."
	PID=`ps -A -o pid,args | grep "mono bin/Debug/helpmebot6.exe" | grep -v grep | awk '{print $1}'`
	kill $PID 2> /dev/null
}

rebuildbot() 
{
	echo "Rebuilding bot..."
	xbuild
}

updatebot() 
{
	echo "Updating sourcecode from subversion"
	svn up
}

case $1 in
	start)
		startbot
	;;
	stop)
		stopbot
	;;
	force-restart)
		stopbot
		startbot
	;;
	restart)
		PID=`ps -A -o pid,args | grep "mono bin/Debug/helpmebot6.exe" | grep -v grep | awk '{print $1}'`
		if [ "$PID" = "" ]; then
			startbot	
	        fi
	;;
	recompile)
		rebuildbot
	;;
	update)
		updatebot
		rebuildbot
		stopbot
		startbot
	;;
	scap)
		updatebot
	;;
	*)
		echo "Usage: helpmebot.sh {start|stop|restart|force-restart|recompile|update|scap}"
		exit 1
	;;
esac
