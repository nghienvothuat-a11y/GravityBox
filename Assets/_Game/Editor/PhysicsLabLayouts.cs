using GravityBox.Gameplay;
using UnityEngine;

namespace GravityBox.Editor
{
    // Authoring dimensions are metres. The steel sphere and contact materials are shared.
    internal sealed class PhysicsLabLayout
    {
        public ContainerShape Shape;
        public string Title, Hint;
        public Vector2[] Outline;
        public Vector2[][] Voids = System.Array.Empty<Vector2[]>();
        public Vector2 Spawn, Exit;
        public float BoundsHalfExtent;
        public float Depth = .09f;
        public float? SpawnY;
        public string Solution;
    }

    internal static class PhysicsLabLayouts
    {
        public static PhysicsLabLayout[] All()
        {
            return new[]
            {
                Layout(ContainerShape.Circle, "Circular box", "Tilt gently. Feel the ball accelerate and follow the curved wall.",
                    Circle(0.17f, 96), new Vector2(-0.06f, 0.015f), new Vector2(0.095f, -0.09f), 0.25f),
                Layout(ContainerShape.Square, "Square box", "Roll into the fixed cube. Compare a gentle touch with a faster impact.",
                    new[] { V(-.16f,-.16f), V(.16f,-.16f), V(.16f,.16f), V(-.16f,.16f) }, V(-.1f,0), V(.105f,-.105f), .25f),
                Layout(ContainerShape.Triangle, "Triangular box", "Follow the angled walls. Watch each contact redirect the ball.",
                    new[] { V(-.17f,-.09815f), V(.17f,-.09815f), V(0,.19630f) }, V(-.05f,-.025f), V(.07f,-.05f), .25f),
                Layout(ContainerShape.LShape, "L-shaped box", "Follow both arms. Change your tilt at the inside corner.",
                    new[] { V(-.25f,-.24f), V(.25f,-.24f), V(.25f,-.06f), V(-.065f,-.06f), V(-.065f,.24f), V(-.25f,.24f) },
                    V(-.155f,.155f), V(.185f,-.155f)),
                Layout(ContainerShape.UShape, "U-shaped box", "Go down one arm, around the bend, then up the other.",
                    new[] { V(-.25f,-.24f), V(.25f,-.24f), V(.25f,.24f), V(.08f,.24f), V(.08f,-.065f), V(-.08f,-.065f), V(-.08f,.24f), V(-.25f,.24f) },
                    V(-.165f,.155f), V(.165f,.155f)),
                Layout(ContainerShape.Annulus, "Ring box", "Guide the ball around the empty centre. Keep changing the slope.",
                    Circle(.25f, 128), V(-.1825f,0), V(.1825f,0), 0, new[] { Circle(.115f, 96, true) }),
                Layout(ContainerShape.Dumbbell, "Twin chambers", "Line up with the narrow neck. Carry momentum into the second chamber.",
                    new[] { V(-.33f,-.07f), V(-.27f,-.14f), V(-.13f,-.14f), V(-.065f,-.05f), V(.065f,-.05f), V(.13f,-.14f), V(.27f,-.14f), V(.33f,-.07f),
                        V(.33f,.07f), V(.27f,.14f), V(.13f,.14f), V(.065f,.05f), V(-.065f,.05f), V(-.13f,.14f), V(-.27f,.14f), V(-.33f,.07f) },
                    V(-.225f,0), V(.235f,0)),
                Layout(ContainerShape.Star, "Star box", "Leave the pointed pocket. Cross the centre and find the upper arm.",
                    Star(), V(-.115f,-.14f), V(0,.207f)),
                Layout(ContainerShape.GravityLock, "Leave it behind", "Park in the blue recess. Let the amber slider fall away, then cross.",
                    new[] { V(-.28f,-.19f), V(.28f,-.19f), V(.28f,.19f), V(-.28f,.19f) },
                    V(-.20f,.12f), V(.19f,-.10f)),
                Layout(ContainerShape.MechanicalMaze, "Opposite ways", "Find a holding recess. The two sliders open in opposite directions.",
                    new[] { V(-.36f,-.28f), V(.36f,-.28f), V(.36f,.28f), V(-.36f,.28f) },
                    V(-.315f,-.240f), V(.315f,.240f), solution: "Cross the zigzag west maze into the first holding recess. Tilt north to retract slider A, then cross to the central maze. Reach the second holding recess and tilt south to retract slider B. Cross into the east maze, navigate its alternating turns and roll completely through the circular exit. Both sliders remain dynamic and can reclose under reversed gravity."),
                new PhysicsLabLayout
                {
                    Shape = ContainerShape.LayeredMaze, Title = "Three dimensions",
                    Hint = "Follow each floor to its transfer opening. The round exit is on the lowest floor.",
                    Outline = new[] { V(-.30f,-.25f), V(.30f,-.25f), V(.30f,.25f), V(-.30f,.25f) },
                    Spawn = V(-.23f,-.18f), SpawnY = .066f, Exit = V(.23f,.18f), Depth = .27f, BoundsHalfExtent = .47f,
                    Solution = "Navigate the upper maze to its transfer opening, fall onto the middle floor under gravity, cross its different maze to the second opening, then follow the lowest maze to the flush circular exit. No teleport, lift animation or scripted layer transition."
                },
                new PhysicsLabLayout
                {
                    Shape = ContainerShape.SphereMaze, Title = "Lost in glass",
                    Hint = "Follow the connected glass maze. Turn at the junctions and find the route to the green exit.",
                    Solution = "Start inside the connected plank framework and follow its physical passages through turns on all three axes. Rotate the sphere to change the supporting face, roll along a run or fall into the next junction. Two blind branches require returning the way the ball entered. Sight gaps between narrow planks are smaller than the ball, so the outer sphere cannot be used as a shortcut. The final passage meets the flush round opening in the shell; the complete steel ball must leave through it. Route metadata is used only for authoring and tests, never to move or steer the ball."
                },
                Layout(ContainerShape.WaterBox, "Steel under water", "Tilt and let the steel ball sink. Watch its wake; compare the same box on level 02.",
                    new[] { V(-.16f,-.16f), V(.16f,-.16f), V(.16f,.16f), V(-.16f,.16f) }, V(-.1f,0), V(.105f,-.105f), .25f,
                    solution: "A completely water-filled version of the square experiment, including its fixed cube. Tilt around the obstacle and towards the round exit. Buoyancy and speed-dependent water drag act on the steel ball while submerged. Water is retained by a level rule, while the ball can fully exit; there is no free water surface or draining simulation."),
                Layout(ContainerShape.MercuryBox, "Steel in mercury", "Steel floats. Bring it close to the green opening; exit assist will draw it out.",
                    new[] { V(-.16f,-.16f), V(.16f,-.16f), V(.16f,.16f), V(-.16f,.16f) }, V(-.1f,0), V(.105f,-.105f), .25f,
                    solution: "The same full square box and fixed cube as levels 02 and 13, filled with mercury. The steel ball is less dense than the liquid and rises against the highest inner face. Rotate to guide it close to the exit; the shared exit assist completes its outward movement, overcoming the partial-floating equilibrium. A ball only partly in the opening has not escaped. Mercury is retained by the same level rule. The silver see-through view is a visualization aid for this opaque liquid, not optical transparency.")
            };
        }

