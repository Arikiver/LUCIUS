using Unity.AI.Navigation;
using UnityEngine;

public class MazeRenderer : MonoBehaviour
{
    public Unity.AI.Navigation.NavMeshSurface surface;
    [SerializeField] MazeGen mazeGenerator; // Updated reference
    [SerializeField] GameObject MazeCellPrefab;
    public float CellSize = 1f;

    private void Start()
    {
        if (mazeGenerator == null)
        {
            Debug.LogError("MazeGenerator not assigned!");
            return;
        }

        if (MazeCellPrefab == null)
        {
            Debug.LogError("MazeCellPrefab not assigned!");
            return;
        }

        GenerateMaze();

        if (surface != null)
        {
            surface.BuildNavMesh();
        }
    }

    private void GenerateMaze()
    {
        MazeGen.MazeCell[,] maze = mazeGenerator.GetMaze(); // Updated reference

        for (int x = 0; x < mazeGenerator.mazeWidth; x++)
        {
            for (int y = 0; y < mazeGenerator.mazeHeight; y++)
            {
                Vector3 position = new Vector3((float)x * CellSize, 0f, (float)y * CellSize);
                GameObject newCell = Instantiate(MazeCellPrefab, position, Quaternion.identity, transform);

                MazeCellObject mazeCell = newCell.GetComponent<MazeCellObject>();
                if (mazeCell == null)
                {
                    Debug.LogError("MazeCellObject component missing from prefab!");
                    continue;
                }

                bool top = maze[x, y].topWall;
                bool left = maze[x, y].leftWall;
                bool right = (x == mazeGenerator.mazeWidth - 1);
                bool bottom = (y == 0);

                mazeCell.Init(top, bottom, right, left);
            }
        }
    }
}
