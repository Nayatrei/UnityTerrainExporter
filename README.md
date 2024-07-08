![image](https://github.com/Nayatrei/UnityTerrainExporter/assets/36463159/c73fcbe1-52b6-44f1-9014-46c85451929a)


Features
Terrain Height Modification:

Allows the user to raise the terrain height by a specified amount across the entire terrain.
Includes an undo feature that allows reverting the terrain to its previous state before the last modification.
Clone Terrain:

Provides functionality to clone the selected terrain, creating an exact copy with the same configurations and modifications.
Convert Trees to GameObjects:

Converts all tree instances on the selected terrain into individual GameObjects for more detailed manipulation or use in game mechanics.
Generate Terrain Mesh:

Converts the selected terrain into a mesh object, which can be exported as an FBX file for use in other applications or for more complex editing outside of Unity.
Export SplatMaps:

Exports the terrain's splatmaps as TGA files, which are useful for texture mapping in other software or for backup and detailed editing.
User Interface Components
Terrain Raise Amount Slider: Adjusts the amount by which the terrain will be raised or lowered.
Raise Terrain Button: Applies the height adjustment across the terrain.
Undo Raise Button: Reverts the terrain to its previous height state before the last change.
Clone Terrain Button: Creates a duplicate of the selected terrain.
Convert Trees Button: Transforms all tree instances into separate GameObjects.
Generate FBX Button: Converts the terrain into a mesh and prompts the user to save it as an FBX file.
Export SplatMaps Button: Saves all splatmaps from the terrain as TGA files.
