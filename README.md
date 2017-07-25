# Introduction 
<img src="https://daparker.visualstudio.com/_apis/public/build/definitions/0b16eba6-9218-4f4b-a629-87fe16048574/2/badge"></img>

This my car dash panel project. It's for my 2013 Subaru BRZ, and the PIDs should work on most BRZ ECUs provided they aren't running OpenFlash, as that changes the oil temp PID. I haven't tested it on other things. I was originally targetting Windows IoT but the Raspberry Pi was having issues staying connected to my OBD2  scantool that were resolved by moving to an intel compute stick. The story for getting an app launched automatically on Windows is more complicated, but can be achieved by adding a scheduled task to the system.

There will be some limited customization of PIDs in a json file (look at displayConfiguration.json for an example) that can be placed in the user settings for the app, but not much in the way of configuring settings for a custom vehicle. I may also add an in-app settings page (in-app interactivity is mostly non existant at this point, as the touch screen didn't even work on my Raspberry).

[HockeyApp](https://rink.hockeyapp.net/manage/apps/553566)

## Update 3: Here's a brief demo of the gauge panel during normal driving

[![update3](http://img.youtube.com/vi/lsFMVJhmXTw/0.jpg)](https://youtu.be/lsFMVJhmXTw "Demo")

## Installation

See [this link](https://htmlpreview.github.io/?https://github.com/daparker2/Tinast_Public/blob/v1.0.15-beta1/doc/Setting%20up%20a%20raspberry%20pi%20for%20automotive.htm) for installation and bringup instructions of a Raspberry Pi bearing this app.

[Click here](https://github.com/daparker2/Tinast_Public/blob/master/Face-Template.psd) to view the mechanical template I used to create my gauge face (PSD).

## Specs

There's some early specs in the wiki folder that could serve as documentation for the interface. Some of the things there are out of date. For example, the early color scheme I selected was too dim on my display so I selected an even higher contrast one.
