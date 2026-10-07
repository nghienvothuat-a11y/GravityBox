using System.Linq;
using GravityBox.Venom;
using UnityEngine;

namespace GravityBox.Editor
{
 /// <summary>
 /// Crate levels 51–60 (Mrk approved the redone designs, 06/10/2026: "triển khai levels 51-60 theo phương án này";
 /// PLANS/COGHE_CRATE_LEVELS_LOGIC.md). A 6 × 5 grid of 12 cm cells; the exit is a hole in the floor under the red crate.
 /// Each crate rides its own rail between two stops; a tap on any of its faces slides it to the other stop (Mrk 07/10/2026).
 /// It has a handle at both ends: the end tapped (or nearest the tap) is where COghe works from, pushing or pulling,
 /// and the move is checked on the grid before COghe sets off (COgheTapRail.CrateFaces). Crates stand taller than COghe and
 /// are not walked on: COghe goes round them on the floor, so a crate whose slide is clear is always one it can reach the
 /// moment a floor road leads there. Layouts and COghe's start cell come from CrateDesigns (generated from the design data).
 /// </summary>
 public static partial class VenomCampaignBuilder
 {
  // 14 cm cells: a one-cell road between crates taller than COghe has to let its ~11 cm body through (12 cm jammed).
  const float CrateCell = .14f;
  // The glass walls stand on the grid's edge: a strip of floor beside the grid, narrower than COghe, would draw routes
  // it cannot fit through (the planner tests links with a 6 mm radius).
  const float CrateBoxWidth = 6 * CrateCell, CrateBoxDepth = 5 * CrateCell;
  static readonly Vector2Int CrateHole = new Vector2Int(3, 2);

  /// <summary>Floor point of a grid position (cell corners at integers, centres at +.5).</summary>
  static Vector3 CrateGrid(float x, float z) => new Vector3((x - 3) * CrateCell, -.30f, (z - 2.5f) * CrateCell);

