using UnityEngine;

namespace Lanternfall.Gameplay.Bosses
{
    /// <summary>
    /// Presentation-review-only loop for guardian VFX. It has no combat,
    /// health, damage, or encounter authority.
    /// </summary>
    [RequireComponent(typeof(GuardianSpectaclePresenter))]
    public sealed class GuardianReviewShowcase : MonoBehaviour
    {
        [SerializeField] private BossAttackPattern pattern;
        [SerializeField] private Color accent = Color.cyan;
        private GuardianSpectaclePresenter _spectacle;
        private float _timer = 1.2f;
        private int _phase;

        public void Configure(BossAttackPattern attackPattern, Color color)
        {
            pattern = attackPattern;
            accent = color;
        }

        private void Start()
        {
            _spectacle = GetComponent<GuardianSpectaclePresenter>();
            _spectacle.Configure(pattern, accent);
        }

        private void Update()
        {
            _timer -= Time.deltaTime;
            if (_timer > 0f || _spectacle == null) return;
            _phase = _phase % 3 + 1;
            _spectacle.Play(_phase);
            _timer = 3.2f;
        }
    }
}
