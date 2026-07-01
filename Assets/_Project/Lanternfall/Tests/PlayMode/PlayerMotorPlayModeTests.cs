using System.Collections;
using Lanternfall.Gameplay.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Lanternfall.Tests
{
    public sealed class PlayerMotorPlayModeTests
    {
        [UnityTest]
        public IEnumerator MotorMovesAndDodgeGrantsTemporaryInvulnerability()
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(20f, 1f, 20f);

            GameObject player = new GameObject("Test Bearer");
            player.transform.position = Vector3.up;
            player.AddComponent<CharacterController>();
            PlayerMotor motor = player.AddComponent<PlayerMotor>();
            yield return null;

            Vector3 start = player.transform.position;
            for (int index = 0; index < 10; index++)
            {
                motor.Step(Vector2.up, false, index == 0, 0.02f);
            }

            Assert.That(player.transform.position.z, Is.GreaterThan(start.z + 1f));
            Assert.That(motor.IsInvulnerable, Is.True);
            motor.Step(Vector2.zero, false, false, 0.25f);
            Assert.That(motor.IsInvulnerable, Is.False);

            Object.Destroy(player);
            Object.Destroy(floor);
        }
    }
}

