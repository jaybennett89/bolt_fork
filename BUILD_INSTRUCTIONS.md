# OS X
 
1. Download and install the latest MDK from http://www.mono-project.com/download/#download-mac
2. Open up a terminal and navigate to the Bolt repository root directory
3. Run this command: cp FAKE/tools/NuGet.Core.dll .
4. Clone bolt_udpkit into src/bolt: git clone https://github.com/BoltEngine/bolt_udpkit src/bolt/udpkit
5. Run this command: chmod a+x Build.sh
6. Run this command: ./Build.sh -ev project /path/to/your/unity/project
7. Open your Unity project and from the top menu bar click Edit then Install Bolt from the bottom of the drop down.
8. Restart Unity

# Windows

1. Make sure you have a .NET SDK installed, if you have visual studio or any other .NET development tools already installed you most likely have it, otherwise install the latest version from this page http://msdn.microsoft.com/en-us/vstudio/aa496123.aspx
2. If you downloaded the Bolt repository as a .zip file make sure to open up properties for the .zip file and click "Unblock" **before** you unpack it.
3. Open a command line terminal and navigate to the root directory of the bolt repository.
4. Run this command Build.bat -ev project C:\path\to\your\unity\project
5. Open your Unity project and run "Bolt/Install Bolt" from the top menu bar
6. Restart Unity
