# MeltyTool

![GitHub](https://img.shields.io/github/license/MeltyPlayer/MeltyTool)
![Unit tests](https://github.com/MeltyPlayer/MeltyTool/actions/workflows/test.yml/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/MeltyPlayer/MeltyTool/badge.svg?service=github)](https://coveralls.io/github/MeltyPlayer/MeltyTool)

## Overview

Multitool for viewing/extracting assets from various N64/GCN/3DS/PC games en-masse.

Separate batch scripts are provided for each supported game in order to simplify the process.

![image](https://user-images.githubusercontent.com/15970939/204156969-084cc1a4-1824-45c9-becc-e44ad69668d6.png)

## Suggestions

If you'd like to vote on new games or features that should be supported, please fill out the following Google Form:

[Suggesting Games/Features for Fin Model Utility](https://docs.google.com/forms/d/e/1FAIpQLSfWviQFZxCAuZHYmrcIRvYy3SXRRXo6TmhJVCDsGKR5zvXJvw/viewform)

## Credits

- Acewell, whose [Noesis script](https://forum.xentax.com/viewtopic.php?p=122302#p122302) was used as the basis for reading Dead Space textures.
- [@Adierking](https://github.com/adierking), whose [Unplug](https://github.com/adierking/unplug) tool was referenced to parse Chibi-Robo!'s qp.bin file.
- [@Arisotura](https://github.com/Arisotura), whose [SM64DSe](https://github.com/Arisotura/SM64DSe) tool was heavily referenced to add support for extracting Super Mario 64 DS .bmd files.
- [@Asia81](https://github.com/Asia81), whose [HackingToolkit9DS](https://github.com/Asia81/HackingToolkit9DS-Deprecated-) was originally used to extract the contents of 3DS .cia files.
- [@Chadderz121](https://github.com/Chadderz121), aka Chadderz, whose [CTools](https://www.chadsoft.co.uk/wiicoder/) suite was originally used to read J3dGraph .bmd texture formats.
- [CloudModding](https://wiki.cloudmodding.com), which provides wikis documenting various Zelda games and their internal formats (e.g. [F3DZEX2](https://wiki.cloudmodding.com/oot/F3DZEX2))
- cooliscool, whose [Utility of Time](http://wiki.maco64.com/Tools/Utility_of_Time) program was used as the basis for the UI and F3DZEX2/F3DEX2 importer.
- [@Cuyler36](https://github.com/Cuyler36), aka CulyerAC, whose [RELDumper](https://github.com/Cuyler36/RELDumper) is used to extract the contents of .rel/.map files.
- [@DavidSM64](https://github.com/DavidSM64), whose [Quad64](https://github.com/DavidSM64/Quad64) is used as the base for the SM64 importer.
- [@DickBlackshack](https://github.com/DickBlackshack), aka Rich Whitehouse, whose [Noesis](https://richwhitehouse.com/index.php?content=inc_projects.php&showproject=91) model viewer's plugins were leveraged to add support for various formats.
- [@Dummiesman](https://github.com/Dummiesman), whose [AngelStudiosBlenderAddon](https://github.com/Dummiesman/AngelStudiosBlenderAddon) Blender plugin was heavily referenced to import Midnight Club 2 assets.
- [@Emill](https://github.com/Emill), whose [Nintendo 64 Fast3D renderer](https://github.com/Emill/n64-fast3d-engine) was heavily referenced to vastly improve TMEM emulation.
- [@EstevanBR](https://github.com/EstevanBR), whose [DATReaderC](https://github.com/EstevanBR/DATReaderC) was referenced as the starting point for the .dat importer.
- [@EXOK](https://github.com/EXOK), whose [Celeste64](https://github.com/EXOK/Celeste64) source was heavily referenced to add support for Celeste 64 maps.
- [@follyfoxe](https://github.com/follyfoxe), whose [SpmViewer](https://github.com/follyfoxe/SpmViewer) was referenced to add support for importing Paper Mario: The Thousand Year Door's animated group models.
- [@Gericom](https://github.com/Gericom), whose [MKDS Course Modifier](https://www.romhacking.net/utilities/1285/) program was used as the basis for the J3dGraph .bmd importer.
- [@gibbed](https://github.com/gibbed), whose [Gibbed.Visceral](https://github.com/gibbed/Gibbed.Visceral) library was used as the base of the .str extractor.
- [@Gota7](https://github.com/Gota7), whose [SM64DSe-Ultimate](https://github.com/Gota7/SM64DSe-Ultimate) tool was heavily referenced to add support for extracting Super Mario 64 DS .bmd files.
- [Hack64](https://hack64.net), which provided documentation for various N64 microcode/display list formats (e.g. [Fast3D Display List Commands](https://hack64.net/wiki/doku.php?id=super_mario_64:fast3d_display_list_commands))
- [@hci64](https://github.com/hcs64), whose [vgm_ripping](https://github.com/hcs64/vgm_ripping) tool was ported to add support for parsing .ast PCM16 data.
- [@HimeWorks](https://github.com/HimeWorks), whose [Noesis plugins](https://himeworks.com/noesis-plugins/) were used to add support for various formats.
- [@intns](https://github.com/intns), whose [MODConv](https://github.com/intns/MODConv) tool was used as the basis for the Pikmin 1 .mod importer.
- [@IronLanguages](https://github.com/IronLanguages), whose [IronPython](https://github.com/IronLanguages/ironpython3) was used to add support for calling Python plugins from C#.
- [@jam1garner](https://github.com/jam1garner), whose [Smash-Forge](https://github.com/jam1garner/Smash-Forge) tool was referenced to add support for Melee models.
- jdh, whose [Virtual Reality Walt Disney World](https://web.archive.org/web/20040608032334/https://vrwdw.tripod.com/) was bundled in the viewer for the sake of preservation.
- [@jkbenaim](https://github.com/jkbenaim), whose [leotools](https://github.com/jkbenaim/leotools) library was heavily referenced to add support for Mario Artist.
- [@Julgodis](https://github.com/Julgodis), whose [picori](https://github.com/Julgodis/picori) library was referenced to implement parsing .ciso files. 
- [@Justin Aquadro](https://github.com/jaquadro), aka [Retriever II](https://www.mfgg.net/index.php?act=user&param=01&uid=2), whose [documentation](https://www.emutalk.net/threads/pm-ttyd-model-file-format-pet-project.27613/) was referenced to add support for importing Paper Mario: The Thousand Year Door's animated group models.
- Killa B, who [documented](https://www.zophar.net/fileuploads/3/21546xutra/picrossleveldata.txt) the Mario Picross format.
- [@KillzXGaming](https://github.com/KillzXGaming), whose [Switch-Toolbox](https://github.com/KillzXGaming/Switch-Toolbox) was referenced to add support for LZSS decompression.
- [@kornman00](https://github.com/kornman00), aka [@KornnerStudios](https://github.com/KornnerStudios), for documenting the Halo Wars formats in [HaloWarsDocs](https://github.com/HaloMods/HaloWarsDocs) and providing the [KSoft suite](https://github.com/KornnerStudios/KSoft) to extract the contents of the game.
- [@LagoLunatic](https://github.com/LagoLunatic), whose [gclib](https://github.com/LagoLunatic/gclib) library's YAY0/YAZ0 decompression logic was ported to C#.
- [@leftp](https://github.com/leftp), whose [SharpDirLister](https://github.com/EncodeGroup/SharpDirLister) API was used to dramatically improve listing out the file hierarchy.
- [@LogicAndTrick](https://github.com/LogicAndTrick), whose [sledge-formats](https://github.com/LogicAndTrick/sledge-formats) library was used to parse Celeste 64 map files.
- [@LordNed](https://github.com/LordNed), whose [J3D-Model-Viewer](https://github.com/LordNed/J3D-Model-Viewer) tool and [JStudio](https://github.com/LordNed/JStudio) library were referenced to fix bugs in the J3dGraph .bmd importer.
- Luca Elia, whose [Rolling Madness 3D](http://www.lucaelia.com/games.php) was bundled in the viewer for the sake of preservation.
- [@LuizZak](https://github.com/LuizZak), whose [FastBitmap](https://github.com/LuizZak/FastBitmap) library was used to optimize working with bitmaps and inspired some optimizations in Fin's image processing methods.
- [@M-1-RLG](https://github.com/M-1-RLG), aka M-1, as his [io_scene_cmb](https://github.com/M-1-RLG/io_scene_cmb) Blender plugin was used as the basis for the .cmb importer. He also provided [thorough documentation](https://github.com/M-1-RLG/010-Editor-Templates/tree/master/Grezzo) on each of Grezzo's formats.
- [@magcius](https://github.com/magcius), aka Jasper, as their model viewer [noclip.website](https://github.com/magcius/noclip.website) was referenced to add support for importing .csab files and Paper Mario: The Thousand Year Door's animated group models.
- Mega-Mario, whose [documentation](https://kuribo64.net/get.php?id=KBNyhM0kmNiuUBb3) was referenced to add support for parsing SM64DS models.
- [@mhvuze](https://github.com/mhvuze), whose [3ds-xfsatool](https://github.com/mhvuze/3ds-xfsatool) is used to extract XFSA archives.
- [@Nanook](https://github.com/Nanook), whose [NKitv1](https://github.com/Nanook/NKitv1) is used to read NKit ISOs.
- [@neimod](https://github.com/neimod), who originally created [ctrtool](https://github.com/neimod/ctr), which is used to extract the contents of 3DS .cias.
- [@NerduMiner](https://github.com/NerduMiner), whose [Pikmin1Toolset](https://github.com/NerduMiner/Pikmin1Toolset) was ported from C++ to C# to add texture support for Pikmin 1.
- [@nickbabcock](https://github.com/nickbabcock), whose [Pfim](https://github.com/nickbabcock/Pfim) library is used to extract the contents of .dds images.
- [@nickworonekin](https://github.com/nickworonekin), whose [narchive](https://github.com/nickworonekin/narchive) tool was heavily referenced to parse Nitro's .narc archives.
- [@nico](https://github.com/nico), aka thakis, whose [szstools](http://amnoid.de/gc/) CLI was originally used to extract the contents of GameCube .isos and whose [ddsview](http://www.amnoid.de/ddsview/index.html) tool was referenced to fix Halo Wars DXT5A/ATI1/BC4 parsing.
- [@Nominom](https://github.com/Nominom), whose [BCnEncoder.NET](https://github.com/Nominom/BCnEncoder.NET) library is used to parse DXT1/3/5 images.
- [@oxyplot](https://github.com/oxyplot), whose [oxyplot-avalonia](https://github.com/oxyplot/oxyplot-avalonia) library is used to render graphs of animation values.
- [@PistonMiner](https://github.com/PistonMiner), whose [ttyd-tools](https://github.com/PistonMiner/ttyd-tools) documentation was referenced to add support for importing Paper Mario: The Thousand Year Door's animated group models.
- [@Ploaj](https://github.com/Ploaj), whose [HSDLib](https://github.com/Ploaj/HSDLib) library was heavily referenced to fix the Super Smash Bros. Melee .dat importer (since HSDLib has nearly perfect accuracy!!) and [Metanoia](https://github.com/Ploaj/Metanoia) library was heavily referenced to add support for parsing Level-5 formats. 
- [@polym0rph](https://github.com/polym0rph), whose [GLSL.tmbundle](https://github.com/polym0rph/GLSL.tmbundle) was used to add TextMate syntax highlighting support for .glsl files in the material viewer.
- [@RenolY2](https://github.com/RenolY2), aka Yoshi2, whose [bw-model-viewer](https://github.com/RenolY2/bw-model-viewer) tool was used as the basis for the .modl importer.
- [@revel8n](https://github.com/revel8n), whose [Smashboards thread](https://smashboards.com/threads/melee-dat-format.292603/) was referenced to add support for the .dat importer.
- [@Sage-of-Mirrors](https://github.com/Sage-of-Mirrors), whose [SuperBMD](https://github.com/Sage-of-Mirrors/SuperBMD) tool was referenced to clean up the J3dGraph .bmd logic and whose [Booldozer](https://github.com/Sage-of-Mirrors/Booldozer) tool was used as the basis for the .mdl importer.
- [@SceneGate](https://github.com/SceneGate), whose [Yarhl](https://github.com/SceneGate/Yarhl), [Ekona](https://github.com/SceneGate/Ekona), and [Lemon](https://github.com/SceneGate/Lemon) libraries were used to rip files from DS and 3DS ROMs.
- [@shravan2x](https://github.com/shravan2x), whose [Gameloop.Vdf](https://github.com/shravan2x/Gameloop.Vdf) library is used to deserialize the contents of Steam's .vdf files.
- [@sinshu](https://github.com/sinshu), whose [MeltySynth](https://github.com/sinshu/meltysynth) library is used to import and play MIDI files.
- [@sopoforic](https://github.com/sopoforic), whose [cgrr-mariospicross](https://github.com/sopoforic/cgrr-mariospicross) tool was referenced for ripping Mario's Picross puzzles.
- [@speps](https://github.com/speps), whose [LibTessDotNet](https://github.com/speps/LibTessDotNet) library is used to triangulate VRML polygons.
- [@srogee](https://github.com/srogee), as his [HaloWarsTools](https://github.com/srogee/HaloWarsTools) program was used as the basis of the Halo Wars importer.
- [@SuperHackio](https://github.com/SuperHackio), whose [Hack.io](https://github.com/SuperHackio/Hack.io) library was referenced to improve the J3dGraph .bmd parser.
- [@TTEMMA](https://github.com/TTEMMA), whose [Gar/Zar UnPacker v0.2](https://gbatemp.net/threads/release-gar-zar-unpacker-v0-1.385264/) tool is used to extract the contents of Ocarina of Time 3D files.
- Twili, for reverse-engineering and documenting the .zar archive format and various additional research.
- [@puggsoy](https://github.com/puggsoy), whose [GMS-Explorer](https://github.com/puggsoy/GMS-Explorer) was heavily referenced to parse GameMaker's data.win files.
- [@UnderminersTeam](https://github.com/UnderminersTeam), whose [UndertaleModTool](https://github.com/UnderminersTeam/UndertaleModTool) was heavily referenced to parse GameMaker images.
- [@vgmstream](https://github.com/vgmstream), whose [https://github.com/vgmstream/vgmstream](https://github.com/vgmstream/vgmstream) tool was ported to add support for parsing .ast ADPCM data.
- [@VPenades](https://github.com/vpenades), whose [SharpGLTF](https://github.com/vpenades/SharpGLTF) library is used to write GLTF models.
- [@Wiimm](https://github.com/Wiimm), who created the [wiimms-iso-tools](https://github.com/Wiimm/wiimms-iso-tools) used to extract the contents of Wii .isos.
- [@xdanieldzd](https://github.com/xdanieldzd), for reverse-engineering and documenting the .cmb and .csab formats. In addition, their [Scarlet](https://github.com/xdanieldzd/Scarlet) tool was referenced for dumping .gar files.
- xyz, whose [Infinite Ground Grid shader](https://godotshaders.com/shader/infinite-ground-grid/) was used to render a better-looking grid in the viewer.

## Supported formats/games

*Not all of the models for each game are currently supported, and not every feature of each model will be accurately recreated in the output GLTF/FBX files. To flag any broken/missing models that you'd like to see fixed, please feel free to file feedback via the Issues tab above.*

- /a (Animated group models) (GCN)
  - Paper Mario: The Thousand Year Door (`paper_mario_the_thousand_year_door.[ciso/gcm/iso/nkit.iso]`)
- .ase.mesh (PC)
  - [Rolling Madness 3D](http://www.lucaelia.com/games.php)
- J3dGraph (.bmd/.bdl) (GCN)
  - Mario Kart: Double Dash (`mario_kart_double_dash.[ciso/gcm/iso/nkit.iso]`)
  - Pikmin 2 (`pikmin_2.[ciso/gcm/iso/nkit.iso]`)
  - Super Mario Sunshine (`super_mario_sunshine.[ciso/gcm/iso/nkit.iso]`)
- .bmd (DS)
  - Super Mario 64 DS (`super_mario_64_ds.nds`) 
- .cmb (3DS)
  - Luigi's Mansion 3D (`luigis_mansion_3d.[3ds/cci/cia]`)
  - Majora's Mask 3D (`majoras_mask_3d.[3ds/cci/cia]`)
  - Ocarina of Time 3D (`ocarina_of_time_3d.[3ds/cci/cia]`)
- .d3d (PC)
  - Game Maker
- .geo (PC)
  - Dead Space (Steam/Epic Games)
- .glo (PC)
  - Glover (Steam)
- HSD, aka HAL sysdolphin (.dat) (GCN)
  - Chibi-Robo! (`chibi_robo.[ciso/gcm/iso/nkit.iso]`)
  - Super Smash Bros. Melee (`super_smash_bros_melee.[ciso/gcm/iso/nkit.iso]`)
- .map (PC)
  - Celeste 64 
- .mod (GCN)
  - Pikmin (`pikmin_1.[ciso/gcm/iso/nkit.iso]`)
- .modl/.out (GCN/WII)
  - Battalion Wars (`battalion_wars_1.[ciso/gcm/iso/nkit.iso]`)
  - Battalion Wars 2 (`battalion_wars_2.iso`)
- (Picross puzzles)
  - Mario's Picross (`marios_picross_1.gb`)
  - Picross 2 (AKA Mario's Picross 2) (`marios_picross_2.gb`)
- .tstlt (N64)
  - Mario Artist: Talent Studio (rip these first with [mfs_manager](https://github.com/LuigiBlood/mfs_manager) and include them in `roms/mario_artist/prereqs`)
- .vis/.xtd (PC)
  - Halo Wars (Steam)
- VRML (.wrl)
  - [Virtual Reality Walt Disney World](https://web.archive.org/web/20040608032334/https://vrwdw.tripod.com/)
- .xc (3DS)
  - Professor Layton vs. Phoenix Wright (`professor_layton_vs_phoenix_wright.[3ds/cci/cia]`)

*Note:*
- For GameCube ROMs, files with `.ciso`, `.gcm`, `.iso`, or `.nkit.iso` extensions should work.
- For 3DS ROMs, files with `.3ds`, `.cci`, or `.cia` extensions should work.

## Usage guide

Download a release via the Releases tab (for stability), or via the green "Code" button above (for latest). Extract this somewhere on your machine.

Then, follow the steps below.

### Viewing/extracting models from ROMs automatically

1) Drop ROM(s) in the `cli/roms/` directory. Make sure their names match the corresponding name above! (	Games with brackets should accept any of the listed extensions.)

#### Viewing/extracting models via the UI

2) Double-click the `launch_ui.bat` file in the `cli/` directory. This will first rip all of the files from the game, and then gather the currently supported models. This can take a while on the first execution, but future executions will reuse the exported files.
3) The viewer will open with nothing initially selected. Navigate through the models in the left-hand column and select a model you wish to view/extract. Once selected, the model will be loaded/rendered with as close of a representation to the original game as possible.
4) The model can be extracted by clicking the <img alt="export" src="https://user-images.githubusercontent.com/15970939/204157246-43aa2e0d-628b-49c7-abb9-bfc2d52b16a0.png" height="16px" /> icon in the top bar. It will automatically be placed relative to the `cli/out/[game_name]/` directory, in a chain of folders matching the ROM's original file structure. The model may not look exactly as it appeared in the viewer, because some material features are not supported in common model formats.
5) All of the models in a given directory can be extracted by clicking the <img alt="export_all" src="https://user-images.githubusercontent.com/15970939/204157353-7b3dffb6-7061-4c3d-b90f-461764598b4d.png" height="16px" /> icon in the top bar. Current progress will be displayed in the status bar on the bottom while models are extracted in the background. To stop this operation, click the <img alt="cancel" src="https://user-images.githubusercontent.com/15970939/230075836-a38a1e1b-489f-4240-ae63-12edaa6050d2.png" height="16px" /> icon on the right side of the status bar.

#### Extracting all files from a given game via a batch script

2) Double-click the corresponding `rip_[game_name].bat` file in the `cli/` directory. This will first rip all of the files from the game, and then the currently supported models. This can take a while on the first execution, but future executions will reuse the exported files.
3) Extracted models will appear within the corresponding `cli/out/[game_name]/` directory.  

### Converting models

The tool can also be used to convert models via the command-line like so:
```
  cli/tools/universal_asset_tool/universal_asset_tool.exe convert -i [input files] -o [output file]
```

Here's an example with some files from Pikmin 1:
```
  cli/tools/universal_asset_tool/universal_asset_tool.exe convert -i chappy.mod chappy.anm -o chappy.fbx
```

The best plugin will automatically be detected based on the list of input files. For a list of supported plugins, use the following command:
```
  cli/tools/universal_asset_tool/universal_asset_tool.exe list_plugins
```

## Notes about exported models

- Both GLTF (.glb) and FBX models will be created when exporting, since each format has slightly different compatibility. FBX is generally preferred due to supporting additional UV channels, but GLTF is better supported within model viewer programs such as Noesis.
- The materials for some models are broken/incomplete due to the complexity of recreating fixed-function pipeline effects in a standalone model. These will need to be manually recreated in whichever program you import the model into.
