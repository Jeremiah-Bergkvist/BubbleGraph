Purpose:
The purpose of this program is to display a packed bubble chart with data found in a file.

Usage:
Click on File->Load and then select a bubble record file to display. The algorithm used in this
program will bruteforce the placement of nodes using a scan of ever enlarging concentric circles
to find empty space for a node. This method is very CPU intensive but will result in a tightly
packed graph.

File Format:
Use the file menu to load a csv file containing bubble records in the following format:
  <number>, <string>
  
  E.G.
  130,thing1
  200,thing2
  310,otherthing
  40,yetanother
  
  Note: Each record is on it's own line

Algorithm:
[*] Sort the nodes by largest to smallest radius
[*] The first node is placed in the middle of the graph.
[*] For all other nodes:
    [.] Set scan distance to first node radius and current node radius
    [.] Start scanning for an empty spot using a 360 degree clockwise scan
    [.] Place node if empty spot found, otherwise increase scan radius and repeat scan

Build Instructions:
This is a .NET/C# Application making use of Windows Presentation Foundation (WPF) to display
and control the graphical user interface. To build the software, open the solution in 
visual studio and select build. There are no external libraries necessary to install/configure.

Built on Windows 10 using Visual Studio 2017 Enterprise
