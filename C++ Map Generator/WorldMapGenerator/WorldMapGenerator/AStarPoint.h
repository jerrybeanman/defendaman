#pragma once


/*--------------------------------------------------------------------------------------------  
--  SOURCE:          AStarPoint
--  
--  PROGRAM:         class AStarPoint
--  
--  FUNCTIONS:       
--  
--  DATE:            class AStarPoint
--  
--  DESIGNERS:       Jaegar Sarauer
--  
--  REVISIONS:       NONE
--  
--  PROGRAMMERS:     Jaegar Sarauer
--  
--  NOTES:           This class is a simple node which contains A* data on distance to and from the 
--                   starting and ending position of where the alorithm is attempting to pathfind to.
--                   A number of these points are tied together as parent/child node lists to find the
--                   best path by updating their data and relinking with faster paths.
------------------------------------------------------------------------------------------*/
class AStarPoint {
	public:
	AStarPoint (int x, int y, int f = 0, int g = 0, int h = 0, AStarPoint *p = '\0') : X (x), Y (y), F (f), G (g), H (h), parent(p) {}
	int X;
	int Y;
	//pathfinding numbers
	int F, G, H;

	AStarPoint *parent;

	~AStarPoint () {

	}
};