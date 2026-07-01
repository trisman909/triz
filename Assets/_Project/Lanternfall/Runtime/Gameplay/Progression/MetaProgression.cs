using System;
using System.Collections.Generic;
using Lanternfall.Gameplay.Save;

namespace Lanternfall.Gameplay.Progression
{
    public sealed class MetaProgression
    {
        private readonly SaveData _data;
        private readonly HashSet<string> _unlocks;

        public MetaProgression(SaveData data)
        {
            _data = data ?? throw new ArgumentNullException(nameof(data));
            _unlocks = new HashSet<string>(data.unlocks);
        }

        public bool IsUnlocked(string stableId) => _unlocks.Contains(stableId);

        public bool Unlock(string stableId)
        {
            if (string.IsNullOrWhiteSpace(stableId) || !_unlocks.Add(stableId))
                return false;
            _data.unlocks.Add(stableId);
            return true;
        }
    }

    public sealed class QuestJournal
    {
        private readonly SaveData _data;
        public QuestJournal(SaveData data) =>
            _data = data ?? throw new ArgumentNullException(nameof(data));

        public QuestRecord Find(string questId) =>
            _data.quests.Find(item => item.questId == questId);

        public QuestRecord Start(string questId)
        {
            QuestRecord existing = Find(questId);
            if (existing != null) return existing;
            var record = new QuestRecord { questId = questId, step = 0 };
            _data.quests.Add(record);
            return record;
        }

        public bool Advance(string questId, int finalStep)
        {
            QuestRecord record = Start(questId);
            if (record.completed) return false;
            record.step++;
            if (record.step >= finalStep) record.completed = true;
            return true;
        }
    }
}

