using System.IO;
using System;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Save;
using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Run;
using Lanternfall.Gameplay.World;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lanternfall.Gameplay.Hub
{
    public sealed class HubController : MonoBehaviour
    {
        [SerializeField] private ContentCatalog contentCatalog;
        private SaveService _saves;
        public static HubController Instance { get; private set; }
        public SaveData Profile { get; private set; }
        public MetaProgression Progression { get; private set; }
        public QuestJournal Quests { get; private set; }
        public RunSession ActiveRun { get; private set; }
        public event Action<string> ClassSelected;
        public string SelectedClassId =>
            Profile?.selectedClassId ?? "class.vanguard";
        public string SelectedClassName
        {
            get
            {
                if (contentCatalog != null)
                    foreach (CharacterClassDefinition definition in contentCatalog.Classes)
                        if (definition != null &&
                            definition.StableId == SelectedClassId)
                            return definition.DisplayName;
                return "Vanguard";
            }
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            string directory = Path.Combine(Application.persistentDataPath, "Saves");
            _saves = new SaveService(new FileSaveStorage(directory));
            Profile = _saves.Load();
            Progression = new MetaProgression(Profile);
            Quests = new QuestJournal(Profile);
            AccessibilityRuntime.Apply(Profile.settings);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnApplicationQuit() => SaveNow();
        public void SaveNow() => _saves?.Save(Profile);

        private void Update()
        {
            ActiveRun?.Tick(Time.unscaledDeltaTime);
        }

        public void Configure(ContentCatalog catalog) => contentCatalog = catalog;

        public bool SelectClass(string stableId)
        {
            if (Profile == null || contentCatalog == null ||
                string.IsNullOrWhiteSpace(stableId)) return false;
            bool exists = false;
            foreach (CharacterClassDefinition definition in contentCatalog.Classes)
                exists |= definition != null && definition.StableId == stableId;
            if (!exists) return false;
            Profile.selectedClassId = stableId;
            ClassSelected?.Invoke(stableId);
            SaveNow();
            return true;
        }

        public bool AdvanceQuest(
            NpcDefinition npc,
            int finalStep,
            string completionUnlock)
        {
            if (npc == null) return false;
            bool changed = Quests.Advance(npc.QuestId, finalStep);
            QuestRecord record = Quests.Find(npc.QuestId);
            if (record != null && record.completed &&
                !string.IsNullOrWhiteSpace(completionUnlock))
                Progression.Unlock(completionUnlock);
            if (changed) SaveNow();
            return changed;
        }

        public void StartRun(string sceneName)
        {
            StartRun(
                sceneName,
                unchecked((ulong)System.DateTime.UtcNow.Ticks));
        }

        public void StartRun(string sceneName, ulong seed)
        {
            PrepareRun(seed, SelectedClassId);
            Profile.statistics.runsStarted++;
            Profile.statistics.bestRunSeed = seed;
            SaveNow();
            SceneManager.LoadScene(sceneName);
        }

        public void PrepareRun(ulong seed, string classId)
        {
            ActiveRun = new RunSession(seed, classId);
        }

        public void CompleteRun()
        {
            if (ActiveRun == null) return;
            Profile.statistics.runsCompleted++;
            Profile.statistics.enemiesDefeated += ActiveRun.EnemiesDefeated;
            Profile.statistics.bossesDefeated += ActiveRun.BossesDefeated;
            Profile.statistics.longestRunSeconds = Mathf.Max(
                Profile.statistics.longestRunSeconds,
                ActiveRun.ElapsedSeconds);
            ActiveRun = null;
            SaveNow();
            SceneManager.LoadScene("LanternfallHub");
        }

        public void FailRun()
        {
            if (ActiveRun == null) return;
            Profile.statistics.deaths++;
            Profile.statistics.enemiesDefeated += ActiveRun.EnemiesDefeated;
            Profile.statistics.bossesDefeated += ActiveRun.BossesDefeated;
            Profile.statistics.longestRunSeconds = Mathf.Max(
                Profile.statistics.longestRunSeconds,
                ActiveRun.ElapsedSeconds);
            ActiveRun = null;
            SaveNow();
            SceneManager.LoadScene("LanternfallHub");
        }
    }
}
