# filtered-logger
A c# addon for Godot that enables a console that shows custom events, and allows filtering by category

It can be configured in the project settings, including the log name and folder where the log will be stored.
It will, by default, output to the GDScript console, C# console, and 2 files(one binary blog file, one text based text file)

PRessing F1 will show or hide the file.



How to Use:
Copy the filteredlogger file into your addons folder. Build once before enabling the plugin.

To Log, Call Logger.Log("your message", "your category") or simply To Log, Call Logger.Log("your message") , which will use the default category. The initial settings should have this as system.

To view the console, press f1. you can then choose filters based off the categories you created.



