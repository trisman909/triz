using System.Collections;
using Lanternfall.Gameplay.Player;
using Lanternfall.Gameplay.Combat;
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

        [UnityTest]
        public IEnumerator ProjectileDeliversResolvedDamageAndReturns()
        {
            GameObject target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.transform.position = new Vector3(0f, 1f, 3f);
            Health health = target.AddComponent<Health>();
            health.Configure(100f, 0f, false);

            WeaponDefinition weapon = ScriptableObject.CreateInstance<WeaponDefinition>();
            weapon.Configure(
                "test.weapon", "Test Weapon", 25f, 2f, 20f, 0f, 0f,
                DamageElement.Ember);

            GameObject projectileObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectileObject.GetComponent<SphereCollider>().enabled = false;
            Projectile projectile = projectileObject.AddComponent<Projectile>();
            bool returned = false;
            projectile.Launch(
                weapon, new Vector3(0f, 1f, 0f), Vector3.forward, 8,
                item =>
                {
                    returned = true;
                    item.gameObject.SetActive(false);
                });

            float timeout = 1f;
            while (!returned && timeout > 0f)
            {
                timeout -= Time.deltaTime;
                yield return null;
            }

            Assert.That(returned, Is.True);
            Assert.That(health.Current, Is.EqualTo(75f).Within(0.01f));
            Object.Destroy(target);
            Object.Destroy(projectileObject);
            Object.Destroy(weapon);
        }
    }
}
