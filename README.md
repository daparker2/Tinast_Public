# Introduction 

This my car dash panel project. It's for my 2013 Subaru BRZ, and the PIDs should work on most BRZ ECUs provided they aren't running OpenFlash, as that changes the oil temp PID. I haven't tested it on other things. I was originally targetting Windows IoT but the Raspberry Pi was having issues staying connected to my OBD2  scantool that were resolved by moving to an intel compute stick. The story for getting an app launched automatically on Windows is more complicated, but can be achieved by adding a scheduled task to the system.

There will be some limited customization of PIDs in a json file (look at displayConfiguration.json for an example) that can be placed in the user settings for the app, but not much in the way of configuring settings for a custom vehicle. I may also add an in-app settings page (in-app interactivity is mostly non existant at this point, as the touch screen didn't even work on my Raspberry).

## Update 3: Here's a brief demo of the gauge panel during normal driving

[![update3](http://img.youtube.com/vi/lsFMVJhmXTw/0.jpg)](https://youtu.be/lsFMVJhmXTw "Demo")

Here's what I have so far. This is running on an Intel Atom compute stick connected to a 5V stepper I tapped into my radio fuse. All the wiring is behind the dash. The system interfaces with the ECU over a standard OBD2 bluetooth scan tool. In my care, I'm using a ScanTool OBD2 MX.

## BOM
1. ~~Raspberry Pi 3~~Intel Compute Stick (the Atom model is fine) running Win10 Desktop.
2. ~~Raspi UPS Hat~~
3. Waveshare 7 inch LCD
4. ScanTool OBD2 MX

## Installation

At the moment I am just providing a sideloadable package release. To install on a system, just run the powershell script at the root. The instructions for getting it launched on a device are kind of up in the air, but you can script an automatic launch of the app with the AUMID if you wanted to go that route (it's what I do). Face_Template.psd in the root of the repo is the sheet metal template I used to create my gauge face.

## Prototype setup

TODO: A schematic. 

I have a 5V stepper wired into my intel compute stick through the dash. The HDMI and USB cable going up to the touch screen come out a hole I cut in the fascia on the passenger side console. At some point, I'll probably add a bolt to hold the fashia in place against the extra weight on it as well. Right now it's kind of just taped in place.

## Specs

There's early some specs in the wiki folder that could serve as documentation for the interface. Some of the things there are out of date. For example, the early color scheme I selected was too dim on my display so I selected an even higher contrast one.
