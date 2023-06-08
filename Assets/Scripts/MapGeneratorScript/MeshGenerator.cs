using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    /*
    Code that will smooth out the blocky cave to have natural looking curves
    */

    public SquareGrid squareGrid;
    public MeshFilter cave;
    public int tileAmount = 8;  // To set the tiling of the texture
	
    List<Vector3> vertices;
	List<int> triangles;

    Dictionary<int, List<Triangle>> triangleDictionary = new Dictionary<int, List<Triangle>>();
    List<List<int>> outlines = new List<List<int>>();
    HashSet<int> checkedVertices = new HashSet<int>();  // A hashset instead of a list because hashsets are faster as doing "is contained" checks

    public void GenerateMesh(int[,] map, float squareSize)
    {
        /*
            Function to generate mesh that can be called from other scripts
        */

        // Clear the lists
        triangleDictionary.Clear();
        outlines.Clear();
        checkedVertices.Clear();
        
        squareGrid = new SquareGrid(map, squareSize);

        vertices = new List<Vector3>();
        triangles = new List<int>();

        for (int x = 0; x < squareGrid.squares.GetLength(0); x ++) 
        {
			for (int y = 0; y < squareGrid.squares.GetLength(1); y ++) 
            {
                TriangulateSquare(squareGrid.squares[x,y]);
            }
        }

        // Generate mesh
        Mesh mesh = new Mesh();
        cave.mesh = mesh;

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
            
        // For texturing
        Vector2[] uvs = new Vector2[vertices.Count];
        for (int i = 0; i < vertices.Count; i++)
        {
            float percentX = Mathf.InverseLerp(-map.GetLength(0)/2 * squareSize, map.GetLength(0)/2 * squareSize, vertices[i].x) * tileAmount;
            float percentY = Mathf.InverseLerp(-map.GetLength(0)/2 * squareSize, map.GetLength(0)/2 * squareSize, vertices[i].z) * tileAmount;
            uvs[i] = new Vector2(percentX, percentY);
        }
        mesh.uv = uvs;
        
        Generate2DColliders();
    }

    void Generate2DColliders()
    {
        /*
            Create colliders for 2d mesh
        */

        EdgeCollider2D[] currentColliders = gameObject.GetComponents<EdgeCollider2D>();
        for (int i = 0; i < currentColliders.Length; i ++)
        {
            Destroy(currentColliders[i]);
        }

        CalculateMeshOutlines();

        foreach(List<int> outline in outlines)
        {
            EdgeCollider2D edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
            Vector2[] edgePoints = new Vector2[outline.Count];

            for (int i = 0; i < outline.Count; i ++)
            {
                edgePoints[i] = new Vector2(vertices[outline[i]].x, vertices[outline[i]].z);
            }

            edgeCollider.points = edgePoints;
        }
    }

    void TriangulateSquare(Square square)
    {
        /*
            Triangulate the mesh of the squares
        */
        
        switch (square.configuration) 
        {
		case 0:
			break;

		// 1 points:
		case 1:
			MeshFromPoints(square.centreLeft, square.centreBottom, square.bottomLeft);
			break;
		case 2:
			MeshFromPoints(square.bottomRight, square.centreBottom, square.centreRight);
			break;
		case 4:
			MeshFromPoints(square.topRight, square.centreRight, square.centreTop);
			break;
		case 8:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
			break;

		// 2 points:
		case 3:
			MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		case 6:
			MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
			break;
		case 9:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
			break;
		case 12:
			MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
			break;
		case 5:
			MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
			break;
		case 10:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;

		// 3 point:
		case 7:
			MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
			break;
		case 11:
			MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
			break;
		case 13:
			MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
			break;
		case 14:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
			break;

		// 4 point:
		case 15:
			MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
			
            // The vertices in this case can never be outline vertices
            checkedVertices.Add(square.topLeft.vertexIndex);
            checkedVertices.Add(square.topRight.vertexIndex);
            checkedVertices.Add(square.bottomRight.vertexIndex);
            checkedVertices.Add(square.bottomLeft.vertexIndex);

            break;
		}
    }

    void MeshFromPoints(params Node[] points)
    {
        /*
            Create mesh from node points
            Note: 'params' allows you to pass in an abritrary number of arguments
        */
        
        AssignVertices(points);

        // As more and more points are present in the mesh, more triangles are created
        if (points.Length >= 3)
			CreateTriangle(points[0], points[1], points[2]);
		if (points.Length >= 4)
			CreateTriangle(points[0], points[2], points[3]);
		if (points.Length >= 5) 
			CreateTriangle(points[0], points[3], points[4]);
		if (points.Length >= 6)
			CreateTriangle(points[0], points[4], points[5]);
    }

    void AssignVertices(Node[] points)
    {
        /*
            Assign vertices from the points
        */
        
        for (int i = 0; i < points.Length; i ++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }

    void CreateTriangle(Node a, Node b, Node c)
    {
        /*
            Create triangle from the points
        */
        
        triangles.Add(a.vertexIndex);
        triangles.Add(b.vertexIndex);
        triangles.Add(c.vertexIndex);

        Triangle triangle = new Triangle(a.vertexIndex, b.vertexIndex, c.vertexIndex);
        AddTriangleToDictionary(triangle.vertexIndexA, triangle);
        AddTriangleToDictionary(triangle.vertexIndexB, triangle);
        AddTriangleToDictionary(triangle.vertexIndexC, triangle);
    }

    void AddTriangleToDictionary(int vertexIndexKey, Triangle triangle)
    {
        /*
            Add triangle to the "triangleDictionary"
        */
        
        // If triangle exists, put in the dictionary
        if(triangleDictionary.ContainsKey(vertexIndexKey))
        {
            triangleDictionary[vertexIndexKey].Add(triangle);
        }
        // Else, create a new triangle and add it to the dictionary
        else
        {
            List<Triangle> triangleList = new List<Triangle>();
            triangleList.Add(triangle);
            triangleDictionary.Add(vertexIndexKey, triangleList);
        }
    }

    void CalculateMeshOutlines()
    {
        /*
            When an outline vertex is found, calculated the outline mesh
        */
        
        for (int vertexIndex = 0; vertexIndex < vertices.Count; vertexIndex ++)
        {
            if(!checkedVertices.Contains(vertexIndex))
            {
                int newOutlineVertex = GetConnectedOutlineVertex(vertexIndex);
                if (newOutlineVertex != -1)
                {
                    checkedVertices.Add(vertexIndex);

                    List<int> newOutline = new List<int>();
                    newOutline.Add(vertexIndex);
                    outlines.Add(newOutline);
                    FollowOutline(newOutlineVertex, outlines.Count - 1);
                    outlines[outlines.Count - 1].Add(vertexIndex);
                }
            }
        }
    }

    void FollowOutline(int vertexIndex, int outlineIndex)
    {
        /*
            Follow the outline of the mesh
        */
        
        outlines[outlineIndex].Add(vertexIndex);
        checkedVertices.Add(vertexIndex);

        int nextVertexIndex = GetConnectedOutlineVertex(vertexIndex);

        if (nextVertexIndex != -1)
        {
            FollowOutline(nextVertexIndex, outlineIndex);
        }
    }

    int GetConnectedOutlineVertex(int vertexIndex)
    {
        /*
            Given a vertex, get the connected outline vertex
        */
        
        List<Triangle> trianglesContainingVertex = triangleDictionary[vertexIndex];

        for (int i = 0; i < trianglesContainingVertex.Count; i++)
        {
            Triangle triangle = trianglesContainingVertex[i];

            for (int j = 0; j < 3; j ++)
            {
                int vertexB = triangle[j];
                if (vertexB != vertexIndex && !checkedVertices.Contains(vertexB))
                {
                    if (isOutlineEdge(vertexIndex, vertexB))
                    {
                        return vertexB;
                    }
                }
            }
        }

        return -1;
    }

    bool isOutlineEdge(int vertexA, int vertexB)
    {
        /*
            Given two vertices, are they an outline edge?
        */
        
        List<Triangle> triangleListA = triangleDictionary[vertexA];
        int sharedTriangleCount = 0;

        for (int i = 0; i < triangleListA.Count; i ++)
        {
            if (triangleListA[i].Contains(vertexB))
            {
                sharedTriangleCount ++;
                if (sharedTriangleCount > 1)
                {
                    break;
                }
            }
        }

        return sharedTriangleCount == 1;
    }

    struct Triangle
    {
        /*
            Custom data type "triangle"
        */

        public int vertexIndexA;
        public int vertexIndexB;
        public int vertexIndexC;
        int[] vertices;

        // Constructor
        public Triangle(int a, int b, int c)
        {
            vertexIndexA = a;
            vertexIndexB = b;
            vertexIndexC = c;

            vertices = new int[3];
            vertices[0] = a;
            vertices[1] = b;
            vertices[2] = c;
        }

        // Indexer
        public int this[int i]
        {
            get
            {
                return vertices[i];
            }
        }

        // Check if a triangle contains a certain vertex index
        public bool Contains(int vertexIndex)
        {
            return vertexIndex == vertexIndexA || vertexIndex == vertexIndexB || vertexIndex == vertexIndexC;
        }
    }

    public class SquareGrid
    {
        /*
            A 2D Array that holds squares
        */

        public Square[,] squares;

        // Constructor
        public SquareGrid(int[,] map, float squareSize)
        {
            int nodeCountx = map.GetLength(0);
            int nodeCounty = map.GetLength(1);
            
            float mapWidth = nodeCountx * squareSize;
            float mapHeight = nodeCounty * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountx, nodeCounty];

            // Loop through the control nodes
            for (int x = 0; x < nodeCountx; x++)
            {
                for (int y = 0; y < nodeCounty; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth/2 + x * squareSize + squareSize/2, 0, -mapHeight/2 + y * squareSize + squareSize/2);
					controlNodes[x,y] = new ControlNode(pos,map[x,y] == 1, squareSize);
                }
            }

            // Make squares out of the control nodes
            squares = new Square[nodeCountx -1,nodeCounty -1];
			for (int x = 0; x < nodeCountx-1; x ++) 
            {
				for (int y = 0; y < nodeCounty-1; y ++) 
                {
					squares[x,y] = new Square(controlNodes[x,y+1], controlNodes[x+1,y+1], controlNodes[x+1,y], controlNodes[x,y]);
				}
			}
        }
    }

    public class Square
    {
        /*
            Square that holds all of the nodes and control nodes
        */
        
        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;

        // Which mesh configuration to use
        public int configuration;

        // Constructor
        public Square(ControlNode topLeft, ControlNode topRight, ControlNode bottomRight, ControlNode bottomLeft)
        {
            this.topLeft = topLeft;
            this.topRight = topRight;
            this.bottomRight = bottomRight;
            this.bottomLeft = bottomLeft;

            // Create the centre nodes
            centreTop = this.topLeft.right;
            centreRight = this.bottomRight.above;
            centreBottom = this.bottomLeft.right;
            centreLeft = this.bottomLeft.above;

            // Check which configuration to use
            if (topLeft.active)
            {
				configuration += 8;
            }
			if (topRight.active)
            {
				configuration += 4;
            }
			if (bottomRight.active)
            {
				configuration += 2;
            }
			if (bottomLeft.active)
            {
				configuration += 1;
            }
        }
    }
    
    public class Node 
    {
        /*
            Node that holds the positions of the coreners of the squares
        */

        public Vector3 position;
        public int vertexIndex = -1;

        // Constructor
        public Node(Vector3 position)
        {
            this.position = position;
        }
    }

    public class ControlNode : Node
    {
        /*
            Control nodes are the nodes that are in the middle of the squares
            This inherits from Node
        */

        public bool active;
        public Node above, right;

        // Constructor
        public ControlNode(Vector3 position, bool active, float squareSize) : base(position)
        {
            this.active = active;
            above = new Node(position + Vector3.forward * squareSize / 2f);
            right = new Node(position + Vector3.right * squareSize / 2f);
        }
    }
}
