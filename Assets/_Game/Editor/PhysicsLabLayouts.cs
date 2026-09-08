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
                    Star(), V(-.115f,-.14f), V(0,.207f))
            };
        }

        private static PhysicsLabLayout Layout(ContainerShape shape, string title, string hint, Vector2[] outline,
            Vector2 spawn, Vector2 exit, float extent = 0, Vector2[][] voids = null)
        {
            if (extent <= 0)
            {
                foreach (Vector2 point in outline) extent = Mathf.Max(extent, point.magnitude);
                extent += .045f; // Sphere and shell fit in any orientation, with room to observe escape.
            }
            return new PhysicsLabLayout { Shape = shape, Title = title, Hint = hint, Outline = outline,
                Spawn = spawn, Exit = exit, BoundsHalfExtent = extent, Voids = voids ?? System.Array.Empty<Vector2[]>() };
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
