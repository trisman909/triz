using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Combat;
using Lanternfall.Gameplay.Hub;
using Lanternfall.Gameplay.Player;
using UnityEngine;

namespace Lanternfall.Gameplay.Presentation
{
    [RequireComponent(typeof(CharacterController))]
    public sealed class PlayerPresentation : MonoBehaviour
    {
        private sealed class ClassStyle
        {
            public string StableId;
            public string Title;
            public Color Body;
            public Color Accent;
            public WeaponPose WeaponPose;
        }

        private enum WeaponPose
        {
            Guard,
            Bow,
            Staff,
            LanternBlade,
            TwinDaggers
        }

        private static readonly ClassStyle[] Styles =
        {
            new ClassStyle
            {
                StableId = "class.vanguard",
                Title = "Vanguard",
                Body = new Color(.58f, .67f, .78f),
                Accent = new Color(.95f, .78f, .35f),
                WeaponPose = WeaponPose.Guard
            },
            new ClassStyle
            {
                StableId = "class.gloamstep",
                Title = "Gloamstep",
                Body = new Color(.2f, .5f, .3f),
                Accent = new Color(.42f, .95f, .58f),
                WeaponPose = WeaponPose.TwinDaggers
            },
            new ClassStyle
            {
                StableId = "class.cantor",
                Title = "Cantor",
                Body = new Color(.44f, .28f, .66f),
                Accent = new Color(1f, .56f, .22f),
                WeaponPose = WeaponPose.Staff
            },
            new ClassStyle
            {
                StableId = "class.artificer",
                Title = "Artificer",
                Body = new Color(.78f, .74f, .65f),
                Accent = new Color(1f, .86f, .4f),
                WeaponPose = WeaponPose.LanternBlade
            },
            new ClassStyle
            {
                StableId = "class.wayfinder",
                Title = "Wayfinder",
                Body = new Color(.18f, .27f, .4f),
                Accent = new Color(.75f, .84f, 1f),
                WeaponPose = WeaponPose.Bow
            }
        };

        private PlayerMotor _motor;
        private PlayerCombat _combat;
        private CharacterController _controller;
        private Transform _root;
        private Transform _body;
        private Transform _lantern;
        private Transform _weapon;
        private TrailRenderer _trail;
        private ParticleSystem _stepDust;
        private ParticleSystem _dodgeBurst;
        private Renderer[] _renderers;
        private MaterialPropertyBlock _properties;
        private Vector3 _defaultRootPosition;
        private Quaternion _defaultBodyRotation;
        private Quaternion _defaultLanternRotation;
        private Quaternion _defaultWeaponRotation;
        private Vector3 _lastPosition;
        private float _bobTime;
        private float _dustTimer;
        private float _dodgeFeedbackTime;
        private string _classVisualId;
        private float _motionAmount;

        public string ClassVisualId => _classVisualId ?? "class.vanguard";
        public string ClassVisualTitle => ResolveStyle().Title;
        public float MotionAmount => _motionAmount;
        public bool DodgeFeedbackActive => _dodgeFeedbackTime > 0f;
        public bool HasFeedbackRig =>
            _root != null && _trail != null && _stepDust != null && _dodgeBurst != null;

        private void Awake()
        {
            _motor = GetComponent<PlayerMotor>();
            _combat = GetComponent<PlayerCombat>();
            _controller = GetComponent<CharacterController>();
            _properties = new MaterialPropertyBlock();
            RemoveRootPlaceholderBody();
            RemoveReviewOnlyBiomeAvatar();
            Build();
            RefreshClassVisual();
            _lastPosition = transform.position;
        }

        private void OnEnable()
        {
            if (_motor != null) _motor.Dodged += OnDodged;
            if (HubController.Instance != null)
                HubController.Instance.ClassSelected += OnClassSelected;
        }

