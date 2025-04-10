using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GenerateWorld : MonoBehaviour
{
    private int _maxWidth = 6;
    private int _maxHeight = 4;
    private float _gridFillPercent = 0.6f; //for a 6x4 grid, should be 15 tiles
    private float _summitCapPercent = 0.2f; //for a 6x4 grid (15 tiles), should be at most 3 tiles
    private float _peakCapPercent = 0.4f; //for a 6x4 grid (15 tiles), should be at most 6 tiles
    private float _basePlaceSummitPercent = 0.35f;
    private float _basePlacePeakPercent = 0.5f;
    private float _bonusPlacePercent = 0.05f;
    private float _placeHolePercent = 0.7f;

    private class WorldTile
    {
        public WorldChunk.HeightLvl heightLvl;
        public GameObject chunk;
        public bool visited;
        public int x;
        public int y;
    }

    private WorldTile[,] _2Dgrid;
    private List<List<WorldTile>> _connectedGroundGroupsList;
    private List<List<WorldTile>> _connectedPeakGroupsList;
    private List<List<WorldTile>> _connectedSummitGroupsList;

    private struct RandomWalker
    {
        public Vector3 dir;
        public Vector3 pos;
    }

    private List<RandomWalker> _walkers;
    private float _walkerDirChangeProb = 0.5f;
    private float _walkerSpawnProb = 0.05f;
    private float _walkerDestroyProb = 0.05f;
    private int _maxWalkers = 1;
    private int _iterationSteps = 100000;

    private float _chunkSize = 15;
    private List<GameObject> _groundChunks;
    private List<GameObject> _peakChunks;
    private List<GameObject> _summitChunks;
    private List<GameObject> _otherChunks;

    private void Awake()
    {
        SetupGrid();
        RandomizeWorldShape();

        NewRandomizeWorldHeight(WorldChunk.HeightLvl.Ground, WorldChunk.HeightLvl.Peak, _peakCapPercent + _summitCapPercent);
        NewRandomizeWorldHeight(WorldChunk.HeightLvl.Peak, WorldChunk.HeightLvl.Summit, _summitCapPercent);
        CheckHeightSurroundings();
        FindHeightGroups();
        CheckStrandedGrounds();

        LoadPrefabList();
        AssignWorldChunks();
        InstantiateChunks();
    }

    #region HelperFuncs
    private void QuitProgram(string reason)
    {
        Debug.Log($"Stopping WorldGen due to {reason}");
        EditorApplication.isPlaying = false;
    }

    private Vector3 GetRandomDirection()
    {
        int rng = Random.Range(0, 4);

        return rng switch
        {
            0 => new Vector3(0, 0, -1),
            1 => new Vector3(-1, 0, 0),
            2 => new Vector3(0, 0, 1),
            _ => new Vector3(1, 0, 0),
        };
    }

    private float GetNbTiles(WorldChunk.HeightLvl countType) //if Void we count all used tiles
    {
        int count = 0;

        foreach (WorldTile tile in _2Dgrid)
        {
            if (tile.heightLvl == countType)
            {
                count++;
            }
        }

        return count;
    }

    private WorldTile[] GetAllNeighbors(WorldTile tile, bool diagonalAlso = false)
    {
        WorldTile[] neighbors;

        if (tile.x == 0)
        {
            if (tile.y == 0)
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x + 1, tile.y],
                            _2Dgrid[tile.x, tile.y + 1]
                        };

                if (diagonalAlso)
                {
                    neighbors.Append(_2Dgrid[tile.x + 1, tile.y + 1]);
                }
            }
            else if (tile.y == _maxHeight - 1)
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x + 1, tile.y],
                            _2Dgrid[tile.x, tile.y - 1]
                        };

                if (diagonalAlso)
                {
                    neighbors.Append(_2Dgrid[tile.x + 1, tile.y - 1]);
                }
            }
            else
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x + 1, tile.y],
                            _2Dgrid[tile.x, tile.y + 1],
                            _2Dgrid[tile.x, tile.y - 1]
                        };

                if (diagonalAlso)
                {
                    neighbors.Append(_2Dgrid[tile.x + 1, tile.y + 1]);
                    neighbors.Append(_2Dgrid[tile.x + 1, tile.y - 1]);
                }
            }
        }
        else if (tile.x == _maxWidth - 1)
        {
            if (tile.y == 0)
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x - 1, tile.y],
                            _2Dgrid[tile.x, tile.y + 1]
                        };

                if (diagonalAlso)
                {
                    neighbors.Append(_2Dgrid[tile.x - 1, tile.y + 1]);
                }
            }
            else if (tile.y == _maxHeight - 1)
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x - 1, tile.y],
                            _2Dgrid[tile.x, tile.y - 1]
                        };

                if (diagonalAlso)
                {
                    neighbors.Append(_2Dgrid[tile.x - 1, tile.y - 1]);
                }
            }
            else
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x - 1, tile.y],
                            _2Dgrid[tile.x, tile.y + 1],
                            _2Dgrid[tile.x, tile.y - 1]
                        };

                if (diagonalAlso)
                {
                    neighbors.Append(_2Dgrid[tile.x - 1, tile.y + 1]);
                    neighbors.Append(_2Dgrid[tile.x - 1, tile.y - 1]);
                }
            }
        }
        else if (tile.y == 0)
        {
            neighbors = new WorldTile[] {
                        _2Dgrid[tile.x + 1, tile.y],
                        _2Dgrid[tile.x - 1, tile.y],
                        _2Dgrid[tile.x, tile.y + 1]
                    };

            if (diagonalAlso)
            {
                neighbors.Append(_2Dgrid[tile.x + 1, tile.y + 1]);
                neighbors.Append(_2Dgrid[tile.x - 1, tile.y + 1]);
            }
        }
        else if (tile.y == _maxHeight - 1)
        {
            neighbors = new WorldTile[] {
                        _2Dgrid[tile.x + 1, tile.y],
                        _2Dgrid[tile.x - 1, tile.y],
                        _2Dgrid[tile.x, tile.y - 1]
                    };

            if (diagonalAlso)
            {
                neighbors.Append(_2Dgrid[tile.x + 1, tile.y - 1]);
                neighbors.Append(_2Dgrid[tile.x - 1, tile.y - 1]);
            }
        }
        else
        {
            neighbors = new WorldTile[] {
                        _2Dgrid[tile.x + 1, tile.y],
                        _2Dgrid[tile.x - 1, tile.y],
                        _2Dgrid[tile.x, tile.y + 1],
                        _2Dgrid[tile.x, tile.y - 1]
                    };

            if (diagonalAlso)
            {
                neighbors.Append(_2Dgrid[tile.x + 1, tile.y + 1]);
                neighbors.Append(_2Dgrid[tile.x - 1, tile.y + 1]);
                neighbors.Append(_2Dgrid[tile.x + 1, tile.y - 1]);
                neighbors.Append(_2Dgrid[tile.x - 1, tile.y - 1]);
            }
        }

        return neighbors;
    }

    private WorldTile[] GetPreviousNeighbors(WorldTile tile)
    {
        WorldTile[] neighbors;

        if (tile.x == 0)
        {
            if (tile.y > 0)
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x, tile.y - 1]
                        };
            }
            else
            {
                neighbors = new WorldTile[] { };
            }
        }
        else if (tile.y == 0)
        {
            neighbors = new WorldTile[] {
                        _2Dgrid[tile.x - 1, tile.y],
                    };
        }
        else
        {
            neighbors = new WorldTile[] {
                        _2Dgrid[tile.x - 1, tile.y],
                        _2Dgrid[tile.x, tile.y - 1]
                    };
        }

        return neighbors;
    }

    private void AssignBasicChunk(WorldTile tile)
    {
        GameObject chunk;

        switch (tile.heightLvl)
        {
            case WorldChunk.HeightLvl.Ground: chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.None).First(); break;
            case WorldChunk.HeightLvl.Peak: chunk = _peakChunks.Where(c => c.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.None).First(); break;
            case WorldChunk.HeightLvl.Summit: chunk = _summitChunks[0]; break;
            default: chunk = _otherChunks[0]; break;
        }

        _2Dgrid[tile.x, tile.y].chunk = chunk;
    }

    private void AssignClimbChunk(IEnumerable<List<WorldTile>> groupList, List<List<WorldTile>> lowerGroupList, WorldChunk.HeightLvl lowerHeight, List<GameObject> lowerChunks, string loopStopReason)
    {
        foreach (List<WorldTile> group in groupList)
        {
            List<WorldTile> lowerNeighbors = new();
            List<List<WorldTile>> neighborGroups = new();

            foreach (WorldTile tile in group)
            {
                WorldTile[] neighbors = GetAllNeighbors(tile);
                lowerNeighbors.AddRange(neighbors.Where(n => n.heightLvl == lowerHeight && !lowerNeighbors.Contains(n)));

                foreach (WorldTile neighbor in neighbors)
                {
                    if (neighbor.heightLvl == WorldChunk.HeightLvl.Peak)
                    {
                        neighborGroups.AddRange(lowerGroupList.Where(g => g.Contains(neighbor) && !neighborGroups.Contains(g)));
                    }
                }
            }

            if (lowerNeighbors.Count == 0)
            {
                continue;
            }

            foreach (List<WorldTile> ng in neighborGroups)
            {
                int iterations = 0;
                int id;
                WorldTile tileToAssign;
                WorldTile neighborInGroup;
                WorldTile[] candidates = lowerNeighbors.Where(n => ng.Contains(n)).ToArray();

                do
                {
                    id = Random.Range(0, candidates.Count());
                    tileToAssign = candidates[id];
                    neighborInGroup = GetAllNeighbors(tileToAssign).Where(n => n.heightLvl == group[0].heightLvl && n.y <= tileToAssign.y && group.Contains(n)).DefaultIfEmpty(null).First();
                    ++iterations;
                }
                while ((tileToAssign.chunk is not null || neighborInGroup is null) && iterations <= _iterationSteps);

                if (iterations > _iterationSteps)
                {
                    QuitProgram(loopStopReason);
                    return;
                }

                if (neighborInGroup.x == tileToAssign.x)
                {
                    WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Up, height = group[0].heightLvl };
                    GameObject chunk = lowerChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                    tileToAssign.chunk = chunk;
                }
                else
                {
                    if (neighborInGroup.x < tileToAssign.x)
                    {
                        WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Left, height = group[0].heightLvl };
                        GameObject chunk = lowerChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                        tileToAssign.chunk = chunk;
                    }
                    else
                    {
                        WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Right, height = group[0].heightLvl };
                        GameObject chunk = lowerChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                        tileToAssign.chunk = chunk;
                    }
                }
            }
        }
    }

    private void AssignHoleChunk(WorldTile tile, bool isHorz)
    {
        GameObject chunk;
        WorldChunk.ChunkExit exit = new() { dir = isHorz ? WorldChunk.Direction.Right : WorldChunk.Direction.Up, height = tile.heightLvl };

        switch (tile.heightLvl)
        {
            case WorldChunk.HeightLvl.Peak: chunk = _peakChunks.Where(c => c.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.Hole && c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First(); break;
            default: chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.Hole && c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First(); break;
        }

        _2Dgrid[tile.x, tile.y].chunk = chunk;
    }

    private bool CheckHorizontalHoleAssign(WorldTile tile)
    {
        if (tile.x != 0 && tile.x != _maxWidth - 1 && Random.value < _placeHolePercent)
        {
            WorldTile tileBefore = _2Dgrid[tile.x - 1, tile.y];
            WorldTile tileAfter = _2Dgrid[tile.x + 1, tile.y];
            WorldTile oppBefore = tile.y - 1 >= 0 ? _2Dgrid[tile.x, tile.y - 1] : null;
            WorldTile oppAfter = tile.y + 1 < _maxHeight ? _2Dgrid[tile.x, tile.y + 1] : null;
            WorldChunk.ChunkExit oppExit = new() { dir = WorldChunk.Direction.Right, height = tile.heightLvl };

            if (
                tileBefore.heightLvl == tile.heightLvl &&
                (tileBefore.chunk is null || tileBefore.chunk.GetComponent<WorldChunk>().encounter != WorldChunk.EncounterType.Hole) &&
                tileAfter.heightLvl == tile.heightLvl &&
                (tileAfter.chunk is null || tileAfter.chunk.GetComponent<WorldChunk>().encounter != WorldChunk.EncounterType.Hole) &&
                (oppBefore is null || oppBefore.chunk is null || oppBefore.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.None || (oppBefore.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.Hole && oppBefore.chunk.GetComponent<WorldChunk>().chunkExits.Contains(oppExit))) &&
                (oppAfter is null || oppAfter.chunk is null || oppAfter.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.None || (oppAfter.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.Hole && oppAfter.chunk.GetComponent<WorldChunk>().chunkExits.Contains(oppExit)))
            )
            {
                AssignHoleChunk(tile, true);
                AssignVoidPostHole(tile, WorldChunk.Direction.Up);
                //AssignVoidPostHole(tile, WorldChunk.Direction.Down);
                return true;
            }
        }

        return false;
    }

    private bool CheckVerticalHoleAssign(WorldTile tile)
    {
        if (tile.y != 0 && tile.y != _maxHeight - 1 && Random.value < _placeHolePercent)
        {
            WorldTile tileBefore = _2Dgrid[tile.x, tile.y - 1];
            WorldTile tileAfter = _2Dgrid[tile.x, tile.y + 1];
            WorldTile oppBefore = tile.x - 1 >= 0 ? _2Dgrid[tile.x - 1, tile.y] : null;
            WorldTile oppAfter = tile.x + 1 < _maxWidth ? _2Dgrid[tile.x + 1, tile.y] : null;
            WorldChunk.ChunkExit oppExit = new() { dir = WorldChunk.Direction.Up, height = tile.heightLvl };

            if (
                tileBefore.heightLvl == tile.heightLvl &&
                (tileBefore.chunk is null || tileBefore.chunk.GetComponent<WorldChunk>().encounter != WorldChunk.EncounterType.Hole) &&
                tileAfter.heightLvl == tile.heightLvl &&
                (tileAfter.chunk is null || tileAfter.chunk.GetComponent<WorldChunk>().encounter != WorldChunk.EncounterType.Hole) &&
                (oppBefore is null || oppBefore.chunk is null || oppBefore.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.None || (oppBefore.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.Hole && oppBefore.chunk.GetComponent<WorldChunk>().chunkExits.Contains(oppExit))) &&
                (oppAfter is null || oppAfter.chunk is null || oppAfter.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.None || (oppAfter.chunk.GetComponent<WorldChunk>().encounter == WorldChunk.EncounterType.Hole && oppAfter.chunk.GetComponent<WorldChunk>().chunkExits.Contains(oppExit)))
            )
            {
                AssignHoleChunk(tile, false);
                AssignVoidPostHole(tile, WorldChunk.Direction.Left);
                //AssignVoidPostHole(tile, WorldChunk.Direction.Right);
                return true;
            }
        }

        return false;
    }

    private void AssignVoidPostHole(WorldTile tile, WorldChunk.Direction nextDir)
    {
        //if tile on same height, isnt climb/hole, doesnt have a lower climb neighbor up to it.
        //  then check next tile in specified direction (if not on edge)
        //otherwise stop recursion
        Debug.Log("recursively place void chunks");
    }

    #endregion //HelperFuncs

    private void SetupGrid()
    {
        _2Dgrid = new WorldTile[_maxWidth, _maxHeight];

        for (int x = 0; x < _maxWidth; x++)
        {
            for (int y = 0; y < _maxHeight; y++)
            {
                WorldTile tile = new();
                tile.heightLvl = WorldChunk.HeightLvl.Void;
                tile.visited = false;
                tile.x = x;
                tile.y = y;
                _2Dgrid[x, y] = tile;
            }
        }

        _walkers = new();
        RandomWalker walker = new();
        walker.dir = GetRandomDirection();
        Vector3 pos = new(Mathf.RoundToInt(_maxWidth / 2.0f), 0, Mathf.RoundToInt(_maxHeight / 2.0f));
        walker.pos = pos;
        _walkers.Add(walker);
    }

    private void RandomizeWorldShape()
    {
        int iterations = 0;
        do
        {
            //Base ground lvl wherever walkers go
            foreach (RandomWalker walker in _walkers)
            {
                _2Dgrid[(int)walker.pos.x, (int)walker.pos.z].heightLvl = WorldChunk.HeightLvl.Ground;
            }

            //Chance destroy walker
            int numberChecks = _walkers.Count;
            for (int i = 0; i < numberChecks; i++)
            {
                if (Random.value < _walkerDestroyProb && _walkers.Count > 1)
                {
                    _walkers.RemoveAt(i);
                    break;
                }
            }

            //Chance walker pick new direction
            for (int i = 0; i < _walkers.Count; i++)
            {
                if (Random.value < _walkerDirChangeProb)
                {
                    RandomWalker thisWalker = _walkers[i];
                    thisWalker.dir = GetRandomDirection();
                    _walkers[i] = thisWalker;
                }
            }

            //Chance spawn new walker
            numberChecks = _walkers.Count;
            for (int i = 0; i < numberChecks; i++)
            {
                if (Random.value < _walkerSpawnProb && _walkers.Count < _maxWalkers)
                {
                    RandomWalker walker = new();
                    walker.dir = GetRandomDirection();
                    walker.pos = _walkers[i].pos;
                    _walkers.Add(walker);
                }
            }

            //Move walkers
            for (int i = 0; i < _walkers.Count; i++)
            {
                RandomWalker walker = _walkers[i];
                walker.pos += walker.dir;
                _walkers[i] = walker;
            }

            //Avoid border of grid
            for (int i = 0; i < _walkers.Count; i++)
            {
                RandomWalker walker = _walkers[i];
                walker.pos.x = Mathf.Clamp(walker.pos.x, 0, _maxWidth - 1);
                walker.pos.z = Mathf.Clamp(walker.pos.z, 0, _maxHeight - 1);
                _walkers[i] = walker;
            }

            //Check to exit loop
            float nb = GetNbTiles(WorldChunk.HeightLvl.Ground);
            if (nb / _2Dgrid.Length > _gridFillPercent)
            {
                break;
            }

            iterations++;
        }
        while (iterations < _iterationSteps);
    }

    private void NewRandomizeWorldHeight(WorldChunk.HeightLvl heightToCheck, WorldChunk.HeightLvl heightToAssign, float capPercent)
    {
        RandomWalker walker = new();
        walker.dir = GetRandomDirection();
        bool validStartPos = false;

        do
        {
            int x = Random.Range(0, _maxWidth);
            int z = Random.Range(0, _maxHeight);

            if (_2Dgrid[x, z].heightLvl == heightToCheck)
            {
                walker.pos = new(x, 0, z);
                validStartPos = true;
            }
        }
        while (!validStartPos);

        int iterations = 0;

        do
        {
            _2Dgrid[(int)walker.pos.x, (int)walker.pos.z].heightLvl = heightToAssign;

            //Chance walker pick new direction
            if (Random.value < _walkerDirChangeProb)
            {
                walker.dir = GetRandomDirection();
            }

            //Avoid border of grid
            int newX = (int)Mathf.Clamp(walker.pos.x + walker.dir.x, 0, _maxWidth - 1);
            int newZ = (int)Mathf.Clamp(walker.pos.z + walker.dir.z, 0, _maxHeight - 1);

            //Move walker if next tile is either heightToCheck OR heightToAssign
            WorldTile nextTile = _2Dgrid[newX, newZ];

            if (nextTile.heightLvl == heightToCheck || nextTile.heightLvl == heightToAssign)
            {
                walker.pos = new(newX, 0, newZ);
            }

            //Check to exit loop
            float nb = GetNbTiles(heightToAssign);
            if (Mathf.Abs(nb / Mathf.Ceil(_2Dgrid.Length * _gridFillPercent) - capPercent) <= 0.01)
            {
                break;
            }

            iterations++;
        }
        while (iterations < _iterationSteps);
    }

    private void CheckHeightSurroundings()
    {
        foreach (WorldTile tile in _2Dgrid)
        {
            if (tile.heightLvl == WorldChunk.HeightLvl.Void || tile.heightLvl == WorldChunk.HeightLvl.Summit)
            {
                continue;
            }

            WorldTile[] neighbors = GetAllNeighbors(tile);

            //Check that a tile isnt surrounded by all higher/voids
            if (tile.heightLvl == WorldChunk.HeightLvl.Ground)
            {
                if (neighbors.Where(tile => tile.heightLvl != WorldChunk.HeightLvl.Ground).Count() == neighbors.Length)
                {
                    WorldTile t = _2Dgrid[tile.x, tile.y];

                    t.heightLvl =
                        neighbors.Where(t => t.heightLvl == WorldChunk.HeightLvl.Peak).Count() > 0 ?
                        WorldChunk.HeightLvl.Peak :
                        WorldChunk.HeightLvl.Summit;

                    _2Dgrid[tile.x, tile.y] = t;
                }
            }
            else if (tile.heightLvl == WorldChunk.HeightLvl.Peak)
            {
                if (neighbors.Where(tile => tile.heightLvl == WorldChunk.HeightLvl.Summit || tile.heightLvl == WorldChunk.HeightLvl.Void).Count() == neighbors.Length)
                {
                    WorldTile t = _2Dgrid[tile.x, tile.y];
                    t.heightLvl = WorldChunk.HeightLvl.Summit;
                    _2Dgrid[tile.x, tile.y] = t;
                }
            }
        }
    }

    //Old version
    private void RandomizeWorldHeight()
    {
        bool checkSummit = false;

        //1st pass
        for (int x = 0; x < _maxWidth; x++)
        {
            for (int y = 0; y < _maxHeight; y++)
            {
                WorldTile tile = _2Dgrid[x, y];
                if (tile.heightLvl == WorldChunk.HeightLvl.Void)
                {
                    continue;
                }

                WorldTile[] neighbors = GetPreviousNeighbors(tile);

                //Roll rng for summit
                if (checkSummit && GetNbTiles(WorldChunk.HeightLvl.Summit) / Mathf.Ceil(_2Dgrid.Length * _gridFillPercent) < _summitCapPercent)
                {
                    int bonusMod = neighbors.Where(
                            tile => tile.heightLvl == WorldChunk.HeightLvl.Peak || 
                            tile.heightLvl == WorldChunk.HeightLvl.Summit
                        ).Count() > 0 ? 
                        1 : 0;

                    float percent = _basePlaceSummitPercent + bonusMod * _bonusPlacePercent;

                    if (Random.value < percent) 
                    {
                        tile.heightLvl = WorldChunk.HeightLvl.Summit;
                        _2Dgrid[x, y] = tile;
                        checkSummit = !checkSummit;
                        continue;
                    }
                }

                //Roll rng for peak
                if (GetNbTiles(WorldChunk.HeightLvl.Peak) / Mathf.Ceil(_2Dgrid.Length * _gridFillPercent) < _peakCapPercent)
                {
                    int bonusMod = neighbors.Where(
                            tile => tile.heightLvl == WorldChunk.HeightLvl.Peak
                        ).Count() > 0 ?
                        1 : 0;

                    float percent = _basePlacePeakPercent + bonusMod * _bonusPlacePercent;

                    if (Random.value < percent)
                    {
                        tile.heightLvl = WorldChunk.HeightLvl.Peak;
                        _2Dgrid[x, y] = tile;
                    }
                }

                checkSummit = !checkSummit;
            }
        }
    }

    private void FindHeightGroups()
    {
        _connectedGroundGroupsList = new();
        _connectedPeakGroupsList = new();
        _connectedSummitGroupsList = new();

        for (int x = 0; x < _maxWidth; x++)
        {
            for (int y = 0; y < _maxHeight; y++)
            {
                WorldTile tile = _2Dgrid[x, y];

                if (tile.visited || tile.heightLvl == WorldChunk.HeightLvl.Void)
                {
                    continue;
                }

                //if tile has no same-height neighbors, make a group of only itself
                if (GetAllNeighbors(tile).Where(n => n.heightLvl == tile.heightLvl).Count() == 0)
                {
                    List<WorldTile> group = new();
                    group.Add(tile);
                    switch (tile.heightLvl)
                    {
                        case WorldChunk.HeightLvl.Ground: _connectedGroundGroupsList.Add(group); break;
                        case WorldChunk.HeightLvl.Peak: _connectedPeakGroupsList.Add(group); break;
                        case WorldChunk.HeightLvl.Summit: _connectedSummitGroupsList.Add(group); break;
                    }

                    _2Dgrid[tile.x, tile.y].visited = true;
                    continue;
                }

                if (x > 0)
                {
                    WorldTile neighbor = _2Dgrid[x - 1, y];

                    if (tile.heightLvl == neighbor.heightLvl)
                    {
                        List<WorldTile> group = new();
                        PopulateHeightGroup(tile, group);

                        switch (tile.heightLvl)
                        {
                            case WorldChunk.HeightLvl.Ground: _connectedGroundGroupsList.Add(group); break;
                            case WorldChunk.HeightLvl.Peak: _connectedPeakGroupsList.Add(group); break;
                            case WorldChunk.HeightLvl.Summit: _connectedSummitGroupsList.Add(group); break;
                        }
                    }
                }

                if (y > 0)
                {
                    WorldTile neighbor = _2Dgrid[x, y - 1];

                    if (tile.heightLvl == neighbor.heightLvl)
                    {
                        List<WorldTile> group = new();
                        PopulateHeightGroup(tile, group);

                        switch (tile.heightLvl)
                        {
                            case WorldChunk.HeightLvl.Ground: _connectedGroundGroupsList.Add(group); break;
                            case WorldChunk.HeightLvl.Peak: _connectedPeakGroupsList.Add(group); break;
                            case WorldChunk.HeightLvl.Summit: _connectedSummitGroupsList.Add(group); break;
                        }
                    }
                }
            }
        }

        foreach (WorldTile tile in _2Dgrid)
        {
            _2Dgrid[tile.x, tile.y].visited = false;
        }
    }

    private void PopulateHeightGroup(WorldTile tile, List<WorldTile> group)
    {
        group.Add(_2Dgrid[tile.x, tile.y]);
        _2Dgrid[tile.x, tile.y].visited = true;
        WorldTile[] neighbors = GetAllNeighbors(tile);

        foreach (WorldTile neighbor in neighbors)
        {
            if (tile.heightLvl == neighbor.heightLvl && !neighbor.visited)
            {
                PopulateHeightGroup(neighbor, group);
            }
        }
    }

    private void CheckStrandedGrounds()
    {
        bool updatedTiles = false;

        foreach (List<WorldTile> group in _connectedGroundGroupsList)
        {
            List<WorldTile> peakNeighbors = new();
            List<WorldTile> summitNeighbors = new();

            foreach (WorldTile tile in group)
            {
                WorldTile[] neighbors = GetAllNeighbors(tile);
                peakNeighbors.AddRange(neighbors.Where(n => n.heightLvl == WorldChunk.HeightLvl.Peak && !peakNeighbors.Contains(n)));
                summitNeighbors.AddRange(neighbors.Where(n => n.heightLvl == WorldChunk.HeightLvl.Summit && !summitNeighbors.Contains(n)));
            }

            if (peakNeighbors.Count > 0)
            {
                continue;
            }

            foreach (WorldTile summit in summitNeighbors)
            {
                WorldTile[] sNeighbors = GetAllNeighbors(summit);
                WorldTile[] sGroundNeighbors = sNeighbors.Where(n => n.heightLvl == WorldChunk.HeightLvl.Ground && n.y >= summit.y).ToArray();
                WorldTile[] sPeakNeighbors = sNeighbors.Where(n => n.heightLvl == WorldChunk.HeightLvl.Peak && n.y >= summit.y).ToArray();

                if (sGroundNeighbors.Count() == 0)
                {
                    continue;
                }
                else if (sPeakNeighbors.Count() > 0)
                {
                    _2Dgrid[summit.x, summit.y].heightLvl = WorldChunk.HeightLvl.Ground;
                    Debug.Log($"{summit.x},{summit.y}");
                    updatedTiles = true;
                    break;
                }
                else
                {
                    _2Dgrid[summit.x, summit.y].heightLvl = WorldChunk.HeightLvl.Peak;
                    Debug.Log($"{summit.x},{summit.y}");
                    updatedTiles = true;
                    break;
                }
            }
        }

        if (updatedTiles)
        {
            FindHeightGroups();
        }
    }


    private void LoadPrefabList()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/WorldChunks" });
        _groundChunks = new();
        _peakChunks = new();
        _summitChunks = new();
        _otherChunks = new();

        foreach (string guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            WorldChunk chunk = prefab.GetComponent<WorldChunk>();
            switch (chunk.heightLvl)
            {
                case WorldChunk.HeightLvl.Ground: _groundChunks.Add(prefab); break;
                case WorldChunk.HeightLvl.Peak: _peakChunks.Add(prefab); break;
                case WorldChunk.HeightLvl.Summit: _summitChunks.Add(prefab); break;
                default: _otherChunks.Add(prefab); break;
            }
        }
    }

    private void AssignWorldChunks()
    {
        AssignClimbChunk(_connectedSummitGroupsList.Where(g => g.Count() == 1), _connectedPeakGroupsList, WorldChunk.HeightLvl.Peak, _peakChunks, "being stuck in a loop while assigning chunks near single summits");
        AssignClimbChunk(_connectedSummitGroupsList.Where(g => g.Count() > 1), _connectedPeakGroupsList, WorldChunk.HeightLvl.Peak, _peakChunks, "being stuck in a loop while assigning chunks near grouped summits");
        //AssignClimbChunk(_connectedPeakGroupsList.Where(g => g.Count() == 1), _connectedGroundGroupsList, WorldChunk.HeightLvl.Ground, _groundChunks, "being stuck in a loop while assigning chunks near single peaks");
        //AssignClimbChunk(_connectedPeakGroupsList.Where(g => g.Count() > 1), _connectedGroundGroupsList, WorldChunk.HeightLvl.Ground, _groundChunks, "being stuck in a loop while assigning chunks near grouped peaks");


        //Assign climb chunks adjacent to single peaks
        foreach (List<WorldTile> group in _connectedPeakGroupsList.Where(g => g.Count() == 1))
        {
            List<WorldTile> lowerNeighbors = new();
            List<List<WorldTile>> neighborGroups = new();

            foreach (WorldTile tile in group)
            {
                WorldTile[] neighbors = GetAllNeighbors(tile);
                lowerNeighbors.AddRange(neighbors.Where(n => n.heightLvl == WorldChunk.HeightLvl.Ground && !lowerNeighbors.Contains(n)));

                foreach (WorldTile neighbor in neighbors)
                {
                    if (neighbor.heightLvl == WorldChunk.HeightLvl.Ground)
                    {
                        neighborGroups.AddRange(_connectedGroundGroupsList.Where(g => g.Contains(neighbor) && !neighborGroups.Contains(g)));
                    }
                }
            }

            if (lowerNeighbors.Count == 0)
            {
                continue;
            }

            foreach (List<WorldTile> ng in neighborGroups)
            {
                int iterations = 0;
                int id;
                WorldTile tileToAssign;
                WorldTile neighborInGroup;
                WorldTile[] candidates = lowerNeighbors.Where(n => ng.Contains(n)).ToArray();

                do
                {
                    id = Random.Range(0, candidates.Count());
                    tileToAssign = lowerNeighbors[id];
                    neighborInGroup = GetAllNeighbors(tileToAssign).Where(n => n.heightLvl == WorldChunk.HeightLvl.Peak && n.y <= tileToAssign.y && group.Contains(n)).DefaultIfEmpty(null).First();
                    ++iterations;
                }
                while ((tileToAssign.chunk is not null || neighborInGroup is null) && iterations <= _iterationSteps);

                if (iterations > _iterationSteps)
                {
                    QuitProgram("being stuck in a loop while assigning chunks near single peaks");
                    return;
                }

                if (neighborInGroup.x == tileToAssign.x)
                {
                    WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Up, height = WorldChunk.HeightLvl.Peak };
                    GameObject chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                    tileToAssign.chunk = chunk;
                }
                else
                {
                    if (neighborInGroup.x < tileToAssign.x)
                    {
                        WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Left, height = WorldChunk.HeightLvl.Peak };
                        GameObject chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                        tileToAssign.chunk = chunk;
                    }
                    else
                    {
                        WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Right, height = WorldChunk.HeightLvl.Peak };
                        GameObject chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                        tileToAssign.chunk = chunk;
                    }
                }
            }
        }


        //Assign chunks adjacent to grouped peaks
        foreach (List<WorldTile> group in _connectedPeakGroupsList.Where(g => g.Count() > 1))
        {
            List<WorldTile> lowerNeighbors = new();
            List<List<WorldTile>> neighborGroups = new();

            foreach (WorldTile tile in group)
            {
                WorldTile[] neighbors = GetAllNeighbors(tile);
                lowerNeighbors.AddRange(neighbors.Where(n => n.heightLvl == WorldChunk.HeightLvl.Ground && !lowerNeighbors.Contains(n)));

                foreach (WorldTile neighbor in neighbors)
                {
                    if (neighbor.heightLvl == WorldChunk.HeightLvl.Ground)
                    {
                        neighborGroups.AddRange(_connectedGroundGroupsList.Where(g => g.Contains(neighbor) && !neighborGroups.Contains(g)));
                    }
                }
            }

            if (lowerNeighbors.Count == 0)
            {
                continue;
            }

            foreach (List<WorldTile> ng in neighborGroups) 
            {
                int iterations = 0;
                int id;
                WorldTile tileToAssign;
                WorldTile neighborInGroup;
                WorldTile[] candidates = lowerNeighbors.Where(n => ng.Contains(n)).ToArray();

                do
                {
                    id = Random.Range(0, candidates.Count());
                    tileToAssign = lowerNeighbors[id];
                    neighborInGroup = GetAllNeighbors(tileToAssign).Where(n => n.heightLvl == WorldChunk.HeightLvl.Peak && n.y <= tileToAssign.y && group.Contains(n)).DefaultIfEmpty(null).First();
                    ++iterations;
                }
                while ((tileToAssign.chunk is not null || neighborInGroup is null) && iterations <= _iterationSteps);

                if (iterations > _iterationSteps)
                {
                    QuitProgram("being stuck in a loop while assigning chunks near grouped peaks");
                    return;
                }

                if (neighborInGroup.x == tileToAssign.x)
                {
                    WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Up, height = WorldChunk.HeightLvl.Peak };
                    GameObject chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                    tileToAssign.chunk = chunk;
                }
                else
                {
                    if (neighborInGroup.x < tileToAssign.x)
                    {
                        WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Left, height = WorldChunk.HeightLvl.Peak };
                        GameObject chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                        tileToAssign.chunk = chunk;
                    }
                    else
                    {
                        WorldChunk.ChunkExit exit = new() { dir = WorldChunk.Direction.Right, height = WorldChunk.HeightLvl.Peak };
                        GameObject chunk = _groundChunks.Where(c => c.GetComponent<WorldChunk>().chunkExits.Contains(exit)).First();
                        tileToAssign.chunk = chunk;
                    }
                }
            }
        }

        //Assign holes
        bool checkHorzFirst = false;

        foreach (WorldTile tile in _2Dgrid)
        {
            if (tile.chunk is not null)
            {
                continue;
            }
            else if (tile.heightLvl == WorldChunk.HeightLvl.Void || tile.heightLvl == WorldChunk.HeightLvl.Summit)
            {
                AssignBasicChunk(tile);
                continue;
            }

            if (checkHorzFirst)
            {
                if (CheckHorizontalHoleAssign(tile))
                {
                    checkHorzFirst = !checkHorzFirst;
                    continue;
                }

                if (CheckVerticalHoleAssign(tile))
                {
                    checkHorzFirst = !checkHorzFirst;
                    continue;
                }
            }
            else
            {
                if (CheckVerticalHoleAssign(tile))
                {
                    checkHorzFirst = !checkHorzFirst;
                    continue;
                }

                if (CheckHorizontalHoleAssign(tile))
                {
                    checkHorzFirst = !checkHorzFirst;
                    continue;
                }
            }

            AssignBasicChunk(tile);
        }
    }

    private void InstantiateChunks()
    {
        foreach (WorldTile tile in _2Dgrid)
        {
            Instantiate(tile.chunk, new(tile.x * _chunkSize, 0, tile.y * _chunkSize * -1), Quaternion.identity);
        }
    }
}
