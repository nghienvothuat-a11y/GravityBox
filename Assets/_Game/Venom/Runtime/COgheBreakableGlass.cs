using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// Thin glass that breaks under enough weight (Mrk 19/10/2026: physics props). Its sensor weighs the tissue and the props
    /// on it. Past CrackLoad cracks show (it is fragile); at BreakLoad the pane goes, what stood on it falls through, and the
    /// shards drop and vanish. The shards are drawn only, never simulated. Retry mends the pane.
    /// </summary>
    public sealed class COgheBreakableGlass : COgheMechanism
    {
        public COgheTissueSensor Sensor;
        public GameObject Pane;
        public float BreakLoad = .12f, CrackLoad = .03f;
        public Renderer[] Cracks = System.Array.Empty<Renderer>();
        public Transform[] Shards = System.Array.Empty<Transform>();
        public bool Broken { get; private set; }
        public bool Cracked { get; private set; }
        private float brokenFor;
        private Vector3[] shardRest, shardVelocity;
        private Quaternion[] shardTurn;

        public override void InitializeMechanism(VenomCampaign game)
        {
            shardRest = new Vector3[Shards.Length]; shardVelocity = new Vector3[Shards.Length]; shardTurn = new Quaternion[Shards.Length];
            for (int i = 0; i < Shards.Length; i++) shardRest[i] = Shards[i].localPosition;
        }
        public override void ResetMechanism(VenomCampaign game)
        {
            if (shardRest == null) InitializeMechanism(game);
            Broken = Cracked = false; brokenFor = 0; Pane.SetActive(true);
            foreach (var crack in Cracks) if (crack != null) crack.enabled = false;
            for (int i = 0; i < Shards.Length; i++) { Shards[i].localPosition = shardRest[i]; Shards[i].localRotation = Quaternion.identity; Shards[i].gameObject.SetActive(false); }
        }
        public override void StepMechanism(VenomCampaign game, float dt)
        {
            if (!Broken)
            {
                if (!Cracked && Sensor.Load >= CrackLoad) { Cracked = true; foreach (var crack in Cracks) if (crack != null) crack.enabled = true; }
                if (Sensor.Load < BreakLoad) return;
                Broken = true; Pane.SetActive(false);
                foreach (var crack in Cracks) if (crack != null) crack.enabled = false;
                for (int i = 0; i < Shards.Length; i++)
                {
                    // Fixed spreads, not random: the same break every time (tests, clips).
                    float a = i * 2.39996f;
                    // a short burst up and out, then down through the hole (straight down they vanished in the tray below)
                    shardVelocity[i] = new Vector3(Mathf.Cos(a) * .12f, .30f + .04f * (i % 3), Mathf.Sin(a) * .12f);   // stays over the pit
                    shardTurn[i] = Quaternion.Euler(140f * Mathf.Cos(a), 0, 140f * Mathf.Sin(a));
                    Shards[i].gameObject.SetActive(true);
                }
                game.Motion.BuildGraph(true);
                return;
            }
            if (brokenFor > .7f) return;
            brokenFor += dt;
            for (int i = 0; i < Shards.Length; i++)
            {
                if (!Shards[i].gameObject.activeSelf) continue;
                shardVelocity[i] += Vector3.down * 9.81f * dt;
                Shards[i].localPosition += shardVelocity[i] * dt;
                Shards[i].localRotation = Quaternion.Slerp(Quaternion.identity, shardTurn[i], brokenFor);
                if (brokenFor > .7f) Shards[i].gameObject.SetActive(false);
            }
        }
    }
}
