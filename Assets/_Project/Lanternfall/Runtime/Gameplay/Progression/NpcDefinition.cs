using UnityEngine;

namespace Lanternfall.Gameplay.Progression
{
    [CreateAssetMenu(menuName = "Lanternfall/Progression/NPC")]
    public sealed class NpcDefinition : ScriptableObject
    {
        [SerializeField] private string stableId = "npc.unset";
        [SerializeField] private string displayName = "Unknown";
        [SerializeField] private string questId = "quest.unset";
        [SerializeField, TextArea] private string greeting = "...";

        public string StableId => stableId;
        public string DisplayName => displayName;
        public string QuestId => questId;
        public string Greeting => greeting;

#if UNITY_EDITOR
        public void Configure(string id, string title, string quest, string line)
        {
            stableId = id;
            displayName = title;
            questId = quest;
            greeting = line;
        }
#endif
    }
}

