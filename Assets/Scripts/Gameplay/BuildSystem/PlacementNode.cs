using UnityEngine;

namespace ReGenesis
{
    // A clickable build spot. Needs a collider on the same GameObject to receive mouse events.
    public class PlacementNode : MonoBehaviour
    {
        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");

        [SerializeField] private Transform buildPoint;
        [Tooltip("Renderer tinted to show the node state. Defaults to the first renderer in the children.")]
        [SerializeField] private Renderer highlightRenderer;

        [Header("State Colors")]
        [SerializeField] private Color emptyColor = new Color(0.2f, 0.9f, 1f);
        [SerializeField] private Color hoverValidColor = new Color(1f, 0.9f, 0.2f);
        [SerializeField] private Color hoverInvalidColor = new Color(1f, 0.25f, 0.25f);
        [SerializeField] private Color occupiedColor = new Color(0.55f, 0.55f, 0.55f);

        private MaterialPropertyBlock propertyBlock;
        private bool hovered;

        public Transform BuildPoint => buildPoint != null ? buildPoint : transform;
        public GameObject Tower { get; private set; }
        public TowerDefinition TowerDefinition { get; private set; }
        public bool IsOccupied => Tower != null;

        void Reset()
        {
            buildPoint = transform.Find("BuildPoint");
            highlightRenderer = GetComponentInChildren<Renderer>();
        }

        void Awake()
        {
            if (highlightRenderer == null)
                highlightRenderer = GetComponentInChildren<Renderer>();

            propertyBlock = new MaterialPropertyBlock();
        }

        void Start()
        {
            RefreshVisual();
        }

        void OnMouseEnter()
        {
            hovered = true;
            RefreshVisual();
        }

        void OnMouseExit()
        {
            hovered = false;
            RefreshVisual();
        }

        void OnMouseOver()
        {
            // Energy or slot counts can change while hovering, so keep the color current.
            RefreshVisual();

            BuildManager buildManager = BuildManager.Instance;
            if (Input.GetMouseButtonDown(1) && IsOccupied && !UIPointer.IsOverUI && buildManager != null
                && buildManager.SelectedTower == null && !buildManager.SelectionClearedThisFrame)
            {
                buildManager.TryDemolish(this);
            }
        }

        void OnMouseDown()
        {
            if (UIPointer.IsOverUI || BuildManager.Instance == null)
                return;

            BuildManager.Instance.TryBuild(this);
        }

        public void SetTower(GameObject tower, TowerDefinition definition)
        {
            Tower = tower;
            TowerDefinition = definition;
            RefreshVisual();
        }

        public void ClearTower()
        {
            Tower = null;
            TowerDefinition = null;
            RefreshVisual();
        }

        void RefreshVisual()
        {
            if (highlightRenderer == null)
                return;

            Color color = emptyColor;
            if (IsOccupied)
            {
                color = occupiedColor;
            }
            else if (hovered)
            {
                BuildManager buildManager = BuildManager.Instance;
                bool hasSelection = buildManager != null && buildManager.SelectedTower != null;
                color = !hasSelection || buildManager.CanPlace(this, out _) ? hoverValidColor : hoverInvalidColor;
            }

            highlightRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(BaseColorId, color);
            propertyBlock.SetColor(ColorId, color);
            highlightRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
