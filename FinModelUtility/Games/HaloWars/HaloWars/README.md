# HaloWarsTools
WIP library to read Halo Wars Definitive Edition files and convert them to common formats for exporting to game engines, 3d modeling software, etc. Based off the binary templates and projects found in the [HaloWarsDocs](https://github.com/HaloMods/HaloWarsDocs) repository.

## Supported File Formats
* `.era` - Ensemble Resource Archive
  * Compressed + encrypted files
* `.xmb` - XML Binary file
  * XML text context
* `.xtd` - Terrain visual data
  * Mesh
  * Ambient Occlusion map
  * Opacity map
* `.xtt` - Terrain texture data
  * Albedo map
* `.ugx` - Universal geometry data
  * Vertex positions/normals/texcoords
  * Materials (limited support)
  * Submeshes
  * Texture paths
* `.vis` - Visual data
  * Meshes
* `.gls` - Global light data
  * Sun inclination/rotation/color
  * Background color
* `.scn` - Main scenario data
  * Object positions/rotations
* `.sc2` - Scenario decoration objects
  * Object positions/rotations
* `.sc3` - Scenario sound objects
  * Object positions/rotations

## How To Use
1. Clone this repository
2. Clone (not download as zip!) [HaloWarsDocs](https://github.com/HaloMods/HaloWarsDocs) so that your folder structure looks like this:
```
Some Folder
    HaloWarsDocs
    HaloWarsTools
```
3. Make sure the following Visual Studio components are installed:

![Requirements](https://raw.githubusercontent.com/srogee/HaloWarsTools/main/Requirements.png?token=ABIQA63R5UYXEVYD7K7SLWDBRBMPW)

4. Build `HaloWarsDocs\PhxTools\PhxTools.sln`. You may have to change an assembly reference to get it to compile, I think I had to replace the TextTransform assembly reference with a newer one.
5. Open `HaloWarsTools\HaloWarsTools.sln`
6. In `Program.cs`, change the following paths so that they point to the right places on your machine
```
string gameDirectory = "Z:\\SteamLibrary\\steamapps\\common\\HaloWarsDE";
string scratchDirectory = "C:\\Users\\rid3r\\Documents\\HaloWarsTools\\scratch";
string outputDirectory = "C:\\Users\\rid3r\\Documents\\HaloWarsTools\\output";
```
8. Build and run the solution!

## Notes
* Textures not mentioned above are stored in various DDS formats and will need to be manually converted for now.
* The first time you run the solution, it will unpack all `.era` files in your specified game directory. This can take a few minutes. Subsequent runs will use the already extracted files in your scratch directory.
