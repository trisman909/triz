using System.IO;
using Lanternfall.Gameplay.Camera;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Enemies;
using Lanternfall.Gameplay.Input;
using Lanternfall.Gameplay.Player;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Lanternfall.Editor
{
    public static class CombatSandboxBuilder
    {
        private const string ScenePath =
            "Assets/_Project/Lanternfall/Scenes/CombatSandbox.unity";

        [MenuItem("Lanternfall/Build Combat Sandbox")]
        public static void Build()
        {
            EnsureFolders();
            ConfigureUrp();

            Scene scene = EditorSceneManager.NewScene(
                NewSceneSetup.EmptyScene,
                NewSceneMode.Single);

            Material floorMaterial = CreateMaterial(
                "ArenaStone", new Color(0.12f, 0.15f, 0.18f), 0.18f);
            Material wallMaterial = CreateMaterial(
                "ArenaEdge", new Color(0.22f, 0.17f, 0.13f), 0.08f);
            Material playerMaterial = CreateMaterial(
                "Bearer", new Color(0.18f, 0.72f, 0.82f), 0.55f);
            Material emberMaterial = CreateMaterial(
                "Ember", new Color(1f, 0.33f, 0.08f), 0.4f);
            Material projectileMaterial = CreateMaterial(
                "Projectile", new Color(0.3f, 0.9f, 1f), 0.7f);

            WeaponDefinition cinderStaff = CreateWeapon(
                "weapon.cinder_staff", "Cinder Staff", 18f, 2.4f, 17f, 2.5f,
                0.1f, DamageElement.Ember);
            CreateWeapon(
                "weapon.prism_bow", "Prism Bow", 30f, 1.25f, 24f, 4f,
                0.18f, DamageElement.Storm);
            CreateWeapon(
                "weapon.echo_blades", "Echo Blades", 11f, 4.5f, 14f, 1.5f,
                0.07f, DamageElement.Gloam);
            AbilityDefinition radiantBurst = CreateAbility(
                "ability.radiant_burst", AbilityKind.RadiantBurst,
                38f, 5f, 7f, DamageElement.Ember);
            CreateAbility(
                "ability.gloam_well", AbilityKind.GloamWell,
                25f, 6.5f, 10f, DamageElement.Gloam);
            Projectile projectilePrefab = CreateProjectilePrefab(projectileMaterial);
            EnemyDefinition[] enemies = CreateEnemyRoster();
            EnemyBrain enemyPrefab = CreateEnemyPrefab(wallMaterial);

            CreateBlock("Floor", new Vector3(0f, -0.5f, 0f),
                new Vector3(24f, 1f, 18f), floorMaterial);
            CreateBlock("Wall North", new Vector3(0f, 1f, 9f),
                new Vector3(24f, 3f, 1f), wallMaterial);
            CreateBlock("Wall South", new Vector3(0f, 1f, -9f),
                new Vector3(24f, 3f, 1f), wallMaterial);
            CreateBlock("Wall East", new Vector3(12f, 1f, 0f),
                new Vector3(1f, 3f, 18f), wallMaterial);
            CreateBlock("Wall West", new Vector3(-12f, 1f, 0f),
                new Vector3(1f, 3f, 18f), wallMaterial);

            for (int index = 0; index < 8; index++)
            {
                float angle = index * Mathf.PI * 0.25f;
                CreateBlock(
                    $"Pillar {index + 1}",
                    new Vector3(Mathf.Cos(angle) * 7f, 1f, Mathf.Sin(angle) * 5f),
                    new Vector3(1.1f, 3f, 1.1f),
                    wallMaterial);
            }

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Bearer";
            player.transform.position = Vector3.up;
            Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
            CharacterController controller = player.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = 0.48f;
            controller.center = Vector3.zero;
            player.GetComponent<Renderer>().sharedMaterial = playerMaterial;
            player.AddComponent<PlayerInputReader>();
            player.AddComponent<PlayerMotor>();
            Health playerHealth = player.AddComponent<Health>();
            playerHealth.Configure(180f, 5f, false);

            GameObject lantern = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            lantern.name = "Lantern";
            lantern.transform.SetParent(player.transform);
            lantern.transform.localPosition = new Vector3(0.45f, 0.55f, 0.15f);
            lantern.transform.localScale = Vector3.one * 0.28f;
            Object.DestroyImmediate(lantern.GetComponent<Collider>());
            lantern.GetComponent<Renderer>().sharedMaterial = emberMaterial;
            Light lanternLight = lantern.AddComponent<Light>();
            lanternLight.type = LightType.Point;
            lanternLight.color = new Color(1f, 0.36f, 0.12f);
            lanternLight.range = 8f;
            lanternLight.intensity = 5f;
            lanternLight.shadows = LightShadows.Soft;
            PlayerCombat combat = player.AddComponent<PlayerCombat>();
            combat.Configure(cinderStaff, radiantBurst, projectilePrefab, lantern.transform);

            CreateDummy("Ashen Target A", new Vector3(-5f, 1f, 2f), wallMaterial);
            CreateDummy("Ashen Target B", new Vector3(5f, 1f, 3f), wallMaterial);
            CreateDummy("Ashen Target C", new Vector3(0f, 1f, 6f), wallMaterial);
            GameObject directorObject = new GameObject("Encounter Director");
            EncounterDirector director = directorObject.AddComponent<EncounterDirector>();
            director.Configure(enemyPrefab, enemies, player.transform, 6);

            GameObject cameraObject = new GameObject("Isometric Camera");
            cameraObject.tag = "MainCamera";
            UnityEngine.Camera camera = cameraObject.AddComponent<UnityEngine.Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.025f, 0.035f, 0.055f);
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            cameraObject.AddComponent<AudioListener>();
            IsometricCameraRig rig = cameraObject.AddComponent<IsometricCameraRig>();
            rig.SetTarget(player.transform);
            cameraObject.transform.position = player.transform.position + new Vector3(0f, 12f, -10f);
            cameraObject.transform.rotation = Quaternion.Euler(48f, 0f, 0f);

            GameObject lightObject = new GameObject("Moon Key Light");
            Light key = lightObject.AddComponent<Light>();
            key.type = LightType.Directional;
            key.color = new Color(0.48f, 0.58f, 0.82f);
            key.intensity = 1.4f;
            key.shadows = LightShadows.Soft;
            lightObject.transform.rotation = Quaternion.Euler(48f, -35f, 0f);

            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.08f, 0.1f, 0.16f);
            RenderSettings.ambientEquatorColor = new Color(0.04f, 0.05f, 0.08f);
            RenderSettings.ambientGroundColor = new Color(0.015f, 0.018f, 0.025f);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };
            AssetDatabase.SaveAssets();
            Debug.Log($"Lanternfall combat sandbox built at {ScenePath}");
        }

        public static void BuildFromCommandLine()
        {
            Build();
        }

        public static void BuildWindowsDevelopment()
        {
            BuildPlayerOptions options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/Windows/Lanternfall.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    $"Windows build failed: {report.summary.result}");
            }
            Debug.Log(
                $"Lanternfall Windows smoke build succeeded: " +
                $"{report.summary.totalSize} bytes");
        }

        private static void ConfigureUrp()
        {
            const string rendererPath =
                "Assets/_Project/Lanternfall/Settings/LanternfallRenderer.asset";
            const string pipelinePath =
                "Assets/_Project/Lanternfall/Settings/LanternfallPipeline.asset";

            UniversalRendererData renderer =
                AssetDatabase.LoadAssetAtPath<UniversalRendererData>(rendererPath);
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, rendererPath);
            }

            UniversalRenderPipelineAsset pipeline =
                AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(pipelinePath);
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                AssetDatabase.CreateAsset(pipeline, pipelinePath);
            }

            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            EditorUtility.SetDirty(pipeline);
        }

        private static void EnsureFolders()
        {
            Directory.CreateDirectory("Assets/_Project/Lanternfall/Scenes");
            Directory.CreateDirectory("Assets/_Project/Lanternfall/Settings");
            Directory.CreateDirectory("Assets/_Project/Lanternfall/Art/Materials");
            AssetDatabase.Refresh();
        }

        private static Material CreateMaterial(string name, Color color, float smoothness)
        {
            string path = $"Assets/_Project/Lanternfall/Art/Materials/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                Shader shader = Shader.Find("Universal Render Pipeline/Lit");
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            material.SetColor("_BaseColor", color);
            material.SetFloat("_Smoothness", smoothness);
            EditorUtility.SetDirty(material);
            return material;
        }

        private static WeaponDefinition CreateWeapon(
            string id,
            string title,
            float damage,
            float rate,
            float speed,
            float knockback,
            float crit,
            DamageElement element)
        {
            string filename = id.Replace('.', '_');
            string path = $"Assets/_Project/Lanternfall/Settings/{filename}.asset";
            WeaponDefinition definition =
                AssetDatabase.LoadAssetAtPath<WeaponDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<WeaponDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }
            definition.Configure(id, title, damage, rate, speed, knockback, crit, element);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static AbilityDefinition CreateAbility(
            string id,
            AbilityKind kind,
            float damage,
            float radius,
            float cooldown,
            DamageElement element)
        {
            string filename = id.Replace('.', '_');
            string path = $"Assets/_Project/Lanternfall/Settings/{filename}.asset";
            AbilityDefinition definition =
                AssetDatabase.LoadAssetAtPath<AbilityDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<AbilityDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }
            definition.Configure(id, kind, damage, radius, cooldown, element);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static Projectile CreateProjectilePrefab(Material material)
        {
            const string path =
                "Assets/_Project/Lanternfall/Art/LanternProjectile.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<Projectile>();

            GameObject source = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            source.name = "Lantern Projectile";
            source.transform.localScale = Vector3.one * 0.3f;
            source.GetComponent<Renderer>().sharedMaterial = material;
            SphereCollider collider = source.GetComponent<SphereCollider>();
            collider.enabled = false;
            source.AddComponent<Projectile>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab.GetComponent<Projectile>();
        }

        private static void CreateDummy(
            string name,
            Vector3 position,
            Material material)
        {
            GameObject dummy = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            dummy.name = name;
            dummy.transform.position = position;
            dummy.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            dummy.GetComponent<Renderer>().sharedMaterial = material;
            Health health = dummy.AddComponent<Health>();
            health.Configure(100f, 15f, false);
        }

        private static EnemyDefinition[] CreateEnemyRoster()
        {
            return new[]
            {
                CreateEnemy("enemy.rubble_claw", EnemyArchetype.Melee, 45, 0, 3.8f, 12, 1.7f, .45f, .65f),
                CreateEnemy("enemy.thorn_scribe", EnemyArchetype.Archer, 35, 0, 3.2f, 10, 6f, .75f, .9f),
                CreateEnemy("enemy.bell_bastion", EnemyArchetype.Tank, 120, 25, 1.8f, 22, 2f, .9f, 1.1f),
                CreateEnemy("enemy.mist_ray", EnemyArchetype.Flying, 32, 0, 4.5f, 9, 2.2f, .4f, .55f),
                CreateEnemy("enemy.underling", EnemyArchetype.Burrowing, 55, 8, 3.1f, 16, 1.8f, .65f, .8f),
                CreateEnemy("enemy.cinder_husk", EnemyArchetype.Explosive, 30, 0, 4.2f, 30, 2.4f, .8f, .2f),
                CreateEnemy("enemy.choir_warden", EnemyArchetype.Summoner, 65, 5, 2.4f, 8, 5.5f, 1.1f, 1.4f),
                CreateEnemy("enemy.gloam_needle", EnemyArchetype.Assassin, 38, 0, 5.3f, 18, 1.6f, .3f, .55f)
            };
        }

        private static EnemyDefinition CreateEnemy(
            string id,
            EnemyArchetype archetype,
            float health,
            float armor,
            float speed,
            float damage,
            float range,
            float windup,
            float recovery)
        {
            string path =
                $"Assets/_Project/Lanternfall/Settings/{id.Replace('.', '_')}.asset";
            EnemyDefinition definition =
                AssetDatabase.LoadAssetAtPath<EnemyDefinition>(path);
            if (definition == null)
            {
                definition = ScriptableObject.CreateInstance<EnemyDefinition>();
                AssetDatabase.CreateAsset(definition, path);
            }
            definition.Configure(
                id, archetype, health, armor, speed, damage, range, windup, recovery);
            EditorUtility.SetDirty(definition);
            return definition;
        }

        private static EnemyBrain CreateEnemyPrefab(Material material)
        {
            const string path = "Assets/_Project/Lanternfall/Art/EnemyBase.prefab";
            GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null) return existing.GetComponent<EnemyBrain>();

            GameObject source = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            source.name = "Enemy Base";
            source.GetComponent<Renderer>().sharedMaterial = material;
            Object.DestroyImmediate(source.GetComponent<CapsuleCollider>());
            CharacterController controller = source.AddComponent<CharacterController>();
            controller.height = 2f;
            controller.radius = .5f;
            source.AddComponent<Health>();
            source.AddComponent<EnemyBrain>();
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(source, path);
            Object.DestroyImmediate(source);
            return prefab.GetComponent<EnemyBrain>();
        }

        private static GameObject CreateBlock(
            string name,
            Vector3 position,
            Vector3 scale,
            Material material)
        {
            GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
            block.name = name;
            block.transform.SetPositionAndRotation(position, Quaternion.identity);
            block.transform.localScale = scale;
            block.GetComponent<Renderer>().sharedMaterial = material;
            return block;
        }
    }
}
