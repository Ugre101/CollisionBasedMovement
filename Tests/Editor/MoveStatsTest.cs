using MovementScripts;
using NUnit.Framework;
using UnityEngine;

namespace Test.EditMode
{
    public class MoveStatsTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void AddModTest()
        {
            var obj = new GameObject();
            var stats = obj.AddComponent<MoveStatsManager>();
            var startValue = stats.WalkSpeed;
            FloatMod mod = new FloatMod(2);
            stats.AddMod(MoveCharacter.MoveModes.Walking,mod);
            Assert.AreEqual(startValue + 2, stats.WalkSpeed);
            // Use the Assert class to test conditions
        }  
        
        /*[Test]
        public void AddTempModTest()
        {
            var stats = new GameObject().AddComponent<MoveStatsManager>();
            var startValue = stats.WalkSpeed;
            var mod = new TempMod(new FloatMod(2),2);
            stats.MoveSpeed.AddTempMod(mod);
            stats.MoveSpeed.Tick(1);

            stats.MoveSpeed.Tick(1);
            Assert.AreEqual(startValue, stats.MoveSpeed.Value);
        }*/
    }
}
