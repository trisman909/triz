using System.IO;
using Lanternfall.Gameplay.Progression;
using Lanternfall.Gameplay.Save;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Lanternfall.Gameplay.Hub
{
    public sealed class HubController : MonoBehaviour
    {
        private SaveService _saves;
        public static HubController Instance { get; private set; }
        public SaveData Profile { get; private set; }
        public MetaProgression Progression { get; private set; }
        public QuestJournal Quests { get; private set; }

        private void Awake()
        {
            Instance = this;
            string directory = Path.Combine(Application.persistentDataPath, "Saves");
            _saves = new SaveService(new FileSaveStorage(directory));
            Profile = _saves.Load();
            Progression = new MetaProgression(Profile);
            Quests = new QuestJournal(Profile);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void OnApplicationQuit() => SaveNow();
        public void SaveNow() => _saves?.Save(Profile);

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
            Profile.statistics.runsStarted++;
            SaveNow();
            SceneManager.LoadScene(sceneName);
        }
    }
}

