using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Localization;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Save;
using UnityEngine;
using UnityEngine.UI;

namespace Lanternfall.Gameplay.UI
{
    /// <summary>
    /// Responsive, code-built HUD shared by every playable scene.
    /// It remains readable at 720p through 4K and supports runtime UI scaling.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(PlayerInputReader))]
    public sealed class GameHud : MonoBehaviour
    {
        private Health _health;
        private PlayerInputReader _input;
        private PlayerCombat _combat;
        private RunInventory _inventory;
        private CanvasScaler _scaler;
        private Image _healthFill;
        private Image _bossFill;
        private GameObject _bossPanel;
        private GameObject _pausePanel;
        private Text _healthText;
        private Text _bossText;
        private Text _resourcesText;
        private Text _abilityText;
        private Text _announcement;
        private Text _pauseCopy;
        private SettingsData _settings;
        private bool _paused;
        private float _announcementTime;

        private static readonly Color Ink = new Color(.025f, .035f, .055f, .92f);
        private static readonly Color Ember = new Color(1f, .43f, .14f);
        private static readonly Color Tide = new Color(.18f, .82f, .92f);

        private void Awake()
        {
            _health = GetComponent<Health>();
            _input = GetComponent<PlayerInputReader>();
            _combat = GetComponent<PlayerCombat>();
            _inventory = GetComponent<RunInventory>();
            _settings = HubController.Instance?.Profile?.settings ?? new SettingsData();
            AccessibilityRuntime.Apply(_settings);
            Build();
            OnHealthChanged(_health.Current, _health.Maximum);
            RefreshResources();
        }

        private void OnEnable()
        {
            _health.Changed += OnHealthChanged;
            if (_inventory != null)
            {
                _inventory.Changed += RefreshResources;
                _inventory.Wallet.Changed += OnWalletChanged;
            }
            BossEncounterSignals.IntroStarted += OnBossIntro;
            BossEncounterSignals.HealthChanged += OnBossHealth;
            BossEncounterSignals.PhaseChanged += OnBossPhase;
            BossEncounterSignals.Defeated += OnBossDefeated;
            AccessibilityRuntime.Changed += ApplyAccessibility;
        }

        private void OnDisable()
        {
            if (_health != null) _health.Changed -= OnHealthChanged;
            if (_inventory != null)
            {
                _inventory.Changed -= RefreshResources;
                _inventory.Wallet.Changed -= OnWalletChanged;
            }
            BossEncounterSignals.IntroStarted -= OnBossIntro;
            BossEncounterSignals.HealthChanged -= OnBossHealth;
            BossEncounterSignals.PhaseChanged -= OnBossPhase;
            BossEncounterSignals.Defeated -= OnBossDefeated;
            AccessibilityRuntime.Changed -= ApplyAccessibility;
            if (_paused) Time.timeScale = 1f;
        }

        private void Update()
        {
            if (_input.PausePressedThisFrame) SetPaused(!_paused);
            if (_paused) ReadSettingsInput();
            if (_combat != null && _abilityText != null)
            {
                float remaining = _combat.AbilityCooldownRemaining;
                _abilityText.text = remaining > 0f
                    ? $"Q / LB  ABILITY  {remaining:0.0}s"
                    : "Q / LB  ABILITY  READY";
            }
            if (_announcementTime > 0f)
            {
                _announcementTime -= Time.unscaledDeltaTime;
                if (_announcementTime <= 0f) _announcement.gameObject.SetActive(false);
            }
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            _pausePanel.SetActive(paused);
            if (paused) RefreshPauseCopy();
            else HubController.Instance?.SaveNow();
        }

