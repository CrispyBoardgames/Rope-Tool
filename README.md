# Rope Tool
 Simple Unity tool to create a rope mesh dynamically.
 - Please read the Design document for an explanation on how the code functions
 - This is a very basic tool that I created. There are probably better ones out there
    with optimized mesh generation. I came up with the algorithm myself after learning 
	the basics of how meshes are created. Thus, I imagine someone who has actually read a 
	book on Computer 3D Graphics could create something better.
 - This tool was made on Unity 2019.4.1f1. I provide no guarantees that it works on other versions.
	But it likely does.
 To use this tool:
	- Import:
		- the "Resources" folder which contains a simple, yet important prefab.
		- EditorLabel.cs
		- All scripts in the "Scripts" folder along with the "Editor" folder
	- Under the Tools Tab, select "Rope Generator"
	- Rope ID is used in the naming of the object, i.e. "Rope1", "Rope2", "Rope{ID}". 
		It is auto-incremented so long as you don't close the tool.
	- Rope Object Name is self-explanatory
	- Number of Sides: Determines the quality of base. Higher -> Circle. Around 10 is enough for small ropes.
	- Radius: Self-explanatory
	- Curve Quality: This value depends on how long your rope is and the curvature required.
		Higher value increases quality but it might be unnecessary. Play around with the values to know what looks good for your needs.
		
	- Material: Drag and drop the material you wish to use. 
	- Spawn Curve Points:
		Creates four points in front of you camera. From left to right: P0, P1, P2, P3
		P0 and P3 are the ends.
		P1 and P2 determine curvature with respect to the ends.
		It uses a simple BezierCurve equation.
		
		Once this button has been clicked, the "Delete Points" and "Create Mesh" buttons are enabled.
		
	- Delete Points:
		Deletes points for whatever reason. Only delete points manually if you closed tool window.
		Deleting without button will make some weird-behavior
		Enables "Spawn Curve Points" button
	
	- Create Mesh:
		Creates the mesh along the points that were created.
		Disables "Delete Points" button
		Disables "Create Mesh" button
		Deletes points
	
	