        private void OnDisable()
        {
            if (_motor != null) _motor.Dodged -= OnDodged;
            if (HubController.Instance != null)
                HubController.Instance.ClassSelected -= OnClassSelected;
        }

        private void LateUpdate()
        {
            if (_root == null) return;
            if (ResolveClassVisualId() != _classVisualId)
                RefreshClassVisual();

            Vector3 planarVelocity = _motor != null
                ? _motor.CurrentPlanarVelocity
                : Vector3.ProjectOnPlane(
                    (transform.position - _lastPosition) /
                    Mathf.Max(Time.deltaTime, .0001f),
                    Vector3.up);
            _lastPosition = transform.position;
            float speed = planarVelocity.magnitude;
            float referenceSpeed = Mathf.Max(
                .1f,
                (_motor?.MoveSpeed ?? 6f) *
                (_motor?.SprintMultiplier ?? 1.35f));
            float speed01 = Mathf.Clamp01(speed / referenceSpeed);
            _motionAmount = speed01;

            float bobAmplitude = AccessibilityRuntime.ReducedMotion
                ? .015f
                : Mathf.Lerp(.02f, .08f, speed01);
            _bobTime += Time.deltaTime * Mathf.Lerp(2.2f, 9.5f, speed01);
            float bob = Mathf.Sin(_bobTime) * bobAmplitude;
            float sway = Mathf.Cos(_bobTime * .5f) *
                (AccessibilityRuntime.ReducedMotion ? .02f : .05f) * speed01;
            _root.localPosition = _defaultRootPosition +
                new Vector3(0f, bob, 0f);

            Vector3 localVelocity =
                transform.InverseTransformDirection(planarVelocity);
            float leanPitch = Mathf.Clamp(-localVelocity.z * 2.2f, -10f, 10f);
            float leanRoll = Mathf.Clamp(-localVelocity.x * 2.6f, -12f, 12f);
            if (_motor != null && _motor.IsDodging)
            {
                leanPitch += 12f;
                leanRoll += Mathf.Sign(leanRoll == 0f ? 1f : leanRoll) * 8f;
            }

            _body.localRotation = _defaultBodyRotation *
                Quaternion.Euler(
                    leanPitch,
                    0f,
                    leanRoll);
            _lantern.localRotation = _defaultLanternRotation *
                Quaternion.Euler(
                    -leanPitch * .4f + sway * 25f,
                    Mathf.Sin(_bobTime * .75f) * 6f * speed01,
                    -leanRoll * .35f);
            _weapon.localRotation = _defaultWeaponRotation *
                Quaternion.Euler(
                    leanPitch * .25f,
                    Mathf.Sin(_bobTime) * 4f * speed01,
                    leanRoll * .55f - sway * 30f);

            float squash =
                _motor != null && _motor.IsDodging && !AccessibilityRuntime.ReducedMotion
                    ? 1f + Mathf.Sin(_motor.DodgeCooldownRemaining * 18f) * .06f
                    : 1f;
            _root.localScale = new Vector3(
                1f - (squash - 1f) * .45f,
                squash,
                1f + (squash - 1f) * .32f);

            UpdateDust(speed01);
            UpdateDodgeFeedback(speed01);
        }

        private void Build()
        {
            _root = new GameObject("Bearer Presentation").transform;
            _root.SetParent(transform, false);
            _defaultRootPosition = new Vector3(0f, -.92f, 0f);
            _root.localPosition = _defaultRootPosition;

            _body = new GameObject("Body").transform;
            _body.SetParent(_root, false);
            BuildBodyGeometry();
            _defaultBodyRotation = _body.localRotation;

            _lantern = new GameObject("Lantern Rig").transform;
            _lantern.SetParent(_body, false);
            _lantern.localPosition = new Vector3(.34f, .94f, .16f);
            BuildLanternGeometry();
            _defaultLanternRotation = _lantern.localRotation;

            _weapon = new GameObject("Weapon Rig").transform;
            _weapon.SetParent(_body, false);
            _weapon.localPosition = new Vector3(-.34f, .9f, .08f);
            BuildWeaponGeometry();
            _defaultWeaponRotation = _weapon.localRotation;

            BuildTrail();
            BuildParticles();
            _renderers = _root.GetComponentsInChildren<Renderer>(true);
        }

