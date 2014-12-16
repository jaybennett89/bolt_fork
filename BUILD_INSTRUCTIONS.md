Delete the asset store version if installed.

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
2. Obtain the source for Bolt
  * Clone the Bolt repository
    * git clone https://github.com/BoltEngine/bolt.git 
  * If you downloaded the zip extract the contents to a working directory
    * https://github.com/BoltEngine/bolt/archive/master.zip
3. In the Bolt Source directory Hold SHIFT then right click, select Open command window here
4. Change to the 'src\bolt' directory
  * cd .\src\bolt
5. Obtain the source for Bolt UDPKit
  * Clone the Bolt UDPKit
    * git clone https://github.com/BoltEngine/bolt_udpkit.git udpkit
  * If you downloaded the zip after extract the contents rename the folder to udpkit
    * https://github.com/BoltEngine/bolt_udpkit/archive/master.zip
6. Change to the root of the Bolt source.
  * cd ..\\..\\
7. Run Build.bat -ev project C:\path\to\your\unity\project
8. Open your Unity project click Edit -> Bolt -> Install Bolt
9. Restart Unity
