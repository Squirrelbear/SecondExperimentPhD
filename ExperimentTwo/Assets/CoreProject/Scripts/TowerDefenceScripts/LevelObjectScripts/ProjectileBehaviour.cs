using UnityEngine;
using System.Collections;

public class ProjectileBehaviour : MonoBehaviour {

    public enum ProjectilePathType { DirectProjectile, SwarmProjectile };
    public enum ProjectileDamageType { SingleTarget, AOERadius };

    // Movement Properties
    public ProjectilePathType pathType = ProjectilePathType.DirectProjectile;
    public float moveSpeed = 8;
    public float rotationSpeed = 5;
    private Transform target; // transform of the targetObj object
    private Transform myTransform;

    // Damage Target
    public GameObject targetObj;
    public AIWayfinder targetScript;
    //public float damage = 1;
    public float aoeDamageRadius = 5;
    public ProjectileDamageType damageType;
    private bool targetSet = false;

    // Associated Tower
    // TODO make these readonly within editor
    private GameObject associatedTower;
    private TowerBehaviour towerScript;

    void Awake()
    {
        myTransform = transform;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	public void update(float deltaTime) {
        if (!targetSet)
        {
            return;
        }

        if(targetObj == null)
        {
            //Debug.Log("No Target Goodbye Projectile");
            destroyProjectile();
            return;
        }
        this.target = targetObj.transform;

        if (pathType == ProjectilePathType.SwarmProjectile)
        {
            myTransform.rotation = Quaternion.Slerp(myTransform.rotation,
                                    Quaternion.LookRotation(target.position - myTransform.position),
                                    rotationSpeed * deltaTime);
        }
        else 
        {
            myTransform.rotation = Quaternion.LookRotation(target.position - myTransform.position);
        }
        myTransform.position += myTransform.forward * moveSpeed * deltaTime;

        float distance = Vector3.Distance(myTransform.position, target.position);
        if ((targetScript != null && distance < targetScript.collidableDamageBarrier) || distance < 0.5f)
        {
            //Debug.Log("Target Hit!");
            //AIWayfinder script = (AIWayfinder)targetObj.GetComponent<AIWayfinder>();
            if (targetScript != null && towerScript != null)
            {
                if (damageType == ProjectileDamageType.SingleTarget)
                {
                    targetScript.applyDamage(towerScript.towerDamage);
                }
                else
                {
                    Vector3 targetPos = targetObj.transform.position;
                    GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
                    foreach (GameObject enemy in enemies)
                    {
                        if (Vector3.Distance(targetPos, enemy.transform.position) < aoeDamageRadius)
                        {
                            AIWayfinder script = enemy.GetComponent<AIWayfinder>();
                            if (script != null)
                            {
                                script.applyDamage(towerScript.towerDamage);
                            }
                        }
                    }
                }
            }

            destroyProjectile();
        }
	}

    public void setTarget(GameObject target, Transform startPoint)
    {
        this.targetObj = target;
        targetSet = true;
        targetScript = target.GetComponent<AIWayfinder>();
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        gameObject.SetActive(true);
    }

    public void setTower(GameObject tower)
    {
        associatedTower = tower;
        towerScript = tower.GetComponent<TowerBehaviour>();
    }

    public void destroyProjectile()
    {
        gameObject.SetActive(false);
        if (towerScript != null)
        {
            towerScript.resetProjectile(gameObject);
        }
    }
}
