using UnityEngine;
using System.Collections;

public class AIWayfinder : MonoBehaviour
{
    public enum UnitType { Basic = 0, Fast = 1, Dangerous = 2, Boss = 3 };

    // Unit Information
    public UnitType unitType;
    public string unitName;

    // Navigation
    // Target Waypoint that can be modified
    public GameObject wayPoint;
    private Transform target;
    public WayPointBehaviour targetBehaviour;
    public float moveSpeed = 5;
    public float rotationSpeed = 3;
    public float targetReachThreshold = 0.4f;
    private Transform myTransform;
    public float moveSpeedMultiplier = 1;
    public float moveSpeedTimer = -1;
    public float dealsDamageToEndWayPoint = 1;
    
    // Action when the end is reached
    public bool destroyOnNoTarget = true;

    // Unit Health
    private HealthBarBehaviour healthBar;
    public float maxUnitHealth = 5;
    public float unitHealth = 5;
    public float collidableDamageBarrier = 0.5f;
    public Texture2D healthBarTexture;

    // Death
    public GameObject explosionPrefab;
    public bool isDead = false;
    public float deathTime = 0;
    public AnimationClip deathAnimation;
    private float timeTillDeath;

    // Aggressive properties
    public AnimationClip walkAnimation;
    public AnimationClip attackAnimation;
    // Limiting to one projectile at a time for simplicity
    public GameObject projectilePrefab;
    public GameObject projectile;
    private float timeSinceLastShot;
    public float shootCooldown = 2.0f;
    public float maxRange = 15;
    public float weaponDamage = 0.2f;
    public Transform firingPosition;
    public GameObject currentTarget;
    public bool isProjectileActive;

    void Awake()
    {
        myTransform = transform;
        timeSinceLastShot = 0;
        isProjectileActive = false;
        if (projectilePrefab != null && unitType == UnitType.Dangerous)
        {
            projectile = spawnProjectile();
        }
    }

	// Use this for initialization
	void Start() 
    {
        target = wayPoint.transform;
        unitHealth = maxUnitHealth;
        timeTillDeath = (unitType == UnitType.Boss) ? 10 : 5;
        healthBar = gameObject.GetComponentInChildren<HealthBarBehaviour>();
        healthBar.setHealth(unitHealth, maxUnitHealth);
	}
	
	// Update is called once per frame
	public void update(float deltaTime) 
    {
        // If the unit is dead drop it into the ground then despawn after a period of time.
        if (isDead)
        {
            deathTime += deltaTime;
            myTransform.position += new Vector3(0, 0, -0.05f) * deltaTime;
            if (deathTime > timeTillDeath)
            {

                if (projectile != null)
                {
                    Destroy(projectile);
                }
                Destroy(this.gameObject);
            }
            return;
        }

        // Do nothing if there is no waypoint
        if (wayPoint == null)
        {
            if (destroyOnNoTarget)
            {
                if (projectile != null)
                {
                    Destroy(projectile);
                }
                Destroy(this.gameObject);
            }
            return;
        }
        // Target not set or the waypoint has changed
        if(target == null || targetBehaviour == null || target.gameObject != wayPoint) 
        {
            updateWayPoint();
        }

        myTransform.rotation = Quaternion.Slerp(myTransform.rotation,
                                Quaternion.LookRotation(target.position - myTransform.position),
                                rotationSpeed * deltaTime);
        myTransform.position += myTransform.forward * moveSpeed * moveSpeedMultiplier * deltaTime;

        updateMoveSpeedMod(deltaTime);

        float distance = Vector2.Distance(new Vector2(myTransform.position.x, myTransform.position.z), new Vector2(target.position.x, target.position.z));
        if(distance < targetReachThreshold)
        {
            EndPointBehaviour script = target.GetComponent<EndPointBehaviour>();
            if (script != null)
            {
                script.applyDamage(dealsDamageToEndWayPoint);
                GameObject.Find("LevelManager").GetComponent<LevelManager>().loggingStats[9]++;
            }

            if(targetBehaviour != null)
            {
                wayPoint = targetBehaviour.nextWaypoint;
                updateWayPoint();
            }
            else
            {
                wayPoint = null;
            }
        }

        if (unitType == UnitType.Dangerous)
        {
            Animation anim = gameObject.GetComponentInChildren<Animation>();

            if (!isDead)
            {
                timeSinceLastShot += deltaTime;//Time.deltaTime;
                if (!isProjectileActive && timeSinceLastShot > shootCooldown * moveSpeedMultiplier)
                {
                    timeSinceLastShot %= shootCooldown;
                    if (currentTarget == null ||
                    Vector3.Distance(currentTarget.transform.position, gameObject.transform.position) > maxRange
                        || currentTarget.GetComponent<BoxCollider>().enabled != true)
                    {
                        currentTarget = findNextTarget();
                        //Debug.Log("New Target");
                    }
                    fireProjectile();
                }
            }

            if (!anim.isPlaying)
            {
                anim.Play(walkAnimation.name);
                anim.wrapMode = WrapMode.Loop;
            }

            if (projectile != null)
            {
                projectile.GetComponent<EnemyProjectileBehaviour>().update(deltaTime);
            }
        }
	}

