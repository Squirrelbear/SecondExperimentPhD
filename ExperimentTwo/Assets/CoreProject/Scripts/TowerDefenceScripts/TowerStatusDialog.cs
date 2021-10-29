using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TowerStatusDialog : MonoBehaviour {

    public enum TowerStatusDialogType { Selection, Selected };

    public TowerStatusDialogType dialogType;
    public bool tooltipVisible = false;
    public GameObject titleObject;
    public GameObject titleDurability;
    public GameObject titleSelecting;
    public GameObject panel;

    // cached references
    public Text tooltipTitleText;
    public Text tooltipDuraText;
    public Text tooltipSelectingText;
    public Camera cam;
    public CameraBehaviour camRef;
    public TowerBehaviour cachedTowerRef;
    public Image imageScript;

	// Use this for initialization
	void Start () {
        if (titleObject != null)
        {
            tooltipTitleText = titleObject.GetComponent<Text>();
        }
        if (titleDurability != null)
        {
            tooltipDuraText = titleDurability.GetComponent<Text>();
        }
        if (titleSelecting != null)
        {
            tooltipSelectingText = titleSelecting.GetComponent<Text>();
        }
        cam = Camera.main;
        camRef = cam.GetComponent<CameraBehaviour>();
        imageScript = panel.GetComponent<Image>();
        hide();
	}
	
	// Update is called once per frame
	public void update () {
        if (dialogType == TowerStatusDialogType.Selected)
        {
            if (camRef.selectedTower == null || camRef.selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
            {
                hide();
                return;
            }
            else
            {
                show();
            }

            if (camRef.selectedTower != cachedTowerRef)
            {
                cachedTowerRef = camRef.selectedTower;
                updateTowerTitle();

            }
        }
        else if (dialogType == TowerStatusDialogType.Selection)
        {
            if (camRef.selectingObject == null 
                || camRef.selectingObject == camRef.selectedObject 
                || camRef.selectingObject.GetComponent<TowerBehaviour>() == null)
            {
                hide();
                return;
            }
            else
            {
                show();
            }

            cachedTowerRef = camRef.selectingObject.GetComponent<TowerBehaviour>();
            tooltipSelectingText.text = (int)((camRef.selectionProgress / camRef.timeToSelect) * 100) + "%";
        }

        if (cam != null)
        {
            transform.rotation = Quaternion.LookRotation(cam.transform.position - transform.position) * Quaternion.Euler(0, 180, 0);
        }

        if (cachedTowerRef != null)
        {
            transform.position = cachedTowerRef.transform.position + new Vector3(0, 5, 0);
            float durability = cachedTowerRef.durability;
            tooltipDuraText.text = (int)(durability*100) + "%";
            if (durability == 0)
            {
                imageScript.color = (dialogType == TowerStatusDialogType.Selected) ? Color.yellow : Color.red;
            }
            else
            {
                imageScript.color = (dialogType == TowerStatusDialogType.Selected) ? Color.green : Color.white;
            }
        }
	}

    private void updateTowerTitle()
    {
        if(cachedTowerRef != null)
        {
            switch(cachedTowerRef.towerType)
            {
                case TowerBehaviour.TowerType.Basic:
                    tooltipTitleText.text = "Basic Tower";
                    break;
                case TowerBehaviour.TowerType.Frost:
                    tooltipTitleText.text = "Frost Tower";
                    break;
                case TowerBehaviour.TowerType.Swarm:
                    tooltipTitleText.text = "Swarm Tower";
                    break;
                case TowerBehaviour.TowerType.Explosive:
                    tooltipTitleText.text = "Explosive Tower";
                    break;
            }
        }
    }

    public void hide()
    {
        tooltipVisible = false;
        gameObject.GetComponent<Canvas>().enabled = false;
    }

    public void show()
    {
        tooltipVisible = true;
        gameObject.GetComponent<Canvas>().enabled = true;
    }
}
