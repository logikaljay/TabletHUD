# TabletHUD

The main reason behind this is because I love TurboHUD but i find there is too much info, a bit too distracting for what I would like to capture/monitor.

All I wanted was experience monitored per area in game, and then displayed in a time/experience and time to next level for paragon grinding.

##To Compile:
* clone redis from https://github.com/MSOpenTech/redis and build it, then run redis-server.
* visit ownedcore and download Enigma.D3 and give Enigma mad props for his work.
* Open the TabletHUD solution in visual studio and do a NuGet Package Restore to retrieve signalR and ServerStack.Redis
* Add the Engima.D3 project to the TabletHUD solution.
* compile

##To Run (debug)
* Build/Run in visual studio (as Administrator if you use UAC) and make sure the Debug setting in the app.config is set to true.
* Make sure D3 is running.
* When solution is running, visit http://<your.local.machine.ip>:9001/web/index.html

##To Run (service)
* navigate to the binary folder and in an administrator command prompt type 'sc create TabletHUD ./tablethud.exe' then 'sc start TabletHUD'
* Make sure D3 is running.
* When solution is running, visit http://<your.local.machine.ip>:9001/web/index.html

##The main issues are:
* ObjectManager becomes null randomly, sometimes after leveling, sometimes after using a waypoint, sometimes after changing Areas.
* Area's are not currently being translated to names, I do not have a lookup table for world id to world name
* Some areas are merging in to one, IE, Northern Highlands and Southern Highlands in act 1 use the same world id - maybe i am doing this wrong.
