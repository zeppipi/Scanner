using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/*
    I will be following the Sebastian Lague tutorial for this
    This will be a huge pat in the back if I can pull this off
    https://youtube.com/playlist?list=PLFt_AvWsXl0eZgMK_DT5_biRkWXftAOf9
*/

public class Generator : MonoBehaviour
{
    // I'll just use the 'public' keyword again

    public int width;
    public int height;

    public string seed;
    public bool useRandomSeed;

    [Range(0, 100)]public int fillPercent;
    [Range(0, 10)]public int smoothingIterations;
    [Range(0, 10)]public int neighbourThreshold = 4;
    [Range(0, 10)]public int borderSize = 5;    // Additive to the map size

    public int squareSize = 1;

    public int wallRegionThreshold = 50;    // The minimum size of a wall region should be 
    public int regionThreshold = 50;    // The minimum size of a region should be 
    public int passageRadius = 1;   // The size of the created passageways between regions

    public bool debugMode = false;  // Turns on gizmos

    // A 2D array
    int[,] map;

    // public void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //     {
    //         GenerateMap();
    //     }
    // }

    public void GenerateMap() {
        map = new int[width, height];
        RandomFillMap();

        for (int i = 0; i < smoothingIterations; i++) {
            SmoothMap();
        }
        
        // Clean up regions
        ProcessMap();

        // Create a border of walls around the map
        int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

        for (int x = 0; x < borderedMap.GetLength(0); x++) 
        {
            for (int y = 0; y < borderedMap.GetLength(1); y++)
            {
                if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) 
                {
					borderedMap[x,y] = map[x-borderSize,y-borderSize];
				}
				else 
                {
					borderedMap[x,y] =1;
				}
            }
        }

