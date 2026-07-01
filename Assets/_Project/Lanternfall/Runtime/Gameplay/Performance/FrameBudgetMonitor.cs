using UnityEngine;

namespace Lanternfall.Gameplay.Performance
{
    public readonly struct FrameBudgetSnapshot
    {
        public FrameBudgetSnapshot(
            int samples, float averageMilliseconds, float worstMilliseconds,
            float overBudgetRatio)
        {
            Samples = samples;
            AverageMilliseconds = averageMilliseconds;
            WorstMilliseconds = worstMilliseconds;
            OverBudgetRatio = overBudgetRatio;
        }

        public int Samples { get; }
        public float AverageMilliseconds { get; }
        public float WorstMilliseconds { get; }
        public float OverBudgetRatio { get; }
        public bool Meets60FpsBudget =>
            Samples > 0 && AverageMilliseconds <= 16.667f &&
            OverBudgetRatio <= .05f;
    }

    /// <summary>Fixed-memory rolling telemetry suitable for release builds.</summary>
    public sealed class FrameBudgetWindow
    {
        private readonly float[] _samples;
        private int _cursor;
        private int _count;

        public FrameBudgetWindow(int capacity = 300)
        {
            _samples = new float[Mathf.Max(30, capacity)];
        }

        public void AddSeconds(float seconds)
        {
            _samples[_cursor] = Mathf.Max(0f, seconds * 1000f);
            _cursor = (_cursor + 1) % _samples.Length;
            _count = Mathf.Min(_count + 1, _samples.Length);
        }

        public FrameBudgetSnapshot Snapshot(float budgetMilliseconds = 16.667f)
        {
            float total = 0f;
            float worst = 0f;
            int overBudget = 0;
            for (int index = 0; index < _count; index++)
            {
                float sample = _samples[index];
                total += sample;
                worst = Mathf.Max(worst, sample);
                if (sample > budgetMilliseconds) overBudget++;
            }
            return new FrameBudgetSnapshot(
                _count,
                _count > 0 ? total / _count : 0f,
                worst,
                _count > 0 ? overBudget / (float)_count : 0f);
        }
    }

    public sealed class FrameBudgetMonitor : MonoBehaviour
    {
        private readonly FrameBudgetWindow _window = new FrameBudgetWindow();
        public FrameBudgetSnapshot Snapshot => _window.Snapshot();

        private void Update() => _window.AddSeconds(Time.unscaledDeltaTime);
    }
}
