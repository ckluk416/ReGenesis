using UnityEngine;

namespace ReGenesis
{
    [CreateAssetMenu(fileName = "Tower_", menuName = "ReGenesis/Tower Definition")]
    public class TowerDefinition : ScriptableObject
    {
        [SerializeField] private string displayName = "Solar Turret";
        [SerializeField] private GameObject prefab;
        [SerializeField, Min(0)] private int buildCost = 50;
        [Tooltip("Share of the build cost returned on demolition. The GDD sets this to 50 percent.")]
        [SerializeField, Range(0f, 1f)] private float refundRatio = 0.5f;

        public string DisplayName => displayName;
        public GameObject Prefab => prefab;
        public int BuildCost => buildCost;
        public int RefundAmount => Mathf.FloorToInt(buildCost * refundRatio);
    }
}
