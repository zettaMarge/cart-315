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

    public enum Direction
    {
        Up,
        Down,
        Left,
        Right
    }

    public enum EncounterType
    {
        None,
        Climb,
        Hole
    }

    [System.Serializable]
    public struct ChunkExit
    {
        public Direction dir;
        public HeightLvl height;
    }

    [SerializeField]
    private HeightLvl _heightLvl;
    public HeightLvl heightLvl { get { return _heightLvl; } }

    [SerializeField]
    private EncounterType _encounter;
    public EncounterType encounter { get { return _encounter; } }

    [SerializeField]
    private ChunkExit[] _chunkExits;
    public ChunkExit[] chunkExits { get { return _chunkExits; } }
}
