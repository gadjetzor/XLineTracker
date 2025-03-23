Overview
X-Line Tracker is a compact utility for EVE Frontier players to track "x" responses in chat channels. When conducting fleet formations, corporation roll calls, or organizing group activities, this tool automatically counts players who respond with "x" in the selected channel and provides a convenient way to see who has already checked in.

Features
Channel Selection: Automatically detects available chat channels from your EVE Frontier logs
Real-time Tracking: Starts counting player responses when you click "Start"
Player List: Shows a complete list of players who have responded with "x"
Compact Interface: Minimal UI footprint that can be expanded when needed
Auto-refresh: Continuously monitors the chat logs for new responses
How to Use
Launch the application: The tool will automatically search for EVE Frontier chat logs
Select a channel: Choose the channel you want to monitor from the dropdown menu (e.g., Corp, Alliance, Local)
Start counting: Click the "Start" button to begin tracking "x" responses
View responses: Click "Show Players" to see the list of players who have responded
Reset: Click "Reset" to clear the current count and start fresh
Technical Details
The application reads EVE Frontier chat log files located in your Documents folder
Only counts each player once, even if they type "x" multiple times
Only tracks responses that occur after you click "Start"
Works with both local time and UTC timestamp formats
Can handle files that are currently being written to by the game
Requirements
Windows operating system
.NET Framework 4.5 or higher
EVE Frontier with chat logging enabled
Log File Locations
The application searches for log files in the following locations:

Documents\Frontier\logs\Chatlogs
OneDrive\Documents\Frontier\logs\Chatlogs
Tips
Position the application on a secondary monitor or in a corner where you can see the total count
Use the "Hide Players" option to save screen space when you don't need to see the full list
The "Refresh" button will force the application to check for the latest log file if a new one has been created
The application works with EVE Frontier's chat log format that includes timestamps like: [ 2025.03.23 01:45:22 ]