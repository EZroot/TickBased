using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FearProj.ServiceLocator;
using GridCoordinate = FearProj.ServiceLocator.GridManager.GridCoordinate;

public class PathFinder
{
    const int MAX_PATH_ITERATIONS = 500;

    private Dictionary<GridCoordinate, float> gCost = new Dictionary<GridCoordinate, float>();
    private Dictionary<GridCoordinate, float> fCost = new Dictionary<GridCoordinate, float>();
    private Dictionary<GridCoordinate, GridCoordinate> cameFrom = new Dictionary<GridCoordinate, GridCoordinate>();
    public static HashSet<GridCoordinate> occupiedSquares = new HashSet<GridCoordinate>();

    public List<GridCoordinate> FindPathImmediately(GridCoordinate start, GridCoordinate end)
    {
        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        var startTile = gridManager.GetTile(start.X, start.Y);
        var endTile = gridManager.GetTile(end.X, end.Y);

        if (startTile.State == GridManager.TileState.Obstacle || endTile.State == GridManager.TileState.Obstacle)
            return null;

        gCost.Clear();
        fCost.Clear();
        cameFrom.Clear();

        HashSet<GridCoordinate> openSetHash = new HashSet<GridCoordinate> { start };
        PriorityQueue<GridCoordinate> openSetQueue = new PriorityQueue<GridCoordinate>();
        HashSet<GridCoordinate> closedSet = new HashSet<GridCoordinate>();

        gCost[start] = 0;
        fCost[start] = Heuristic(start, end);
        openSetQueue.Enqueue(start, fCost[start]);

        int _currentIterationCount = 0;

        while (openSetQueue.Count > 0 && _currentIterationCount < MAX_PATH_ITERATIONS)
        {
            GridCoordinate current = openSetQueue.Dequeue();
            _currentIterationCount++;

            if (current.Equals(end))
                return ReconstructPath(current);
                

            openSetHash.Remove(current);
            closedSet.Add(current);

            foreach (GridCoordinate neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor))
                    continue;

                float tentativeGCost = gCost[current] + Distance(current, neighbor);

                if (tentativeGCost < gCost.GetValueOrDefault(neighbor, float.MaxValue))
                {
                    cameFrom[neighbor] = current;
                    gCost[neighbor] = tentativeGCost;
                    fCost[neighbor] = tentativeGCost + Heuristic(neighbor, end);

                    if (!openSetHash.Contains(neighbor))
                    {
                        openSetHash.Add(neighbor);
                        openSetQueue.Enqueue(neighbor, fCost[neighbor]);
                    }
                }
            }
        }

        return null;
    }

    private List<GridCoordinate> ReconstructPath(GridCoordinate end)
    {
        List<GridCoordinate> path = new List<GridCoordinate>();
        GridCoordinate current = end;

        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }

        path.Add(current);
        path.Reverse();
        return path;
    }

    private float Distance(GridCoordinate a, GridCoordinate b)
    {
        float baseCost = Mathf.Sqrt(Mathf.Pow(b.X - a.X, 2) + Mathf.Pow(b.Y - a.Y, 2));
        float penalty = GetPenalty(b);
        return baseCost + penalty;
    }

    private float GetPenalty(GridCoordinate coordinate)
    {
        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        var tile = gridManager.GetTile(coordinate.X, coordinate.Y);

        float penalty = 0;

        if (tile.ObjectOnTile != null)
        {
            penalty += 45.0f; // Add a penalty if there's an NPC
        }
        return penalty; // Placeholder, replace with real penalty value
    }

    private float Heuristic(GridCoordinate a, GridCoordinate b)
    {
        return Mathf.Abs(a.X - b.X) + Mathf.Abs(a.Y - b.Y);
    }
    private IEnumerable<GridCoordinate> GetNeighbors(GridCoordinate current)
    {
        List<GridCoordinate> neighbors = new List<GridCoordinate>();
        var gridManager = ServiceLocator.Get<IServiceGridManager>();
        int maxX = gridManager.Grid.Width - 1;
        int maxY = gridManager.Grid.Height - 1;
        int x = current.X;
        int y = current.Y;

        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0) continue;

                int newX = x + dx;
                int newY = y + dy;
                GridCoordinate newCoordinate = new GridCoordinate(newX, newY);

                if (newX >= 0 && newX <= maxX && newY >= 0 && newY <= maxY && !occupiedSquares.Contains(newCoordinate))
                {
                    GridManager.Tile neighborTile = gridManager.GetTile(newX, newY);
                    if (neighborTile.State != GridManager.TileState.Obstacle)
                    {
                        neighbors.Add(new GridCoordinate(newX, newY));
                    }
                }
            }
        }
        return neighbors;
    }
}