        private void BuildBodyGeometry()
        {
            CreateShape(
                _body,
                PrimitiveType.Capsule,
                "Cloak Torso",
                new Vector3(0f, .86f, 0f),
                new Vector3(.75f, 1.1f, .68f),
                UrpPresentationMaterials.Lit);
            CreateShape(
                _body,
                PrimitiveType.Cube,
                "Shoulders",
                new Vector3(0f, 1.24f, 0f),
                new Vector3(.92f, .18f, .4f),
                UrpPresentationMaterials.Lit);
            CreateShape(
                _body,
                PrimitiveType.Sphere,
                "Hood",
                new Vector3(0f, 1.38f, .02f),
                new Vector3(.42f, .48f, .42f),
                UrpPresentationMaterials.Lit);
            CreateShape(
                _body,
                PrimitiveType.Cylinder,
                "Hem",
                new Vector3(0f, .24f, 0f),
                new Vector3(.9f, .18f, .9f),
                UrpPresentationMaterials.Lit);
        }

        private void BuildLanternGeometry()
        {
            CreateShape(
                _lantern,
                PrimitiveType.Cylinder,
                "Lantern Handle",
                new Vector3(0f, .06f, 0f),
                new Vector3(.06f, .2f, .06f),
                UrpPresentationMaterials.Emissive);
            CreateShape(
                _lantern,
                PrimitiveType.Cube,
                "Lantern Cage",
                new Vector3(0f, -.12f, 0f),
                new Vector3(.22f, .28f, .22f),
                UrpPresentationMaterials.Emissive);
            CreateShape(
                _lantern,
                PrimitiveType.Sphere,
                "Lantern Core",
                new Vector3(0f, -.12f, 0f),
                new Vector3(.12f, .12f, .12f),
                UrpPresentationMaterials.Transparent);
        }

        private void BuildWeaponGeometry()
        {
            CreateShape(
                _weapon,
                PrimitiveType.Cylinder,
                "Grip",
                new Vector3(0f, .02f, 0f),
                new Vector3(.06f, .24f, .06f),
                UrpPresentationMaterials.Emissive);
            CreateShape(
                _weapon,
                PrimitiveType.Cube,
                "Weapon Silhouette",
                new Vector3(0f, .32f, .06f),
                new Vector3(.12f, .48f, .14f),
                UrpPresentationMaterials.Lit);
        }

        private void BuildTrail()
        {
            _trail = _root.gameObject.AddComponent<TrailRenderer>();
            _trail.sharedMaterial = UrpPresentationMaterials.Transparent;
            _trail.time = .16f;
            _trail.startWidth = .52f;
            _trail.endWidth = .08f;
            _trail.minVertexDistance = .05f;
            _trail.autodestruct = false;
            _trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            _trail.receiveShadows = false;
            _trail.emitting = false;
        }

        private void BuildParticles()
        {
            _stepDust = CreateParticles(
                "Footstep Dust",
                new Color(.72f, .72f, .72f, .42f),
                .42f,
                .35f);
            _dodgeBurst = CreateParticles(
                "Dodge Burst",
                new Color(.4f, .86f, 1f, .8f),
                .62f,
                .22f);
        }

