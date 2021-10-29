using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {

    public enum CameraState { Neutral, PlacingTower, ControllingTower };

    public enum CursorMode { LookAt = 0, Click = 1, MobileTap = 2 };

    public GameObject selectingObject;
    public float timeToSelect = 1.5f;
    public float selectionProgress = 0;
    public float deselectionProgress = 0;

    public TowerBehaviour selectedTower;
    public GameObject selectedObject;
    public bool clickDebugMode;
    //private Camera mCam;
    public CursorMode cursorMode;
    public bool lastKeyState;

    public TowerSnapBehaviour[] towerSnapPoints;
    public bool isTowerSnapped = false;
    public TowerSnapBehaviour tempSnapCache = null;
    public CameraState cameraState = CameraState.Neutral;
    private Vector3 lastCameraPoint;
    public Transform towerDefenceCameraLookAt;
    public Transform otherModesCameraLookAt;
    public GameObject firstCameraSnapNode;
    public GameObject towerDefenceFirstSnap;
    public GameObject[] cameraSnaps;

    // frame click state
    public bool clickUsed = false;
    public bool clickOccured = false;

    private Ray ray;

    public GameObject[] towerPrefabs;

    public TowerConstructionManager towerConstructionManager = null;
    public LevelManager levelManager = null;
    public ExperimentState experimentState = null;
    public PeripheryBehaviour peripheryBehaviour = null;
    public MenuSensitivityTest menuSensitivityTest = null;

    public bool useTerrainSnap = true;

    public GameObject constructionSnapObj;
    public ConstructionSnapBehaviour constructionSnap = null;
    public GameObject[] constructionPrefabs;

    public GameObject cursorObject;
    public Reticle cursor;

    private bool firstUpdate = true;
    private float timerTillFirstUpdateTrigger = 0;

    public ReplayEngine replayEngine;

    public GameObject camSnapTemp;
    public float camSnapTimer;

    public float rotationX = 0, rotationY = 0;
    public float minY = -45.0f;
    public float maxY = 45.0f;
    public float sensX = 100.0f;
    public float sensY = 100.0f;

    private bool pause = false;

	// Use this for initialization
	void Start () {
        cameraSnaps = GameObject.FindGameObjectsWithTag("CameraSnap");
        GameObject[] towerSnapObjs = GameObject.FindGameObjectsWithTag("TowerSnap");
        towerSnapPoints = new TowerSnapBehaviour[towerSnapObjs.Length];
        for(int i = 0; i < towerSnapObjs.Length; i++) 
        {
            towerSnapPoints[i] = towerSnapObjs[i].GetComponent<TowerSnapBehaviour>();
            towerSnapObjs[i].GetComponent<MeshRenderer>().enabled = false;
        }

        towerConstructionManager = GameObject.Find("TowerConstructionAssets").GetComponent<TowerConstructionManager>();
        levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        //menuSensitivityTest = GameObject.Find("MenuTestAssets").GetComponent<MenuSensitivityTest>();
        experimentState = gameObject.GetComponent<ExperimentState>();
        peripheryBehaviour = gameObject.GetComponent<PeripheryBehaviour>();

        constructionSnap = constructionSnapObj.GetComponent<ConstructionSnapBehaviour>();
        cursor = cursorObject.GetComponent<Reticle>();
        setCameraToSnap(firstCameraSnapNode, otherModesCameraLookAt);

        replayEngine = gameObject.GetComponent<ReplayEngine>();

        camSnapTemp = null;
        camSnapTimer = 0;
	}

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            pause = !pause;
            ErrorLog.logData("Setting pause to: " + pause);
        }
        if(pause)
        {
            return;
        }

        if(Input.GetKeyDown(KeyCode.Alpha0))
        {
            clickDebugMode = !clickDebugMode;
        }

        if(firstUpdate && experimentState.skipToState == 0 && replayEngine.replayMode != ReplayEngine.ReplayMode.Replay)
        {
            timerTillFirstUpdateTrigger += Time.deltaTime;
            if (timerTillFirstUpdateTrigger >= 5)
            {
                if (firstCameraSnapNode != null && otherModesCameraLookAt != null)
                {
                    // This is an override to fix the rotation introduced by the default orientation of the Oculus
                    setCameraToSnap(firstCameraSnapNode, otherModesCameraLookAt);
                }
                firstUpdate = false;
            }
        }

        replayEngine.preUpdate();

        // Master definition of deltaTime for all objects
        float deltaTime = replayEngine.getDeltaTime();// Time.deltaTime;

        if (levelManager == null)
        {
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        }
        if (towerConstructionManager == null)
        {
            towerConstructionManager = GameObject.Find("TowerConstructionAssets").GetComponent<TowerConstructionManager>();
        }

        //mCam = Camera.main;

        this.ray = getUpdatedRay();

        updateClickState();
        updateCameraPosition(deltaTime);

        experimentState.update(deltaTime);
        peripheryBehaviour.update(deltaTime);
        if (experimentState.gameSuperState == ExperimentState.GameSuperState.MainMenu)
        {

        }
        else if (experimentState.gameSuperState == ExperimentState.GameSuperState.MenuSensitivityTest)
        {
            if (menuSensitivityTest == null)
            {
                menuSensitivityTest = GameObject.Find("MenuTestAssets").GetComponent<MenuSensitivityTest>();
                menuSensitivityTest.experimentState = experimentState;
                menuSensitivityTest.peripheryBehaviour = peripheryBehaviour;
            }

            menuSensitivityTest.update(deltaTime);
            /*if (experimentState.gameState == ExperimentState.GameState.MenuSensitivityTest && menuSensitivityTest.taskState == MenuSensitivityTest.TaskState.AllTasksComplete)
            {
                experimentState.nextState();
            }*/
        }
        else if (experimentState.gameSuperState == ExperimentState.GameSuperState.TowerConstruction)
        {
            updateTowerConstruction(deltaTime);
        }
        else if (experimentState.gameSuperState == ExperimentState.GameSuperState.TowerDefence)
        {
            updateTowerDefence(deltaTime);
        }

        replayEngine.postUpdate();
	}

    private void updateClickState()
    {
        clickUsed = false;
        clickOccured = false;
        if (cursorMode == CursorMode.Click || clickDebugMode)
        {
            clickOccured = replayEngine.getTriggerState() == false && lastKeyState == true;
            lastKeyState = replayEngine.getTriggerState();
            //clickOccured = Input.GetMouseButtonDown(0) == false && lastKeyState == true;
            // replay event info
            //curFrame.triggerEdge = clickOccured;
            //lastKeyState = Input.GetMouseButtonDown(0);
        }
        else if (cursorMode == CursorMode.MobileTap)
        {
            /*if (server.tapped)
            {
                server.tapped = false;
                clickOccured = true;
                curFrame.triggerEdge = true;
            }*/
        }
    }

    private Ray getUpdatedRay()
    {
        Ray ray;
        if (clickDebugMode)
        {
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        }
        else
        {
            ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        }
        return ray;
    }

    private bool attemptSelection(float deltaTime)
    {
        if(selectedObject != null && selectedTower != null  && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            return false;
        }

        if(clickOccured && clickDebugMode)
        {
            //print("Searching for object");
            GameObject tempObj = findTowerInRay(ray);
            if (tempObj != null)
            {
                selectTower(tempObj);
                isTowerSnapped = false;
                clickUsed = true;
            }
        }
        else 
        {
            GameObject tempObj = findTowerInRay(ray);
            if (tempObj != null)
            {
                if (tempObj == selectedObject)
                {
                    // object is already selected don't start again
                }
                else if (selectingObject == null || selectingObject != tempObj)
                {
                    selectionProgress = 0;
                    selectingObject = tempObj;
                }
                else
                {
                    selectionProgress += deltaTime;
                    if (selectionProgress >= timeToSelect)
                    {
                        selectTower(tempObj);
                        selectionProgress = 0;
                        selectingObject = null;
                        isTowerSnapped = false;
                    }
                    else
                    {
                        cursor.updateCursor(selectionProgress / timeToSelect);
                    }
                }
            }
            else
            {
                selectionProgress = 0;
                selectingObject = null;
            }
        }

        return true;
    }

    private void updateCameraPosition(float deltaTime)
    {
        if (experimentState.gameState == ExperimentState.GameState.TowerDefence_Test)
        {
            GameObject cameraSnap = findObjectByTagInRay(ray, "CameraSnap");
            if (camSnapTemp == null || cameraSnap != camSnapTemp)
            {
                camSnapTemp = cameraSnap;
                camSnapTimer = 0;
            }
            else if(cameraSnap != null)
            {
                camSnapTimer += deltaTime;
                if (camSnapTimer > 1.5f)
                {
                    camSnapTemp = null;
                    camSnapTimer = 0;
                    setCameraToSnap(cameraSnap, towerDefenceCameraLookAt);
                    levelManager.loggingStats[12]++;
                }
                else
                {
                    cursor.updateCursor(camSnapTimer / 1.5f);
                }
            }
            
        }
        /*else if (replayEngine.checkKey(KeyCode.F6))
        {
            setCameraToSnap(firstCameraSnapNode, otherModesCameraLookAt);
            ErrorLog.logData("Keyboard action: F6, fix camera look at to default.");
        }*/

        if(clickDebugMode)
        {
            if (Input.GetMouseButton(0))
            {
                rotationX += Input.GetAxis("Mouse X") * sensX * Time.deltaTime;
                rotationY += Input.GetAxis("Mouse Y") * sensY * Time.deltaTime;
                rotationY = Mathf.Clamp(rotationY, minY, maxY);
                transform.localEulerAngles = new Vector3(-rotationY, rotationX, 0);
            }
        }
    }

    public void setCameraToSnap(GameObject cameraSnap, Transform facing)
    {
        if (cameraSnap != null && facing != null)
        {
            transform.parent.position = cameraSnap.transform.GetChild(0).transform.position;
            transform.parent.rotation = Quaternion.LookRotation(facing.position - transform.parent.position);
            //new Vector3(0, 0, 0) - transform.position);

            foreach (GameObject curSnap in cameraSnaps)
            {
                if (curSnap == cameraSnap)
                {
                    curSnap.SetActive(false);
                }
                else
                {
                    curSnap.SetActive(true);
                }
            }

            // Make sure to clear the cache of events. Often are rotations that can trigger periphery events when 
            // teleporting the camera around the place.
            peripheryBehaviour.ignoreEventsForNext(peripheryBehaviour.thresholdTime);
        }
    }

    private GameObject findObjectByTagInRay(Ray ray, string tag)
    {
        RaycastHit[] hit;
        hit = Physics.RaycastAll(ray);
        if (hit.Length == 0)
            return null;

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].transform.tag.Equals(tag))
            {
                return hit[i].transform.gameObject;
            }
        }
        return null;
    }

    private GameObject findTowerInRay(Ray ray)
    {
        return findObjectByTagInRay(ray, "Tower");
        /*
        RaycastHit[] hit;
        hit = Physics.RaycastAll(ray);
        if (hit.Length == 0)
            return null;

        for (int i = 0; i < hit.Length; i++)
        {
            if (hit[i].transform.name.StartsWith("Tower"))
            {
                //print("Tower hit.");
                //selectedObject = hit[i].transform.gameObject;
                //selectedTower = selectedObject.GetComponent<TowerBehaviour>();
                //print("Object collected");
                return hit[i].transform.gameObject;
            }
        }
        return null;*/
    }

    public void selectTower(GameObject tower)
    {
        selectedObject = tower;

        if (tower != null)
        {
            selectedTower = tower.GetComponent<TowerBehaviour>();
            /*if (selectedTower != null)
            {
                foreach(TowerSnapBehaviour snapPoint in towerSnapPoints)
                {
                    snapPoint.gameObject.GetComponent<MeshRenderer>().enabled = true;
                }
                //selectedTower.setTowerState(TowerBehaviour.TowerState.TowerPlacement);
            }*/
        }
    }

    public void deSelectTower()
    {
        tempSnapCache.associatedTower = selectedTower.gameObject;
        if(selectedTower != null)
        {
            selectedTower.setTowerState(TowerBehaviour.TowerState.TowerAIControlled);
        }

        selectedObject = null;
        selectedTower = null;

        foreach (TowerSnapBehaviour snapPoint in towerSnapPoints)
        {
            snapPoint.gameObject.GetComponent<MeshRenderer>().enabled = false;
        }

        levelManager.updateMenuRightText();
    }

    public void forceDeselectImmediately()
    {
        selectedObject = null;
        selectedTower = null;
        selectingObject = null;
    }

    public bool attemptDeSelectTower(float deltaTime)
    {
        if (selectedTower != null && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            if(isTowerSnapped)
            {
                if (deselectionProgress >= timeToSelect)
                {
                    deSelectTower();
                    return true;
                }
                else
                {
                    deselectionProgress += deltaTime;
                    cursor.updateCursor(deselectionProgress / timeToSelect);
                    return false;
                }
            }
            else
            {
                deselectionProgress = 0;
            }
        }
        deselectionProgress = 0;
        return false;
    }

    private void updateSelectedTower()
    {
        if (selectedTower != null && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement)
        {
            if (useTerrainSnap)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 9999.0f, 1 << 8))
                {
                    //print(hit.transform.name);
                    // get the hit point:
                    Vector3 newPoint = ray.GetPoint(hit.distance);
                    newPoint.y += 1.59f;

                    float distanceSnap = 99999;
                    Vector3 snapLocation = newPoint;
                    isTowerSnapped = false;
                    foreach (TowerSnapBehaviour towerSnapPoint in towerSnapPoints)
                    {
                        if (!towerSnapPoint.isAvailable()) { continue; }

                        float distanceTemp = Vector3.Distance(towerSnapPoint.snapPoint, newPoint);
                        if (distanceTemp < distanceSnap && distanceTemp < 5)
                        {
                            snapLocation = towerSnapPoint.snapPoint;
                            distanceSnap = distanceTemp;
                            isTowerSnapped = true;
                            tempSnapCache = towerSnapPoint;
                            break;
                        }
                    }

                    selectedObject.transform.position = snapLocation;
                }
            }
            else
            {
                // create a plane at 0,0,0 whose normal points to +Y:
                Plane hPlane = new Plane(Vector3.up, new Vector3(0, 1.59f, 0));
                // Plane.Raycast stores the distance from ray.origin to the hit point in this variable:
                float distance = 0;
                // if the ray hits the plane...
                if (hPlane.Raycast(ray, out distance))
                {
                    // get the hit point:
                    Vector3 newPoint = ray.GetPoint(distance);

                    float distanceSnap = 99999;
                    Vector3 snapLocation = newPoint;
                    isTowerSnapped = false;
                    foreach (TowerSnapBehaviour towerSnapPoint in towerSnapPoints)
                    {
                        float distanceTemp = Vector3.Distance(towerSnapPoint.snapPoint, newPoint);
                        if (distanceTemp < distanceSnap && distanceTemp < 1)
                        {
                            snapLocation = towerSnapPoint.snapPoint;
                            distanceSnap = distanceTemp;
                            isTowerSnapped = true;
                        }
                    }

                    selectedObject.transform.position = snapLocation;
                }
            }
        }
    }

    public TowerBehaviour getSelectedTower()
    {
        return selectedTower;
    }

    private bool keyboardAction()
    {
        if (replayEngine.checkKey(KeyCode.E))//Input.GetKeyDown(KeyCode.E))
        {
            ErrorLog.logData("Keyboard action: E, change to tower perspective.",false);
            changeToTowerPerspective();
            return true;
        }
        else if (replayEngine.checkKey(KeyCode.M))//Input.GetKeyDown(KeyCode.M))
        {
            ErrorLog.logData("Keyboard action: M, move tower.", false);
            moveSelectedTower();
            return true;
        }
        else if (replayEngine.checkKey(KeyCode.R))//Input.GetKeyDown(KeyCode.R))
        {
            ErrorLog.logData("Keyboard action: R, repair tower.", false);
            levelManager.repairSelectedTower();
            return true;
        }
        else if (replayEngine.checkKey(KeyCode.D))//Input.GetKeyDown(KeyCode.D))
        {
            ErrorLog.logData("Keyboard action: D, destroy tower.", false);
            levelManager.destroySelectedTower();
            return true;
        }
        else if (replayEngine.checkKey(KeyCode.Alpha1))//Input.GetKeyDown(KeyCode.Alpha1))
        {
            ErrorLog.logData("Keyboard action: 0, create tower id with id", false);
            spawnTower(0);
        }
        else if (replayEngine.checkKey(KeyCode.Alpha2))//Input.GetKeyDown(KeyCode.Alpha2))
        {
            ErrorLog.logData("Keyboard action: 1, create tower id with id", false);
            spawnTower(1);
        }
        else if (replayEngine.checkKey(KeyCode.Alpha3))//Input.GetKeyDown(KeyCode.Alpha3))
        {
            ErrorLog.logData("Keyboard action: 2, create tower id with id", false);
            spawnTower(2);
        }
        else if (replayEngine.checkKey(KeyCode.Alpha4))//Input.GetKeyDown(KeyCode.Alpha4))
        {
            ErrorLog.logData("Keyboard action: 3, create tower id with id", false);
            spawnTower(3);
        }
        /*else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            spawnTower(4);
        }*/

        return false;
    }

    private void changeToTowerPerspective()
    {
        ErrorLog.logData("Tower perspective triggered. THIS SHOULD NOT BE RUN BY USERS!", true);
        GameObject tempObj = findTowerInRay(ray);
        if (tempObj != null)
        {
            TowerBehaviour targettedScript = tempObj.GetComponent<TowerBehaviour>();
            if (targettedScript != null)
            {
                if (cameraState != CameraState.ControllingTower)
                {
                    lastCameraPoint = transform.position;
                }
                cameraState = CameraState.ControllingTower;

                transform.position = targettedScript.cameraControlPoint.position;
                transform.rotation = Quaternion.LookRotation(new Vector3(0,0,0) - transform.position);
            }
        }
        else if(cameraState == CameraState.ControllingTower)
        {
            cameraState = CameraState.Neutral;
            transform.position = lastCameraPoint;
            transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 0) - transform.position);
        }
    }

    public void spawnTower(int towerID)
    {
        if (towerID < 0 || towerID >= towerPrefabs.Length || (selectedTower != null && selectedTower.towerState == TowerBehaviour.TowerState.TowerPlacement))
        {
            return;
        }
        GameObject tower = (GameObject)Instantiate(towerPrefabs[towerID], new Vector3(0, 1.59f, 0), Quaternion.identity);
        levelManager.addTower(tower);
        selectTower(tower);
        moveSelectedTower();
    }

    public void moveSelectedTower()
    {
        if (selectedTower != null)
        {
            isTowerSnapped = false;
            deselectionProgress = 0;
            foreach(TowerSnapBehaviour towerSnap in towerSnapPoints)
            {
                if (towerSnap.associatedTower == selectedTower.gameObject)
                {
                    towerSnap.associatedTower = null;
                }
            }
            
            selectedTower.setTowerState(TowerBehaviour.TowerState.TowerPlacement);
            foreach (TowerSnapBehaviour snapPoint in towerSnapPoints)
            {
                snapPoint.gameObject.GetComponent<MeshRenderer>().enabled = true;
            }

            levelManager.updateMenuRightText();
        }
    }

    public void updateTowerDefence(float deltaTime)
    {
        if (keyboardAction())
        {
            // do nothing
        }
        else if (attemptSelection(deltaTime))
        {
            // do nothing
        }
        else if (clickDebugMode && isTowerSnapped && clickOccured && !clickUsed)
        {
            // Debug only deselection. Places tower back down on a snap with a click
            deSelectTower();
        }
        else if(attemptDeSelectTower(deltaTime))
        {
            // do nothing
        }
        else
        {
            updateSelectedTower();
        }

        levelManager.update(Time.deltaTime);
    }

    public void updateTowerConstruction(float deltaTime)
    {
        towerConstructionManager.update(deltaTime);

        if (experimentState.gameState != ExperimentState.GameState.TowerConstruction_Test1
            && experimentState.gameState != ExperimentState.GameState.TowerConstruction_Test2
            && experimentState.gameState != ExperimentState.GameState.TowerConstruction_Test3
            && experimentState.gameState != ExperimentState.GameState.TowerConstruction_Test4)
        {
            // do nothing
        }
        else if (selectedObject == null)
        {
            int action = peripheryBehaviour.menuBehaviour.menuAction;
            if(action > 2000 && action < 2004)
            {
                peripheryBehaviour.leftMenuDef = ""; //ExperimentState.TOWERCOMPONENTMENU;
                peripheryBehaviour.rightMenuDef = ""; //ExperimentState.TOWERCOMPONENTMENU;
                action = (action % 2000) - 1;
                createTowerComponent(action);
            }
            else if (action == 2009)
            {
                constructionSnap.removeComponent();
            }
        }
        else
        {
            //print("Running update.");
            updateSelectedContructionObject();
        }
    }

    public void createTowerComponent(int componentID)
    {
        if (componentID < 0 || componentID >= constructionPrefabs.Length || selectedObject != null)
        {
            return;
        }
        selectedObject = (GameObject)Instantiate(constructionPrefabs[componentID], new Vector3(0, 1.59f, 0), Quaternion.identity);
    }

    private void updateSelectedContructionObject()
    {
        if (selectedObject != null)
        {
            if (useTerrainSnap)
            {
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, 9999.0f, 1 << 8))
                {
                    Vector3 newPoint = ray.GetPoint(hit.distance);
                    newPoint.y += 1.59f;

                    if (Vector3.Distance(constructionSnap.snapPoint, newPoint) < 5)
                    {
                        constructionSnap.addTowerComponent(selectedObject);
                        selectedObject = null;
                        if (constructionSnap.towerComponents.Count < 6)
                        {
                            peripheryBehaviour.leftMenuDef = ExperimentState.TOWERCOMPONENTMENU;
                            peripheryBehaviour.rightMenuDef = ExperimentState.TOWERCOMPONENTMENU;
                        }
                    }
                    else
                    {
                        selectedObject.transform.position = newPoint;
                        //print("Updated to: " + newPoint);
                    }
                }
            }
            else
            {
                ErrorLog.logData("ERROR: Don't leave useTerrainSnap disabled. This function is not supported.");
            }
        }
    }
}