        // Call MeshGenerator
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        meshGen.GenerateMesh(borderedMap, squareSize);
    }

    void ProcessMap()
    {
        /*
            Remove small regions of walls and empty space
        */

        // Remove walls
        List<List<Coord>> wallRegions = GetRegions(1);

        foreach(List<Coord> wallRegion in wallRegions)
        {
            if(wallRegion.Count < wallRegionThreshold)
            {
                foreach(Coord tile in wallRegion)
                {
                    map[tile.tileX, tile.tileY] = 0;
                }
            }
        }

        // Remove empty space
        List<List<Coord>> regions = GetRegions(0);
        List<Room> remainingRooms = new List<Room>();

        foreach(List<Coord> region in regions)
        {
            if(region.Count < regionThreshold)
            {
                foreach(Coord tile in region)
                {
                    map[tile.tileX, tile.tileY] = 1;
                }
            }
            else
            {
                remainingRooms.Add(new Room(region, map));
            }
        }

        // Sort rooms before connecting them
        remainingRooms.Sort();
        remainingRooms[0].isMainRoom = true;
        remainingRooms[0].isAccessibleFromMainRoom = true;

        ConnectClosestRooms(remainingRooms);
    }

    void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false)
    {
        /*
            Connects the closest rooms
        */

        List<Room> roomListA = new List<Room>();
        List<Room> roomListB = new List<Room>();

        if(forceAccessibilityFromMainRoom)
        {
            foreach(Room room in allRooms)
            {
                if(room.isAccessibleFromMainRoom)
                {
                    roomListB.Add(room);
                }
                else
                {
                    roomListA.Add(room);
                }
            }
        }
        else
        {
            roomListA = allRooms;
            roomListB = allRooms;
        }

        int bestDistance = 0;
		Coord bestTileA = new Coord ();
		Coord bestTileB = new Coord ();
		Room bestRoomA = new Room ();
		Room bestRoomB = new Room ();
		bool possibleConnectionFound = false;

		foreach (Room roomA in roomListA) 
        {
			if(!forceAccessibilityFromMainRoom)
            {
                possibleConnectionFound = false;
                if(roomA.connectedRooms.Count > 0)
                {
                    continue;
                }
            }

			foreach (Room roomB in roomListB) 
            {
				if (roomA == roomB || roomA.IsConnected(roomB)) 
                {
					continue;
				}

				for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA ++) 
                {
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB ++) 
                    {
						Coord tileA = roomA.edgeTiles[tileIndexA];
						Coord tileB = roomB.edgeTiles[tileIndexB];
						int distanceBetweenRooms = (int)(Mathf.Pow (tileA.tileX-tileB.tileX,2) + Mathf.Pow (tileA.tileY-tileB.tileY,2));

						if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) 
                        {
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}

			if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}

        if(possibleConnectionFound && forceAccessibilityFromMainRoom)
        {
            CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
            ConnectClosestRooms(allRooms, true);
        }
        
        if(!forceAccessibilityFromMainRoom)
        {
            ConnectClosestRooms(allRooms, true);
        }
    }

    void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB)
    {
        /*
            Creates a passage between two rooms
        */

        Room.ConnectRooms(roomA, roomB);

        List<Coord> line = GetLine(tileA, tileB);
        foreach(Coord c in line)
        {
            DrawCircle(c, passageRadius);
        }
    }

    void DrawCircle(Coord c, int r)
    {
        /*
            Creates a circle
            Used to determine how thick a created passage is and to actually drill the passage
        */

        for(int x = -r; x <= r; x++)
        {
            for(int y = -r; y <= r; y++)
            {
                if(x*x + y*y <= r*r)
                {
                    int realX = c.tileX + x;
                    int realY = c.tileY + y;
                    
                    if(IsInMapRange(realX, realY))
                    {
                        map[realX, realY] = 0;
                    }
                }
            }
        }

    }

    List<Coord> GetLine(Coord from, Coord to)
    {
        /*
            Gets the list of coordinates that lays on the line connecting from two points
        */

        List<Coord> line = new List<Coord>();

        int x = from.tileX;
        int y = from.tileY;

        int dx = to.tileX - from.tileX;
        int dy = to.tileY - from.tileY;

        bool inverted = false;
        
        int step = Math.Sign(dx);
        int gradientStep = Math.Sign(dy);

        int longest = Mathf.Abs(dx);
        int shortest = Mathf.Abs(dy);

        if (longest < shortest)
        {
            inverted = true;
            
            step = Math.Sign(dy);
            gradientStep = Math.Sign(dx);
            longest = Mathf.Abs(dy);
            shortest = Mathf.Abs(dx);
        }

        int gradientAccumulation = longest / 2;
        for (int i = 0; i < longest; i++)
        {
            line.Add(new Coord(x, y));

            if (inverted)
            {
                y += step;
            }
            else 
            {
                x += step;
            }

            gradientAccumulation += shortest;
            if (gradientAccumulation >= longest)
            {
                if (inverted)
                {
                    x += gradientStep;
                }
                else 
                {
                    y += gradientStep;
                }
                gradientAccumulation -= longest;
            }
        }

        return line;
    }

    Vector3 CoordToWorldPoint(Coord tile)
    {
        /*
            Converts a tile coordinate to a world coordinate
        */

        return new Vector3(-width / 2 + 0.5f + tile.tileX, 2, -height / 2 + 0.5f + tile.tileY);
    }

    List<List<Coord>> GetRegions(int tileType)
    {
        /*
            Returns a list of regions
        */

        List<List<Coord>> regions = new List<List<Coord>>();
        int[,] mapFlags = new int[width, height]; // 0 = not checked, 1 = checked

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if(mapFlags[x, y] == 0 && map[x, y] == tileType)
                {
                    List<Coord> newRegion = GetRegionTiles(x, y);
                    regions.Add(newRegion);

                    // Mark the tiles as checked
                    foreach (Coord tile in newRegion)
                    {
                        mapFlags[tile.tileX, tile.tileY] = 1;
                    }
                }
            }
        }

        return regions;
    }

    List<Coord> GetRegionTiles(int startX, int startY)
    {
        /*
            Gets a list of coordinates which represents a region in the cave
            We are using the 'Flood Flow' algorithm, but honestly, it looks like BFS
        */

        List<Coord> tiles = new List<Coord>();
        int[,] mapFlags = new int[width, height]; // 0 = not checked, 1 = checked
        int tileType = map[startX, startY];

        Queue<Coord> queue = new Queue<Coord>();
        queue.Enqueue(new Coord(startX, startY));
        mapFlags[startX, startY] = 1;   // Start the queue

        // "BFS" loop
        while (queue.Count > 0)
        {
            Coord tile = queue.Dequeue();   // serve
            tiles.Add(tile);

            // Loop through all the adjacencies of the tile
            for (int x = tile.tileX - 1; x <= tile.tileX + 1; x ++)
            {
                for (int y = tile.tileY - 1; y <= tile.tileY + 1; y ++)
                {
                    if (IsInMapRange(x, y) && (y == tile.tileY || x == tile.tileX))
                    {
                        if (mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            queue.Enqueue(new Coord(x, y));
                        }
                    }
                }
            }
        }

        return tiles;
    }

    public bool IsInMapRange(int x, int y)
    {
        /*
            Checks if a tile is in the map
        */

        return x >= 0 && x < width && y >= 0 && y < height;

    }

    void RandomFillMap() {
        /*
            Makes a random tile of 1s and 0s
        */

        if (useRandomSeed) {
            seed = DateTime.Now.ToString();
        }

        System.Random pseudoRandom = new System.Random(seed.GetHashCode());

        // Filling the map with 1s and 0s
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++){
                // Cover the edges of the screen with walls
                if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
                    map[x, y] = 1;
                } else {
                    // same as "map[x, y] = (pseudoRandom.Next(0, 100) < fillPercent) ? 1 : 0;"
                    if (pseudoRandom.Next(0, 100) < fillPercent) {
                        map[x, y] = 1;
                    } else {
                        map[x, y] = 0;
                    }
                }
            }
        }
    }

    void SmoothMap() {
        /*
            Smooths the noise from 'RandomFillMap()'
        */

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int neighbouringTiles = wallCount(x, y);

                if (neighbouringTiles > neighbourThreshold) {    // Becomes wall when incased
                    map[x, y] = 1;
                } else if (neighbouringTiles < neighbourThreshold) { // Becomes empty when isolated
                    map[x, y] = 0;
                }
            }
        }
    }

    int wallCount(int gridX, int gridY)
    {
        /*
            Does what you think it does, given a point, count it's neighbouring walls
        */
        int wallCount = 0;

        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if (IsInMapRange(neighbourX, neighbourY)) {
                    if (neighbourX != gridX || neighbourY != gridY) {
                        wallCount += map[neighbourX, neighbourY];
                    }
                } else {
                    wallCount++;
                }
            }
        }

        return wallCount;
    }

    struct Coord
    {
        /*
            A data type to hold coordinates
            Note: allegedly the use of this can be replaced with Vector2Int
        */

        public int tileX;
        public int tileY;

        public Coord(int x, int y) {
            tileX = x;
            tileY = y;
        }
    }

    class Room : IComparable<Room>
    {
        /*
            Class that describes a room in the cave
            Note: Turn to Icomparable so that we can sort the rooms by size
        */

        public List<Coord> tiles;
        public List<Coord> edgeTiles;
        public List<Room> connectedRooms;
        public int roomSize;
        
        public bool isAccessibleFromMainRoom;
        public bool isMainRoom;

        // Constructor overloading
        public Room(){}
        public Room(List<Coord> roomTiles, int[,] map)
        {
            tiles = roomTiles;
            roomSize = tiles.Count;
            connectedRooms = new List<Room>();

            edgeTiles = new List<Coord>();
            foreach(Coord tile in tiles)
            {
                for (int x = tile.tileX - 1; x <= tile.tileX + 1; x ++)
                {
                    for (int y = tile.tileY - 1; y <= tile.tileY + 1; y ++)
                    {
                        if (x == tile.tileX || y == tile.tileY)
                        {
                            if (map[x, y] == 1)
                            {
                                edgeTiles.Add(tile);
                            }
                        }
                    }
                }
            }
        }

        public static void ConnectRooms(Room roomA, Room roomB)
        {
            /*
                Takes two rooms and add each other's room in their recpective 'connectedRooms' list
            */
            
            if(roomA.isAccessibleFromMainRoom)
            {
                roomB.SetAccessibleFromMainRoom();
            } else if (roomB.isAccessibleFromMainRoom)
            {
                roomA.SetAccessibleFromMainRoom();
            }

            roomA.connectedRooms.Add(roomB);
            roomB.connectedRooms.Add(roomA);
        }

        public bool IsConnected(Room otherRoom)
        {
            /*
                Checks if two rooms are connected
            */

            return connectedRooms.Contains(otherRoom);
        }

        public int CompareTo(Room otherRoom)
        {
            /*
                Compares the size of two rooms
            */

            return otherRoom.roomSize.CompareTo(roomSize);
        }

        public void SetAccessibleFromMainRoom()
        {
            /*
                Set a rooms accessibility to the main room
            */

            if(!isAccessibleFromMainRoom)
            {
                isAccessibleFromMainRoom = true;
                foreach(Room connectedRoom in connectedRooms)
                {
                    connectedRoom.SetAccessibleFromMainRoom();
                }
            }
        }
    }

    public int[,] getMap()
    {
        /*
            Lets other scripts access the map
        */
        return map;
    }

    public int getMapWidth()
    {
        /*
            Lets other scripts access the map width
        */
        return width;
    }

    public int getMapHeight()
    {
        /*
            Lets other scripts access the map height
        */
        return height;
    }
    
    void OnDrawGizmos() {
		if (map != null && debugMode) {
			for (int x = 0; x < width; x ++) {
				for (int y = 0; y < height; y ++) {
					Gizmos.color = (map[x,y] == 1)?Color.black:Color.white;
					Vector3 pos = new Vector3(-width/2 + x + .5f,-height/2 + y+.5f, 0);
					Gizmos.DrawCube(pos,Vector3.one);
				}
			}
		}
	}
}
