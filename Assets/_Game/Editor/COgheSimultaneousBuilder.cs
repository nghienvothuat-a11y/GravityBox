using GravityBox.Venom;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GravityBox.Editor
{
    public static partial class VenomCampaignBuilder
    {
        private static VenomSurfacePatch CoopGripStrip(ExpansionContext c, string name, float x, float bottom = -.30f, float top = .08f, float width = .20f)
        {
            bool hole = Mathf.Abs(x - c.Exit.x) < width * .5f && c.Exit.y > bottom && c.Exit.y < top;
            Vector3 centre = new Vector3(x, (bottom + top) * .5f, .397f);
            var strip = Panel(c.Root, name, centre, Vector3.back, new Vector2(width, top - bottom), stone, hole,
                new Vector2(x - c.Exit.x, c.Exit.y - centre.y), c.Owner.ApertureRadius, c.Surfaces);
            strip.ExteriorGlass = true;
            return strip;
        }
        private static COgheTapRail CoopHold(ExpansionContext c, string label, VenomSurfacePatch surface, Vector3 point)
        {
            Vector3 normal = surface.Normal;
            var rail = ExpansionRail(c, label + " spring grip", point, Vector3.down, .028f, 0,
                Mathf.Abs(normal.x) > .5f ? new Vector3(.025f, .024f, .046f) : new Vector3(.046f, .024f, .025f), .020f, .002f, false, true);
            var prop = rail.GetComponent<VenomMovableProp>(); prop.Manipulable = false;
            prop.ManipulationGrip.localPosition = normal * .025f;
            var task = rail.gameObject.AddComponent<COgheTapRail>();
            task.Rail = rail; task.Handle = prop.ManipulationGrip; task.WorkingSurface = surface;
            task.Label = label; task.HoldAtEnd = true; task.PickHandleOnly = task.TrackStandPoint = true;
            task.TouchSize = Vector3.one * .082f; task.StandOffset = Vector3.down * .070f;
            task.Speed = .055f; task.StallSeconds = 5;
            return task;
        }
        private static COgheRailSlider CoopExitCover(ExpansionContext c, bool lift = false)
        {
            var cover = ViewGate(c, "Wall exit shutter", c.Exit + Vector3.back * .018f,
                lift ? Vector3.up : Vector3.left, lift ? .14f : .15f, new Vector3(.125f, .125f, .020f));
            ViewExitSurface(c).NavigationHoleBlocked = true;
            return cover;
        }
        private static void CoopWallLock(ExpansionContext c, COgheTapRail task, Vector3 input)
        {
            Vector3 normal = task.WorkingSurface.Normal;
            Vector3 sideways = Mathf.Abs(normal.x) > .5f ? Vector3.forward : Vector3.right;
            Vector3 point = task.Rail.Start + normal * .030f + sideways * .035f + Vector3.up * .010f;
            task.InterlockPin = MechanismVisual(c.Root, task.Label + " wall locking pin", point, new Vector3(.014f, .050f, .014f), metal);
            TapLink(c.Root, "Retained bridge catch linkage", input, point);
        }
        private static COgheCooperativeDrive CoopDrive(ExpansionContext c, string name,
            COgheCooperativeDrive.MechanismKind kind, COgheTapRail a, COgheTapRail b,
            COgheRailSlider output, Vector3 size, bool final = false, COgheRailSlider cover = null)
        {
            var drive = new GameObject(name, typeof(COgheCooperativeDrive)).GetComponent<COgheCooperativeDrive>();
            drive.transform.SetParent(c.Root, false); drive.Kind = kind; drive.A = a; drive.B = b;
            drive.Output = output; drive.OutputSize = size; drive.FinalGate = final; drive.FinalCover = cover;
            drive.Aperture = final ? ViewExitSurface(c) : null;
            drive.Waiting = plastic; drive.Ready = mint;
            var sound = drive.gameObject.AddComponent<AudioSource>(); sound.playOnAwake = false; sound.volume = .22f;
            sound.clip = AssetDatabase.LoadAssetAtPath<AudioClip>(ViewFolder + "/Audio/Cooperative catch.wav");
            var mount = new GameObject("Shared visible actuator").transform; mount.SetParent(c.Root, false);
            mount.localPosition = kind == COgheCooperativeDrive.MechanismKind.TwinPull ? new Vector3(output.Start.x, .075f, .33f) :
                kind == COgheCooperativeDrive.MechanismKind.BrakeBridge ? b.Rail.Start + b.Handle.localPosition + new Vector3(0, .020f, -.030f) :
                output.Start + new Vector3(0, .025f, -size.z * .5f - .035f);
            drive.Junction = mount;
            drive.Balance = MechanismVisual(mount, "Balance and paired cylinders", Vector3.zero, new Vector3(.14f, .012f, .020f), metal);
            Vector3 pin = output.Start + output.Axis * output.Travel + Vector3.left * (size.x * .5f + .026f);
            drive.CatchPin = MechanismVisual(c.Root, "Terminal catch pin", pin, new Vector3(.042f, .013f, .017f), metal);
            drive.Brake = MechanismVisual(mount, "Safety interlock jaws", new Vector3(0, -.04f, 0), new Vector3(kind == COgheCooperativeDrive.MechanismKind.BrakeBridge ? .072f : .035f, .020f, .025f), plastic);
            Renderer Lamp(float x) => MechanismVisual(mount, "Input state", new Vector3(x, .024f, -.006f), Vector3.one * .014f, plastic, PrimitiveType.Sphere).GetComponent<Renderer>();
            drive.LampA = Lamp(-.05f); drive.LampB = Lamp(.05f);
            if (kind == COgheCooperativeDrive.MechanismKind.TwinPull)
            {
                drive.Pulleys = new Transform[2];
                var inputs = new[] { a, b };
                for (int i = 0; i < 2; i++)
                {
                    Vector3 p = inputs[i].Rail.Start; p.y = .075f;
                    drive.Pulleys[i] = MechanismVisual(c.Root, "Cable pulley", p, new Vector3(.04f, .009f, .04f), metal, PrimitiveType.Cylinder);
                    drive.Pulleys[i].localRotation = Quaternion.FromToRotation(Vector3.up, inputs[i].WorkingSurface.Normal);
                }
            }
            if (kind == COgheCooperativeDrive.MechanismKind.PairedValves)
            {
                drive.PistonRods = new Transform[2];
                for (int i = 0; i < 2; i++)
                {
                    Vector3 p = output.Start + new Vector3(i == 0 ? -.065f : .065f, .007f, -size.z * .5f - .025f);
                    MechanismVisual(c.Root, "Porcelain pressure cylinder", p, new Vector3(.029f, .055f, .029f), stone, PrimitiveType.Cylinder);
                    drive.PistonRods[i] = MechanismVisual(c.Root, "Measured piston rod", p + Vector3.down * .0135f, new Vector3(.010f, .013f, .010f), metal);
                    TapLink(output.transform, "Piston arm", new Vector3(i == 0 ? -.065f : .065f, 0, -size.z * .5f), new Vector3(i == 0 ? -.065f : .065f, 0, -size.z * .5f - .025f));
                }
            }
            LineRenderer Branch(string label, COgheTapRail input)
            {
                var line = new GameObject(label, typeof(LineRenderer)).GetComponent<LineRenderer>();
                line.transform.SetParent(drive.transform, false); line.useWorldSpace = true; line.positionCount = drive.Pulleys.Length == 2 ? 4 : 3;
                line.startWidth = line.endWidth = kind == COgheCooperativeDrive.MechanismKind.PairedValves ? .006f : .003f;
                line.sharedMaterial = kind == COgheCooperativeDrive.MechanismKind.PairedValves ? CoopPressureMaterial() : metal;
                for (int i = 0; i < line.positionCount; i++) line.SetPosition(i, Vector3.Lerp(input.HandPoint, mount.position, i / (line.positionCount - 1f)));
                line.numCapVertices = 3; line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                return line;
            }
            drive.BranchA = Branch("A force branch", a); drive.BranchB = Branch("B force branch", b);
            TapLink(c.Root, "Visible linkage housing", mount.localPosition, output.Start);
            if (kind == COgheCooperativeDrive.MechanismKind.BrakeBridge)
            { b.RequiredGrip = a; output.LatchAtEnd = output.LatchAtStart = false; }
            return drive;
        }
        private static Material CoopPressureMaterial()
        {
            string path = ViewFolder + "/Pressure cyan.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.SetColor("_BaseColor", new Color(.16f, .55f, .67f));
                material.SetFloat("_Smoothness", .48f); material.SetFloat("_Metallic", .12f);
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }
        private static void CoopWallShell(ExpansionContext c, bool banks = false)
        {
            c.Outward = Vector3.forward;
            if (banks)
            {
                // Continuous fixed working banks; only the return deck spans the low, slippery basin.
                ChapterFloor(c, "Departure bank", new Vector3(-.35f, -.30f, 0), new Vector2(.42f, .80f));
                ChapterFloor(c, "Receiving bank", new Vector3(.35f, -.30f, 0), new Vector2(.42f, .80f));
                var basin = ChapterFloor(c, "Recovery basin", new Vector3(0, -.42f, 0), new Vector2(1.12f, .80f)); basin.Slippery = true;
                Panel(c.Root, "Recovery climb", new Vector3(-.14f, -.36f, 0), Vector3.right, new Vector2(.80f, .12f), stone, false, Vector2.zero, 0, c.Surfaces);
                ChapterShell(c, false);
            }
            else ChapterShell(c);
            foreach (var s in c.Surfaces) if (s.ExteriorGlass)
                s.Slippery = !(s.name == "Outer pane 2" && (c.Number == 23 || c.Number == 27));
            ChapterKnife(c);
        }
        private static void CoopTube(ExpansionContext c)
        {
            ChapterTube(c, "Two-way role transfer", new Vector3(-.21f, -.255f, -.20f), new Vector3(-.10f, -.19f, -.20f),
                new Vector3(0, -.12f, -.10f), new Vector3(.23f, -.12f, -.10f), new Vector3(.33f, -.20f, -.20f),
                new Vector3(.38f, -.255f, -.20f), new Vector3(.43f, -.255f, -.20f));
        }
        private static COgheCooperativeDrive CoopBridge(ExpansionContext c, bool far, bool final)
        {
            var a = CoopHold(c, "A", CoopGripStrip(c, "Brake grip strip", -.43f), new Vector3(-.43f, -.135f, .37f));
            var bank = c.Surfaces.Find(s => s.name == (far ? "Receiving bank" : "Departure bank"));
            // The Boss return bridge docks in front of the upper lift, keeping its top tappable after lifting.
            var b = ChapterBridge(c, "B", 0, .27f, far ? -.02f : -.20f, c.Number == 30 ? .04f : .20f, bank, far ? .43f : -.30f);
            b.StandOffset = Vector3.right * (far ? .07f : -.07f);
            return CoopDrive(c, "Brake release and return bridge", COgheCooperativeDrive.MechanismKind.BrakeBridge,
                a, b, b.Rail, new Vector3(.27f, .026f, .14f), final, final ? CoopExitCover(c) : null);
        }
        private static COgheCooperativeDrive CoopPulls(ExpansionContext c, string aName = "A", string bName = "B", bool corner = false)
        {
            var a = CoopHold(c, aName, CoopGripStrip(c, "Left pull grip strip", -.34f), new Vector3(-.34f, -.12f, .37f));
            COgheTapRail b;
            if (corner)
            {
                var side = Panel(c.Root, "Corner climbing strip", new Vector3(.557f, -.11f, .13f), Vector3.left, new Vector2(.22f, .38f), stone, false, Vector2.zero, 0, c.Surfaces);
                side.ExteriorGlass = true;
                b = CoopHold(c, bName, side, new Vector3(.53f, -.08f, .13f));
            }
            else b = CoopHold(c, bName, CoopGripStrip(c, "Right pull grip strip", .34f), new Vector3(.34f, -.12f, .37f));
            CoopGripStrip(c, "Exit climbing strip", c.Exit.x);
            return CoopDrive(c, "Twin spring pulls and wall shutter", COgheCooperativeDrive.MechanismKind.TwinPull,
                a, b, CoopExitCover(c, true), new Vector3(.125f, .125f, .020f), true);
        }
        private static COgheCooperativeDrive CoopLift(ExpansionContext c, string aName = "A", string bName = "B")
        {
            float valveX = c.Number == 30 ? -.24f : -.43f;
            var a = CoopHold(c, aName, CoopGripStrip(c, "Left valve grip strip", valveX), new Vector3(valveX, c.Number == 30 ? .005f : -.135f, .37f));
            float rightValveZ = c.Number == 30 ? .04f : -.22f;
            var right = Panel(c.Root, "Right valve climbing strip", new Vector3(.557f, -.11f, rightValveZ), Vector3.left, new Vector2(.22f, .38f), stone, false, Vector2.zero, 0, c.Surfaces);
            right.ExteriorGlass = true;
            var b = CoopHold(c, bName, right, new Vector3(.53f, -.135f, rightValveZ));
            // The elevator inserts the missing piece between a fixed ramp and the upper exit landing.
            Vector3 start = new Vector3(-.28f, -.30f, -.09f), end = new Vector3(-.28f, -.16f, .16f);
            Vector3 along = (end - start).normalized, normal = Vector3.Cross(Vector3.right, along).normalized;
            if (normal.y < 0) normal = -normal;
            var ramp = Panel(c.Root, "Porcelain approach ramp", (start + end) * .5f, normal, new Vector2(.20f, Vector3.Distance(start, end)), stone, false, Vector2.zero, 0, c.Surfaces);
            // A closed wedge reaches the floor. Tissue approaching from behind cannot tuck under
            // a floating ramp skin and later hook its trailing particle beneath the landing lip.
            var rampMesh = PanelMesh(ramp.Size, false, Vector2.zero, 0); var rampVertices = rampMesh.vertices;
            for (int i = 0; i < rampVertices.Length; i++) if (rampVertices[i].z < -.001f)
            {
                Vector3 p = ramp.transform.TransformPoint(rampVertices[i]); p.y = -.308f;
                rampVertices[i] = ramp.transform.InverseTransformPoint(p);
            }
            rampMesh.vertices = rampVertices; rampMesh.RecalculateBounds(); rampMesh.RecalculateNormals(); rampMesh = Save(rampMesh);
            ramp.GetComponent<MeshFilter>().sharedMesh = rampMesh; ((MeshCollider)ramp.Shape).sharedMesh = rampMesh;
            if (c.Number != 28)
            {
                // Match the dock's thickness so a trailing particle cannot hook under a thinner receiving lip.
                var landing = ChapterFloor(c, "Upper departure", new Vector3(-.24f, -.16f, .28f), new Vector2(.28f, .24f));
                var mesh = PanelMesh(landing.Size, false, Vector2.zero, 0);
                var vertices = mesh.vertices;
                for (int i = 0; i < vertices.Length; i++) if (vertices[i].z < -.001f) vertices[i].z = -.026f;
                mesh.vertices = vertices; mesh.RecalculateBounds(); mesh.RecalculateNormals(); mesh = Save(mesh);
                landing.GetComponent<MeshFilter>().sharedMesh = mesh;
                Object.DestroyImmediate(landing.Shape);
                var shape = landing.gameObject.AddComponent<BoxCollider>(); shape.center = Vector3.back * .013f;
                shape.size = new Vector3(.28f, .24f, .026f); shape.sharedMaterial = contact; shape.contactOffset = .0003f; landing.Shape = shape;
            }
            ChapterFloor(c, "Upper exit landing", new Vector3(.33f, -.16f, .28f), new Vector2(.46f, .24f));
            CoopGripStrip(c, "Upper exit climbing strip", c.Exit.x, -.16f, width: .30f);
            var lift = ChapterMovingDeck(c, "Twin cylinder climbing bridge", new Vector3(0, -.281f, .27f), Vector3.up, .108f, new Vector2(.198f, .18f));
            lift.LatchAtEnd = false;
            return CoopDrive(c, "Paired pressure lift", COgheCooperativeDrive.MechanismKind.PairedValves,
                a, b, lift, new Vector3(.198f, .026f, .18f), true, CoopExitCover(c));
        }
        private static void BuildSimultaneousLevel(ExpansionContext c)
        {
            int n = c.Number;
            c.Exit = new Vector3(n == 23 || n == 27 ? 0 : .35f, n == 26 || n == 28 || n == 30 ? -.06f : -.15f, .40f);
            bool banks = n == 24 || n == 25 || n == 29 || n == 30;
            CoopWallShell(c, banks);
            if (n == 23 || n == 27) { CoopPulls(c, corner: n == 27); return; }
            if (n == 24 || n == 25)
            {
                CoopGripStrip(c, "Exit climbing strip", c.Exit.x, width: .42f);
                if (n == 25) CoopTube(c);
                CoopBridge(c, n == 25, true); return;
            }
            if (n == 26) { CoopLift(c); return; }
            if (n == 28)
            {
                var lift = CoopLift(c);
                var selector = ChapterTask(c, "T", -.12f, -.15f, .12f);
                var returnDeck = ChapterMovingDeck(c, "First caught route", new Vector3(-.24f, -.281f, .28f), Vector3.up, .108f, new Vector2(.268f, .228f));
                // Leave clearance for the adjacent piston while this first deck is already docked.
                // The second deck's wider final top closes the seam only after it reaches this height.
                var firstTop = returnDeck.GetComponent<COgheDockedBridgeDeck>().DockedTop;
                firstTop.Size = new Vector2(.276f, .248f);
                ((BoxCollider)firstTop.Shape).size = new Vector3(.276f, .248f, .026f);
                returnDeck.LatchAtEnd = false;
                var first = CoopDrive(c, "Selected return route", COgheCooperativeDrive.MechanismKind.PairedValves,
                    lift.A, lift.B, returnDeck, new Vector3(.268f, .026f, .228f));
                first.Selector = lift.Selector = selector.Rail; lift.SelectorEnd = true;
                selector.RequiredRail = returnDeck;
                TapLock(c, selector, first.CatchPin.localPosition);
                return;
            }
            CoopTube(c);
            var bridge = CoopBridge(c, true, false);
            if (n == 29)
            {
                // Both final grips are on the receiving side; the bridge lets the brake operator join.
                var cGrip = CoopHold(c, "C", CoopGripStrip(c, "Far rear pull strip", .48f, width: .14f), new Vector3(.48f, -.12f, .37f));
                var side = Panel(c.Root, "Far side pull strip", new Vector3(.557f, -.11f, -.05f), Vector3.left, new Vector2(.20f, .38f), stone, false, Vector2.zero, 0, c.Surfaces);
                side.ExteriorGlass = true;
                var dGrip = CoopHold(c, "D", side, new Vector3(.53f, -.12f, -.05f));
                CoopGripStrip(c, "Exit climbing strip", c.Exit.x, width: .16f);
                CoopDrive(c, "Far twin pull exit", COgheCooperativeDrive.MechanismKind.TwinPull, cGrip, dGrip, CoopExitCover(c, true), new Vector3(.125f, .125f, .020f), true);
            }
            else
            {
                var lift = CoopLift(c, "C", "D");
                lift.A.RequiredRail = bridge.Output;
                lift.B.RequiredRail = bridge.Output;
                CoopWallLock(c, lift.A, bridge.CatchPin.localPosition);
                CoopWallLock(c, lift.B, bridge.CatchPin.localPosition);
            }
        }
    }
}
