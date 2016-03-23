#pragma once

class AStarPoint {
	public:
	AStarPoint (int x, int y, int f = 0, int g = 0, int h = 0, AStarPoint *p = '\0') : X (x), Y (y), F (f), G (g), H (h), parent(p) {}
	int X;
	int Y;
	//pathfinding numbers
	int F, G, H;

	AStarPoint *parent;

	private:
};