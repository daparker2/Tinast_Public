#Introduction 
This my car dash panel project. It's for my flashed ECU 2013 Subaru BRZ. It probably won't work on anything else. The LCD has a touchscreen, but Windows IoT won't support the nonstandard 1024x600 resolution, so the touchscreen will probably never work right.

There will be some limited customization of PIDs in a json file, but probably no way to deploy changes there without branching. There might be limited datalogging but we're pretty much a long way from that.

##BOM
1. Raspberry Pi 3
2. Raspi UPS Hat
3. Waveshare 7 inch LCD
4. ScanTool OBD MX

I modified the Raspi to power the unit on automatically on ignition. The LCD backlight is also turned on at ignition (it's not on the UPS system).

The UPS is mainly because booting into IoT is damn slow, so I can put it in a low power mode while the ignition is off.

## Layout
The gauge layout should look something like this:

![layout](https://daparker.visualstudio.com/0b16eba6-9218-4f4b-a629-87fe16048574/_api/_versioncontrol/itemContent?repositoryId=7f54c86c-6802-4911-9f5b-9a97bd2317e2&path=%2Fimg%2Flayout.png&version=GBmaster&contentOnly=true&__v=5 "Layout")

The touchscreen won't work right witwh the display being a nonstandard HDMI resolution, so it's not used.

## Prototype setup

TODO: A schematic. 

![schematic](https://daparker.visualstudio.com/0b16eba6-9218-4f4b-a629-87fe16048574/_api/_versioncontrol/itemContent?repositoryId=7f54c86c-6802-4911-9f5b-9a97bd2317e2&path=%2Fimg%2Fproto1.jpg&version=GBmaster&contentOnly=true&__v=5 "Proto1")

Since the touchscreen isn't used, I might try cutting a custom connector for powering the UPS and backlight.