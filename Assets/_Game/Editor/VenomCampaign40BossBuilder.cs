using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static void BuildCampaign39(ExpansionContext c) => BuildFortyTwoStageRooms(c, false);
        private static void BuildCampaign40(ExpansionContext c) => BuildFortyTwoStageRooms(c, true);

        private static void BuildFortyTwoStageRooms(ExpansionContext c, bool boss)
        {
            float half = boss ? .80f : .60f;
            ConfigureNew(c, boss ? "CỖ MÁY ĐOÀN TỤ" : "Ba nơi, hai nhịp", "", false, new Vector3(42, 24, 0), boss ? .87f : .68f);
            c.Definition.Boss = boss;
            c.Definition.InitialCameraZone = 0;
            c.Definition.CameraZones = new[] {
                new VenomCameraZone(boss ? "Q · A · Dao" : "A · Dao", new Vector3(boss ? -.50f : -.4f, -.13f, 0), new Vector3(boss ? .636f : .436f, .376f, .636f)),
                new VenomCameraZone("B · Dao", new Vector3(0, -.13f, 0), new Vector3(.436f, .376f, .636f)),
                new VenomCameraZone(boss ? "C · H" : "C · E", new Vector3(boss ? .50f : .4f, -.13f, 0), new Vector3(boss ? .636f : .436f, .376f, .636f)) };
            c.Spawn = new Vector3(boss ? -.73f : -.48f, -.267f, -.17f);
            c.Exit = new Vector3(half, -.19f, .16f); c.Outward = Vector3.right;
            c.Owner.ApertureRadius = .041f;
            ExpansionShell(c, half);
            var firstDoor = ExpansionDivider(c, -.20f, false, -.15f, .032f);
            firstDoor.name = "D1 reunion gate";
            var secondDoor = ExpansionDivider(c, .20f, false, -.15f, .032f);
            secondDoor.name = "D2 reunion gate";
            firstDoor.LatchAtEnd = secondDoor.LatchAtEnd = true;
            FortyMark(c, "D1", new Vector3(-.24f, -.287f, .25f));
            FortyMark(c, "D2", new Vector3(.16f, -.287f, .25f));
            ExpansionKnife(c, "I gravity knife", -.41f, -.16f).TouchHalfSize = new Vector3(.030f, .10f, .08f);
            ExpansionKnife(c, "II gravity knife", 0, -.17f);
            ExpansionBossPipes(c, -.15f, .032f);
            // A stays left of the first cut and in front of the preparation
            // alcove. Its holder walks away from the waiting worker immediately;
            // the clear front aisle also avoids the sealed P alcove on reunion.
            var a = ExpansionPad(c, "A input clutch", new Vector3(boss ? -.66f : -.46f, -.297f, boss ? -.22f : .18f), .012f, .112f);
            var b = ExpansionPad(c, "B output clutch", new Vector3(.12f, -.297f, .17f), .012f, .112f);

            var handle = ExpansionRail(c, "C two output handle", new Vector3(.33f, -.254f, -.16f), Vector3.right, .13f, .065f,
                new Vector3(.040f, .050f, .040f), .07f, .009f, false, true);
            // C has a clear front working aisle through BOTH directions. A
            // neutral spring replaces a catch so the operator must remain.
            var handleProp = handle.GetComponent<VenomMovableProp>();
            handleProp.ManipulationGrip.localPosition = new Vector3(0, -.006f, -.028f);
            handleProp.ManipulationGrip.localScale = new Vector3(.052f, .018f, .022f);
            handleProp.ManipulationGrip.gameObject.AddComponent<BoxCollider>();
            handleProp.ManipulationHandleOnly = true;
            var plane = new GameObject("C command plane").transform; plane.SetParent(c.Root, false); plane.localPosition = new Vector3(0, -.274f, 0);
            handleProp.ManipulationPlane = plane;
            var stop = ExpansionRail(c, "II mechanical selector stop", new Vector3(.435f, -.254f, -.16f), Vector3.up, .105f, 0,
                new Vector3(.012f, .052f, .048f), .03f, .012f, false, false); stop.LatchAtEnd = true;
            var machine = new GameObject("Two stage cooperative winch", typeof(COgheTwoStageWinch)).GetComponent<COgheTwoStageWinch>();
            machine.transform.SetParent(c.Root, false);
            machine.Input = a; machine.Output = b; machine.Handle = handle; machine.SelectorStop = stop;
            machine.ReunionDoors = new[] { firstDoor, secondDoor };
            if (boss)
            {
                BuildFortyPreparation(c, machine);
                var cap = ExpansionRail(c, "H final cover", new Vector3(half - .011f, -.19f, .16f), Vector3.back, .145f, 0,
                    new Vector3(.012f, .13f, .13f), .20f, .36f, false, true); cap.LatchAtEnd = true;
                // Put H's grip on its room-facing face. The default front
                // grip sits inside the real locking pin at the opening pose.
                var capProp = cap.GetComponent<VenomMovableProp>();
                capProp.ManipulationGrip.localPosition = new Vector3(-.026f, -.025f, 0);
                capProp.ManipulationGrip.localRotation = Quaternion.LookRotation(Vector3.left, Vector3.up);
                capProp.ManipulationGrip.localScale = new Vector3(.052f, .014f, .018f);
                capProp.ManipulationGrip.gameObject.AddComponent<BoxCollider>();
                capProp.ManipulationHandleOnly = true;
                MechanismVisual(cap.transform, "H handle mount", new Vector3(-.016f, -.025f, 0), new Vector3(.022f, .009f, .012f), metal);
                machine.FinalCap = cap;
                machine.SecondOutput = ExpansionRail(c, "H physical locking pin", new Vector3(half - .021f, -.19f, .082f), Vector3.up, .15f, 0,
                    new Vector3(.036f, .07f, .012f), .035f, .015f, false, false);
            }
            else
                machine.SecondOutput = ExpansionRail(c, "E final shutter", new Vector3(half - .011f, -.19f, .16f), Vector3.up, .18f, 0,
                    new Vector3(.012f, .13f, .13f), .035f, .015f, false, false);
            machine.SecondOutput.LatchAtEnd = true;
            machine.FirstDrum = MechanismVisual(c.Root, "I reunion drum", new Vector3(.34f, .12f, .21f), new Vector3(.05f, .024f, .05f), metal, PrimitiveType.Cylinder);
            machine.SecondDrum = MechanismVisual(c.Root, "II output drum", new Vector3(.43f, .12f, .21f), new Vector3(.05f, .024f, .05f), metal, PrimitiveType.Cylinder);
            foreach (var door in machine.ReunionDoors) FortyCable(c, "I reunion cable", new Vector3(.34f, .18f, .21f), door.Start + Vector3.up * .40f);
            FortyCable(c, "II output cable", new Vector3(.43f, .18f, .21f), machine.SecondOutput.Start + Vector3.up * .36f);
            FortyCable(c, "A input linkage", a.transform.localPosition + Vector3.up * .006f, new Vector3(a.transform.localPosition.x, -.291f, .27f));
            FortyCable(c, "A to B linkage", new Vector3(a.transform.localPosition.x, -.291f, .27f), new Vector3(.12f, -.291f, .27f));
            FortyCable(c, "B output linkage", new Vector3(.12f, -.291f, .27f), new Vector3(.43f, -.291f, .27f));
            FortyMark(c, "I", new Vector3(.33f, -.287f, -.235f));
            FortyMark(c, "II", new Vector3(.47f, -.287f, -.235f));
        }

        private static void BuildFortyPreparation(ExpansionContext c, COgheTwoStageWinch machine)
        {
            var q = ExpansionRail(c, "Q preparation carriage", new Vector3(-.71f, -.220f, .055f), Vector3.right, .27f, .10f,
                new Vector3(.11f, .15f, .12f), .20f, .36f, false, true); q.LatchAtEnd = true;
            var pin = ExpansionRail(c, "P preparation pin", new Vector3(-.495f, -.225f, .06f), Vector3.back, .20f, 0,
                new Vector3(.018f, .070f, .076f), .06f, .026f, false, true); pin.LatchAtEnd = true;
            var prop = pin.GetComponent<VenomMovableProp>();
            prop.ManipulationGrip.localPosition = new Vector3(-.115f, -.006f, .072f);
            prop.ManipulationGrip.localScale = new Vector3(.052f, .018f, .022f);
            prop.ManipulationGrip.gameObject.AddComponent<BoxCollider>(); prop.ManipulationHandleOnly = true;
            MechanismVisual(pin.transform, "P handle linkage", new Vector3(-.058f, -.006f, .067f), new Vector3(.115f, .010f, .010f), metal);
            // Q's back face at z=.115 closes an alcove beginning at z=.120.
            // Its 5 mm perimeter gaps are narrower than a tissue particle;
            // the cheeks, roof and back remain outside Q's lateral sweep.
            foreach (float side in new[] { -1f, 1f })
                Panel(c.Root, "P sealed alcove cheek", new Vector3(-.61f + side * .060f, -.220f, .170f), Vector3.left * side,
                    new Vector2(.100f, .160f), glass, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "P sealed alcove roof", new Vector3(-.61f, -.140f, .170f), Vector3.down,
                new Vector2(.120f, .100f), glass, false, Vector2.zero, 0, c.Surfaces);
            Panel(c.Root, "P sealed alcove back", new Vector3(-.61f, -.220f, .220f), Vector3.back,
                new Vector2(.120f, .160f), glass, false, Vector2.zero, 0, c.Surfaces);
            // The Q body masks the P handle at its initial position. P's real
            // transverse pin intersects Q's forward rail until it is withdrawn.
            machine.PreparationCarriage = q; machine.PreparationPin = pin;
            var movingWheel = ExpansionWheel(q.transform, "Q intermediate gear", new Vector3(0, .100f, .028f), .032f, 16, 0);
            var fixedWheel = ExpansionWheel(c.Root, "Q dock output gear", new Vector3(-.376f, -.120f, .083f), .032f, 16, 11.25f);
            var drive = new GameObject("Q measured gear drive", typeof(COgheGearTrain)).GetComponent<COgheGearTrain>();
            drive.transform.SetParent(c.Root, false); drive.InputClutch = machine.Input;
            drive.Wheels = new[] { movingWheel, fixedWheel }; drive.PitchRadii = new[] { .032f, .032f }; drive.ToothCounts = new[] { 16, 16 };
            drive.MeshTolerance = q.CatchTolerance + .0005f;
            machine.PreparationTransmission = drive;
        }

        private static void FortyCable(ExpansionContext c, string name, Vector3 a, Vector3 b)
        {
            Vector3 delta = b - a;
            var cable = MechanismVisual(c.Root, name, (a + b) * .5f, new Vector3(.003f, delta.magnitude, .003f), metal);
            cable.localRotation = Quaternion.FromToRotation(Vector3.up, delta.normalized);
        }

        private static void FortyMark(ExpansionContext c, string text, Vector3 point)
        {
            var go = new GameObject("Selector label " + text, typeof(TextMesh)); go.transform.SetParent(c.Root, false);
            go.transform.localPosition = point; go.transform.localRotation = Quaternion.Euler(90, 0, 0);
            var mark = go.GetComponent<TextMesh>(); mark.text = text; mark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mark.fontSize = 64; mark.characterSize = .006f; mark.anchor = TextAnchor.MiddleCenter; mark.color = new Color(.10f, .17f, .21f);
            go.GetComponent<Renderer>().sharedMaterial = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(COgheDayLabBuilder.Folder + "/World labels.mat");
        }
    }
}
