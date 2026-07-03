using System.Text;
using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Bosses;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Localization;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Save;
using Lanternfall.Gameplay.Run;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;
using UnityEngine.SceneManagement;
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
        private Text _routeText;
        private GameObject _subtitlePanel;
        private Text _subtitleText;
        private GameObject _summaryPanel;
        private Text _summaryText;
        private GameObject _titlePanel;
        private CanvasGroup _titleGroup;
        private GameObject _onboardingPanel;
        private Texture2D _iconAtlas;
        private readonly System.Collections.Generic.List<Sprite> _runtimeIcons =
            new System.Collections.Generic.List<Sprite>(12);
        private SettingsData _settings;
        private bool _paused;
        private float _announcementTime;
        private float _subtitleTime;
        private float _routeRefresh;
        private int _bindingSlot;
        private string _rebindStatus = "READY";
        private float _titleTime;
        private static bool _onboardingSeen;

        public bool SubtitleVisible =>
            _subtitlePanel != null && _subtitlePanel.activeSelf;
        public bool SummaryVisible =>
            _summaryPanel != null && _summaryPanel.activeSelf;
        public string RouteText => _routeText?.text ?? string.Empty;

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
            _input.LoadBindingOverrides(_settings.bindingOverrides);
            AccessibilityRuntime.Apply(_settings);
            Build();
            ActorPresentation actor =
                GetComponent<ActorPresentation>() ??
                gameObject.AddComponent<ActorPresentation>();
            actor.Configure(Tide, 1f, false);
            OnHealthChanged(_health.Current, _health.Maximum);
            RefreshResources();
            RefreshRoute();
            ShowPendingSummary();
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
            GameplayPresentationSignals.Subtitle += OnSubtitle;
            if (HubController.Instance != null)
                HubController.Instance.AchievementUnlocked +=
                    OnAchievementUnlocked;
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
            GameplayPresentationSignals.Subtitle -= OnSubtitle;
            if (HubController.Instance != null)
                HubController.Instance.AchievementUnlocked -=
                    OnAchievementUnlocked;
            if (_paused) Time.timeScale = 1f;
            foreach (Sprite icon in _runtimeIcons)
                if (icon != null) Destroy(icon);
            _runtimeIcons.Clear();
        }

        private void Update()
        {
            if (_input.PausePressedThisFrame) SetPaused(!_paused);
            if (_paused) ReadSettingsInput();
            if (SummaryVisible && _input.InteractPressedThisFrame)
                _summaryPanel.SetActive(false);
            if (_titlePanel != null && _titlePanel.activeSelf)
            {
                _titleTime -= Time.unscaledDeltaTime;
                _titleGroup.alpha = Mathf.Clamp01(_titleTime);
                if (_titleTime <= 0f) _titlePanel.SetActive(false);
            }
            if (_onboardingPanel != null && _onboardingPanel.activeSelf &&
                (_input.Move.sqrMagnitude > .1f ||
                 _input.InteractPressedThisFrame ||
                 _input.PrimaryFireHeld))
            {
                _onboardingPanel.SetActive(false);
                _onboardingSeen = true;
            }
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
            if (_subtitleTime > 0f)
            {
                _subtitleTime -= Time.unscaledDeltaTime;
                if (_subtitleTime <= 0f)
                    _subtitlePanel.SetActive(false);
            }
            _routeRefresh -= Time.unscaledDeltaTime;
            if (_routeRefresh <= 0f)
            {
                _routeRefresh = .25f;
                RefreshRoute();
                RefreshResources();
            }
        }

        public void SetPaused(bool paused)
        {
            _paused = paused;
            Time.timeScale = paused ? 0f : 1f;
            _pausePanel.SetActive(paused);
            if (paused)
            {
                RefreshPauseCopy();
                GameplayPresentationSignals.RaiseCue(
                    PresentationCue.UiConfirm,
                    transform.position);
            }
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
            _iconAtlas = Resources.Load<Texture2D>(
                "UI/Lanternfall_UI_IconAtlas");

            GameObject healthPanel = Panel(
                canvasObject.transform, "Vitality", new Vector2(24f, -24f),
                new Vector2(420f, 92f), new Vector2(0f, 1f));
            _healthFill = Bar(healthPanel.transform, "Health", Tide, 18f, 50f);
            Icon(healthPanel.transform, "Lantern Icon", 0,
                new Vector2(20f, -18f), 44f, new Vector2(0f, 1f));
            _healthText = Label(healthPanel.transform, "Health Text",
                "LANTERN  180 / 180", 22, TextAnchor.UpperLeft);
            SetRect(_healthText.rectTransform,
                new Vector2(68f, -10f), new Vector2(330f, 34f), new Vector2(0f, 1f));

            GameObject resources = Panel(
                canvasObject.transform, "Run Status", new Vector2(24f, 24f),
                new Vector2(520f, 82f), Vector2.zero);
            _resourcesText = Label(resources.transform, "Resources",
                "GOLD 0   ECHOES 0/3   BUFFS —   DEBUFFS —", 19,
                TextAnchor.MiddleLeft);
            Stretch(_resourcesText.rectTransform, 16f);
            Icon(resources.transform, "Echo Icon", 1,
                new Vector2(-42f, 0f), 34f, new Vector2(1f, .5f));

            GameObject ability = Panel(
                canvasObject.transform, "Ability", new Vector2(-24f, 24f),
                new Vector2(320f, 72f), new Vector2(1f, 0f));
            _abilityText = Label(ability.transform, "Cooldown",
                "Q / LB  ABILITY  READY", 19, TextAnchor.MiddleCenter);
            Stretch(_abilityText.rectTransform, 10f);
            Icon(ability.transform, "Ability Icon", 2,
                new Vector2(18f, 0f), 38f, new Vector2(0f, .5f));

            GameObject minimap = Panel(
                canvasObject.transform, "Minimap", new Vector2(-24f, -24f),
                new Vector2(250f, 150f), Vector2.one);
            _routeText = Label(minimap.transform, "Route",
                "NARTHEX ROUTE\n◆ CURRENT CHAMBER\n? UNCHARTED", 18,
                TextAnchor.MiddleCenter);
            Stretch(_routeText.rectTransform, 12f);
            Icon(minimap.transform, "Route Icon", 3,
                new Vector2(18f, -18f), 36f, new Vector2(0f, 1f));

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
            Icon(_pausePanel.transform, "Settings Icon", 4,
                new Vector2(24f, -24f), 46f, new Vector2(0f, 1f));
            _pausePanel.SetActive(false);

            _subtitlePanel = Panel(
                canvasObject.transform, "Subtitles", new Vector2(0f, 42f),
                new Vector2(980f, 104f), new Vector2(.5f, 0f));
            _subtitleText = Label(
                _subtitlePanel.transform,
                "Subtitle Copy",
                string.Empty,
                24,
                TextAnchor.MiddleCenter);
            Stretch(_subtitleText.rectTransform, 14f);
            _subtitlePanel.SetActive(false);

            _summaryPanel = Panel(
                canvasObject.transform, "Run Summary", Vector2.zero,
                new Vector2(760f, 650f), new Vector2(.5f, .5f));
            _summaryText = Label(
                _summaryPanel.transform,
                "Run Summary Copy",
                string.Empty,
                25,
                TextAnchor.MiddleCenter);
            Stretch(_summaryText.rectTransform, 34f);
            _summaryPanel.SetActive(false);
            Icon(_summaryPanel.transform, "Summary Icon", 5,
                new Vector2(32f, -32f), 58f, new Vector2(0f, 1f));

            if (SceneManager.GetActiveScene().name == "LanternfallHub")
            {
                _titlePanel = Panel(
                    canvasObject.transform, "Title Card", Vector2.zero,
                    new Vector2(920f, 250f), new Vector2(.5f, .62f));
                _titleGroup = _titlePanel.AddComponent<CanvasGroup>();
                Text title = Label(
                    _titlePanel.transform,
                    "Title",
                    "L A N T E R N F A L L\n" +
                    "<size=24>CARRY THE LAST LIGHT</size>",
                    54,
                    TextAnchor.MiddleCenter);
                title.supportRichText = true;
                Stretch(title.rectTransform, 24f);
                Icon(_titlePanel.transform, "Title Lantern", 0,
                    new Vector2(38f, -38f), 72f, new Vector2(0f, 1f));
                _titleTime = 4.5f;

                if (!_onboardingSeen)
                {
                    _onboardingPanel = Panel(
                        canvasObject.transform,
                        "First Light Guide",
                        new Vector2(24f, 124f),
                        new Vector2(610f, 184f),
                        Vector2.zero);
                    Text guide = Label(
                        _onboardingPanel.transform,
                        "Guide",
                        "FIRST LIGHT\n" +
                        "WASD / LEFT STICK  MOVE\n" +
                        "E / A  CHOOSE A CALLING · ENTER THE DESCENT\n" +
                        "ESC / START  SETTINGS & REMAPPING",
                        20,
                        TextAnchor.MiddleLeft);
                    Stretch(guide.rectTransform, 22f);
                    Icon(_onboardingPanel.transform, "Guide Icon", 6,
                        new Vector2(-24f, 20f), 44f, new Vector2(1f, 0f));
                }
            }
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
            if (!_input.IsRebinding &&
                _input.SelectBindingPressedThisFrame &&
                _input.RebindSlotCount > 0)
            {
                _bindingSlot =
                    (_bindingSlot + 1) % _input.RebindSlotCount;
                _rebindStatus = "SELECTED";
                changed = true;
            }
            if (!_input.IsRebinding &&
                _input.StartRebindPressedThisFrame)
            {
                if (_input.BeginInteractiveRebind(
                    _bindingSlot,
                    OnRebindFinished))
                    _rebindStatus = "PRESS A CONTROL · ESC CANCELS";
                changed = true;
            }
            if (!_input.IsRebinding &&
                _input.ResetBindingsPressedThisFrame)
            {
                _input.ResetBindingOverrides();
                _settings.bindingOverrides =
                    _input.SaveBindingOverrides();
                _rebindStatus = "DEFAULTS RESTORED";
                HubController.Instance?.SaveNow();
                changed = true;
            }
            if (changed) AccessibilityRuntime.Apply(_settings);
            if (changed)
            {
                GameplayPresentationSignals.RaiseCue(
                    PresentationCue.UiConfirm,
                    transform.position);
                RefreshPauseCopy();
            }
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
                "CONTROLS · KEYBOARD, MOUSE & CONTROLLER\n" +
                $"TAB / D-PAD DOWN  SELECT\n{_input.BindingDisplay(_bindingSlot)}\n" +
                $"F2 / SELECT  LISTEN FOR INPUT  [{_rebindStatus}]\n" +
                "F3 / D-PAD UP  RESET ALL BINDINGS";
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
            string resonance = "NONE";
            if (_inventory != null)
            {
                ResonanceSummary summary = _inventory.Chain.Evaluate();
                resonance = summary.Awakened ? "AWAKENED" :
                    summary.Harmonies > 0 ? $"HARMONY ×{summary.Harmonies}" :
                    summary.Clashes > 0 ? $"CLASH ×{summary.Clashes}" :
                    "QUIET";
            }
            RunSession run = HubController.Instance?.ActiveRun;
            string vow = run?.ActiveVow.HasValue == true
                ? VowPedestal.Title(run.ActiveVow.Value).ToUpperInvariant()
                : run?.LastVowOutcome == VowOutcome.Fulfilled
                    ? "VOW KEPT"
                    : run?.LastVowOutcome == VowOutcome.Broken
                        ? "VOW BROKEN"
                        : "NO VOW";
            _resourcesText.text =
                $"{L("hud.gold", "GOLD")} {gold}   " +
                $"{L("hud.echoes", "ECHOES")} {relics}/3   " +
                $"CALLING {calling.ToUpperInvariant()}   " +
                $"{resonance}   {vow}";
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

        private void OnBossPhase(int phase)
        {
            Announce($"{L("boss.phase", "Guardian phase")} {phase}");
            GameplayPresentationSignals.RaiseSubtitle(
                "LANTERN",
                $"Guardian phase {phase}. The attack pattern changes.");
        }

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

        private void RefreshRoute()
        {
            if (_routeText == null) return;
            RunSession run = HubController.Instance?.ActiveRun;
            if (run == null)
            {
                _routeText.text =
                    "LANTERN COURT\nSELECT A CALLING\nENTER THE DESCENT";
                return;
            }
            RunRoomPlan room = run.Current;
            var route = new StringBuilder(64);
            route.Append($"BIOME {room.BiomeIndex + 1}/5 · " +
                         $"ROOM {room.RoomIndex + 1}/8\n");
            for (int index = 0; index < RunSession.MainRoomsPerBiome; index++)
                route.Append(index < room.RoomIndex ? "● " :
                    index == room.RoomIndex ? "◆ " : "○ ");
            route.Append($"\n{room.Kind.ToString().ToUpperInvariant()} · " +
                         $"{run.ElapsedSeconds / 60f:0.0} MIN");
            _routeText.text = route.ToString();
        }

        private void OnSubtitle(string speaker, string copy, float duration)
        {
            if (!AccessibilityRuntime.Subtitles ||
                _subtitleText == null) return;
            _subtitleText.text =
                $"<b>{speaker.ToUpperInvariant()}</b>\n{copy}";
            _subtitleText.supportRichText = true;
            _subtitlePanel.SetActive(true);
            _subtitleTime = duration;
        }

        private void OnAchievementUnlocked(AchievementDefinition definition)
        {
            if (definition == null) return;
            Announce($"ACHIEVEMENT · {definition.Title}");
            GameplayPresentationSignals.RaiseSubtitle(
                "LANTERN",
                $"Achievement unlocked: {definition.Title}.");
        }

        private void OnRebindFinished(bool success, string status)
        {
            _rebindStatus = status;
            if (success)
            {
                _settings.bindingOverrides =
                    _input.SaveBindingOverrides();
                HubController.Instance?.SaveNow();
            }
            RefreshPauseCopy();
        }

        private void ShowPendingSummary()
        {
            RunSummaryData summary =
                HubController.Instance?.ConsumeRunSummary();
            DisplayRunSummary(summary);
        }

        public void DisplayRunSummary(RunSummaryData summary)
        {
            if (summary == null || _summaryText == null) return;
            int minutes = Mathf.FloorToInt(summary.ElapsedSeconds / 60f);
            int seconds = Mathf.FloorToInt(summary.ElapsedSeconds % 60f);
            _summaryText.text =
                $"{L("run.summary", "RUN SUMMARY")}\n\n" +
                $"{(summary.Victory ? L("run.victory", "THE LANTERN ENDURES") : L("run.defeat", "THE MEMORY FADES"))}\n\n" +
                $"TIME  {minutes:00}:{seconds:00}\n" +
                $"ROOMS  {summary.RoomsCleared}/40\n" +
                $"ENEMIES  {summary.EnemiesDefeated}\n" +
                $"GUARDIANS  {summary.GuardiansDefeated}/5\n" +
                $"GOLD  {summary.Gold}\n" +
                $"ECHOES  {summary.Echoes}/3\n" +
                $"VOWS KEPT / BROKEN  {summary.VowsFulfilled} / {summary.VowsBroken}\n" +
                $"SEED  {summary.Seed}\n\nE / A  RETURN TO COURT";
            _summaryPanel.SetActive(true);
        }

        private static GameObject Panel(
            Transform parent, string name, Vector2 position,
            Vector2 size, Vector2 anchor)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            Image image = panel.GetComponent<Image>();
            image.color = Ink;
            Outline outline = panel.AddComponent<Outline>();
            outline.effectColor = new Color(.58f, .39f, .16f, .72f);
            outline.effectDistance = new Vector2(2f, -2f);
            SetRect(panel.GetComponent<RectTransform>(), position, size, anchor);
            return panel;
        }

        private Image Icon(
            Transform parent,
            string name,
            int index,
            Vector2 position,
            float size,
            Vector2 anchor)
        {
            if (_iconAtlas == null) return null;
            const int cells = 4;
            float cell = _iconAtlas.width / (float)cells;
            int x = index % cells;
            int y = (index / cells) % cells;
            Sprite sprite = Sprite.Create(
                _iconAtlas,
                new Rect(x * cell, y * cell, cell, cell),
                new Vector2(.5f, .5f),
                100f);
            _runtimeIcons.Add(sprite);
            GameObject item = new GameObject(
                name, typeof(RectTransform), typeof(Image));
            item.transform.SetParent(parent, false);
            Image image = item.GetComponent<Image>();
            image.sprite = sprite;
            image.preserveAspect = true;
            image.raycastTarget = false;
            SetRect(
                item.GetComponent<RectTransform>(),
                position,
                Vector2.one * size,
                anchor);
            return image;
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