        private ParticleSystem CreateParticles(
            string name,
            Color color,
            float size,
            float lifetime)
        {
            GameObject particleObject = new GameObject(name);
            particleObject.transform.SetParent(_root, false);
            particleObject.transform.localPosition = new Vector3(0f, .08f, 0f);
            ParticleSystem system = particleObject.AddComponent<ParticleSystem>();
            var main = system.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = lifetime;
            main.startSpeed = .9f;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 24;
            var emission = system.emission;
            emission.enabled = false;
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.radius = .12f;
            shape.angle = 22f;
            var velocity = system.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.World;
            velocity.x = new ParticleSystem.MinMaxCurve(0f);
            velocity.y = new ParticleSystem.MinMaxCurve(.76f);
            velocity.z = new ParticleSystem.MinMaxCurve(0f);
            var colorOverLifetime = system.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new[]
                {
                    new GradientColorKey(color, 0f),
                    new GradientColorKey(color * .7f, 1f)
                },
                new[]
                {
                    new GradientAlphaKey(color.a, 0f),
                    new GradientAlphaKey(0f, 1f)
                });
            colorOverLifetime.color = gradient;
            ParticleSystemRenderer renderer =
                system.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = UrpPresentationMaterials.Particle;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.Distance;
            return system;
        }

        private void UpdateDust(float speed01)
        {
            if (_stepDust == null || !_controller.isGrounded) return;
            _dustTimer -= Time.deltaTime;
            if (speed01 < .16f || _dustTimer > 0f) return;
            _dustTimer = Mathf.Lerp(.42f, .16f, speed01);
            int burst = AccessibilityRuntime.ReducedMotion ? 2 : 4;
            _stepDust.transform.position =
                transform.position + Vector3.up * .06f;
            _stepDust.Emit(burst);
        }

        private void UpdateDodgeFeedback(float speed01)
        {
            if (_trail == null) return;
            if (_motor != null && _motor.IsDodging)
            {
                _dodgeFeedbackTime = .18f;
                _trail.emitting = true;
                SetEmissivePulse(1.7f);
                return;
            }

            _dodgeFeedbackTime = Mathf.Max(0f, _dodgeFeedbackTime - Time.deltaTime);
            _trail.emitting = _dodgeFeedbackTime > 0f;
            SetEmissivePulse(Mathf.Lerp(1f, 1.15f, speed01));
        }

        private void SetEmissivePulse(float intensity)
        {
            if (_renderers == null) return;
            foreach (Renderer renderer in _renderers)
            {
                renderer.GetPropertyBlock(_properties);
                if (renderer.sharedMaterial == UrpPresentationMaterials.Emissive ||
                    renderer.sharedMaterial == UrpPresentationMaterials.Transparent)
                    _properties.SetColor(
                        "_EmissionColor",
                        ResolveStyle().Accent * intensity);
                renderer.SetPropertyBlock(_properties);
            }
        }

        private void OnDodged()
        {
            _dodgeFeedbackTime = .22f;
            if (_dodgeBurst != null)
            {
                _dodgeBurst.transform.position =
                    transform.position + Vector3.up * .16f;
                _dodgeBurst.Emit(AccessibilityRuntime.ReducedMotion ? 5 : 9);
            }
        }

        private void OnClassSelected(string _) => RefreshClassVisual();

        private void RemoveRootPlaceholderBody()
        {
            Renderer rootRenderer = GetComponent<Renderer>();
            if (rootRenderer != null)
                Destroy(rootRenderer);
            MeshFilter rootMesh = GetComponent<MeshFilter>();
            if (rootMesh != null)
                Destroy(rootMesh);
        }

        private void RefreshClassVisual()
        {
            _classVisualId = ResolveClassVisualId();
            ClassStyle style = ResolveStyle();
            ApplyPalette(style);
            ApplyWeaponPose(style.WeaponPose);
        }

        private string ResolveClassVisualId()
        {
            if (HubController.Instance?.ActiveRun != null &&
                !string.IsNullOrWhiteSpace(HubController.Instance.ActiveRun.ClassId))
                return HubController.Instance.ActiveRun.ClassId;
            if (HubController.Instance != null &&
                !string.IsNullOrWhiteSpace(HubController.Instance.SelectedClassId))
                return HubController.Instance.SelectedClassId;
            return "class.vanguard";
        }

