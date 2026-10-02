using System;
using UnityEngine;

namespace ReGenesis
{
    // Remembers the selected tower and builds or demolishes towers on placement nodes.
    public class BuildManager : MonoBehaviour
    {
        public static BuildManager Instance { get; private set; }

        [Tooltip("Maximum towers for this stage. The GDD gives Stage 1 four slots.")]
        [SerializeField, Min(0)] private int towerSlotLimit = 4;

        private int selectionClearedFrame = -1;

        public TowerDefinition SelectedTower { get; private set; }
        public int TowersBuilt { get; private set; }
        public int TowerSlotLimit => towerSlotLimit;

        // True during the frame a selection was cancelled, so the same right click does not also demolish a tower.
        public bool SelectionClearedThisFrame => selectionClearedFrame == Time.frameCount;

        public event Action<TowerDefinition> SelectionChanged;
        public event Action TowersChanged;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("More than one BuildManager in the scene. Removing the extra one.", this);
                Destroy(this);
                return;
            }

            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        void Update()
        {
            if (SelectedTower != null && (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1)))
                ClearSelection();
        }

        // Hook this to a tower button and pass the tower's TowerDefinition asset.
        public void SelectTower(TowerDefinition tower)
        {
            if (tower == null || tower.Prefab == null)
            {
                Debug.LogWarning("SelectTower needs a TowerDefinition with a prefab assigned.", this);
                return;
            }

            SelectedTower = tower;
            SelectionChanged?.Invoke(tower);
        }

        public void ClearSelection()
        {
            if (SelectedTower == null)
                return;

            SelectedTower = null;
            selectionClearedFrame = Time.frameCount;
            SelectionChanged?.Invoke(null);
        }

        public bool CanPlace(PlacementNode node, out string reason)
        {
            if (SelectedTower == null)
                reason = "No tower selected.";
            else if (GameManager.Instance != null && !GameManager.Instance.CanBuild)
                reason = "Building is not allowed right now.";
            else if (node.IsOccupied)
                reason = "This node already has a tower.";
            else if (TowersBuilt >= towerSlotLimit)
                reason = "No tower slots left.";
            else if (EnergyBank.Instance != null && !EnergyBank.Instance.CanAfford(SelectedTower.BuildCost))
                reason = "Not enough energy.";
            else
                reason = null;

            return reason == null;
        }

        public bool TryBuild(PlacementNode node)
        {
            if (!CanPlace(node, out string reason))
            {
                Debug.Log($"Cannot build on {node.name}: {reason}", node);
                return false;
            }

            if (EnergyBank.Instance != null && !EnergyBank.Instance.TrySpend(SelectedTower.BuildCost))
                return false;

            Transform buildPoint = node.BuildPoint;
            GameObject tower = Instantiate(SelectedTower.Prefab, buildPoint.position, buildPoint.rotation);
            node.SetTower(tower, SelectedTower);

            TowersBuilt++;
            TowersChanged?.Invoke();
            ClearSelection();
            return true;
        }

        public bool TryDemolish(PlacementNode node)
        {
            if (!node.IsOccupied)
                return false;
            if (GameManager.Instance != null && !GameManager.Instance.CanBuild)
                return false;

            if (EnergyBank.Instance != null && node.TowerDefinition != null)
                EnergyBank.Instance.Add(node.TowerDefinition.RefundAmount);

            Destroy(node.Tower);
            node.ClearTower();

            TowersBuilt = Mathf.Max(0, TowersBuilt - 1);
            TowersChanged?.Invoke();
            return true;
        }
    }
}
