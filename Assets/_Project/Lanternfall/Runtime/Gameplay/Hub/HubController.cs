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
        [SerializeField] private AchievementCatalog achievementCatalog;
        private SaveService _saves;
        private AchievementTracker _achievements;
        public static HubController Instance { get; private set; }
        public SaveData Profile { get; private set; }
        public MetaProgression Progression { get; private set; }
        public QuestJournal Quests { get; private set; }
        public RunSession ActiveRun { get; private set; }
        public RunSummaryData PendingRunSummary { get; private set; }
        public event Action<string> ClassSelected;
        public event Action<AchievementDefinition> AchievementUnlocked;
        public event Action<RunSummaryData> RunSummaryAvailable;
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
            _achievements = new AchievementTracker(Profile);
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

        public void Configure(
            ContentCatalog catalog,
            AchievementCatalog achievements)
        {
            contentCatalog = catalog;
            achievementCatalog = achievements;
        }

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
            ReportAchievement(AchievementMetric.ClassesSelected);
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
            if (changed) ReportAchievement(AchievementMetric.QuestsAdvanced);
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
            ReportAchievement(AchievementMetric.RunsStarted);
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
            CaptureRunSummary(true);
            Profile.statistics.runsCompleted++;
            Profile.statistics.enemiesDefeated += ActiveRun.EnemiesDefeated;
            Profile.statistics.bossesDefeated += ActiveRun.BossesDefeated;
            Profile.statistics.longestRunSeconds = Mathf.Max(
                Profile.statistics.longestRunSeconds,
                ActiveRun.ElapsedSeconds);
            ReportAchievement(AchievementMetric.RunsCompleted);
            ActiveRun = null;
            SaveNow();
            SceneManager.LoadScene("LanternfallHub");
        }

        public void FailRun()
        {
            if (ActiveRun == null) return;
            CaptureRunSummary(false);
            Profile.statistics.deaths++;
            Profile.statistics.enemiesDefeated += ActiveRun.EnemiesDefeated;
            Profile.statistics.bossesDefeated += ActiveRun.BossesDefeated;
            Profile.statistics.longestRunSeconds = Mathf.Max(
                Profile.statistics.longestRunSeconds,
                ActiveRun.ElapsedSeconds);
            ReportAchievement(AchievementMetric.Deaths);
            ActiveRun = null;
            SaveNow();
            SceneManager.LoadScene("LanternfallHub");
        }

        public void ReportAchievement(
            AchievementMetric metric,
            int amount = 1,
            string uniqueContext = null)
        {
            if (_achievements == null || achievementCatalog == null ||
                amount <= 0) return;
            if (!string.IsNullOrWhiteSpace(uniqueContext))
            {
                string key = $"{metric}:{uniqueContext}";
                if (Profile.achievementContexts.Contains(key)) return;
                Profile.achievementContexts.Add(key);
            }
            int unlocked = _achievements.Report(
                achievementCatalog,
                metric,
                amount,
                definition => AchievementUnlocked?.Invoke(definition));
            if (unlocked > 0) SaveNow();
        }

        public void ReportRoomCompleted(RunRoomPlan room)
        {
            if (ActiveRun == null) return;
            ActiveRun.RoomsCleared++;
            ReportAchievement(AchievementMetric.RoomsCleared);
            switch (room.Kind)
            {
                case Lanternfall.Core.Run.RoomKind.Secret:
                    ReportAchievement(AchievementMetric.SecretsVisited);
                    break;
                case Lanternfall.Core.Run.RoomKind.Healing:
                    ReportAchievement(AchievementMetric.HealingRooms);
                    break;
                case Lanternfall.Core.Run.RoomKind.Treasure:
                    ReportAchievement(AchievementMetric.TreasureRooms);
                    break;
                case Lanternfall.Core.Run.RoomKind.Challenge:
                    ReportAchievement(AchievementMetric.ChallengeRooms);
                    break;
            }
        }

        public RunSummaryData ConsumeRunSummary()
        {
            RunSummaryData result = PendingRunSummary;
            PendingRunSummary = null;
            return result;
        }

        private void CaptureRunSummary(bool victory)
        {
            RunSession run = ActiveRun;
            PendingRunSummary = new RunSummaryData(
                victory,
                run.Seed,
                run.ClassId,
                run.ElapsedSeconds,
                run.RoomsCleared,
                run.EnemiesDefeated,
                run.BossesDefeated,
                run.Gold,
                run.EchoIds.Count,
                run.VowsFulfilled,
                run.VowsBroken);
            RunSummaryAvailable?.Invoke(PendingRunSummary);
        }
    }
}
