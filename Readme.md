# Grisaia Extract ![AppIcon](https://i.imgur.com/fDnJLIe.png)

[![Latest Release](https://img.shields.io/github/release/trigger-death/GrisaiaExtractor.svg?style=flat&label=version)](https://github.com/trigger-death/GrisaiaExtractor/releases/latest)
[![Latest Release Date](https://img.shields.io/github/release-date-pre/trigger-death/GrisaiaExtractor.svg?style=flat&label=released)](https://github.com/trigger-death/GrisaiaExtractor/releases/latest)
[![Total Downloads](https://img.shields.io/github/downloads/trigger-death/GrisaiaExtractor/total.svg?style=flat)](https://github.com/trigger-death/GrisaiaExtractor/releases)
[![Creation Date](https://img.shields.io/badge/created-june%202018-A642FF.svg?style=flat)](https://github.com/trigger-death/GrisaiaExtractor/commit/2ae789f18d7387024f2b92b85cc6a21709796ed7)
[![Discord](https://img.shields.io/discord/436949335947870238.svg?style=flat&logo=discord&label=chat&colorB=7389DC&link=https://discord.gg/vB7jUbY)](https://discord.gg/vB7jUbY)

A ripping tool (Primarily for images) for the Grisaia games. *(Phantom Trigger not supported)*

This is basically a polished, easy-to-use wrapper for existing programs that extract Grisaia files. (Although much of the original code has been ported to C#)

**All ripping code written by asmodean:** http://asmodean.reverse.net/pages/exkifint.html

**Additional thanks to ripping documentation on reddit:** https://www.reddit.com/r/grisaia/wiki/ripping

I originally planned to turn this into a full-fledged Windows application that would include sprite combining but after realising I wouldn't reach that point, I wrote up a final console version for use instead.

![Window Preview](https://i.imgur.com/adjsvrN.png)

### [Image Album](https://imgur.com/a/7xouR3f)

## About

* **Created By:** Robert Jordan
* **Ripping By:** asmodean
* **Version:** 1.0.1.0
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

**Example of Sorted Images:** *Note, Windows draws folder thumbnails poorly, they do not actually have alpha issues.*

![Sorting Preview](https://i.imgur.com/cm07Hzd.png)
