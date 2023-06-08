using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSpawnner : MonoBehaviour
{
    public GameObject playerPrefab;
    public GameObject caveGenerator;

    private int[,] caveMap;
    private int[] mainRoomLocation;

    // Start is called before the first frame update
    void Start()
    {   
        // Get generator
        Generator caveGeneratorScript = caveGenerator.GetComponent<Generator>();
        caveGeneratorScript.GenerateMap();

        caveMap = caveGeneratorScript.getMap();

        int width = caveMap.GetLength(0);
        int height = caveMap.GetLength(1);
    
        // Make sure the player doesn't spawn inside a wall
        List<int[]> curBestPos = new List<int[]>();
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                if(caveMap[x,y] == 0)
                {
                    curBestPos.Add(new int[] {x, y});
                }
            }
        }

        // Get the best position and spawn the player
        int[] bestPos = BestPosition(caveMap, curBestPos, 1);
        Instantiate (playerPrefab, new Vector3(-width/2 + bestPos[0] + .5f,-height/2 + bestPos[1] +.5f, 0), Quaternion.identity);
    }

    int[] BestPosition(int[,] map, List<int[]> bestPos, int iteration, List<int[]> previousBestPos = null)
    {
        /*
            Approximately gives the best position for the player to spawn in
        */
        
        if (bestPos.Count == 0)
        {
            return previousBestPos[0];
        }
        if (bestPos.Count == 1)
        {
            return bestPos[0];
        }
        else
        {
            List<int[]> newBest = new List<int[]>();

            for (int index = 0; index < bestPos.Count; index++)
            {
                int x = bestPos[index][0];
                int y = bestPos[index][1];

                if (map[x + iteration, y + iteration] == 0 && map[x - iteration, y - iteration] == 0 && map[x + iteration, y - iteration] == 0 && map[x - iteration, y + iteration] == 0)
                {
                    newBest.Add(new int[] {x, y});
                }
            }
            
            // Recurse
            return BestPosition(map, newBest, iteration + 1, bestPos);
        }
    }
}
