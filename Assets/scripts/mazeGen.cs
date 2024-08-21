using System.Collections.Generic;
using UnityEngine;

public class mazeGen : MonoBehaviour
{
    [Range(5, 500)]
    public int mazeWidth = 5, mazeHeight = 5;
    public int startX, startY;
    MazeCell[,] maze;

    Vector2Int currentCell;

    public MazeCell[,] GetMaze()
    {
        maze = new MazeCell[mazeWidth, mazeHeight];

        for (int x = 0; x < mazeWidth; x++)
        {
            for (int y = 0; y < mazeHeight; y++)
            {
                maze[x, y] = new MazeCell(x, y);
            }
        }

        CarvePath(startX, startY);

        return maze;
    }

    List<Direction> directions = new List<Direction>
    {
        Direction.Up, Direction.Down, Direction.Left, Direction.Right,
    };

    List<Direction> GetRandomDirection()
    {
        List<Direction> dir = new List<Direction>(directions);

        List<Direction> ranDir = new List<Direction>();

        while (dir.Count > 0)
        {
            int ran = Random.Range(0, dir.Count);
            ranDir.Add(dir[ran]);
            dir.RemoveAt(ran);
        }

        return ranDir;
    }

    bool IsCellValid(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mazeWidth || y >= mazeHeight || maze[x, y].visited)
            return false;
        else
            return true;
    }

    Vector2Int CheckNeighbour()
    {
        List<Direction> ranDir = GetRandomDirection();

        for (int i = 0; i < ranDir.Count; i++)
        {
            Vector2Int neighbour = currentCell;

            switch (ranDir[i])
            {
                case Direction.Up:
                    neighbour.y++;
                    break;
                case Direction.Down:
                    neighbour.y--;
                    break;
                case Direction.Right:
                    neighbour.x++;
                    break;
                case Direction.Left:
                    neighbour.x--;
                    break;
            }

            if (IsCellValid(neighbour.x, neighbour.y))
                return neighbour;
        }

        return currentCell;
    }

    void Breakwalls(Vector2Int primaryCell, Vector2Int secondaryCell)
    {
        if (primaryCell.x > secondaryCell.x)
            maze[primaryCell.x, primaryCell.y].leftWall = false;
        else if (primaryCell.x < secondaryCell.x)
            maze[secondaryCell.x, secondaryCell.y].leftWall = false;
        else if (primaryCell.y < secondaryCell.y)
            maze[primaryCell.x, primaryCell.y].topWall = false;
        else if (primaryCell.y > secondaryCell.y)
            maze[secondaryCell.x, secondaryCell.y].topWall = false;
    }

    void CarvePath(int x, int y)
    {
        if (x < 0 || y < 0 || x >= mazeWidth || y >= mazeHeight)
        {
            x = y = 0;
            Debug.LogWarning("Starting position is out of bounds, defaulting to 0, 0");
        }

        currentCell = new Vector2Int(x, y);

        List<Vector2Int> path = new List<Vector2Int>();

        bool deadEnd = false;

        while (!deadEnd)
        {
            Vector2Int nextCell = CheckNeighbour();

            if (nextCell == currentCell)
            {
                for (int i = path.Count - 1; i >= 0; i--)
                {
                    currentCell = path[i];
                    path.RemoveAt(i);
                    nextCell = CheckNeighbour();

                    if (nextCell != currentCell)
                        break;
                }

                if (nextCell == currentCell)
                    deadEnd = true;
            }
            else
            {
                Breakwalls(currentCell, nextCell);
                maze[currentCell.x, currentCell.y].visited = true;
                currentCell = nextCell;
                path.Add(currentCell);
            }
        }
    }
}

public enum Direction { Up, Down, Right, Left }
public class MazeCell
{
    public bool visited;
    public int x, y;

    public bool topWall;
    public bool leftWall;

    public Vector2Int position
    {
        get
        {
            return new Vector2Int(x, y);
        }
    }

    public MazeCell(int x, int y)
    {
        this.x = x;
        this.y = y;

        visited = false;

        topWall = leftWall = true;
    }
}
