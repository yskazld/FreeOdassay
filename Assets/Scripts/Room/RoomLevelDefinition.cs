using UnityEngine;

namespace FreelanceOdyssey.Room
{
    [CreateAssetMenu(fileName = "RoomLevel", menuName = "FreelanceOdyssey/Room Level", order = 0)]
    public class RoomLevelDefinition : ScriptableObject
    {
        public int level = 1;
        public Sprite background;
        public int upgradeCost = 500;
        [Tooltip("Idle XP multiplier applied after upgrade")] public float idleXpMultiplier = 1f;
    }
}
