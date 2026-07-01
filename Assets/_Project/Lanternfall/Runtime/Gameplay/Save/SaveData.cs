using System;
using System.Collections.Generic;

namespace Lanternfall.Gameplay.Save
{
    [Serializable]
    public sealed class SaveData
    {
        public const int CurrentVersion = 1;
        public int version = CurrentVersion;
        public List<string> unlocks = new List<string>();
        public List<string> achievements = new List<string>();
        public List<string> cosmetics = new List<string>();
        public List<QuestRecord> quests = new List<QuestRecord>();
        public SettingsData settings = new SettingsData();
        public StatisticsData statistics = new StatisticsData();
    }

    [Serializable]
    public sealed class SettingsData
    {
        public float masterVolume = 1f;
        public float musicVolume = .8f;
        public float effectsVolume = .9f;
        public float cameraShake = 1f;
        public float flashIntensity = 1f;
        public float uiScale = 1f;
        public bool reducedMotion;
        public bool subtitles = true;
        public bool highContrastTelegraphs;
        public string locale = "en";
        public string bindingOverrides = string.Empty;
    }

    [Serializable]
    public sealed class StatisticsData
    {
        public int runsStarted;
        public int runsCompleted;
        public int deaths;
        public int enemiesDefeated;
        public int bossesDefeated;
        public float longestRunSeconds;
        public ulong bestRunSeed;
    }

    [Serializable]
    public sealed class QuestRecord
    {
        public string questId;
        public int step;
        public bool completed;
    }

    [Serializable]
    internal sealed class SaveEnvelope
    {
        public string payload;
        public string checksum;
    }
}
