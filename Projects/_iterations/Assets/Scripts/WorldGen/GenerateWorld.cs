using System.Collections.Generic;
using System.Linq;
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

    private struct WorldTile
    {
        public WorldChunk.HeightLvl heightLvl;
        public WorldChunk chunk;
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
    private int _maxWalkers = 5;
    private int _iterationSteps = 100000;
    private float _chunkSize = 15;

    private void Awake()
    {
        SetupGrid();
        RandomizeWorldShape();
        RandomizeWorldHeight();
        FindHeightGroups();
        GenerateWorldChunks();
        //spawn player + pick if not picked up
    }

    #region HelperFuncs
    private Vector3 GetRandomDirection()
    {
        int rng = Mathf.FloorToInt(Random.value * 3.99f);

        return rng switch
        {
            0 => new Vector3(0, 0, -1),
            1 => new Vector3(-1, 0, 0),
            2 => new Vector3(0, 0, 1),
            _ => new Vector3(1, 0, 0),
        };
    }

    private int GetNbTiles(WorldChunk.HeightLvl countType = WorldChunk.HeightLvl.Void) //if Void we count all used tiles
    {
        int count = 0;

        foreach (WorldTile tile in _2Dgrid)
        {
            if ((countType == WorldChunk.HeightLvl.Void && tile.heightLvl != WorldChunk.HeightLvl.Void) || tile.heightLvl == countType)
            {
                count++;
            }
        }

        return count;
    }

    private void ResetVisited()
    {
        for (int x = 0; x < _maxWidth; x++)
        {
            for (int y = 0; y < _maxHeight; y++)
            {
                WorldTile tile = _2Dgrid[x, y];
                tile.visited = false;
                _2Dgrid[x, y] = tile;
            }
        }
    }

    private WorldTile[] GetAllNeighbors(WorldTile tile)
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
            }
            else
            {
                neighbors = new WorldTile[] {
                            _2Dgrid[tile.x + 1, tile.y],
                            _2Dgrid[tile.x, tile.y + 1],
                            _2Dgrid[tile.x, tile.y - 1]
                        };
            }
        }
        else if (tile.y == 0)
        {
            neighbors = new WorldTile[] {
                        _2Dgrid[tile.x + 1, tile.y],
                        _2Dgrid[tile.x - 1, tile.y],
                        _2Dgrid[tile.x, tile.y + 1]
                    };
        }
        else
        {
            neighbors = new WorldTile[] {
                        _2Dgrid[tile.x + 1, tile.y],
                        _2Dgrid[tile.x - 1, tile.y],
                        _2Dgrid[tile.x, tile.y + 1],
                        _2Dgrid[tile.x, tile.y - 1]
                    };
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
                _2Dgrid[(int)walker.pos.x, (int)walker.pos.y].heightLvl = WorldChunk.HeightLvl.Ground;
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
                walker.pos.x = Mathf.Clamp(walker.pos.x, 1, _maxWidth - 2);
                walker.pos.z = Mathf.Clamp(walker.pos.y, 1, _maxHeight - 2);
                _walkers[i] = walker;
            }

            //Check to exit loop
            if (GetNbTiles() / _2Dgrid.Length > _gridFillPercent)
            {
                break;
            }

            iterations++;
        }
        while (iterations < _iterationSteps);
    }

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
                if (checkSummit && GetNbTiles(WorldChunk.HeightLvl.Summit) / _2Dgrid.Length < _summitCapPercent)
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
                if (GetNbTiles(WorldChunk.HeightLvl.Peak) / _2Dgrid.Length < _peakCapPercent)
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

        //2nd pass to double-check there isnt a tile whose neighbors are all a mix of higher/void
        for (int x = 0; x < _maxWidth; x++)
        {
            for (int y = 0; y < _maxHeight; y++)
            {
                WorldTile tile = _2Dgrid[x, y];

                if (tile.heightLvl == WorldChunk.HeightLvl.Void || tile.heightLvl == WorldChunk.HeightLvl.Peak)
                {
                    continue;
                }

                WorldTile[] neighbors = GetAllNeighbors(tile);

                if (tile.heightLvl == WorldChunk.HeightLvl.Ground)
                {
                    if (neighbors.Where(tile => tile.heightLvl != WorldChunk.HeightLvl.Ground).Count() == neighbors.Length)
                    {
                        tile.heightLvl =
                            neighbors.Where(tile => tile.heightLvl == WorldChunk.HeightLvl.Peak).Count() > 0 ?
                            WorldChunk.HeightLvl.Peak :
                            WorldChunk.HeightLvl.Summit;

                        _2Dgrid[x, y] = tile;
                    }
                }
                else if (tile.heightLvl == WorldChunk.HeightLvl.Peak)
                {
                    if (neighbors.Where(tile => tile.heightLvl == WorldChunk.HeightLvl.Summit || tile.heightLvl == WorldChunk.HeightLvl.Void).Count() == neighbors.Length)
                    {
                        tile.heightLvl = WorldChunk.HeightLvl.Summit;
                        _2Dgrid[x, y] = tile;
                    }
                }
            }
        }
    }

    private void FindHeightGroups()
    {
        for (int x = 0; x < _maxWidth; x++)
        {
            for (int y = 0; y < _maxHeight; y++)
            {
                WorldTile tile = _2Dgrid[x, y];

                if (tile.visited || tile.heightLvl == WorldChunk.HeightLvl.Void)
                {
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

        ResetVisited();
    }

    private void PopulateHeightGroup(WorldTile tile, List<WorldTile> group)
    {
        group.Add(tile);
        tile.visited = true;
        WorldTile[] neighbors = GetAllNeighbors(tile);

        foreach (WorldTile neighbor in neighbors)
        {
            if (tile.heightLvl == neighbor.heightLvl && !neighbor.visited)
            {
                PopulateHeightGroup(neighbor, group);
            }
        }
    }

    private void GenerateWorldChunks()
    {
        /*
            IMPORTANT: *-1 when shifting on z so that 0,0 is top left
            Logic: foreach in grid
                check surrounding tiles, keep track of increase in height
                if any height increase,
                    basic logic: assign chunk w all matching sides
                    adv logic: make sure each peak/summit group has at least 1 climb encounter
                watch for chunk transitions in any case   
        */
    }
}