  static void PlusCrate(ExpansionContext c, int k)
  {
   var design = CrateDesigns[k];
   c.Exit = CrateGrid(CrateHole.x + .5f, CrateHole.y + .5f); c.Outward = Vector3.down;
   c.Spawn = CrateGrid(design.spawn.x + .5f, design.spawn.z + .5f) + Vector3.up * .05f;
   CrateShell(c);
   var floor = c.Surfaces[0];
   for (int i = 0; i < design.crates.Length; i++)
   {
    var cr = design.crates[i];
    bool square = cr.w == 2 && cr.h == 2;
    // Taller than COghe (the design: it walks round crates, never over them).
    float height = square ? .07f : .06f;
    // 1 mm clear of the cell edges: the rails keep neighbours apart, and the 2 mm between two crates is too narrow for a
    // route (the planner tests links with a 6 mm radius), so COghe is never sent into the seam between crates.
    var size = new Vector3(cr.w * CrateCell - .002f, height, cr.h * CrateCell - .002f);
    // 2 mm clear of the floor (the rail carries it): a crate base flush with the floor catches the rim of the exit hole
    // when it slides back onto the floor across it (level 59's red crate stopped there).
    var start = CrateGrid(cr.x + cr.w * .5f, cr.z + cr.h * .5f) + Vector3.up * (height * .5f + .002f);
    var axis = (cr.axis == 'x' ? Vector3.right : Vector3.forward) * Mathf.Sign(cr.shift);
    float travel = Mathf.Abs(cr.shift) * CrateCell;
    string name = cr.red ? "Red crate" : $"Crate {i}";
    var rail = ExpansionRail(c, name, start, axis, travel, 0, size, square ? .07f : .05f, .012f, false, true);
    // The faces stay in the navigation (they keep routes out of the crate and out of the 2 mm seams) but are slippery:
    // a route to the floor pays twenty times over them, so COghe goes round on the floor whenever a floor road exists.
    foreach (var face in rail.GetComponentsInChildren<VenomSurfacePatch>(true)) { face.Slippery = true; face.MotionFrame = rail.Body; }
    // Two stops, braked whenever no pull is under way (TapRail locks a rail with stops while idle). End latches would
    // brake a crate 3 mm before its pull counts as arrived and leave the pull hanging.
    rail.LatchAtStart = rail.LatchAtEnd = false;
    // The pull stops pushing once the crate is within the catch, so it seats a few millimetres short: 6 mm counts as there.
    rail.CatchTolerance = .006f;
    var prop = rail.GetComponent<VenomMovableProp>(); prop.Manipulable = false;
    // Handles at both ends of the slide: the far end (+axis) and the near end; COghe works the one on its side.
    float half = Mathf.Abs(Vector3.Dot(size * .5f, axis));
    var grip = prop.ManipulationGrip; grip.localPosition = axis * (half + .008f); grip.localRotation = Quaternion.LookRotation(axis);
    var other = MechanismVisual(prop.transform, name + " near handle", -axis * (half + .008f), grip.localScale, metal); other.localRotation = Quaternion.LookRotation(-axis);
    var task = rail.gameObject.AddComponent<COgheTapRail>(); task.Rail = rail; task.Handle = grip; task.AlternateHandle = other; task.WorkingSurface = floor;
    task.Stops = new[] { 0f, travel }; task.SeatAtStops = true;
    task.Label = cr.red ? "B" : "A"; task.TwoSided = true; task.StandOffset = axis * .062f; task.TrackStandPoint = true; task.StallSeconds = 5;
    // A tap anywhere on the crate's top takes it (handles of crates end to end would sit in the same gap). The pick box
    // reaches just over the top only: a tap on the floor past a crate is not taken as a tap on it.
    task.PickHandleOnly = false; task.TouchSize = new Vector3(size.x + .004f, Mathf.Max(.006f, 2 * (height * .5f + .006f - .023f)), size.z + .004f);
    // Any face takes a tap; the grid check needs the crate's size and the floor (root space, cell corners at the edges).
    task.CrateFaces = true; task.CrateSize = size; task.CrateCell = CrateCell;
    task.ArenaMin = new Vector2(-CrateBoxWidth * .5f, -CrateBoxDepth * .5f); task.ArenaMax = new Vector2(CrateBoxWidth * .5f, CrateBoxDepth * .5f);
   }
   // Printed grid on the floor: the cells the crates slide in.
   for (int x = 0; x <= 6; x++) PlusGridLine(c, CrateGrid(x, 0), CrateGrid(x, 5));
   for (int z = 0; z <= 5; z++) PlusGridLine(c, CrateGrid(0, z), CrateGrid(6, z));
  }

  static void PlusGridLine(ExpansionContext c, Vector3 a, Vector3 b)
  {
   var d = b - a;
   var line = MechanismVisual(c.Root, "Crate grid line", (a + b) * .5f + Vector3.up * .0006f, new Vector3(.0015f, .0004f, d.magnitude), metal);
   line.localRotation = Quaternion.LookRotation(d, Vector3.up);
  }

  /// <summary>Glass box with the exit cut in the floor (crate levels): the walls and ceiling as NextShell, no hole in them.</summary>
  static void CrateShell(ExpansionContext c)
  {
   float bx = CrateBoxWidth * .5f, bz = CrateBoxDepth * .5f;
   Panel(c.Root, "Laboratory floor", new Vector3(0, -.30f, 0), Vector3.up, new Vector2(CrateBoxWidth, CrateBoxDepth), stone, true, new Vector2(-c.Exit.x, c.Exit.z), c.Owner.ApertureRadius, c.Surfaces);
   const float top = .10f; float mid = (top - .30f) * .5f, height = top + .30f;
   var pos = new[] { new Vector3(0, top, 0), new Vector3(0, mid, -bz), new Vector3(0, mid, bz), new Vector3(-bx, mid, 0), new Vector3(bx, mid, 0) };
   var normal = new[] { Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
   for (int i = 0; i < 5; i++)
   {
    var p = Panel(c.Root, "Outer pane " + i, pos[i], normal[i], i == 0 ? new Vector2(CrateBoxWidth, CrateBoxDepth) : new Vector2(i < 3 ? CrateBoxWidth : CrateBoxDepth, height), glass, false, Vector2.zero, 0, c.Surfaces);
    p.ExteriorGlass = true; p.Selectable = true; p.Slippery = true;
   }
  }
 }
}
