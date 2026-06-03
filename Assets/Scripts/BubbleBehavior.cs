using UnityEngine;

public class BubbleBehavior : MonoBehaviour
{
    [Header("Bubble Settings")]
    public float lifeTime = 3f;
    public float upwardForce = 3f;
    public float floatDelay = 1f;
    public float damage = 10f;

    [Header("Audio")]
    public AudioClip bubblePopClip;
    [Range(0f, 1f)] public float bubblePopVolume = 1f;

    [Header("Particles")]
    public GameObject bubblePopParticles;
    public GameObject bubbleExplosion;

    private Rigidbody rb;
    private float timer = 0f;
    private float ignoreCollisionTime = 0.15f;
    private bool hasHit = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        if (rb != null) rb.useGravity = false;
    }

    void FixedUpdate()
    {
        timer += Time.fixedDeltaTime;

        if (timer > floatDelay && rb != null)
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Acceleration);

        if (timer >= lifeTime)
        {
            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }

            if (bubbleExplosion != null)
            {
                GameObject spawnedParticles = Instantiate(bubbleExplosion, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore player
        if (other.transform.root.CompareTag("Player"))
        {
            hasHit = false;
            return;
        }

        if (timer < ignoreCollisionTime || hasHit) return;

        hasHit = true;

        // === JELLYFISH ===
        if (other.CompareTag("Jellyfish"))
        {
            Jellyfish jelly = other.GetComponent<Jellyfish>();
            if (jelly != null)
                jelly.TakeDamage(damage);


            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }
            Destroy(gameObject);
            return;

        }
        // === STARFISH ===
        else if (other.CompareTag("Starfish"))
        {
            Starfish star = other.GetComponent<Starfish>();
            if (star != null)
                star.TakeDamage(damage);

            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }

            Destroy(gameObject);
            return;
        }
        // === LEVIATHAN ===
        else if (other.CompareTag("Leviathan"))
        {
            Debug.Log("Leviathan Hit");
            Leviathan levi = other.GetComponentInParent<Leviathan>();
            if (levi != null)
                levi.TakeBubbleDamage(damage);

            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            Destroy(gameObject);
            return;
        }
        // === URCHIN ===
        else if (other.GetComponent<BlackSeaUrchin>() != null || other.GetComponentInParent<BlackSeaUrchin>() != null)
        {
            BlackSeaUrchin urchin = other.GetComponentInParent<BlackSeaUrchin>();
            /*if (urchin != null)
                urchin.PlayBubbleHitSound();*/

            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            Destroy(gameObject);
            return;
        }

        // === SQUID SNIPER (NEW) ===
        else if (other.GetComponent<SquidSniper>() != null || other.GetComponentInParent<SquidSniper>() != null)
        {
            SquidSniper squid = other.GetComponentInParent<SquidSniper>();
            if (squid != null)
                squid.TakeDamage(damage);

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }

            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            Destroy(gameObject);
            return;
        }

        // === CRAB MONSTER BOSS ===
        else if (other.GetComponent<CrabMonsterBoss>() != null || other.GetComponentInParent<CrabMonsterBoss>() != null)
        {
            CrabMonsterBoss crabBoss = other.GetComponentInParent<CrabMonsterBoss>();
            if (crabBoss != null)
                crabBoss.TakeDamage(damage);

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }

            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            Destroy(gameObject);
            return;
        }

        // === CRAB MONSTER ===
        else if (other.GetComponent<CrabMonsterEnemy>() != null || other.GetComponentInParent<CrabMonsterEnemy>() != null)
        {
            CrabMonsterEnemy crab = other.GetComponentInParent<CrabMonsterEnemy>();
            if (crab != null)
                crab.TakeDamage(damage);

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }

            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            Destroy(gameObject);
            return;
        }// === JELLYBOSS ===
        else if (other.CompareTag("JellyBoss"))
        {
            JellyBoss jelly = other.GetComponent<JellyBoss>();
            if (jelly != null)
                jelly.TakeDamage(damage);


            if (bubblePopParticles != null)
            {
                GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

                // 2. Try to get the Particle System component from the clone
                ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

                if (ps != null)
                {
                    // 3. Calculate how long it takes to play completely
                    float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                    // 4. Destroy the cloned particle object after that exact delay
                    Destroy(spawnedParticles, totalDuration);
                }
                else
                {
                    // Fallback: If no system is found, delete the clone after 3 seconds anyway
                    Destroy(spawnedParticles, 3.0f);
                }
            }

            if (bubblePopClip != null)
            {
                AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
            }
            Destroy(gameObject);
            return;

        }

        // Destroy on walls / environment
        if (bubblePopClip != null)
        {
            AudioSource.PlayClipAtPoint(bubblePopClip, transform.position, bubblePopVolume);
        }

        if (bubblePopParticles != null)
        {
            GameObject spawnedParticles = Instantiate(bubblePopParticles, transform.position, Quaternion.identity);

            // 2. Try to get the Particle System component from the clone
            ParticleSystem ps = spawnedParticles.GetComponent<ParticleSystem>();

            if (ps != null)
            {
                // 3. Calculate how long it takes to play completely
                float totalDuration = ps.main.duration + ps.main.startLifetime.constantMax;

                // 4. Destroy the cloned particle object after that exact delay
                Destroy(spawnedParticles, totalDuration);
            }
            else
            {
                // Fallback: If no system is found, delete the clone after 3 seconds anyway
                Destroy(spawnedParticles, 3.0f);
            }
        }
        Destroy(gameObject);
    }
}
