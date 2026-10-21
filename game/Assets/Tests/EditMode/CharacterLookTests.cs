using NUnit.Framework;
using TremMasmorra.Core.Character;
using TremMasmorra.Core.Loadout;
using TremMasmorra.Core.Run;

namespace TremMasmorra.Tests
{
    public class CharacterLookTests
    {
        [Test]
        public void Look_DoesNotAffectCombatStats()
        {
            var plain = new PlayerLoadout();
            var look = new CharacterLook { Gender = Gender.Feminino, Height = 1.2f, HairStyle = 2, SkinTone = 3 };
            var other = new PlayerLoadout();
            Assert.AreEqual(plain.MaxInUse, other.MaxInUse);
            Assert.AreEqual(plain.Backpack.Count, other.Backpack.Count);
            Assert.IsNull(other.AbilityQ);
            Assert.IsNull(other.AbilityE);
            Assert.AreEqual(1.2f, look.Height);
        }

        [Test]
        public void Height_ClampedToRange()
        {
            var look = new CharacterLook { Height = 5f };
            Assert.AreEqual(CharacterLook.MaxHeight, look.Height);
            look.Height = 0.1f;
            Assert.AreEqual(CharacterLook.MinHeight, look.Height);
        }

        [Test]
        public void HairAndSkin_WrapAround()
        {
            var look = new CharacterLook { HairStyle = -1, SkinTone = 4 };
            Assert.AreEqual(CharacterLook.HairStyles - 1, look.HairStyle);
            Assert.AreEqual(0, look.SkinTone);
        }
    }
}
