# Grisaia Extract ![AppIcon](https://i.imgur.com/fDnJLIe.png)
A ripping tool (Primarily for images) for the Grisaia games. *(Phantom Trigger not supported)*

**All ripping code written by asmodean:** http://asmodean.reverse.net/pages/exkifint.html

I originally planned to turn this into a full-fledged Windows application that would include sprite combining but after releasing I wouldn't reach that point, I wrote up a final console version for use instead.

![Window Preview](https://i.imgur.com/adjsvrN.png)

### [Image Album](https://imgur.com/a/7xouR3f)

### [Get Grisaia Extract](https://github.com/trigger-death/GrisaiaExtractor/releases/tag/1.0.0.0)

## About

* **Created By:** Robert Jordan
* **Ripping By:** asmodean
* **Version:** 1.0.0.0
* **Language:** C#/C++

## Features

* Designed to be a bit more user friendly than existing tools. Users no longer need to run a program through the command line or write batch files to convert .hg3's.
* Outputs .hg3's directly to .png's with proper transparency. Now an entire int file can be output in one go with disk space to spare.
* Optional ability to sort all images to a categorized directory based on its name.
* Attempts to locate all existing Grisaia games.
* Fixes a bug with `hgx2bmp.exe` where a small selection of .hg3 didn't get all of their images extracted.
* An .ini settings file called `GrisaiaExtract.ini` can be modified after the program is run to modify certain defaults and settings.

## Cons

* Currently no command line support.
* May not play well when not run through a Windows Console.
