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
    /// While COghe is split Clever is off (Mrk 22/10/2026).
    /// </summary>
    public sealed class COgheIntelligence : MonoBehaviour
    {
        public const float IdleDelay = 5f, LookTime = 1f;
        private enum Phase { Waiting, Looking, Walking }

        private VenomCampaign game;
        private readonly List<COgheSolutionSteps.Step> steps = new List<COgheSolutionSteps.Step>();
        private readonly List<Component> targets = new List<Component>();
        private Phase phase;
        private float phaseStart, guidedInput = -1, puzzledInput = -1, stillSince;
        private Vector3 stillFrom;
        /// <summary>A guided walk is given up (COghe wonders instead of grinding against a wall) when COghe stays within
        /// <see cref="StallMove"/> m of one spot for <see cref="StallTime"/> s.</summary>
        public const float StallTime = 10f, StallMove = .02f;
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
        /// <summary>Guided walks given up because COghe made no headway.</summary>
        public int Stalls { get; private set; }
        public bool Looking => phase == Phase.Looking;
        /// <summary>COghe is in more than one part: Clever is off.</summary>
        public bool Split => game != null && game.Matter != null && game.Matter.TotalFragmentCount > 1;
        /// <summary>Guided walks started since the level began.</summary>
        public int Walks { get; private set; }

        public void Initialize(VenomCampaign owner) { game = owner; game.MechanismTouched += OnTouched; ResetForLevel(); }
        private void OnDestroy() { if (game != null) game.MechanismTouched -= OnTouched; }

        public void ResetForLevel()
        {
            steps.Clear(); targets.Clear(); Progress = 0; GuidedStep = -1; Puzzled = 0; Stalls = 0; Walks = 0; phase = Phase.Waiting;
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
            // Mrk 22/10/2026: once COghe is split, Clever has no effect (it neither leads nor wonders) until it is whole
            // again; the lesson still counts the steps the player works meanwhile.
            if (Split) { if (phase == Phase.Looking) ClearLook(); phase = Phase.Waiting; return; }
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
                    GuidedStep = Progress; guidedInput = input; phase = Phase.Walking; phaseStart = stillSince = now; Walks++;
                    stillFrom = game.Motion.Centre(game.Motion.Selected);
                    return;
                case Phase.Walking:
                    if (game.Motion.Get(game.Motion.Selected) == null || now - phaseStart > 30 || input != guidedInput) { phase = Phase.Waiting; return; }
                    Vector3 centre = game.Motion.Centre(game.Motion.Selected);
                    if (Vector3.Distance(centre, stillFrom) > StallMove) { stillFrom = centre; stillSince = now; }
                    else if (now - stillSince > StallTime)
                    {
                        // no way through after all: stop and wonder, until the player's next touch
                        game.Motion.Cancel(game.Motion.Selected); phase = Phase.Waiting; Stalls++;
                        game.Personality?.Force(COgheAct.Shape, COgheShape.Question);
                    }
                    return;
            }
        }

        private void ClearLook() { if (game != null && game.Personality != null) game.Personality.GuideLook = null; }
        private bool Busy()
        {
            int selected = game.Motion.Selected;
            if (game.Motion.Get(selected) != null) return true;
            foreach (var m in game.Mechanisms)
            {
                if (m is COgheTapRail rail && rail.Busy) return true;
                if (m.Occupies(game, selected)) return true;   // in a tube, on a rope, riding a lift: wait for the player
            }
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
            if (target is COgheTapRail rail) return rail.GuideStandPoint(game.Motion.Selected);   // a crate: a face it can work
            Vector3 centre = game.Motion.Centre(game.Motion.Selected), at = target.transform.position;
            if (target is COgheTissueSensor)
            {
                // beside the pad, not on it: standing on it is the player's move
                Vector3 away = Vector3.ProjectOnPlane(centre - at, game.Root.up);
                if (away.sqrMagnitude < .0001f) away = -game.Root.forward;
                return at + away.normalized * .09f;
            }
            // a tube's mouth, a rope swing's ring stand: not the device's own origin, which can be mid-air
            if (target is COgheMechanism device && device.GuidePoint(game, game.Motion.Selected, out Vector3 point)) return point;
            return at;
        }
    }
}
