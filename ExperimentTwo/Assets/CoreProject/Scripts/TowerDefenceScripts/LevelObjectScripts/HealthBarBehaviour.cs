using UnityEngine;
using System.Collections;

public class HealthBarBehaviour : MonoBehaviour {

    private Camera cam;
    public float scaleMaxY = 0.2f;
    public float scaleMaxZ = 0.05f;
    private float healthMax = 1;
    private float curHealth = 1;

	// Use this for initialization
	void Start () {
        cam = Camera.main;
	}
	
	// Update is called once per frame
	void Update () {
        if (cam == null)
            return;

        transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(90, 0, 0);
        transform.localScale = new Vector3((curHealth / healthMax) * scaleMaxY, 1, scaleMaxZ);
	}

    public void setHealth(float health, float maxHealth)
    {
        this.healthMax = maxHealth;
        this.curHealth = health;
    }

    public void hide()
    {
        gameObject.GetComponent<MeshRenderer>().enabled = false;
    }
}
