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
        Left,
        Right
    }

    [SerializeField]
    private int _groundLvl;

    [SerializeField]
    private (Direction dir, HeightLvl height)[] _climbableWalls;

    [SerializeField]
    private Vector3[] _spawnLocations;
}