    private void updateWayPoint() 
    {
        if (wayPoint == null)
        { 
            //target = null;
            targetBehaviour = null;
            return;
        }

        target = wayPoint.transform;
        targetBehaviour = wayPoint.GetComponent<WayPointBehaviour>();
    }

    public void applyDamage(float amount)
    {
        if (isDead)
            return;

        unitHealth -= amount;
        healthBar.setHealth(unitHealth, maxUnitHealth);
        if (unitHealth <= 0)
        {
            /*if (explosionPrefab != null)
            {
                Instantiate(explosionPrefab, transform.position, transform.rotation);
            }*/
            isDead = true;
            // disable physics collisions
            healthBar.hide();
            gameObject.GetComponent<BoxCollider>().enabled = false;
            gameObject.GetComponent<Rigidbody>().drag = (unitType == UnitType.Boss) ? 9 : 5;
            Animation anim = gameObject.GetComponentInChildren<Animation>();
            if (deathAnimation != null && anim != null)
            {
                anim.Play(deathAnimation.name);
            }

            GameObject levelManager = GameObject.Find("LevelManager");
            if (levelManager != null)
            {
                levelManager.GetComponent<LevelManager>().addCurrency(getGoldValueOnKill());
                levelManager.GetComponent<LevelManager>().loggingStats[8]++;
            }
            //Destroy(this.gameObject);
        }
    }

    public void applySpeedModifier(float speedAmount, float duration)
    {
        moveSpeedMultiplier = speedAmount;
        moveSpeedTimer = duration;
    }

    private void updateMoveSpeedMod(float deltaTime)
    {
        if(moveSpeedTimer == -1)
        {
            return;
        }

        moveSpeedTimer -= deltaTime;
        if (moveSpeedTimer <= 0)
        {
            moveSpeedTimer = -1;
            moveSpeedMultiplier = 1;
        }
    }
    
    public int getGoldValueOnKill()
    {
        switch(unitType)
        {
            case UnitType.Basic:
                return 4;
            case UnitType.Fast:
                return 3;
            case UnitType.Dangerous:
                return 6;
            case  UnitType.Boss:
                return 20;
        }
        return 0;
    }

    private GameObject spawnProjectile()
    {
        GameObject projectile = (GameObject)Instantiate(projectilePrefab, transform.position, transform.rotation);
        EnemyProjectileBehaviour script = (EnemyProjectileBehaviour)projectile.GetComponent<EnemyProjectileBehaviour>();
        script.setParent(this.gameObject);
        script.destroyProjectile();
        return projectile;
    }

    private GameObject findNextTarget(GameObject thatIsNotThis = null)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Tower");
        GameObject bestTarget = null;
        float bestTargetDistance = maxRange + 10000;
        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(target.transform.position, gameObject.transform.position);
            if (target != thatIsNotThis && distance < bestTargetDistance && distance < maxRange && target.GetComponent<BoxCollider>().enabled == true)
            {
                bestTarget = target;
            }
        }
        return bestTarget;
    }

    private void fireProjectile()
    {
        // Don't fire if the unit is completely destroyed.
        if (unitHealth <= 0)
        {
            return;
        }

        if (currentTarget != null)
        {
            EnemyProjectileBehaviour projectileBehaviour = projectile.GetComponent<EnemyProjectileBehaviour>();
            projectileBehaviour.setTarget(currentTarget, firingPosition);
            Animation anim = gameObject.GetComponentInChildren<Animation>();
            anim.wrapMode = WrapMode.Once;
            anim.Play(attackAnimation.name);
            isProjectileActive = true;
            //Debug.Log("Fired Shot at " + currentTarget.ToString());
        }
    }

    public void resetProjectile()
    {
        isProjectileActive = false;
    }

    /*void OnGUI()
    {
        if (unitHealth > 0 && healthBarTexture != null)
        {
            Vector3 position = Camera.main.WorldToViewportPoint(transform.position);
            //print(position.x + " " + position.y);
            GUI.DrawTexture(new Rect(position.x * Screen.width - 25, (1-position.y) * Screen.height, (unitHealth / maxUnitHealth) * 50, 8), healthBarTexture);
        }
    }*/
}
