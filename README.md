# Unity_BuildingSystem
Simple building system in Unity. Place objects into the scene from a top down perpsective, switch from building to viewing mode to move around the scene. Objects can be edited and deleted, and scaled. Placement snapping is also implemented, can place objects onto, and on top of, each other. A procedural wall generation feature is added, a wall can be created from clicks at two points, representing either end. The wall generation achieved by simply creating a cuboid mesh, and aligning it with both points.

Bugs & other:
-sometimes snapping collision bugs when changing scale 
-wall collision detecion, was implemented but removed due to bugs
-wall normals are strange, can solve by adding verts or manually configuring normals
-code needs refactoring/ cleaning