        private ClassStyle ResolveStyle()
        {
            foreach (ClassStyle style in Styles)
                if (style.StableId == ClassVisualId)
                    return style;
            return Styles[0];
        }

        private void ApplyPalette(ClassStyle style)
        {
            if (_renderers == null) return;
            foreach (Renderer renderer in _renderers)
            {
                renderer.GetPropertyBlock(_properties);
                Color baseColor =
                    renderer.name.Contains("Lantern") ||
                    renderer.name.Contains("Grip")
                        ? style.Accent
                        : style.Body;
                if (renderer.name.Contains("Core"))
                    baseColor = Color.Lerp(style.Accent, Color.white, .22f);
                _properties.SetColor("_BaseColor", baseColor);
                _properties.SetColor("_Color", baseColor);
                _properties.SetColor(
                    "_EmissionColor",
                    (renderer.name.Contains("Lantern") || renderer.name.Contains("Grip"))
                        ? style.Accent * 1.25f
                        : style.Body * .18f);
                renderer.SetPropertyBlock(_properties);
            }
        }

        private void ApplyWeaponPose(WeaponPose pose)
        {
            if (_weapon == null) return;
            _weapon.localPosition = new Vector3(-.34f, .9f, .08f);
            _defaultWeaponRotation = Quaternion.Euler(18f, -8f, 10f);
            Transform silhouette = _weapon.Find("Weapon Silhouette");
            if (silhouette == null) return;
            switch (pose)
            {
                case WeaponPose.Guard:
                    silhouette.localScale = new Vector3(.18f, .65f, .2f);
                    _weapon.localPosition = new Vector3(-.44f, .84f, .02f);
                    _defaultWeaponRotation = Quaternion.Euler(8f, -24f, 18f);
                    break;
                case WeaponPose.Bow:
                    silhouette.localScale = new Vector3(.08f, .74f, .44f);
                    _weapon.localPosition = new Vector3(-.42f, .88f, -.04f);
                    _defaultWeaponRotation = Quaternion.Euler(4f, 8f, 62f);
                    break;
                case WeaponPose.Staff:
                    silhouette.localScale = new Vector3(.09f, .92f, .09f);
                    _weapon.localPosition = new Vector3(-.36f, .88f, .08f);
                    _defaultWeaponRotation = Quaternion.Euler(-10f, 0f, 6f);
                    break;
                case WeaponPose.LanternBlade:
                    silhouette.localScale = new Vector3(.1f, .78f, .18f);
                    _weapon.localPosition = new Vector3(-.38f, .86f, .12f);
                    _defaultWeaponRotation = Quaternion.Euler(16f, 16f, 28f);
                    break;
                case WeaponPose.TwinDaggers:
                    silhouette.localScale = new Vector3(.08f, .42f, .16f);
                    _weapon.localPosition = new Vector3(-.28f, .78f, .2f);
                    _defaultWeaponRotation = Quaternion.Euler(36f, -4f, 42f);
                    break;
            }
            _weapon.localRotation = _defaultWeaponRotation;
        }

        private void RemoveReviewOnlyBiomeAvatar()
        {
            Transform legacy = transform.Find("Production Bearer Model");
            if (legacy == null) return;
            if (Application.isPlaying) Destroy(legacy.gameObject);
            else DestroyImmediate(legacy.gameObject);
        }

        private static void CreateShape(
            Transform parent,
            PrimitiveType type,
            string name,
            Vector3 localPosition,
            Vector3 localScale,
            Material material)
        {
            GameObject shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = localPosition;
            shape.transform.localScale = localScale;
            shape.GetComponent<Renderer>().sharedMaterial = material;
            Collider collider = shape.GetComponent<Collider>();
            if (collider != null)
            {
                if (Application.isPlaying) Destroy(collider);
                else DestroyImmediate(collider);
            }
        }
    }
}