        private static PhysicsLabLayout Layout(ContainerShape shape, string title, string hint, Vector2[] outline,
            Vector2 spawn, Vector2 exit, float extent = 0, Vector2[][] voids = null, string solution = null)
        {
            if (extent <= 0)
            {
                foreach (Vector2 point in outline) extent = Mathf.Max(extent, point.magnitude);
                extent += .045f; // Sphere and shell fit in any orientation, with room to observe escape.
            }
            return new PhysicsLabLayout { Shape = shape, Title = title, Hint = hint, Outline = outline,
                Spawn = spawn, Exit = exit, BoundsHalfExtent = extent, Voids = voids ?? System.Array.Empty<Vector2[]>() , Solution = solution };
        }

        private static Vector2[] Circle(float radius, int count, bool clockwise = false)
        {
            var result = new Vector2[count];
            for (int i = 0; i < count; i++)
            {
                float angle = (clockwise ? -1 : 1) * i * Mathf.PI * 2 / count;
                result[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }
            return result;
        }

        private static Vector2[] Star()
        {
            var result = new Vector2[10];
            for (int i = 0; i < result.Length; i++)
            {
                float angle = Mathf.PI * .5f + i * Mathf.PI / 5;
                result[i] = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (i % 2 == 0 ? .29f : .145f);
            }
            return result;
        }
        private static Vector2 V(float x, float z) => new Vector2(x, z);
    }
}
