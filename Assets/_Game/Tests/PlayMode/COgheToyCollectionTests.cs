using System.Collections;
using System.Linq;
using GravityBox.Venom;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace GravityBox.Tests
{
    // The toys in the Items menu (Mrk 18/10/2026): a collection with empty slots for the ones still to be won.
    public partial class COgheProductUITests
    {
        [UnityTest] public IEnumerator ToysAreACollectionWithEmptySlots()
        {
            try
            {
                FreshShop(false); game.Progress.HomeUnlocked = true; COgheToys.Give(COgheToys.Drum);
                // straight in, before COghe's hello: the player's choice comes first
                yield return Click("Home"); var p = game.Personality; yield return new WaitForSecondsRealtime(.3f);
                yield return Click("Items"); Assert.AreEqual(COgheProductPopup.Collection, ui.Popup);
                Assert.IsTrue(ui.GetComponentsInChildren<Text>().Any(t => t.text == "Toys 1/" + COgheToys.Catalog.Length), "How many are won");
                Text On(string card, string label) => ui.GetComponentsInChildren<Button>().First(b => b.name == card).GetComponentsInChildren<Text>().First(t => t.name == label);
                Assert.AreEqual("Play", On("Toy " + COgheToys.Drum, "State").text, "The drum is won");
                Assert.AreEqual("???", On("Toy " + COgheToys.Horn, "Name").text, "The horn is still a secret");
                Assert.AreEqual("Chapter 3", On("Toy " + COgheToys.Horn, "State").text, "and says where it is won");
                yield return Click("Toy " + COgheToys.Horn); Assert.AreEqual(COgheProductPopup.Collection, ui.Popup, "A toy still to win only says where");
                yield return Click("Toy " + COgheToys.Drum); Assert.AreEqual(COgheProductPopup.None, ui.Popup);
                var drum = game.HomeRoom.Find(COgheToys.Drum);
                yield return AwaitBonus(() => p.Playing == drum, 10, "COghe goes and plays it");
            }
            finally { EndShop(); ui.ShowMenu(); }
        }
    }
}
