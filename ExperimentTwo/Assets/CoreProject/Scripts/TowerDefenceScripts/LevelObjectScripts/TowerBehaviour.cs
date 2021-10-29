using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TowerBehaviour : MonoBehaviour {

    public enum TowerType { Basic = 0, Swarm = 1, Sniper = 2, Explosive = 3, Frost = 4 };

    public enum TowerState { TowerPlacement, TowerAIControlled, TowerUserControlled, TowerInactive };

    // Tower properties
    public TowerType towerType;
    public float shootCooldown = 0.5f;
    public float maxRange = 5;
    public float towerDamage = 1;
    public float durability = 1.0f;
    public float collidableDamageBarrier = 0.5f;

    // Projectile properties
    public Transform firingPosition;
    public GameObject projectilePrefab;
    public List<GameObject> offProjectiles;
    public List<GameObject> onProjectiles;

    // Tower state
    public GameObject currentTarget;
    public GameObject playerTarget;
    //public bool isFiring = true;
    public TowerState towerState;
    public Material mat;
    public bool matAlphaIncreasing = false;

    // User control point
    public Transform cameraControlPoint;

    // Camera reference
    private CameraBehaviour cameraRef;

    // Timers
    private float timeSinceLastShot;

    // Child references
    public GameObject towerRange;
    private TowerRangeBehaviour towerRangeScript;
    public GameObject[] fireParticles;

    private GameObject frostTowerEffect;

    public bool trailerDemoMode = false;

    void Awake()
    {
        onProjectiles = new List<GameObject>();
        offProjectiles = new List<GameObject>();
        if (towerType != TowerType.Frost)
        {
            for (int i = 0; i < 10; i++)
            {
                offProjectiles.Add(spawnProjectile());
            }
        }
        firingPosition = transform;

        timeSinceLastShot = 0;
        if (towerType == TowerType.Frost)
        {
            frostTowerEffect = transform.FindChild("FrostTowerAttack").gameObject;
        }

        foreach(GameObject fire in fireParticles)
        {
            fire.SetActive(false);
        }
    }

	// Use this for initialization
	void Start () {
        //StartCoroutine(controlTower());
        cameraControlPoint = transform.FindChild("CamPoint");
        towerRangeScript = gameObject.GetComponentInChildren<TowerRangeBehaviour>();
        if (towerRangeScript != null)
        {
            towerRange = towerRangeScript.gameObject;
        }

        mat = GetComponentInChildren<MeshRenderer>().material;
        /*Color newColor2 = mat.color;
        newColor2.a = 0.4f;
        mat.color = newColor2;*/
        //setTowerState(TowerState.TowerAIControlled);
        configCustomStats();
	}
	
	// Update is called once per frame
	public void update (float deltaTime) {
        if (cameraRef == null && !trailerDemoMode)
        {
            cameraRef = GameObject.Find("Main Camera").GetComponent<CameraBehaviour>();
        }

        if (towerState == TowerState.TowerAIControlled && durability > 0)
        {
            timeSinceLastShot += deltaTime;//Time.deltaTime;
            if (timeSinceLastShot > shootCooldown)
            {
                timeSinceLastShot -= shootCooldown;
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
        else if(towerState == TowerState.TowerUserControlled)
        {
            timeSinceLastShot += Time.deltaTime;
            if (timeSinceLastShot > shootCooldown)
            {
                timeSinceLastShot -= shootCooldown;
                if (currentTarget == null || currentTarget == playerTarget ||
                Vector3.Distance(currentTarget.transform.position, gameObject.transform.position) > maxRange
                    || currentTarget.GetComponent<BoxCollider>().enabled != true)
                {
                    currentTarget = findNextTarget(playerTarget);
                    //Debug.Log("New Target");
                }
                fireProjectile();
            }
        }
        else if(towerState == TowerState.TowerPlacement)
        {
            /*Color newColor2 = mat.color;
            if (matAlphaIncreasing)
            {
                newColor2.a += deltaTime / 2; //Time.deltaTime / 2;
            }
            else
            {
                newColor2.a -= deltaTime / 2; // Time.deltaTime / 2;
            }

            if (newColor2.a < 0.2)
            {
                newColor2.a = 0.2f;
                matAlphaIncreasing = true;
            }
            else if (newColor2.a > 0.4)
            {
                newColor2.a = 0.4f;
                matAlphaIncreasing = false;
            }
            mat.color = newColor2;*/
        }

        if (towerRangeScript != null)
        {
            towerRangeScript.setRange(maxRange);
            towerRange = towerRangeScript.gameObject;
        }
        else
        {
            towerRangeScript = gameObject.GetComponentInChildren<TowerRangeBehaviour>();
            if (towerRangeScript != null)
            {
                towerRange = towerRangeScript.gameObject;
            }
        }

        if (!trailerDemoMode)
        {
            if (cameraRef.selectedTower == this)// && towerState == TowerState.TowerPlacement)
            {
                towerRange.GetComponent<MeshRenderer>().enabled = true;
            }
            else
            {
                towerRange.GetComponent<MeshRenderer>().enabled = false;
            }
        }

        // Fixes error on updating projectiles when objects are removed from onProjectiles mid-update
        GameObject[] tempProjectilesToUpdate = new GameObject[onProjectiles.Count];
        onProjectiles.CopyTo(tempProjectilesToUpdate);

        foreach (GameObject projectile in tempProjectilesToUpdate)
        {
            projectile.GetComponent<ProjectileBehaviour>().update(deltaTime);
        }
	}

    /*private IEnumerator controlTower()
    {
        while (isFiring)
        {
            yield return new WaitForSeconds(shootCooldown);
            if(currentTarget == null || 
                Vector3.Distance(currentTarget.transform.position, gameObject.transform.position) > maxRange)
            {
                currentTarget = findNextTarget();
                //Debug.Log("New Target");
            }
            fireProjectile();
        }
    }*/

    private void fireProjectile()
    {
        // Don't fire if the tower is completely destroyed.
        if(durability <= 0)
        {
            return;
        }

        if(towerType == TowerType.Frost)
        {
            slowAllTargetsInRange(maxRange, towerDamage);
        }
        else if (currentTarget != null)
        {
            GameObject projectile = null;
            if(offProjectiles.Count == 0) {
                projectile = spawnProjectile();
            }
            else
            {
                projectile = offProjectiles[0];
                offProjectiles.RemoveAt(0);
            }
            onProjectiles.Add(projectile);
            ProjectileBehaviour projectileBehaviour = projectile.GetComponent<ProjectileBehaviour>();
            projectileBehaviour.setTarget(currentTarget, firingPosition);
            //durability -= 0.005f;
            applyDamage(0.005f);
            //Debug.Log("Fired Shot at " + currentTarget.ToString());
        }
    }

    private void slowAllTargetsInRange(float range, float speedMultiplier)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        bool targetHit = false;
        foreach (GameObject target in targets)
        {
            if (Vector3.Distance(target.transform.position, gameObject.transform.position) < range)
            {
                AIWayfinder targetScript = target.GetComponent<AIWayfinder>();
                if (targetScript != null)
                {
                    targetScript.applySpeedModifier(speedMultiplier, shootCooldown * 4);
                }
                targetHit = true;
            }
        }

        if (targetHit)
        {
            applyDamage(0.005f);
            //durability -= 0.005f;
            //print(durability);
        }
    }

    private GameObject findNextTarget(GameObject thatIsNotThis = null)
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float bestTargetDistance = maxRange+10000;
        foreach(GameObject target in targets) 
        {
            float distance = Vector3.Distance(target.transform.position, gameObject.transform.position);
            if(target != thatIsNotThis && distance < bestTargetDistance && distance < maxRange && target.GetComponent<BoxCollider>().enabled == true)
            {
                bestTarget = target;
            }
        }
        return bestTarget;
    }

    private GameObject spawnProjectile()
    {
        GameObject projectile = (GameObject)Instantiate(projectilePrefab, transform.position, transform.rotation);
        ProjectileBehaviour script = (ProjectileBehaviour)projectile.GetComponent<ProjectileBehaviour>();
        script.setTower(gameObject);
        script.destroyProjectile();
        return projectile;
    }

    public void resetProjectile(GameObject projectile)
    {
        onProjectiles.Remove(projectile);
        offProjectiles.Add(projectile);
    }

    public void setTowerState(TowerState state)
    {
        towerState = state;

        if (towerState == TowerState.TowerPlacement)
        {
            /*Color newColor2 = mat.color;
            newColor2.a = 0.4f;
            mat.color = newColor2;*/
            /*if (towerRange != null)
            {
                towerRange.SetActive(true);
            }*/
        }
        else
        {
            Color newColor2 = mat.color;
            newColor2.a = 1;
            mat.color = newColor2;
            /*if (towerRange != null)
            {
                towerRange.SetActive(false);
            }*/
        }
    }

    public void applyDamage(float amount)
    {
        durability -= amount;
        
        if (durability <= 0)
        {
            durability = 0;
            /*foreach (TowerSnapBehaviour snap in cameraRef.towerSnapPoints)
            {
                if (snap.associatedTower == this.gameObject)
                {
                    snap.associatedTower = null;
                }
            }*/
            //DestroyObject(this.gameObject);
            if (towerType == TowerType.Frost)
            {
                frostTowerEffect.SetActive(false);
            }
        }
        else if(towerType == TowerType.Frost)
        {
            frostTowerEffect.SetActive(true);
        }

        // Show/Hide visible damage effects
        if (amount > 0)
        {
            if (durability < 0.5f)
            {
                fireParticles[0].SetActive(true);
            }
            if (durability < 0.25f)
            {
                fireParticles[1].SetActive(true);
            }
            if (durability < 0.1f)
            {
                fireParticles[2].SetActive(true);
            }
        }
        else if(amount < 0)
        {
            if (durability > 0.5f)
            {
                fireParticles[0].SetActive(false);
            }
            if (durability > 0.25f)
            {
                fireParticles[1].SetActive(false);
            }
            if (durability > 0.1f)
            {
                fireParticles[2].SetActive(false);
            }
        }
    }

    public void repairTower()
    {
        durability = 1;
        fireParticles[0].SetActive(false);
        fireParticles[1].SetActive(false);
        fireParticles[2].SetActive(false);
    }

    public void destroyTower()
    {
        foreach(GameObject obj in onProjectiles)
        {
            Destroy(obj);
        }

        foreach(GameObject obj in offProjectiles)
        {
            Destroy(obj);
        }
    }

    private void configCustomStats()
    {
        int modIndex = 0;
        switch(towerType)
        {
            case TowerType.Basic:
                modIndex = 0;
                break;
            case TowerType.Frost:
                modIndex = 1;
                break;
            case TowerType.Swarm:
                modIndex = 2;
                break;
            case TowerType.Explosive:
                modIndex = 3;
                break;
        }

        towerDamage = TowerConstructionManager.towerModStats[modIndex, 0];
        maxRange = TowerConstructionManager.towerModStats[modIndex, 1];
        shootCooldown = TowerConstructionManager.towerModStats[modIndex, 2];
    }
}