        private void Build()
        {
            GameObject canvasObject = new GameObject("Lanternfall HUD");
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            _scaler = canvasObject.AddComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            _scaler.referenceResolution = new Vector2(1920f, 1080f);
            _scaler.matchWidthOrHeight = .5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject healthPanel = Panel(
                canvasObject.transform, "Vitality", new Vector2(24f, -24f),
                new Vector2(420f, 92f), new Vector2(0f, 1f));
            _healthFill = Bar(healthPanel.transform, "Health", Tide, 18f, 50f);
            _healthText = Label(healthPanel.transform, "Health Text",
                "LANTERN  180 / 180", 22, TextAnchor.UpperLeft);
            SetRect(_healthText.rectTransform,
                new Vector2(18f, -10f), new Vector2(390f, 34f), new Vector2(0f, 1f));

            GameObject resources = Panel(
                canvasObject.transform, "Run Status", new Vector2(24f, 24f),
                new Vector2(520f, 82f), Vector2.zero);
            _resourcesText = Label(resources.transform, "Resources",
                "GOLD 0   ECHOES 0/3   BUFFS —   DEBUFFS —", 19,
                TextAnchor.MiddleLeft);
            Stretch(_resourcesText.rectTransform, 16f);

            GameObject ability = Panel(
                canvasObject.transform, "Ability", new Vector2(-24f, 24f),
                new Vector2(320f, 72f), new Vector2(1f, 0f));
            _abilityText = Label(ability.transform, "Cooldown",
                "Q / LB  ABILITY  READY", 19, TextAnchor.MiddleCenter);
            Stretch(_abilityText.rectTransform, 10f);

            GameObject minimap = Panel(
                canvasObject.transform, "Minimap", new Vector2(-24f, -24f),
                new Vector2(250f, 150f), Vector2.one);
            Text mapText = Label(minimap.transform, "Route",
                "NARTHEX ROUTE\n◆ CURRENT CHAMBER\n? UNCHARTED", 18,
                TextAnchor.MiddleCenter);
            Stretch(mapText.rectTransform, 12f);

            _bossPanel = Panel(
                canvasObject.transform, "Guardian", new Vector2(0f, -30f),
                new Vector2(780f, 96f), new Vector2(.5f, 1f));
            _bossFill = Bar(_bossPanel.transform, "Guardian Health", Ember, 18f, 50f);
            _bossText = Label(_bossPanel.transform, "Guardian Name",
                "GUARDIAN", 22, TextAnchor.UpperCenter);
            SetRect(_bossText.rectTransform,
                new Vector2(0f, -8f), new Vector2(740f, 34f), new Vector2(.5f, 1f));
            _bossPanel.SetActive(false);

            _announcement = Label(canvasObject.transform, "Announcement",
                string.Empty, 34, TextAnchor.MiddleCenter);
            SetRect(_announcement.rectTransform,
                Vector2.zero, new Vector2(900f, 100f), new Vector2(.5f, .68f));
            _announcement.gameObject.SetActive(false);

            _pausePanel = Panel(
                canvasObject.transform, "Pause & Settings", Vector2.zero,
                new Vector2(620f, 570f), new Vector2(.5f, .5f));
            _pauseCopy = Label(_pausePanel.transform, "Pause Copy",
                string.Empty, 22, TextAnchor.MiddleCenter);
            Stretch(_pauseCopy.rectTransform, 28f);
            _pausePanel.SetActive(false);
            ApplyAccessibility();
        }

        private void ApplyAccessibility()
        {
            if (_scaler != null)
                _scaler.referenceResolution =
                    new Vector2(1920f, 1080f) / AccessibilityRuntime.UiScale;
            RefreshPauseCopy();
        }

        private void ReadSettingsInput()
        {
            bool changed = false;
            if (_input.UiScaleUpPressedThisFrame)
            {
                _settings.uiScale += .1f;
                changed = true;
            }
            if (_input.UiScaleDownPressedThisFrame)
            {
                _settings.uiScale -= .1f;
                changed = true;
            }
            if (_input.ReducedMotionPressedThisFrame)
            {
                _settings.reducedMotion = !_settings.reducedMotion;
                changed = true;
            }
            if (_input.SubtitlesPressedThisFrame)
            {
                _settings.subtitles = !_settings.subtitles;
                changed = true;
            }
            if (_input.HighContrastPressedThisFrame)
            {
                _settings.highContrastTelegraphs =
                    !_settings.highContrastTelegraphs;
                changed = true;
            }
            if (changed) AccessibilityRuntime.Apply(_settings);
        }

