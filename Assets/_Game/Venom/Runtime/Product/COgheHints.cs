using System.Collections.Generic;

namespace GravityBox.Venom
{
    /// <summary>
    /// Hints (Mrk, 05/10/2026): a level that introduces a new mechanism teaches how to work it on screen; every other level
    /// keeps its answer behind the Hint button. The solution hint for each level, keyed by stable content ID (so it
    /// follows the content when the catalog is reordered). English, like the rest of the product UI.
    /// </summary>
    public static class COgheHints
    {
        /// <summary>How to operate the mechanism a teaching level introduces: shown with no button, never the answer.</summary>
        public static readonly Dictionary<string, string> Operation = new Dictionary<string, string>
        {
            ["coghe.spatial.pilot.01"] = "Tap a spot to send COghe there.",
            ["coghe.spatial.pilot.02"] = "COghe climbs ivory. It slides off lavender.",
            ["coghe.spatial.pilot.03"] = "Drag to look around the box.",
            ["coghe.spatial.pilot.04"] = "Tap a handle and COghe pulls it.",
            ["coghe.spatial.pilot.08"] = "Tap the lift button to ride. It pops up again when the lift arrives.",
            ["coghe.spatial.next.14"] = "Tap a tube to go through it.",
            ["coghe.spatial.next.16"] = "Tap the Q machine to split COghe in two. Close halves merge again.",
            ["coghe.spatial.next.18"] = "Tap the ring to hold on, then tap where to swing.",
            ["coghe.spatial.next.21"] = "A weight on the tray pulls the bridge.",
            ["coghe.spatial.plus.e06"] = "Walk past the middle and the plank tips your way.",
            ["coghe.spatial.plus.e08"] = "Stand on the motor pad to run the gears.",
            ["coghe.spatial.plus.e16"] = "The gears turn the table.",
        };

        /// <summary>The answer, revealed by the Hint button.</summary>
        public static readonly Dictionary<string, string> Solution = new Dictionary<string, string>
        {
            ["coghe.spatial.pilot.05"] = "Pull the lever on the floor first: it lifts the glass cover off the handle on the wall.",
            ["coghe.spatial.pilot.06"] = "Climb up and pull the wall handle: it opens the box around the winch. Then turn the winch.",
            ["coghe.spatial.pilot.07"] = "The pin that locks the bridge is worked from down in the pit. Pull it there, then slide the bridge.",
            ["coghe.spatial.pilot.08"] = "Ride up, pull the handle up there to open the door below, then press the button again to ride down.",
            ["coghe.spatial.pilot.09"] = "Free the bridge's pin first. Sliding the bridge clears the lift shaft and makes the path in one move.",
            ["coghe.spatial.next.11"] = "Push crate A against the ledge and use it as a step.",
            ["coghe.spatial.next.12"] = "Move the low block aside first, push the tall block through, then bring the low block back as a step.",
            ["coghe.spatial.next.13"] = "Load the crate on the tray and ride up with it, then use it as the last step.",
            ["coghe.spatial.next.14"] = "Pull A to open the tube, then tap the tube.",
            ["coghe.spatial.next.15"] = "At the junction, take the branch to the balcony and pull A. Then come back and take the other branch.",
            ["coghe.spatial.next.16"] = "Split in Q and put one half on each pad.",
            ["coghe.spatial.next.17"] = "One half holds pad A while the other goes through the tube and pulls B.",
            ["coghe.spatial.next.18"] = "Hold the ring, then tap the far bank.",
            ["coghe.spatial.next.19"] = "Bring the landing within reach of the swing first.",
            ["coghe.spatial.next.21"] = "Push the crate onto the tray: its weight pulls the bridge down level.",
            ["coghe.spatial.next.22"] = "The far span goes in first, then the others in order.",
            ["coghe.spatial.next.23"] = "Switch the route on the ledge, then go back to the junction.",
            ["coghe.spatial.next.24"] = "Split again in Q until there is a part for every pad.",
            ["coghe.spatial.next.25"] = "The big part pushes; the small parts hold the pads.",
            ["coghe.spatial.next.26"] = "One half opens the landing from below; the other half swings.",
            ["coghe.spatial.next.27"] = "Four parts, four stations: one holds the lever while one rides and latches it.",
            ["coghe.spatial.next.28"] = "Outside, switch the route; inside, hold the latch.",
            ["coghe.spatial.next.29"] = "Seat both pieces in the frame first (the far one first), then merge and lift.",
            ["coghe.spatial.plus.e01"] = "Step onto the tray beside the crate and press the button to ride up.",
            ["coghe.spatial.plus.e02"] = "Check where each tube comes out before you go in.",
            ["coghe.spatial.plus.e03"] = "One part holds pad A; the other latches the door with B.",
            ["coghe.spatial.plus.e04"] = "Pull cart A, then push crate B against the ledge.",
            ["coghe.spatial.plus.e05"] = "Hold ring A to reach the island, then ring B.",
            ["coghe.spatial.plus.e06"] = "Walk past the middle of each plank and ride it down.",
            ["coghe.spatial.plus.e07"] = "Block A goes in first, then push block B to the edge.",
            ["coghe.spatial.plus.e08"] = "Stand on pad A to run the machine.",
            ["coghe.spatial.plus.e09"] = "The heavy pad needs half of COghe; a quarter is too light.",
            ["coghe.spatial.plus.e10"] = "Swing to the island, then take the tube there.",
            ["coghe.spatial.plus.e11"] = "One part holds lever B; the other rides up and latches C.",
            ["coghe.spatial.plus.e12"] = "Put gear G in its slot first, then run the machine.",
            ["coghe.spatial.plus.e13"] = "One part per tube; stand on both high pads at once.",
            ["coghe.spatial.plus.e14"] = "Mesh the lower layer first, then the upper one.",
            ["coghe.spatial.plus.e15"] = "One part stands on the motor; the other rides the lift.",
            ["coghe.spatial.plus.e16"] = "Stand on pad P so the table joins the two sides.",
            ["coghe.spatial.plus.e17"] = "Machine 1 pushes gear G into machine 2.",
            ["coghe.spatial.plus.e18"] = "One part keeps the motor running; the other meshes each layer in turn.",
        };

        public static string OperationFor(string id) => id != null && Operation.TryGetValue(id, out var text) ? text : null;
        public static string SolutionFor(string id) => id != null && Solution.TryGetValue(id, out var text) ? text : null;
    }
}
