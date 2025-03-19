using System.Collections.Generic;
using UnityEngine;

public class GenerateWorld : MonoBehaviour
{
    private int _maxWidth;
    private int _maxHeight;

    private struct WorldTile
    {
        public WorldChunk.HeightLvl heightLvl;
        public WorldChunk chunk;
    }

    private WorldTile[,] _2Dgrid;

    private struct RandomWalker
    {
        public Vector3 dir;
        public Vector3 pos;
    }

    private List<RandomWalker> _walkers;
    private float _gridFillPercent = 0.2f;
    private float _walkerDirChangeProb = 0.5f;
    private float _walkerSpawnProb = 0.05f;
    private float _walkerDestroyProb = 0.05f;
    private int _maxWalkers = 5;
    private int _iterationSteps = 100000;

    private float _summitCapPercent;
    private float _peakCapPercent;

    private void Awake()
    {
        SetupGrid();
        RandomizeWorldShape();
        RandomizeWorldHeight();
        //foreach coord, place random matching chunk
        //spawn player + pick if not picked up
    }

    private Vector3 GetRandomDirection()
    {
        int rng = Mathf.FloorToInt(Random.value * 3.99f);

        return rng switch
        {
            0 => new Vector3(0,0,-1),
            1 => new Vector3(-1,0,0),
            2 => new Vector3(0,0,1),
            _ => new Vector3(1,0,0),
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

    private void SetupGrid()
    {
        _2Dgrid = new WorldTile[_maxWidth, _maxHeight];

        for (int x = 0; x < _maxWidth; x++)
        {
            for (int y = 0; y < _maxHeight; y++)
            {
                WorldTile tile = new();
                tile.heightLvl = WorldChunk.HeightLvl.Void;
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
            //base Ground lvl wherever walkers go
            foreach (RandomWalker walker in _walkers)
            {
                _2Dgrid[(int)walker.pos.x, (int)walker.pos.y].heightLvl = WorldChunk.HeightLvl.Ground;
            }

            //chance: destroy Walker
            int numberChecks = _walkers.Count;
            for (int i = 0; i < numberChecks; i++)
            {
                if (Random.value < _walkerDestroyProb && _walkers.Count > 1)
                {
                    _walkers.RemoveAt(i);
                    break;
                }
            }

            //chance: Walker pick new direction
            for (int i = 0; i < _walkers.Count; i++)
            {
                if (Random.value < _walkerDirChangeProb)
                {
                    RandomWalker thisWalker = _walkers[i];
                    thisWalker.dir = GetRandomDirection();
                    _walkers[i] = thisWalker;
                }
            }

            //chance: spawn new Walker
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

            //move Walkers
            for (int i = 0; i < _walkers.Count; i++)
            {
                RandomWalker walker = _walkers[i];
                walker.pos += walker.dir;
                _walkers[i] = walker;
            }

            //avoid border of grid
            for (int i = 0; i < _walkers.Count; i++)
            {
                RandomWalker walker = _walkers[i];
                walker.pos.x = Mathf.Clamp(walker.pos.x, 1, _maxWidth - 2);
                walker.pos.z = Mathf.Clamp(walker.pos.y, 1, _maxHeight - 2);
                _walkers[i] = walker;
            }

            //check to exit loop
            if ((float)GetNbTiles() / (float)_2Dgrid.Length > _gridFillPercent)
            {
                break;
            }

            iterations++;
        } 
        while (iterations < _iterationSteps);
    }

    private void RandomizeWorldHeight()
    {
        /*
            Logic: foreach in grid
                if y-1 is ground, stay ground
                else if !summitCap
                    if y-1 is !(peak|ground), random chance summit, more likely if summit already around
                else if !peakCap, 
                    if y-1 is !ground, random chance peak, more likely if (peak|summit) already around
                else ground

                2nd loop
                    if ground && all surrounding are (peak|summit), set to peak
        */
    }

    private void GenerateWorldChunks()
    {
        /*
            Logic: foreach in grid
                check surrounding tiles, keep track of increase in height
                if any height increase,
                    basic logic: assign chunk w all matching sides

                    adv logic:
                    check which increase side has less surrounding matching heights
                    assign matching chunk
                else
                    
                    
        */
    }
}
