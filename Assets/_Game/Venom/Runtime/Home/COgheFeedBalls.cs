using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>One small steel ball thrown into the Home (COghe's food): a real rigidbody that bounces and rolls until it
    /// rests, then is taken over by COghe's eating act.</summary>
    public sealed class COgheFeedBall : MonoBehaviour
    {
        public const float Radius = .016f;
        public Rigidbody Body { get; private set; }
        public SphereCollider Shape { get; private set; }
        /// <summary>Still on the floor for a moment: COghe can go and eat it.</summary>
        public bool Resting { get; private set; }
        /// <summary>COghe has it (kinematic, no collisions): it is being eaten.</summary>
        public bool Taken { get; private set; }
        public Vector3 Position => transform.position;
        private Vector3 gravity;
        private float still, age;

        internal void Initialize(Rigidbody body, SphereCollider shape, Vector3 down) { Body = body; Shape = shape; gravity = down * 9.81f; }
        /// <summary>Every simulation step, with the game's own physics step (never FixedUpdate: tests step physics by hand).</summary>
        internal void Step(float dt)
        {
            if (Taken) return;
            Body.AddForce(gravity, ForceMode.Acceleration);   // the project has no global gravity
            // rolling resistance (PhysX has none): a slow ball settles instead of creeping round the floor for ever
            bool slow = Body.linearVelocity.sqrMagnitude < .12f * .12f;
            Body.linearDamping = slow ? 2.5f : .2f; Body.angularDamping = slow ? 5f : .9f;
            age += dt; float speed = Body.linearVelocity.magnitude;
            still = speed < .03f ? still + dt : 0;
            Resting = still > .3f || (age > 4 && speed < .15f);   // a ball still jittering in a corner after 4 s is food all the same
        }
        internal void Take() { Taken = true; Resting = false; Body.isKinematic = true; Shape.enabled = false; }
        /// <summary>The eating act moves the ball (and shrinks it into COghe).</summary>
        public void Place(Vector3 position, float scale) { transform.position = position; transform.localScale = Vector3.one * (Radius * 2 * Mathf.Max(0, scale)); }
        private void OnCollisionEnter(Collision collision)
        {
            // a clink for a real knock (the speed into the surface), not for rolling along it
            float speed = collision.contactCount > 0 ? Mathf.Abs(Vector3.Dot(collision.relativeVelocity, collision.GetContact(0).normal)) : collision.relativeVelocity.magnitude;
            if (speed > .25f) COgheAudio.Instance?.Play("metal_clink", Mathf.Clamp01(speed / 1.6f) * .55f, 0, .07f);
        }
    }

    /// <summary>
    /// Feeding at Home (Mrk): each Feed throws three small steel balls in from the player's side. They bounce and roll for
    /// real (random aim and spin) off the floor, the glass and the furniture, and COghe walks to each one that comes to rest
    /// and eats it. The balls never touch COghe's tissue, and the furniture and glass guards only stop balls.
    /// </summary>
    public sealed class COgheFeedBalls : IDisposable
    {
        public const int PerFeed = 3, MaxBalls = 9, IgnoreTaps = 2;   // Unity's built-in Ignore Raycast layer
        private readonly VenomCampaign game;
        private readonly COgheHomeRoom room;
        private readonly Transform holder;
        private readonly List<COgheFeedBall> balls = new List<COgheFeedBall>(MaxBalls);
        private readonly List<Collider> guards = new List<Collider>();
        private SphereCollider toyGuard;
        private readonly Material steel;
        private readonly PhysicsMaterial bounce;
        /// <summary>Tests: a fixed sequence of throws (otherwise every visit throws differently).</summary>
        public static int? SeedForTests;
        private readonly System.Random rnd = new System.Random(SeedForTests ?? Environment.TickCount);
        private int pending; private float nextThrow;
        public IReadOnlyList<COgheFeedBall> Balls => balls;
        public bool Active => pending > 0 || balls.Count > 0;
        public int Thrown { get; private set; }

        public COgheFeedBalls(VenomCampaign owner, COgheHomeRoom homeRoom, Material look)
        {
            game = owner; room = homeRoom;
            holder = new GameObject("Feed balls").transform; holder.SetParent(room.Root, false);
            steel = new Material(look) { name = "Feed steel" };
            steel.SetColor("_BaseColor", new Color(.84f, .86f, .88f)); steel.SetFloat("_Metallic", .65f); steel.SetFloat("_Smoothness", .82f);
            bounce = new PhysicsMaterial("Feed steel") { bounciness = .55f, dynamicFriction = .22f, staticFriction = .3f,
                bounceCombine = PhysicsMaterialCombine.Maximum, frictionCombine = PhysicsMaterialCombine.Average };
            BuildGuards();
        }

        // Ball-only walls: the side glass and the back wall carried up to a metre, a lip at the open front, a thick floor
        // under the real one, and every earned piece of furniture (a box, the ball a sphere).
        private void BuildGuards()
        {
            // on the Ignore Raycast layer: taps go through them to the floor and the furniture
            var g = new GameObject("Feed guards") { layer = IgnoreTaps }; g.transform.SetParent(holder, false);
            void Box(Vector3 centre, Vector3 size) { var c = g.AddComponent<BoxCollider>(); c.center = centre; c.size = size; guards.Add(c); }
            float w = COgheHomeRoom.HalfWidth, d = COgheHomeRoom.HalfDepth;
            Box(new Vector3(-w - .01f, .5f, 0), new Vector3(.02f, 1, d * 2)); Box(new Vector3(w + .01f, .5f, 0), new Vector3(.02f, 1, d * 2));
            Box(new Vector3(0, .5f, d + .01f), new Vector3(w * 2, 1, .02f));   // well above the low back wall too
            Box(new Vector3(0, .05f, -d - .01f), new Vector3(w * 2, .1f, .02f));
            Box(new Vector3(0, -.05f, 0), new Vector3(w * 2 + .04f, .1f, d * 2 + .04f));   // a thick floor: a ball squeezed in a gap can't be pushed through
            foreach (var item in room.Items)
            {
                if (!room.Present(item)) continue;
                var part = item.Part("Ball");
                if (item.Id == "BALL" && part != null)
                {
                    var t = new GameObject("Feed guard") { layer = IgnoreTaps }; t.transform.SetParent(part, false);
                    toyGuard = t.AddComponent<SphereCollider>(); toyGuard.radius = .45f; guards.Add(toyGuard); continue;
                }
                var b = room.BoundsOf(item);
                Box(holder.InverseTransformPoint(b.center), Vector3.Scale(b.size, new Vector3(.88f, 1, .88f)));
            }
            foreach (var guard in guards) { guard.sharedMaterial = bounce; Ignore(guard); }
        }
        private void Ignore(Collider c) { foreach (var body in game.Matter.Bodies) { var t = body.GetComponent<Collider>(); if (t != null) Physics.IgnoreCollision(c, t); } }

        /// <summary>Feed: three more balls on their way (a few at a time; the room holds nine).</summary>
        public void Feed() { pending = Mathf.Min(pending + PerFeed, MaxBalls - balls.Count); nextThrow = Mathf.Min(nextThrow, 0); }

        public void Step(float dt)
        {
            nextThrow -= dt;
            if (pending > 0 && nextThrow <= 0 && balls.Count < MaxBalls) { Throw(); pending--; nextThrow = .16f; }
            for (int i = balls.Count - 1; i >= 0; i--)
            {
                if (balls[i] == null) { balls.RemoveAt(i); continue; }
                balls[i].Step(dt);
                // one that escaped anyway (a freak bounce over the glass) is simply gone
                if (room.Root.InverseTransformPoint(balls[i].Position).y < -.2f) Consume(balls[i]);
            }
        }

        private float R(float a, float b) => a + (float)rnd.NextDouble() * (b - a);
        private COgheFeedBall Make()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = "Feed ball " + (++Thrown); go.layer = IgnoreTaps;
            go.transform.SetParent(holder, false); go.transform.localScale = Vector3.one * COgheFeedBall.Radius * 2;
            go.GetComponent<Renderer>().sharedMaterial = steel;
            var shape = go.GetComponent<SphereCollider>(); shape.sharedMaterial = bounce; Ignore(shape);
            var body = go.AddComponent<Rigidbody>();
            body.mass = .03f; body.useGravity = false; body.linearDamping = .2f; body.angularDamping = .9f; body.maxAngularVelocity = 200;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic; body.interpolation = RigidbodyInterpolation.Interpolate;
            var ball = go.AddComponent<COgheFeedBall>(); ball.Initialize(body, shape, -room.Root.up);
            return ball;
        }

        /// <summary>The bonus "Hiểu ra": one ball handed straight to COghe at <paramref name="at"/>, already taken (no physics):
        /// its catch-and-gulp moves it and eats it.</summary>
        public COgheFeedBall Serve(Vector3 at)
        {
            var ball = Make(); ball.Body.position = at; ball.transform.position = at; ball.Take(); balls.Add(ball);
            return ball;
        }

        private void Throw()
        {
            var ball = Make(); var body = ball.Body; var go = ball.gameObject;
            // from the player's side, over the front lip, a low arc under the Home's gravity onto a random spot of the open
            // aisle; the bounces and the roll scatter it from there
            Vector3 from = room.Root.TransformPoint(new Vector3(R(-.2f, .2f), .3f, -COgheHomeRoom.HalfDepth - .04f));
            Vector3 to = room.Root.TransformPoint(new Vector3(R(-.16f, .16f), COgheFeedBall.Radius, R(-.5f, .55f)));
            float time = R(.42f, .55f);
            body.position = from; go.transform.position = from;
            body.linearVelocity = (to - from) / time + room.Root.up * (.5f * 9.81f * time);
            body.angularVelocity = new Vector3(R(-30, 30), R(-30, 30), R(-30, 30));
            balls.Add(ball);
            COgheAudio.Instance?.Play("creature_whoosh", .18f, 0, .05f);
        }

        /// <summary>The nearest ball at rest that nobody is eating yet.</summary>
        public COgheFeedBall NearestResting(Vector3 from)
        {
            COgheFeedBall best = null; float nearest = float.MaxValue;
            foreach (var b in balls) { if (b == null || !b.Resting || b.Taken) continue; float d = (b.Position - from).sqrMagnitude; if (d < nearest) { nearest = d; best = b; } }
            return best;
        }
        public COgheFeedBall NearestMoving(Vector3 from)
        {
            COgheFeedBall best = null; float nearest = float.MaxValue;
            foreach (var b in balls) { if (b == null || b.Resting || b.Taken) continue; float d = (b.Position - from).sqrMagnitude; if (d < nearest) { nearest = d; best = b; } }
            return best;
        }
        public void Take(COgheFeedBall ball) { if (ball != null) ball.Take(); }
        public void Consume(COgheFeedBall ball)
        {
            if (ball == null) return;
            balls.Remove(ball); UnityEngine.Object.Destroy(ball.gameObject);
        }

        public void Dispose()
        {
            foreach (var b in balls) if (b != null) UnityEngine.Object.Destroy(b.gameObject);
            balls.Clear(); pending = 0;
            if (holder != null) UnityEngine.Object.Destroy(holder.gameObject);
            if (toyGuard != null) UnityEngine.Object.Destroy(toyGuard.gameObject);
            UnityEngine.Object.Destroy(steel); UnityEngine.Object.Destroy(bounce);
        }
    }
}
