using UnityEngine;
using System.Collections;

public class EndPointBehaviour : MonoBehaviour {

    public float maxUnitHealth = 25;
    public float unitHealth = 25;
    public Texture2D healthBarTexture;
    private HealthBarBehaviour healthBar;

    public GameObject gameOverObject;

	// Use this for initialization
	void Start () {
        healthBar = gameObject.GetComponentInChildren<HealthBarBehaviour>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void applyDamage(float amount)
    {
        unitHealth -= amount;
        if (unitHealth <= 0)
        {
            unitHealth = 0;
            //Destroy(this.gameObject);
            ErrorLog.logData("Game Over!");
            if (gameOverObject != null)
            {
                gameOverObject.SetActive(true);
            }
        }
        healthBar.setHealth(unitHealth, maxUnitHealth);
    }

    /*
    void OnGUI()
    {
        if (unitHealth > 0 && healthBarTexture != null)
        {
            Vector3 position = Camera.main.WorldToViewportPoint(transform.position);
            //print(position.x + " " + position.y);
            GUI.DrawTexture(new Rect(position.x * Screen.width - 25, (1 - position.y) * Screen.height, (unitHealth / maxUnitHealth) * 50, 8), healthBarTexture);
        }
    }*/
}
