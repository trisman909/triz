using System.Collections.Generic;

namespace Lanternfall.Core.Stats
{

public enum ModifierKind { Flat, AdditivePercent, MultiplicativePercent }

public readonly struct StatModifier
{
    public StatModifier(string source, float value, ModifierKind kind)
    { Source = source; Value = value; Kind = kind; }
    public string Source { get; }
    public float Value { get; }
    public ModifierKind Kind { get; }
}

public sealed class StatValue
{
    private readonly List<StatModifier> _modifiers = new();
    public StatValue(float baseValue) => BaseValue = baseValue;
    public float BaseValue { get; set; }
    public void Add(StatModifier modifier) => _modifiers.Add(modifier);
    public void RemoveSource(string source) =>
        _modifiers.RemoveAll(item => item.Source == source);
    public float Evaluate()
    {
        float flat = 0, additive = 0, multiplier = 1;
        foreach (StatModifier item in _modifiers)
            switch (item.Kind)
            {
                case ModifierKind.Flat: flat += item.Value; break;
                case ModifierKind.AdditivePercent: additive += item.Value; break;
                case ModifierKind.MultiplicativePercent:
                    multiplier *= 1 + item.Value; break;
            }
        return (BaseValue + flat) * (1 + additive) * multiplier;
    }
}

}
