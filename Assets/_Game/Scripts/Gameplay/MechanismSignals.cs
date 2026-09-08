using System;
using System.Collections.Generic;

namespace GravityBox.Gameplay
{
    /// <summary>Instance-scoped signals; identical channel names in two levels cannot leak state.</summary>
    public sealed class MechanismSignals
    {
        private readonly Dictionary<string, bool> values = new Dictionary<string, bool>();
        public event Action<string, bool> Changed;
        public bool Read(string channel) => values.TryGetValue(channel, out bool value) && value;
        public void Set(string channel, bool value)
        {
            if (Read(channel) == value) return;
            values[channel] = value;
            Changed?.Invoke(channel, value);
        }
        public void Clear() => values.Clear();
    }
}
