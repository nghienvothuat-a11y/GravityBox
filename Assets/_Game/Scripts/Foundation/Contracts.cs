using System;
using System.Collections.Generic;

namespace GravityBox.Foundation
{
    public interface IResettable
    {
        void CaptureInitialState();
        void ResetState();
    }

    public interface IMechanism
    {
        bool IsActive { get; }
        void SetActive(bool active);
    }

    /// <summary>Level-scoped registry. Registration order is restoration order.</summary>
    public sealed class ResetRegistry
    {
        private readonly List<IResettable> entries = new List<IResettable>(32);
        public int Count => entries.Count;

        public void Register(IResettable entry)
        {
            if (entry == null || entries.Contains(entry)) return;
            entries.Add(entry);
            entry.CaptureInitialState();
        }

        public void RestoreAll()
        {
            for (int i = 0; i < entries.Count; i++) entries[i].ResetState();
        }

        public void Clear() => entries.Clear();
    }

    public enum SessionState { Loading, Active, Paused, Completing, Failed, Finished }
    public enum RotationMode { Assisted, Free, QuarterTurn }

    /// <summary>Pure session rules; all terminal transitions are idempotent.</summary>
    public sealed class GameSession
    {
        public SessionState State { get; private set; } = SessionState.Loading;
        public event Action<SessionState> Changed;

        public void BeginLoading() => Set(SessionState.Loading);
        public void Activate() => Set(SessionState.Active);
        public void Finish() => Set(SessionState.Finished);
        public bool TryComplete() => TryEnd(SessionState.Completing);
        public bool TryFail() => TryEnd(SessionState.Failed);

        public void TogglePause()
        {
            if (State == SessionState.Active) Set(SessionState.Paused);
            else if (State == SessionState.Paused) Set(SessionState.Active);
        }

        private bool TryEnd(SessionState next)
        {
            if (State != SessionState.Active) return false;
            Set(next);
            return true;
        }

        private void Set(SessionState next)
        {
            if (State == next) return;
            State = next;
            Changed?.Invoke(next);
        }
    }
}
