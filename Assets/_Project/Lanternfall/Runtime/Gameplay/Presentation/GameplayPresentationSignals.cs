using System;
using UnityEngine;

namespace Lanternfall.Gameplay.Presentation
{
    public enum PresentationCue
    {
        Footstep,
        Dodge,
        WeaponFire,
        Ability,
        Impact,
        EnemyTelegraph,
        EnemyDeath,
        GuardianTelegraph,
        GuardianDeath,
        EchoCollected,
        UiConfirm,
        Hazard
    }

    /// <summary>
    /// Allocation-free presentation boundary. Combat and progression publish
    /// semantic cues without depending on audio, VFX, or subtitle presenters.
    /// </summary>
    public static class GameplayPresentationSignals
    {
        public static event Action<PresentationCue, Vector3> Cue;
        public static event Action<string, string, float> Subtitle;

        public static void RaiseCue(PresentationCue cue, Vector3 position) =>
            Cue?.Invoke(cue, position);

        public static void RaiseSubtitle(
            string speaker, string copy, float duration = 2.5f)
        {
            if (string.IsNullOrWhiteSpace(copy)) return;
            Subtitle?.Invoke(
                string.IsNullOrWhiteSpace(speaker) ? "LANTERN" : speaker,
                copy,
                Mathf.Max(.5f, duration));
        }
    }
}
