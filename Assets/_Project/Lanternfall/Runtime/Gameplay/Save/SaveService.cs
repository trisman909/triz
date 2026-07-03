using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Lanternfall.Gameplay.Save
{
    public sealed class SaveService
    {
        private readonly ISaveStorage _storage;

        public SaveService(ISaveStorage storage)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        }

        public void Save(SaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            data.version = SaveData.CurrentVersion;
            string payload = JsonUtility.ToJson(data);
            var envelope = new SaveEnvelope
            {
                payload = payload,
                checksum = Checksum(payload)
            };
            _storage.WriteAtomic(JsonUtility.ToJson(envelope));
        }

        public SaveData Load()
        {
            if (!_storage.Exists) return NewProfile();
            try
            {
                return Decode(_storage.Read());
            }
            catch (Exception primaryError)
            {
                string backup = _storage.ReadBackup();
                if (!string.IsNullOrEmpty(backup))
                {
                    try { return Decode(backup); }
                    catch (Exception) { }
                }
                Debug.LogWarning($"Save recovery failed: {primaryError.Message}");
                return NewProfile();
            }
        }

        private static SaveData Decode(string serialized)
        {
            SaveEnvelope envelope = JsonUtility.FromJson<SaveEnvelope>(serialized);
            if (envelope == null || string.IsNullOrEmpty(envelope.payload) ||
                envelope.checksum != Checksum(envelope.payload))
                throw new InvalidDataException("Save checksum mismatch.");
            SaveData data = JsonUtility.FromJson<SaveData>(envelope.payload);
            if (data == null) throw new InvalidDataException("Save payload is empty.");
            return Migrate(data);
        }

        private static SaveData Migrate(SaveData data)
        {
            if (data.version < 1)
            {
                data.settings = data.settings ?? new SettingsData();
                data.statistics = data.statistics ?? new StatisticsData();
                data.unlocks = data.unlocks ?? new System.Collections.Generic.List<string>();
                data.quests = data.quests ?? new System.Collections.Generic.List<QuestRecord>();
                data.version = 1;
            }
            if (data.version < 2)
            {
                if (string.IsNullOrWhiteSpace(data.selectedClassId))
                    data.selectedClassId = "class.vanguard";
                data.achievements =
                    data.achievements ?? new System.Collections.Generic.List<string>();
                data.cosmetics =
                    data.cosmetics ?? new System.Collections.Generic.List<string>();
                data.version = 2;
            }
            if (data.version < 3)
            {
                data.achievementProgress =
                    data.achievementProgress ??
                    new System.Collections.Generic.List<AchievementProgressRecord>();
                data.achievementContexts =
                    data.achievementContexts ??
                    new System.Collections.Generic.List<string>();
                data.version = 3;
            }
            if (data.version > SaveData.CurrentVersion)
                throw new InvalidDataException("Save comes from a newer game version.");
            return data;
        }

        private static SaveData NewProfile()
        {
            var data = new SaveData();
            data.unlocks.Add("class.vanguard");
            data.unlocks.Add("weapon.cinder_staff");
            data.unlocks.Add("biome.drowned_narthex");
            return data;
        }

        private static string Checksum(string value)
        {
            using (SHA256 sha = SHA256.Create())
                return Convert.ToBase64String(
                    sha.ComputeHash(Encoding.UTF8.GetBytes(value)));
        }
    }
}
