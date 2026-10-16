using System;
using System.Collections.Generic;
using UnityEngine;

namespace GravityBox.Venom
{
    /// <summary>
    /// The order in which each level's devices are worked in its reference solution (Resources/COgheUI/SolutionSteps.json,
    /// recorded by the Explicit test RecordSolutionSteps from the same solutions the solve tests play). Used by
    /// <see cref="COgheIntelligence"/>: COghe walks to the next of these by itself, the player still works the device.
    /// A step names a component under the level's apparatus by its transform path: a rail handle, a weight pad, or another
    /// device that takes a tap.
    /// </summary>
    public static class COgheSolutionSteps
    {
        public const string RailKind = "rail", PadKind = "pad", DeviceKind = "device";
        [Serializable] public sealed class Step { public string Path; public string Kind; }
        [Serializable] public sealed class Level { public string Id; public List<Step> Steps = new List<Step>(); }
        [Serializable] public sealed class Book { public List<Level> Levels = new List<Level>(); }

        private static Book book;
        public static Book Current
        {
            get
            {
                if (book != null) return book;
                var text = Resources.Load<TextAsset>("COgheUI/SolutionSteps");
                book = text != null ? JsonUtility.FromJson<Book>(text.text) ?? new Book() : new Book();
                return book;
            }
        }
        /// <summary>Tests replace the book (null: load it again).</summary>
        public static void ResetForTests(Book replacement = null) => book = replacement;
        public static List<Step> For(string levelId)
        {
            foreach (var level in Current.Levels) if (level.Id == levelId) return level.Steps;
            return null;
        }

        /// <summary>The transform path of <paramref name="target"/> below <paramref name="root"/> ("a/b/c").</summary>
        public static string PathOf(Transform root, Transform target)
        {
            var names = new List<string>();
            for (var t = target; t != null && t != root; t = t.parent) names.Insert(0, t.name);
            return string.Join("/", names);
        }
        public static Component Resolve(Transform root, Step step)
        {
            if (root == null || step == null || string.IsNullOrEmpty(step.Path)) return null;
            var t = root.Find(step.Path); if (t == null) return null;
            if (step.Kind == RailKind) return t.GetComponent<COgheTapRail>();
            if (step.Kind == PadKind) return t.GetComponent<COgheTissueSensor>();
            return t.GetComponent<COgheMechanism>();
        }
    }
}
