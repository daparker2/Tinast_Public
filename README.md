# Introduction 

This my car dash panel project. It's for my flashed ECU 2013 Subaru BRZ, but the PIDs should work on non-flashed BRZ models. I haven't tested it on other things. I was originally targetting Windows IoT but the Raspi was having issues staying connected to my scantool that were resolved by moving to an intel compute stick. Unfortunately the story for getting an app launched automatically on Windows is more complicated. I may provide a provisioning package to automate starting this automatically later on, but for now I'm mainly evaluating the system for stability and trying to find an anti-glare LCD cover that works.

There will be some limited customization of PIDs in a json file (look at displayConfiguration.json for an example) that can be placed in the user settings for the app, but not much in the way of configuring settings for a custom vehicle. 

## Update 2: It seems to run. In the vehicle.

[![update2](http://img.youtube.com/vi/ap7DToZnHCE/0.jpg)](https://www.youtube.com/watch?v=ap7DToZnHCE "Sort of works")

Here's what I have so far. I had to ditch the pi, as the Bluetooth radio was crummy. Works much better on this Compute Stick, but Windows Desktop definitely isn't permissive in an automotive environment. It doesn't like choppy power, nor does it like suddenly being shut off or power cycled. Still, it unblocked me for now.

The display is not readable. I'm going to build another one that offsets the display pointing toward me (easiest thing I could do is put the left hand bolts on spacers...) as well as coat it in anti glare coating. It's also not waterproof. I'm hoping the anti glare screen protector will keep water out of it when I'm cleaning.

## BOM
1. ~~Raspberry Pi 3~~Intel Compute Stick (the Atom model is fine) running Win10 Desktop.
2. ~~Raspi UPS Hat~~
3. Waveshare 7 inch LCD
4. ScanTool OBD MX

## Installation

At the moment I am just providing a sideloadable package release. To install on a system, just run the powershell script at the root. The instructions for getting it launched on a device are kind of up in the air, but you can script an automatic launch of the app with the AUMID if you wanted to go that route (it's what I do). Face_Template.psd in the root of the repo is the sheet metal template I used to create my gauge face.

## Prototype setup

TODO: A schematic. 

I have a 5V stepper wired into my intel compute stick through the dash. The HDMI and USB cable going up to the touch screen come out a hole I cut in the fascia on the passenger side console. At some point, I'll probably add a bolt to hold the fashia in place against the extra weight on it as well.

## Specs

There's early some specs in the wiki folder that could serve as documentation for the interface. Some of the things there are out of date. For example, the early color scheme I selected was too dim on my display so I selected an even higher contrast one.
