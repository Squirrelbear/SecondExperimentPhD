using UnityEngine;
using System.Collections;

public class MasterAnimationSequence : MonoBehaviour {

    public int animState = 0;

    public float timer = 20;

    public GameObject towerScrollBy;
    public GameObject towerContruction;
    public GameObject basePreview;
    public GameObject attackPreview;
    public GameObject outroAttack;
    public GameObject outroAttackUnit;

    public AISpawner spawner;
    public TowerBehaviour[] preSpawnedTowers;

	// Use this for initialization
	void Start () {
        AudioListener[] myListeners = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];

        foreach (AudioListener thisListener in myListeners)
        {
            if (thisListener.enabled) { thisListener.enabled = false; }
        }

        //animState = 4;
	}

    private string waveDefinition = "1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;1:0;1:0;1:1;1:1;1:2;1:2;-1:-1";


	// Update is called once per frame
	void Update () {
        AudioListener[] myListeners = FindObjectsOfType(typeof(AudioListener)) as AudioListener[];

        foreach (AudioListener thisListener in myListeners)
        {
            if (thisListener.enabled) { thisListener.enabled = false; }
        }

        if (animState == 0)
        {
            //attackPreview.SetActive(true);
            //spawner.spawnWave(waveDefinition);
            towerContruction.SetActive(true);
            animState++;
        }
        else
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                animState++;
                if (animState == 2)
                {
                    towerContruction.SetActive(false);
                    towerScrollBy.SetActive(true);
                }
                else if (animState == 3)
                {
                    towerScrollBy.SetActive(false);
                    basePreview.SetActive(true);
                }
                else if (animState == 4)
                {
                    basePreview.SetActive(false);
                    attackPreview.SetActive(true);
                    spawner.spawnWave(waveDefinition);
                }
                else if (animState == 5)
                {
                    attackPreview.SetActive(false);
                    outroAttack.SetActive(true);
                }
                else if(animState == 6)
                {
                    outroAttackUnit.SetActive(true);
                }

                if (animState == 4)
                {
                    timer = 60;
                }
                else if(animState == 5)
                {
                    timer = 10;
                }
                else
                {
                    timer = 20;
                }
            }

            if (animState == 4)
            {
                spawner.update(Time.deltaTime);
                foreach(TowerBehaviour tB in preSpawnedTowers)
                {
                    tB.update(Time.deltaTime);
                    tB.durability = 1;
                    if (tB.towerType != TowerBehaviour.TowerType.Frost)
                    {
                        tB.towerDamage = 0;
                    }
                }
            }
        }
	}
}
