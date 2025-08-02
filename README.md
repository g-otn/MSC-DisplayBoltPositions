# Show Bolt Positions

A Quality-of-Life, MSCLoader mod for the My Summer Car game.

## Setup (Development)
### Requirements
- [Visual Studio 2022 setup with MSCLoader](https://github.com/piotrulos/MSCModLoader/wiki/Install-Visual-Studio-for-MSCLoader)
- Steam copy of My Summer Car [installed with MSCLoader](https://github.com/piotrulos/MSCModLoader/wiki/How-to-install-MSCLoader-using-MSCLInstaller)

### Steps
1. Set environment variable `STEAM_APPS_COMMON` for your user. This is used by Visual Studio project
to find project references (`.dll`) inside the game folder and by post-build script to copy mod build file to Mods folder (assumes MSCLoader Mods folder is in game install)
Example value:
```
C:\...\Steam\steamapps\common
```

2. Open the Visual Studio solution file


## Description
_Don't want to watch a video just to know where exactly the bolts of a specific part are located?
Maybe you just find it hard to see them among lots of pieces and awkward crouching or leaning?
Or you just want to unbolt something quickly?
Then this mod is for you._

This mod displays little semi-transparent spheres as indicators, which can be seen from behind objects, at each bolt of the part you are looking at are.

## Features
- Look at a car part and visualize where the bolts are:

![demo1.gif](https://iili.io/FrYnvRe.gif)
![demo2.gif](https://iili.io/FrYxF4V.gif)
![demo3.gif](https://iili.io/FrcaUXV.gif)

## Options
- Toggle indicators using <kbd>Shift</kbd>+<kbd>G</kbd>
- Change the style of the indicators such as size and color
- Toggle between indicators showing up only in tool mode or not

## Known issues
- **Does not work with every part.** Due to how bolts are coded or behave within certain parts, the indicators may not appear when looking at them. I have not tested every part, especially late-game ones.
- Does not work with most already bolted wheel-related parts. (wishbones, breaking discs, coils, etc) However, if you fully unbolt all bolts of that part, they should show up.
- Some parts may not work immediately, but if you look at the bolt of that part, the indicators will appear (check subframe example image)
- For some parts, or when looking at some parts from certain angles, indicators will show up for multiple parts, and may overlap.

## Acknowledgments
- [Display Bolt Sizes](https://github.com/AToxicNinja/MSC-DisplayBoltSizes)﻿ by AToxicNinja: Inspiration for this mod
- [Developer Toolset II](https://www.nexusmods.com/mysummercar/mods/345)﻿ by Fredrik: Made modding this game bearable
- [MSCLoader](https://www.nexusmods.com/mysummercar/mods/147) by piotrulos﻿: Great [documentation﻿](https://github.com/piotrulos/MSCModLoader/wiki)
