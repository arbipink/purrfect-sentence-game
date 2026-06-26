using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SimpleVFXManager : MonoBehaviour
{
    public static SimpleVFXManager Instance { get; private set; }

    [Header("Prefab Effects")]
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private GameObject correctAnswerEffectPrefab;
    [SerializeField] private GameObject enemyDefeatedEffectPrefab;
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private GameObject wrongAnswerEffectPrefab;
    [SerializeField] private GameObject levelCompleteEffectPrefab;

    [Header("Fallback Slash Trail")]
    [SerializeField] private Color slashTrailColor = new Color(1f, 0.92f, 0.35f, 1f);
    [SerializeField] private float slashTrailWidth = 0.08f;
    [SerializeField] private float slashTrailDuration = 0.18f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void PlaySlash(Vector3 position)
    {
        PlaySlash(position, position + Vector3.up * 0.8f + Vector3.right * 0.2f);
    }

    public void PlaySlash(Vector3 startPosition, Vector3 endPosition)
    {
        Vector3 direction = endPosition - startPosition;
        Vector3 effectPosition = startPosition + direction * 0.5f;
        Quaternion effectRotation = direction.sqrMagnitude > 0.0001f
            ? Quaternion.LookRotation(direction.normalized)
            : Quaternion.identity;

        if (slashEffectPrefab != null)
        {
            SpawnEffect(slashEffectPrefab, effectPosition, effectRotation, 2f);
            return;
        }

        StartCoroutine(PlayFallbackSlashTrail(startPosition, endPosition));
    }

    public void PlayCorrectAnswer(Vector3 position)
    {
        if (correctAnswerEffectPrefab != null)
        {
            SpawnEffect(correctAnswerEffectPrefab, position, Quaternion.identity, 2f);
            return;
        }

        SpawnFallbackParticles(position, new Color(1f, 0.92f, 0.25f, 1f), 28, 1.2f);
    }

    public void PlayEnemyDefeated(Vector3 position)
    {
        if (enemyDefeatedEffectPrefab != null)
        {
            SpawnEffect(enemyDefeatedEffectPrefab, position, Quaternion.identity, 2f);
            return;
        }

        SpawnFallbackParticles(position, new Color(1f, 0.45f, 0.2f, 1f), 22, 1.1f);
    }

    public void PlayDamage(Vector3 position, GameObject target = null)
    {
        if (damageEffectPrefab != null)
        {
            SpawnEffect(damageEffectPrefab, position, Quaternion.identity, 1.5f);
        }

        StartCoroutine(PlayDamageFlash());
    }

    public void PlayWrongAnswer(Transform target = null)
    {
        Vector3 position = target != null ? target.position : Vector3.zero;

        if (wrongAnswerEffectPrefab != null)
        {
            SpawnEffect(wrongAnswerEffectPrefab, position, Quaternion.identity, 1.5f);
        }

        Transform shakeTarget = target != null ? target : Camera.main != null ? Camera.main.transform : null;
        if (shakeTarget != null)
        {
            StartCoroutine(ShakeTransform(shakeTarget, 0.16f, 0.08f));
        }
    }

    public void PlayLevelComplete(Vector3 position)
    {
        if (levelCompleteEffectPrefab != null)
        {
            SpawnEffect(levelCompleteEffectPrefab, position, Quaternion.identity, 2.5f);
            return;
        }

        SpawnFallbackParticles(position, new Color(0.35f, 1f, 0.65f, 1f), 40, 1.6f);
    }

    private void SpawnEffect(GameObject prefab, Vector3 position, Quaternion rotation, float fallbackLifetime)
    {
        if (prefab == null)
        {
            return;
        }

        GameObject instance = Instantiate(prefab, position, rotation);
        Destroy(instance, GetEffectLifetime(instance, fallbackLifetime));
    }

    private float GetEffectLifetime(GameObject instance, float fallbackLifetime)
    {
        if (instance == null)
        {
            return fallbackLifetime;
        }

        float lifetime = fallbackLifetime;
        ParticleSystem[] particleSystems = instance.GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem particleSystem in particleSystems)
        {
            if (particleSystem == null)
            {
                continue;
            }

            ParticleSystem.MainModule main = particleSystem.main;
            if (!main.loop)
            {
                lifetime = Mathf.Max(lifetime, main.duration + main.startLifetime.constantMax);
            }
        }

        return lifetime;
    }

    private IEnumerator PlayFallbackSlashTrail(Vector3 startPosition, Vector3 endPosition)
    {
        GameObject trailObject = new GameObject("Fallback Slash Trail");
        LineRenderer lineRenderer = trailObject.AddComponent<LineRenderer>();
        Material trailMaterial = null;

        Shader shader = Shader.Find("Sprites/Default");
        if (shader != null)
        {
            trailMaterial = new Material(shader);
            lineRenderer.material = trailMaterial;
        }

        lineRenderer.positionCount = 2;
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = slashTrailWidth;
        lineRenderer.endWidth = 0f;
        lineRenderer.startColor = slashTrailColor;
        lineRenderer.endColor = new Color(slashTrailColor.r, slashTrailColor.g, slashTrailColor.b, 0f);
        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, endPosition);

        yield return new WaitForSeconds(slashTrailDuration);

        Destroy(trailObject);
        if (trailMaterial != null)
        {
            Destroy(trailMaterial);
        }
    }

    private void SpawnFallbackParticles(Vector3 position, Color color, int count, float lifetime)
    {
        GameObject particlesObject = new GameObject("Fallback Sparkle VFX");
        particlesObject.transform.position = position;

        ParticleSystem particleSystem = particlesObject.AddComponent<ParticleSystem>();
        ParticleSystem.MainModule main = particleSystem.main;
        main.startLifetime = lifetime;
        main.startSpeed = 2.5f;
        main.startSize = 0.18f;
        main.startColor = color;
        main.loop = false;
        main.playOnAwake = false;

        ParticleSystem.EmissionModule emission = particleSystem.emission;
        emission.rateOverTime = 0f;

        ParticleSystem.ShapeModule shape = particleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        particleSystem.Emit(count);
        Destroy(particlesObject, lifetime + 0.5f);
    }

    private IEnumerator PlayDamageFlash()
    {
        GameObject canvasObject = new GameObject("Damage Flash Canvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32767;

        CanvasGroup canvasGroup = canvasObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        GameObject imageObject = new GameObject("Damage Flash");
        imageObject.transform.SetParent(canvasObject.transform, false);

        Image image = imageObject.AddComponent<Image>();
        image.color = new Color(1f, 0f, 0f, 0.28f);
        image.raycastTarget = false;

        RectTransform rectTransform = image.rectTransform;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        float duration = 0.22f;
        float elapsed = 0f;

        while (elapsed < duration && image != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0.28f, 0f, elapsed / duration);
            image.color = new Color(1f, 0f, 0f, alpha);
            yield return null;
        }

        Destroy(canvasObject);
    }

    private IEnumerator ShakeTransform(Transform target, float duration, float strength)
    {
        Vector3 originalLocalPosition = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration && target != null)
        {
            elapsed += Time.unscaledDeltaTime;
            Vector3 offset = Random.insideUnitSphere * strength;
            offset.z = 0f;
            target.localPosition = originalLocalPosition + offset;
            yield return null;
        }

        if (target != null)
        {
            target.localPosition = originalLocalPosition;
        }
    }
}
