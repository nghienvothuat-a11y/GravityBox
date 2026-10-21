using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The Intelligence ability (ability cards, Mrk 16/10/2026). When the player has not touched the screen for
    /// <see cref="IdleDelay"/> seconds, COghe looks around for a second and walks to the next device of the level's
    /// solution (<see cref="COgheSolutionSteps"/>): beside a rail's handle, next to a weight pad, up to any other device. It
    /// never works a device itself; once the player has, it remembers the step after, up to as many steps as its
    /// Intelligence level. Past that it stops and wonders (the question-mark shape). Steps advance only in solution order,
    /// so a player who does something else is led back to the earliest step not done. Intelligence 0: nothing happens.
    /// </summary>
    public sealed class COgheIntelligence : MonoBehaviour
    {
        public const float IdleDelay = 5f, LookTime = 1f;
        private enum Phase { Waiting, Looking, Walking }

        private VenomCampaign game;
        private readonly List<COgheSolutionSteps.Step> steps = new List<COgheSolutionSteps.Step>();
        private readonly List<Component> targets = new List<Component>();
        private Phase phase;
        private float phaseStart, guidedInput = -1, puzzledInput = -1;
        private int journeysAtStep;
        private bool deviceTouched;

        /// <summary>Tests: the Intelligence level to use instead of the saved one.</summary>
        public static int? LevelForTests;
        /// <summary>Tests: run the ability outside the product (the solve tests' scenes).</summary>
        public static bool ForceForTests;
        public static bool Applies(VenomCampaign game) => ForceForTests || COgheProductMode.Applies(game);
        public int Remembered => Mathf.Clamp(LevelForTests ?? COgheAbilities.StepsRemembered, 0, COgheAbilities.Max);
        /// <summary>Solution steps done, in order.</summary>
        public int Progress { get; private set; }
        public int StepCount => steps.Count;
        /// <summary>The step COghe last walked to (-1: none since the level started).</summary>
        public int GuidedStep { get; private set; } = -1;
        /// <summary>Times COghe stopped and wondered because it remembered no further.</summary>
        public int Puzzled { get; private set; }
        public bool Looking => phase == Phase.Looking;

        public void Initialize(VenomCampaign owner) { game = owner; game.MechanismTouched += OnTouched; ResetForLevel(); }
        private void OnDestroy() { if (game != null) game.MechanismTouched -= OnTouched; }

        public void ResetForLevel()
        {
            steps.Clear(); targets.Clear(); Progress = 0; GuidedStep = -1; Puzzled = 0; phase = Phase.Waiting;
            guidedInput = puzzledInput = -1; deviceTouched = false; ClearLook();
            if (game == null || game.Definition == null || game.Owner == null) return;
            var recorded = COgheSolutionSteps.For(game.Definition.Id); if (recorded == null) return;
            foreach (var step in recorded)
            {
                var target = COgheSolutionSteps.Resolve(game.Owner.Apparatus, step);
                if (target == null) { Debug.LogWarning($"COghe Intelligence: step {step.Path} not found in {game.Definition.Id}"); continue; }
                steps.Add(step); targets.Add(target);
            }
            BeginStep();
        }

        private void BeginStep()
        {
            deviceTouched = false;
            journeysAtStep = Progress < targets.Count && targets[Progress] is COgheTapRail rail ? rail.CompletedJourneys : 0;
        }
        private void OnTouched(COgheMechanism touched)
        { if (Progress < targets.Count && ReferenceEquals(touched, targets[Progress])) deviceTouched = true; }

        private bool StepDone(int index)
        {
            var target = targets[index];
            if (target is COgheTapRail rail) return rail.CompletedJourneys > journeysAtStep || (rail.HoldAtEnd && rail.Holding);
            if (target is COgheTissueSensor pad) return pad.Active;
            return deviceTouched;
        }

        /// <summary>Called by the campaign every simulation step, outside Home.</summary>
        public void Step(float dt)
        {
            if (game == null || targets.Count == 0) return;
            while (Progress < targets.Count && StepDone(Progress)) { Progress++; BeginStep(); }
            if (game.Owner.Completed || game.Owner.Lost || game.InTube || game.Attached) { phase = Phase.Waiting; return; }
            if (Remembered <= 0 || Progress >= targets.Count) { phase = Phase.Waiting; return; }

            float now = game.Matter.SimulationTime, input = game.LastInputTime;
            bool idle = now - input >= IdleDelay && !Busy();
            switch (phase)
            {
                case Phase.Waiting:
                    if (!idle) return;
                    if (Progress >= Remembered)
                    {
                        if (puzzledInput == input) return;                       // once per pause of the player
                        puzzledInput = input; Puzzled++;
                        game.Personality?.Force(COgheAct.Shape, COgheShape.Question);
                        return;
                    }
                    if (guidedInput == input && GuidedStep == Progress) return;  // already there, waiting for the tap
                    phase = Phase.Looking; phaseStart = now;
                    return;
                case Phase.Looking:
                    if (!idle) { phase = Phase.Waiting; ClearLook(); return; }
                    if (game.Personality != null) game.Personality.GuideLook = LookPoint(now - phaseStart);
                    if (now - phaseStart < LookTime) return;
                    ClearLook();
                    game.Motion.Move(game.Motion.Selected, Destination(Progress));
                    GuidedStep = Progress; guidedInput = input; phase = Phase.Walking; phaseStart = now;
                    return;
                case Phase.Walking:
                    if (game.Motion.Get(game.Motion.Selected) == null || now - phaseStart > 30 || input != guidedInput) phase = Phase.Waiting;
                    return;
            }
        }

        private void ClearLook() { if (game != null && game.Personality != null) game.Personality.GuideLook = null; }
        private bool Busy()
        {
            if (game.Motion.Get(game.Motion.Selected) != null) return true;
            foreach (var m in game.Mechanisms) if (m is COgheTapRail rail && rail.Busy) return true;
            return game.Personality != null && game.Personality.Act != COgheAct.None && game.Personality.Act != COgheAct.Home;
        }

        // Left, right, then the device it is thinking of.
        private Vector3 LookPoint(float t)
        {
            Vector3 centre = game.Motion.Centre(game.Motion.Selected), target = TargetPoint(Progress);
            Vector3 side = Vector3.Cross(Vector3.up, (target - centre).normalized); if (side.sqrMagnitude < .01f) side = game.Root.right;
            if (t < LookTime * .35f) return centre + side * .3f;
            if (t < LookTime * .7f) return centre - side * .3f;
            return target;
        }
        private Vector3 TargetPoint(int index)
        {
            var target = targets[index];
            if (target is COgheTapRail rail) return rail.HandPoint;
            return target.transform.position;
        }
        private Vector3 Destination(int index)
        {
            var target = targets[index];
            if (target is COgheTapRail rail) return rail.StandPoint;
            Vector3 centre = game.Motion.Centre(game.Motion.Selected), at = target.transform.position;
            if (target is COgheTissueSensor)
            {
                // beside the pad, not on it: standing on it is the player's move
                Vector3 away = Vector3.ProjectOnPlane(centre - at, game.Root.up);
                if (away.sqrMagnitude < .0001f) away = -game.Root.forward;
                return at + away.normalized * .09f;
            }
            return at;
        }
    }
}
