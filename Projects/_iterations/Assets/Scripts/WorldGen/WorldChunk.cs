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

    [SerializeField]
    private int _groundLvl;

    [SerializeField]
    private (Direction dir, HeightLvl height)[] _chunkTransitions;

    [SerializeField]
    private Vector3[] _spawnLocations;

    [SerializeField]
    private GameObject[] _hazardGroups;
}
