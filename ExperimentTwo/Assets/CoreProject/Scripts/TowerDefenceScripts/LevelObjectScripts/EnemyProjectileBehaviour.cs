using UnityEngine;
using System.Collections;

public class EnemyProjectileBehaviour : MonoBehaviour
{
    // Movement Properties
    public float moveSpeed = 8;
    private Transform target; // transform of the targetObj object
    private Transform myTransform;

    // Damage Target
    public GameObject targetObj;
    public TowerBehaviour targetScript;
    //public float damage = 1;
    private bool targetSet = false;

    public AIWayfinder parentScript;

    void Awake()
    {
        myTransform = transform;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    public void update(float deltaTime)
    {
        if (!targetSet)
        {
            return;
        }

        if (targetObj == null)
        {
            //Debug.Log("No Target Goodbye Projectile");
            destroyProjectile();
            return;
        }
        this.target = targetObj.transform;

        myTransform.rotation = Quaternion.LookRotation(target.position - myTransform.position);
        myTransform.position += myTransform.forward * moveSpeed * deltaTime;

        float distance = Vector3.Distance(myTransform.position, target.position);
        if ((targetScript != null && distance < targetScript.collidableDamageBarrier) || distance < 0.5f)
        {
            if (targetScript != null)
            {
                targetScript.applyDamage(parentScript.weaponDamage);
                //print("At distance: " + distance + " damage dealt: " + parentScript.weaponDamage + " new health: " + targetScript.durability);
            }

            destroyProjectile();
        }
    }

    public void setTarget(GameObject target, Transform startPoint)
    {
        this.targetObj = target;
        targetSet = true;
        targetScript = target.GetComponent<TowerBehaviour>();
        transform.position = startPoint.position;
        transform.rotation = startPoint.rotation;
        gameObject.SetActive(true);
    }

    public void setParent(GameObject enemyObject)
    {
        parentScript = enemyObject.GetComponent<AIWayfinder>();
    }

    public void destroyProjectile()
    {
        if (parentScript != null)
        {
            parentScript.resetProjectile();
        }
        target = null;
        targetScript = null;
        gameObject.SetActive(false);
    }
}
