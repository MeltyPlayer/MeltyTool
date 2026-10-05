TODO

- pin sidebars
- Add checkboxes for each model element
- See if you can find flags for material blending and such
- Need to hide files that have no geometry
- Need to add concept of layers or something. For example, book.geo
- Once everything is working, export models and upload to Modeler's Resource
- Refactor
- PR

Pain Points
Workflow

- Batch files to extract models are clunky and unnecessary. It's already a tool that can be used via command line, and they just run one simple command.
- It seems like the WinForms UI loads models almost instantly when selecting them, but the Avalonia UI takes a couple seconds. Am I going insane?
- warn if exiting without saving config, otherwise seems broken.

Nice to have

- FoV should be adjustable with the scroll wheel or something
- When loading a model, there should be an option to automatically position the camera such that the model bounds are in frame and the camera is looking at the center of the bounds
- Merge the console output into the UI in a "Log" tab or something
- Would be great to have buttons to view vertex colors, normals, toggle backface culling, toggle wireframe, toggle lighting, etc
  \_ Make it so you can center bounds on axis

Readme

- Readme could be better organized, list of supported games/formats should be first and should be a table
- Readme should include guide on adding a new model format with example code
- Readme should say what project to set as startup project (UniversalAssetTool.Ui vs UniversalAssetTool.Ui.Avalonia.Desktop)

Maintainability

- It should be easier to add a new game extractor.
- Project structure and naming conventions are confusing and inconsistent. Repo is MeltyTool, sln is FinModelUtility, project is UniversalAssetTool. Decide on a name please :)
- All warnings should be resolved, either fixed or suppressed
- Fully migrate to one or the other UI frameworks, don't maintain 2 different ones.
- If choosing Avalonia - polish it up, the current styling is all over the place (different fonts and font sizes, some things have drop shadows, some things don't)
- If choosing Avalonia - change the way the sidebars get hidden. Have a little button on them to open/close them instead of the weird hover thing, and default them open