        private void RefreshPauseCopy()
        {
            if (_pauseCopy == null || _settings == null) return;
            _pauseCopy.text =
                "LANTERN PAUSED\n\nESC / START  RESUME\n\nACCESSIBILITY\n" +
                $"- / +  or D-PAD   UI SCALE  {_settings.uiScale:0.0}x\n" +
                $"R / R3   REDUCED MOTION  {OnOff(_settings.reducedMotion)}\n" +
                $"T / Y   SUBTITLES  {OnOff(_settings.subtitles)}\n" +
                $"H / L3   HIGH CONTRAST  {OnOff(_settings.highContrastTelegraphs)}\n\n" +
                "CONTROLS\nKeyboard & mouse / controller\n" +
                "Binding override profile supported";
        }

        private static string OnOff(bool value) => value ? "ON" : "OFF";

        private static string L(string key, string fallback) =>
            LocalizationRuntime.Get(key, fallback);

        private void OnHealthChanged(float current, float maximum)
        {
            _healthFill.fillAmount = maximum > 0f ? current / maximum : 0f;
            _healthText.text =
                $"{L("hud.health", "LANTERN")}  {Mathf.CeilToInt(current)} / " +
                $"{Mathf.CeilToInt(maximum)}";
        }

        private void OnWalletChanged(CurrencyKind _, int __) => RefreshResources();

        private void RefreshResources()
        {
            int gold = _inventory?.Wallet.Get(CurrencyKind.Gold) ?? 0;
            int relics = _inventory?.Owned.Count ?? 0;
            string calling = HubController.Instance?.SelectedClassName ?? "Bearer";
            _resourcesText.text =
                $"{L("hud.gold", "GOLD")} {gold}   " +
                $"{L("hud.echoes", "ECHOES")} {relics}/3   " +
                $"CALLING {calling.ToUpperInvariant()}   " +
                $"{L("hud.buffs", "BUFFS")} —   " +
                $"{L("hud.debuffs", "DEBUFFS")} —";
        }

        private void OnBossIntro(string bossName)
        {
            _bossPanel.SetActive(true);
            _bossFill.fillAmount = 1f;
            _bossText.text = bossName.ToUpperInvariant();
            Announce(bossName);
        }

        private void OnBossHealth(float current, float maximum) =>
            _bossFill.fillAmount = maximum > 0f ? current / maximum : 0f;

        private void OnBossPhase(int phase) =>
            Announce($"{L("boss.phase", "Guardian phase")} {phase}");

        private void OnBossDefeated(string _)
        {
            _bossPanel.SetActive(false);
            Announce(L("boss.defeated", "GUARDIAN MEMORY CLAIMED"));
        }

        private void Announce(string copy)
        {
            _announcement.text = copy.ToUpperInvariant();
            _announcement.gameObject.SetActive(true);
            _announcementTime = 2.5f;
        }

        private static GameObject Panel(
            Transform parent, string name, Vector2 position,
            Vector2 size, Vector2 anchor)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = Ink;
            SetRect(panel.GetComponent<RectTransform>(), position, size, anchor);
            return panel;
        }

        private static Image Bar(
            Transform parent, string name, Color color, float inset, float top)
        {
            GameObject background = new GameObject(
                $"{name} Track", typeof(RectTransform), typeof(Image));
            background.transform.SetParent(parent, false);
            background.GetComponent<Image>().color = new Color(.07f, .08f, .1f, 1f);
            RectTransform track = background.GetComponent<RectTransform>();
            track.anchorMin = new Vector2(0f, 0f);
            track.anchorMax = new Vector2(1f, 1f);
            track.offsetMin = new Vector2(inset, 14f);
            track.offsetMax = new Vector2(-inset, -top);
            GameObject fill = new GameObject(
                name, typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(background.transform, false);
            Image image = fill.GetComponent<Image>();
            image.color = color;
            image.type = Image.Type.Filled;
            image.fillMethod = Image.FillMethod.Horizontal;
            Stretch(fill.GetComponent<RectTransform>(), 3f);
            return image;
        }

        private static Text Label(
            Transform parent, string name, string copy, int size, TextAnchor alignment)
        {
            GameObject label = new GameObject(name, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(parent, false);
            Text text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = copy;
            text.fontSize = size;
            text.alignment = alignment;
            text.color = Color.white;
            text.supportRichText = false;
            return text;
        }

        private static void SetRect(
            RectTransform rect, Vector2 position, Vector2 size, Vector2 anchor)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        private static void Stretch(RectTransform rect, float inset)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.one * inset;
            rect.offsetMax = -Vector2.one * inset;
        }
    }
}
