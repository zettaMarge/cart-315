using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    public enum HeightLvl
    {
        Void,
        Ground,
        Peak,
        Summit
    }

    private enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    [System.Serializable]
    private struct ChunkExit
    {
        public Direction dir;
        public HeightLvl height;
    }

    [SerializeField]
    private HeightLvl _heightLvl;
    public HeightLvl heightLvl { get { return _heightLvl; } }

    [SerializeField]
    private ChunkExit[] _chunkExits;

    [SerializeField]
    private Vector3[] _spawnLocations;

    [SerializeField]
    private GameObject[] _hazardGroups;
}
