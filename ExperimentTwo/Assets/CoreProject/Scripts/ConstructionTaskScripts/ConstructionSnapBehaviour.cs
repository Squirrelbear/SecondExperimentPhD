using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ConstructionSnapBehaviour : MonoBehaviour
{

    public Vector3 snapPoint;
    public List<GameObject> towerComponents = new List<GameObject>();

    private const float snapYIncrement = 1f;
    public int maxTowerComponents = 10;

    public int[] componentCounts;
    public int versionID = 0;

    public ExtraDataRecorder extraDataRecorder;
    public ExtraDataRecorder.ExtraDataCollection constructionChoices;
    private int towerID = 0;
    private int removalsCount = 0;

    // Use this for initialization
    void Start()
    {
        snapPoint = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        // deprecated y position: actualTowerYPos
        componentCounts = new int[3];
        for(int i = 0; i < 3; i++)
        {
            componentCounts[i] = 0;
        }

        extraDataRecorder = ExtraDataRecorder.getSingleton();
        constructionChoices = extraDataRecorder.getDataCollection("Construction Task Data");
        constructionChoices.logData("TimeStamp,TowerID,VersionID,Damage,Range,FireRate,Removed", false);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public bool canAddMore()
    {
        return towerComponents.Count-1 < maxTowerComponents;//associatedTower == null;
    }

    public Vector3 getTempSnapLocation()
    {
        return new Vector3(transform.position.x, transform.position.y + (snapYIncrement * (towerComponents.Count + 1)), transform.position.z);
    }

    public void addTowerComponent(GameObject nextComponent)
    {
        towerComponents.Add(nextComponent);
        Vector3 v = new Vector3(transform.position.x, transform.position.y + (snapYIncrement * towerComponents.Count), transform.position.z);
        nextComponent.transform.position = v;
        int modID = (int)(nextComponent.GetComponent<TowerComponentBehaviour>().towerCompType) - 1;
        componentCounts[modID]++;
        versionID++;
        ErrorLog.logData("Added one tower component. " + towerID + "," + versionID + "," + componentCounts[0] + "," + componentCounts[1] + "," + componentCounts[2] + "," + removalsCount, false);
    }

    public void removeComponent()
    {
        if (towerComponents.Count == 0)
        {
            return;
        }

        GameObject old = towerComponents[towerComponents.Count - 1];
        int modID = (int)(old.GetComponent<TowerComponentBehaviour>().towerCompType) - 1;
        componentCounts[modID]--;
        towerComponents.RemoveAt(towerComponents.Count - 1);
        Destroy(old);
        versionID++;
        removalsCount++;
        ErrorLog.logData("Removed one tower component. " + towerID + "," + versionID + "," + componentCounts[0] + "," + componentCounts[1] + "," + componentCounts[2] + "," + removalsCount, false);
    }

    public void removeAll()
    {
        constructionChoices.logData(towerID + "," + (versionID+1) + "," + componentCounts[0] + "," + componentCounts[1] + "," + componentCounts[2] + "," + removalsCount, true);
        removalsCount = 0;
        towerID++;

        foreach(GameObject towerComponent in towerComponents)
        {
            Destroy(towerComponent);
        }
        towerComponents.Clear();
        for (int i = 0; i < 3; i++)
        {
            componentCounts[i] = 0;
        }
        versionID++;
        ErrorLog.logData("Removed all tower components. Ready for next tower. " + versionID, false);
    }

    public int[] getComponentCounts()
    {
        return componentCounts;
    }

    public int getVersionID()
    {
        return versionID;
    }
}
