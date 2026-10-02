using System.Collections.Generic;
using UnityEngine;

namespace ReGenesis
{
    // Placeholder HUD drawn with IMGUI, so it works without any Canvas or UI package setup.
    // Replace it with a Canvas HUD once the gameplay loop is proven.
    public class PrototypeHUD : MonoBehaviour
    {
        [SerializeField] private List<TowerDefinition> buildableTowers = new List<TowerDefinition>();
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField, Range(0.5f, 3f)] private float uiScale = 1.25f;

        private readonly List<Rect> blockingRects = new List<Rect>();
        private GUIStyle titleStyle;

        void LateUpdate()
        {
            // Rects come from the previous OnGUI pass, which is close enough for blocking clicks.
            Vector2 mouse = Input.mousePosition;
            Vector2 guiMouse = new Vector2(mouse.x, Screen.height - mouse.y) / uiScale;

            bool over = false;
            foreach (Rect rect in blockingRects)
            {
                if (rect.Contains(guiMouse))
                {
                    over = true;
                    break;
                }
            }
            UIPointer.IsOverUI = over;
        }

        void OnDisable()
        {
            UIPointer.IsOverUI = false;
        }

        void OnGUI()
        {
            if (Event.current.type == EventType.Layout)
                blockingRects.Clear();

            if (titleStyle == null)
            {
                titleStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 28,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            GUI.matrix = Matrix4x4.Scale(new Vector3(uiScale, uiScale, 1f));
            float width = Screen.width / uiScale;
            float height = Screen.height / uiScale;

            DrawTopBar(width);
            DrawBuildBar(height);
            DrawCenterPanel(width, height);
        }

        void DrawTopBar(float width)
        {
            Rect area = Block(new Rect(10f, 10f, width - 20f, 30f));
            GUI.Box(area, GUIContent.none);

            EnergyCoreHealth core = EnergyCoreHealth.Instance;
            string coreText = core != null ? $"Core {core.CurrentHealth:0} / {core.MaxHealth:0}" : "Core -";
            string waveText = waveSpawner != null ? $"Wave {waveSpawner.CurrentWaveNumber} / {waveSpawner.TotalWaves}" : "Wave -";
            string energyText = EnergyBank.Instance != null ? $"Energy {EnergyBank.Instance.Energy}" : "Energy -";
            string slotText = BuildManager.Instance != null
                ? $"Slots {BuildManager.Instance.TowersBuilt} / {BuildManager.Instance.TowerSlotLimit}"
                : "Slots -";
            string stateText = GameManager.Instance != null ? GameManager.Instance.State.ToString() : "-";

            GUI.Label(new Rect(area.x + 10f, area.y + 5f, area.width - 20f, 20f),
                $"{coreText}     {waveText}     {energyText}     {slotText}     Pollution 0%     State: {stateText}");
        }

        void DrawBuildBar(float height)
        {
            const float rowHeight = 30f;
            const float barWidth = 460f;
            int rows = buildableTowers.Count + 3;
            Rect area = Block(new Rect(10f, height - 10f - rows * (rowHeight + 4f) - 8f, barWidth, rows * (rowHeight + 4f) + 8f));
            GUI.Box(area, GUIContent.none);

            BuildManager buildManager = BuildManager.Instance;
            GameManager gameManager = GameManager.Instance;
            float y = area.y + 6f;

            foreach (TowerDefinition tower in buildableTowers)
            {
                if (tower == null)
                    continue;

                if (GUI.Button(new Rect(area.x + 6f, y, barWidth - 12f, rowHeight), $"{tower.DisplayName}  ({tower.BuildCost} energy)")
                    && buildManager != null)
                {
                    buildManager.SelectTower(tower);
                }
                y += rowHeight + 4f;
            }

            string selected = buildManager != null && buildManager.SelectedTower != null
                ? $"Selected: {buildManager.SelectedTower.DisplayName}  (Esc or right click to cancel)"
                : "Selected: none";
            GUI.Label(new Rect(area.x + 8f, y + 5f, barWidth - 16f, rowHeight), selected);
            y += rowHeight + 4f;

            GUI.enabled = gameManager != null && gameManager.CanStartWave;
            if (GUI.Button(new Rect(area.x + 6f, y, barWidth - 12f, rowHeight), "Start Wave"))
                gameManager.StartNextWave();
            GUI.enabled = true;
            y += rowHeight + 4f;

            GUI.Label(new Rect(area.x + 8f, y + 5f, barWidth - 16f, rowHeight),
                "Left click node: build    Right click tower: demolish    P: pause");
        }

        void DrawCenterPanel(float width, float height)
        {
            GameManager gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            string title;
            string buttonText;
            switch (gameManager.State)
            {
                case GameState.Victory:
                    title = "SYSTEM RESTORED";
                    buttonText = "Restart";
                    break;
                case GameState.Defeat:
                    title = "CORE CORRUPTED";
                    buttonText = "Restart";
                    break;
                case GameState.Paused:
                    title = "PAUSED";
                    buttonText = "Resume";
                    break;
                default:
                    return;
            }

            Rect panel = Block(new Rect(width / 2f - 200f, height / 2f - 80f, 400f, 160f));
            GUI.Box(panel, GUIContent.none);
            GUI.Label(new Rect(panel.x, panel.y + 20f, panel.width, 50f), title, titleStyle);

            if (GUI.Button(new Rect(panel.x + 120f, panel.y + 95f, 160f, 40f), buttonText))
            {
                if (gameManager.State == GameState.Paused)
                    gameManager.TogglePause();
                else
                    gameManager.Restart();
            }
        }

        Rect Block(Rect rect)
        {
            if (Event.current.type == EventType.Layout)
                blockingRects.Add(rect);
            return rect;
        }
    }
}
