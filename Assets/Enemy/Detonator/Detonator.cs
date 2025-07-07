using System.Collections;
using UnityEngine;

public class Detonator : MonoBehaviour
{
    public float speed = 2f;
    public int health = 50;
    public float wanderInterval = 3f;
    public float moveToPlayerInterval = 4f;
    public float followPlayerDuration = 4f;
    public float explosionDistance = 1.5f;
    public GameObject explosionPrefab;
    public float explosionDelay = 2f;
    private Rigidbody2D rb;

    [Header("Explosion Audio")]
    public AudioClip explodeAudioClip;
    public float minPitch = 0.95f;
    public float maxPitch = 1.15f;

    private Transform player;
    private Collider2D areaBoundary;
    private Vector2 targetPosition;
    private bool isMoving = false;
    private bool followingPlayer = false;
    private bool isExploding = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D myCollider;
    private AudioSource audioSource;

    private float collisionCheckDistance = 0.1f;
    private ContactFilter2D wallFilter;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        areaBoundary = transform.parent.GetComponent<Collider2D>();
        if (areaBoundary == null)
        {
            Debug.LogError("Detonator's parent (SpawnAnchor) does not have a Collider2D component!");
            return;
        }

        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player object not found! Make sure the Player is tagged correctly.");
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer not found! Ensure the detonator has one.");
        }

        myCollider = GetComponent<Collider2D>();
        if (myCollider == null)
        {
            Debug.LogError("Detonator missing Collider2D!");
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        wallFilter = new ContactFilter2D();
        wallFilter.SetLayerMask(LayerMask.GetMask("Wall"));
        wallFilter.useLayerMask = true;

        SetRandomTargetPosition();
        StartCoroutine(MoveToPlayerEveryInterval());
    }

    void Update()
    {
        if (isExploding)
            return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= explosionDistance && !isExploding)
        {
            StartCoroutine(StartExplosionSequence());
        }
        else if (followingPlayer)
        {
            MoveTowardsPlayer();
        }
        else if (isMoving)
        {
            MoveTowardsTarget();
        }
    }

    void SetRandomTargetPosition()
    {
        if (areaBoundary != null)
        {
            Bounds bounds = areaBoundary.bounds;
            float randomX = Random.Range(bounds.min.x, bounds.max.x);
            float randomY = Random.Range(bounds.min.y, bounds.max.y);
            targetPosition = new Vector2(randomX, randomY);
            isMoving = true;
        }
    }

    void MoveTowardsTarget()
    {
        Vector2 currentPosition = transform.position;
        Vector2 direction = (targetPosition - currentPosition).normalized;

        if (!IsPathBlocked(direction))
        {
            transform.position = Vector2.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
        }
        else
        {
            isMoving = false;
        }

        if (Vector2.Distance(currentPosition, targetPosition) < 0.1f)
        {
            isMoving = false;
        }
    }

    IEnumerator MoveToPlayerEveryInterval()
    {
        while (true)
        {
            yield return new WaitForSeconds(moveToPlayerInterval);

            if (!isExploding && !followingPlayer)
            {
                StartCoroutine(MoveTowardPlayerForDuration());
            }
        }
    }

    IEnumerator MoveTowardPlayerForDuration()
    {
        followingPlayer = true;
        float elapsedTime = 0f;

        while (elapsedTime < followPlayerDuration)
        {
            elapsedTime += Time.deltaTime;
            if (player != null && !isExploding)
            {
                MoveTowardsPlayer();
            }
            yield return null;
        }

        followingPlayer = false;
        SetRandomTargetPosition();
    }

    void MoveTowardsPlayer()
    {
        Vector2 playerDirection = (player.position - transform.position).normalized;
        if (!IsPathBlocked(playerDirection))
        {
            transform.position += (Vector3)(playerDirection * speed * Time.deltaTime);
        }
    }

    bool IsPathBlocked(Vector2 direction)
    {
        RaycastHit2D[] hits = new RaycastHit2D[1];
        int hitCount = Physics2D.Raycast(transform.position, direction, wallFilter, hits, collisionCheckDistance);
        Debug.DrawRay(transform.position, direction * collisionCheckDistance, Color.red);

        return hitCount > 0;
    }

    IEnumerator StartExplosionSequence()
    {
        isExploding = true;
        isMoving = false;
        followingPlayer = false;

        if (explodeAudioClip != null && audioSource != null)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
            audioSource.PlayOneShot(explodeAudioClip);
            audioSource.pitch = 1f;
        }

        float elapsedTime = 0f;
        bool toggleColor = false;

        while (elapsedTime < explosionDelay)
        {
            toggleColor = !toggleColor;
            spriteRenderer.color = toggleColor ? Color.red : Color.white;
            yield return new WaitForSeconds(0.1f);
            elapsedTime += 0.1f;
        }

        spriteRenderer.color = Color.white;

        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }

        // Disable collider and script; destroy immediately (NO fade-out)
        if (myCollider != null) myCollider.enabled = false;
        this.enabled = false;
        Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isExploding)
            return;
        if (collision.gameObject.CompareTag("Bullet"))
        {
            MonoBehaviour[] components = collision.gameObject.GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour component in components)
            {
                System.Reflection.FieldInfo damageField = component.GetType().GetField("damage");
                if (damageField != null)
                {
                    int bulletDamage = (int)damageField.GetValue(component);
                    TakeDamage(bulletDamage);
                    break;
                }
            }
            Destroy(collision.gameObject);
        }
    }

    public void TakeDamage(int damageAmount)
    {
        if (isExploding) return;
        health -= damageAmount;
        Debug.Log("Detonator took damage! Remaining Health: " + health);

        StartCoroutine(DarkenEffect());

        if (health <= 0)
        {
            Die();
        }
    }

    private IEnumerator DarkenEffect()
    {
        spriteRenderer.color = new Color(0.5f, 0.5f, 0.5f, 1f);
        yield return new WaitForSeconds(0.2f);
        spriteRenderer.color = Color.white;
    }

    void Die()
    {
        Debug.Log("Detonator has died!");
        isMoving = false;
        followingPlayer = false;
        isExploding = true;
        if (myCollider != null) myCollider.enabled = false;
        this.enabled = false;
        StartCoroutine(FadeOutAndDestroy(0.4f));
    }

    IEnumerator FadeOutAndDestroy(float fadeDuration)
    {
        if (spriteRenderer == null) { Destroy(gameObject); yield break; }
        Color baseColor = spriteRenderer.color;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            yield return null;
        }
        Destroy(gameObject);
    }
}