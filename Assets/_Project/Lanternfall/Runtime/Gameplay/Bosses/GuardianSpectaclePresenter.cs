using Lanternfall.Gameplay.Accessibility;
using Lanternfall.Gameplay.Camera;
using Lanternfall.Gameplay.Presentation;
using UnityEngine;

namespace Lanternfall.Gameplay.Bosses
{
    /// <summary>
    /// Accessible guardian pattern spectacle made from lines, light, and a
    /// bounded particle burst. It never owns damage or encounter state.
    /// </summary>
    public sealed class GuardianSpectaclePresenter : MonoBehaviour
    {
        private BossAttackPattern _pattern;
        private Color _accent = new Color(1f, .35f, .08f);
        private LineRenderer _glyph;
        private ParticleSystem _particles;
        private Light _pulseLight;
        private float _visibleTime;

        public BossAttackPattern Pattern => _pattern;
        public int BurstCount { get; private set; }

        private void Awake()
        {
            GameObject glyphObject = new GameObject("Guardian Pattern Glyph");
            glyphObject.transform.SetParent(transform, false);
            glyphObject.transform.localPosition = Vector3.up * .06f;
            _glyph = glyphObject.AddComponent<LineRenderer>();
            _glyph.useWorldSpace = false;
            _glyph.loop = false;
            _glyph.widthMultiplier = .11f;
            _glyph.numCapVertices = 2;
            _glyph.numCornerVertices = 2;
            _glyph.shadowCastingMode =
                UnityEngine.Rendering.ShadowCastingMode.Off;
            _glyph.receiveShadows = false;
            _glyph.sharedMaterial = UrpPresentationMaterials.Transparent;
            _glyph.enabled = false;

            GameObject particleObject =
                new GameObject("Guardian Pattern Sparks");
            particleObject.transform.SetParent(transform, false);
            _particles = particleObject.AddComponent<ParticleSystem>();
            ParticleSystem.MainModule main = _particles.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = .7f;
            main.startSpeed = 4f;
            main.startSize = .09f;
            main.maxParticles = 96;
            ParticleSystem.EmissionModule emission = _particles.emission;
            emission.enabled = false;
            ParticleSystem.ShapeModule shape = _particles.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 1.2f;
            ParticleSystemRenderer particleRenderer =
                particleObject.GetComponent<ParticleSystemRenderer>();
            particleRenderer.sharedMaterial =
                UrpPresentationMaterials.Particle;

            _pulseLight = gameObject.AddComponent<Light>();
            _pulseLight.type = LightType.Point;
            _pulseLight.range = 9f;
            _pulseLight.intensity = 0f;
            _pulseLight.shadows = LightShadows.None;
        }

        private void Update()
        {
            if (_visibleTime <= 0f) return;
            _visibleTime -= Time.deltaTime;
            float normalized = Mathf.Clamp01(_visibleTime / .7f);
            if (_glyph != null)
            {
                Color color = TelegraphColor();
                color.a = normalized *
                    AccessibilityRuntime.FlashIntensity;
                _glyph.startColor = color;
                _glyph.endColor = color;
                _glyph.widthMultiplier =
                    AccessibilityRuntime.ReducedMotion
                        ? .12f
                        : .1f + Mathf.Sin(Time.time * 16f) * .035f;
                if (_visibleTime <= 0f) _glyph.enabled = false;
            }
            if (_pulseLight != null)
                _pulseLight.intensity =
                    normalized * 5f * AccessibilityRuntime.FlashIntensity;
        }

        public void Configure(BossAttackPattern pattern, Color accent)
        {
            _pattern = pattern;
            _accent = accent;
            if (_pulseLight != null) _pulseLight.color = TelegraphColor();
            if (_particles != null)
            {
                ParticleSystem.MainModule main = _particles.main;
                main.startColor = TelegraphColor();
            }
        }

        public void Play(int phase)
        {
            if (_glyph == null) return;
            BuildGlyph(Mathf.Clamp(phase, 1, 3));
            _glyph.enabled = true;
            _visibleTime = .7f;
            if (!AccessibilityRuntime.ReducedMotion)
                _particles.Emit(18 + phase * 12);
            BurstCount++;
            IsometricCameraRig camera =
                FindAnyObjectByType<IsometricCameraRig>();
            camera?.Shake(.09f + phase * .025f, .18f);
        }

        private void BuildGlyph(int phase)
        {
            switch (_pattern)
            {
                case BossAttackPattern.PrismCharge:
                case BossAttackPattern.MirrorVolley:
                    BuildLanes(phase);
                    break;
                case BossAttackPattern.RootSummon:
                case BossAttackPattern.StonePillars:
                    BuildRadial(phase);
                    break;
                case BossAttackPattern.TimeFracture:
                    BuildBrokenDial(phase);
                    break;
                default:
                    BuildRing(phase);
                    break;
            }
        }

        private void BuildRing(int phase)
        {
            int points = 32 + phase * 8;
            _glyph.positionCount = points;
            _glyph.loop = true;
            float radius = 2.2f + phase * .7f;
            for (int index = 0; index < points; index++)
            {
                float angle = index * Mathf.PI * 2f / points;
                _glyph.SetPosition(index, new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius));
            }
        }

        private void BuildLanes(int phase)
        {
            int lanes = 3 + phase;
            _glyph.loop = false;
            _glyph.positionCount = lanes * 4;
            int cursor = 0;
            for (int lane = 0; lane < lanes; lane++)
            {
                float x = (lane - (lanes - 1) * .5f) * .75f;
                _glyph.SetPosition(cursor++, new Vector3(x - .16f, 0f, -5f));
                _glyph.SetPosition(cursor++, new Vector3(x - .16f, 0f, 5f));
                _glyph.SetPosition(cursor++, new Vector3(x + .16f, 0f, 5f));
                _glyph.SetPosition(cursor++, new Vector3(x + .16f, 0f, -5f));
            }
        }

        private void BuildRadial(int phase)
        {
            int spokes = 6 + phase * 2;
            _glyph.loop = false;
            _glyph.positionCount = spokes * 3;
            int cursor = 0;
            for (int index = 0; index < spokes; index++)
            {
                float angle = index * Mathf.PI * 2f / spokes;
                Vector3 direction = new Vector3(
                    Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                _glyph.SetPosition(cursor++, Vector3.zero);
                _glyph.SetPosition(cursor++, direction * (2.2f + phase * .5f));
                _glyph.SetPosition(cursor++, direction * (1.5f + phase * .4f) +
                    new Vector3(-direction.z, 0f, direction.x) * .55f);
            }
        }

        private void BuildBrokenDial(int phase)
        {
            int marks = 10 + phase * 2;
            _glyph.loop = false;
            _glyph.positionCount = marks * 2;
            for (int index = 0; index < marks; index++)
            {
                float angle = index * Mathf.PI * 2f / marks;
                Vector3 direction = new Vector3(
                    Mathf.Cos(angle), 0f, Mathf.Sin(angle));
                _glyph.SetPosition(index * 2, direction * 2.1f);
                _glyph.SetPosition(index * 2 + 1,
                    direction * (index % 3 == 0 ? 3.5f : 2.7f));
            }
        }

        private Color TelegraphColor() =>
            AccessibilityRuntime.HighContrastTelegraphs
                ? new Color(1f, .92f, .05f)
                : _accent;
    }
}
