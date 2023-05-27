using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointSpawnners : MonoBehaviour
{
    private GameObject player;
    private GameObject pointGameobject;

    private Generator generator;
    private int[,] map;

    [SerializeField] private GameObject pointPrefab;
    [SerializeField] private int distance;
    [SerializeField] private GameObject caveGenerator;

    // Start is called before the first frame update
    void Awake()
    {
        // Extract scripts
        generator = caveGenerator.GetComponent<Generator>();
    }

    // Update is called once per frame
    void Update()
    {
        // There's delay in when these two variables are made so they're done here
        if (player == null)
        {
            player = GameObject.FindWithTag("Player");
        }
        if (map == null)
        {
            map = generator.getMap();
        }

        // Spawn a new point prefab when there is none on the map
        if (pointGameobject == null)
        {
            int[] playerMapPos = WorldToMapPoint(player.transform.position.x, player.transform.position.y);
            
            // Only spawn when the player truly is not in the wall
            if(map[playerMapPos[0], playerMapPos[1]] == 0)
            {
                // Spawn a new point prefab
                int[] spawnPoint = SpawnPoint(distance, playerMapPos[0], playerMapPos[1]);
                pointGameobject = Instantiate(pointPrefab, MapToWorldPoint(spawnPoint[0], spawnPoint[1]), Quaternion.identity);
            }
        }
    }

    int[] WorldToMapPoint(float x, float y)
    {
        /*
            Turns a world location into a point in the map matrix
        */
        int[] res = new int[2];
        float width = generator.getMapWidth();
        float height = generator.getMapHeight();

        int xRes = (int)(-(-width/2 + 0.5f) + x);
        int yRes = (int)(-(-height/2 + 0.5f) + y);

        res[0] = xRes;
        res[1] = yRes;
        return res;
    }

    Vector3 MapToWorldPoint(int x, int y)
    {
        /*
            Converts a tile coordinate to a world coordinate
        */

        return new Vector3(-generator.getMapWidth() / 2 + 0.5f + x, -generator.getMapHeight() / 2 + 0.5f + y, 0);
    }

    // This should be return a void I just realized
    int[] SpawnPoint(int distance, int xPoint, int yPoint)
    {
        /*
            Respawns the point prefab at a new random location given a distance variable
        */

        // Create variables
        List<int[]> resBackup = new List<int[]>();
        List<int[]> res = new List<int[]>();
        int[,] mapFlags = new int[generator.getMapWidth(), generator.getMapHeight()];
        int tileType = map[xPoint, yPoint];

        // Create queue
        Queue<int[]> currentQueue = new Queue<int[]>();
        Queue<int[]> bufferQueue = new Queue<int[]>();
        currentQueue.Enqueue(new int[2] { xPoint, yPoint });
        mapFlags[xPoint, yPoint] = 1;

        // Start loop
        while(distance > 0 && (currentQueue.Count > 0 || bufferQueue.Count > 0))
        {
            // Check if one "iteration" has passed and add distance
            if(currentQueue.Count == 0)
            {
                currentQueue = bufferQueue;
                bufferQueue = new Queue<int[]>();
                distance--;
            }
            
            int[] currentTile = currentQueue.Dequeue();  // Serve

            // Loop through all adjacent tiles
            for (int x = currentTile[0] - 1; x <= currentTile[0] + 1; x++)
            {
                for (int y = currentTile[1] - 1; y <= currentTile[1] + 1; y++)
                {
                    if(generator.IsInMapRange(x, y) && (y == currentTile[1] || x == currentTile[0]))
                    {
                        // Add more to queue
                        if(mapFlags[x, y] == 0 && map[x, y] == tileType)
                        {
                            mapFlags[x, y] = 1;
                            bufferQueue.Enqueue(new int[2] { x, y });
                            resBackup.Add(new int[2] { x, y }); // Backup for when the algo can't find a valid position far enough

                            // Check if the tile is far enough
                            if(distance == 1)
                            {
                                res.Add(new int[2] { x, y });
                            }
                        }
                    }
                }
            }
        }

        // Collect res
        while (currentQueue.Count > 0)
        {
            int[] currentTile = currentQueue.Dequeue();
            res.Add(new int[2] { currentTile[0], currentTile[1] });
        }

        // Pick one at random
        int[] resFinal;
        if (res.Count == 0)
        {
            resFinal = resBackup[Random.Range(0, resBackup.Count)];
        }
        else
        {
            resFinal = res[Random.Range(0, res.Count)];
        }

        // Returning for debug
        return resFinal;
    }
}
