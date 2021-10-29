using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ExperimentState : MonoBehaviour {

    public static int GAMEVERSION = 5;

    public enum GameSuperState { MainMenu = 0, 
                            MenuSensitivityTest = 100, 
                            TowerConstruction = 200, 
                            TowerDefence = 300, 
                            PeripheryHover = 400,
                            Complete = 500 };

    public PeripheryBehaviour peripheryBehaviour;
    public List<GameObject> stateObjects;

    private int[] maxStateOfEach = new int[]{4,7,7,8,3,0};

    public GameObject menuInformation;
    public GameObject menuQuestion;

    private InformationMenuBehaviour infoMenuBehaviour;
    private QuestionMenuBehaviour questionMenuBehaviour;
    public Transform menuPositionMainMenu;

    private bool firstUpdate = true;

    private int[] questionResponses = new int[6];

    public int skipToState = 0;

    public ExtraDataRecorder extraDataRecorder;
    public ExtraDataRecorder.ExtraDataCollection timeEventLog;
    public ExtraDataRecorder.ExtraDataCollection questionEventLog;
    public float timeSinceLastState = 0;

    public enum GameState
    {
        MainMenu = 0,
        MainMenu_Intro01 = 1,       // Welcome and thankyou; evaluation for this experiment should consider 
                                    //if periphery menu system is useful and not based on the quality of the game
        MainMenu_Intro02 = 2,       // Show stages of experiment
        MainMenu_Intro03 = 3,       // Explain after hitting continue that the user should look either left or right 
        MainMenu_WaitForBegin = 4,

        MenuSensitivityTest = 100,
        MenuSensitivityTest_Intro1 = 101,
        MenuSensitivityTest_Intro2 = 102,
        MenuSensitivityTest_Test = 103,
        MenuSensitivityTest_QuestionIntro = 104,
        MenuSensitivityTest_Question1 = 105,
        MenuSensitivityTest_Question2 = 106,
        MenuSensitivityTest_Question3 = 107,

        TowerConstruction = 200,
        TowerConstruction_Intro1 = 201,
        TowerConstruction_Intro2 = 202,
        TowerConstruction_Test1 = 203,
        TowerConstruction_Test2 = 204,
        TowerConstruction_Test3 = 205,
        TowerConstruction_Test4 = 206,
        TowerConstruction_Question1 = 207,

        TowerDefence = 300,
        TowerDefence_Intro01 = 301,
        TowerDefence_Intro02 = 302,
        TowerDefence_Intro03 = 303,
        TowerDefence_Intro04 = 304,
        TowerDefence_Intro05 = 305,
        TowerDefence_Intro06 = 306,
        TowerDefence_Test = 307,
        TowerDefence_Question1 = 308,

        PeripheryHover = 400,
        PeripheryHover_Intro01 = 401,
        PeripheryHover_Test = 402,
        PeripheryHover_Question1 = 403,

        Complete = 500
    };

    // Menu definitions
    public const string TOWERCOMPONENTMENU = "Add Component,Damage +5%,2001,Range +20%,2002,Fire Rate -5%,2003,Remove Component,2009";
    public const string MAINMENUWAITFORBEGINMENU = "Main Menu,Select Any To Begin,-1,Select Any To Begin,-1,Select Any To Begin,-1,Select Any To Begin,-1";
    public const string SENSITIVITYTESTMENULEFT = "Menu Left Revealed,Select Any To Hide,1,Then Return,1,To The,1,Centre Panel Again,1";
    public const string SENSITIVITYTESTMENURIGHT = "Menu Right Revealed,Select Any To Hide,1,Then Return,1,To The,1,Centre Panel Again,1";
    public const string TOWERDEFENCEMENULEFT = "Modify Tower,Move Tower,3000,Repair Tower [-%R%],3001,Destroy Tower [+%D%],3002,Deselect Tower,3003";
    public const string TOWERDEFENCEMENURIGHT = "Create Tower,%B%,1000,%F%,1001,%S%,1002,%E%,1003";

    public const string FINALPERIPHERYMENULEFT = "Modify Tower,Move Tower,5000,Repair Tower [-50],5000,Destroy Tower [+50],5000,Deselect Tower,5000";
    public const string FINALPERIPHERYMENURIGHT = "Create Tower,Basic Tower [30],5000,Frost Tower [40],5000,Swarm Tower [50],5000,Explosive Tower [50],5000";

    // Main menu dialogue
    private const string MAINMENUINTROONE = "Welcome to this short experiment. Thank you for taking the time to participate, your input is greatly appreciated. "
                                          + "While evaluating this experiment for any questions that are asked consider the hands free nature of the menu."
                                          + " The game itself is just a means to demonstrate a few simple examples where this menu could be applied.";
    private const string MAINMENUINTROTWO = "There will be three stages you will be asked to complete for this experiment.\n"
                                          + "1. Menu Sensitivity Test\n2. Tower Construction Test\n3. Tower Defence Minigame";
    private const string MAINMENUINTROTHREE = "If you have any questions or run into problems at any point please ask."
                                            + "\nAfter selecting Continue on this dialogue you should look quickly to the left or right to reveal the main menu. "
                                            + "Selecting any option there will begin the first task.";

    // Sensitivity dialogue
    private const string SENSITIVITYINTROONE = "For this first task you will be asked to reveal or not reveal a menu like the one you made appear for the main menu."
                                             + "The panel that will appear in the middle will show you the current task on a button at the bottom. Follow the instructions."
                                             + "There will be 24 stages that have been placed in a random order.";
    private const string SENSITIVITYINTROTWO = "The difference between sensitivities is as follows:\n"
                                             + "1. Low Sensitivity: Will make it easier to trigger a menu.  You can turn your head more slowly left or right.\n"
                                             + "2. Medium Sensitivity: A baseline for how quickly you need to turn your head to trigger a menu.\n"
                                             + "3. High Sensitivity: Will require faster turning of the head and more direct rotation left or right.";

    // Construction dialogue
    private const string CONSTRUCTIONINTROONE = "In this game there are four different types of towers:\n\n"
                                              + "1. Basic Tower: Fires normal shots periodically.\n"
                                              + "2. Frost Tower: Slows nearby enemies.\n"
                                              + "3. Swarm Tower: Swarms enemies with cubes.\n"
                                              + "4. Explosive Tower: Fires explosive shots that AoE.";
    private const string CONSTRUCTIONINTROTWO = "Your task for each of the four types of towers is to customise them with 6 component modifications. "
                                              + "You can modify the following properties:\n\n"
                                              + "1. Damage: The amount of damage the tower deals.\n"
                                              + "2. Range: How far away the tower can shoot.\n"
                                              + "3. Fire Rate: How fast the tower shoots.\n\n"
                                              + "To add components look left or right to show the menu.";

    // Tower Defence dialogue
    private const string TOWERDEFENCEINTROONE = "Now that you have prepared your customised towers you need to know the enemies you will be facing.\n\n"
                                              + "1. Basic Spider (front left): A basic medium speed unit.\n"
                                              + "2. Fast Spider (front middle): A fast moving unit.\n"
                                              + "3. Dangerous Spider (front right): A medium speed unit that will damage your towers.\n"
                                              + "4. Boss Spider (back): A slow, but heavily armoured queen.\n\n"
                                              + "Successfully killing an enemy will award you gold to spend on new towers and managing existing ones.";
    private const string TOWERDEFENCEINTROTWO = "To create a tower you can make the menu appear by looking to the right. The menu shown will appear as the "
                                              + "one in front of you. Selecting a tower will then allow you to place them into the world. "
                                              + "The cost as gold to build a tower is shown in the brackets to the right. If you do not have enough, "
                                              + "than the button will show \"Insufficent Funds\" instead.";
    private const string TOWERDEFENCEINTROTHREE = "Towers that you have placed into the game can be modified in a few ways. You can hover over a tower to "
                                              + "see the durability. If you hover for long enough the tower will be selected. If you look to the left while "
                                              + "you have a tower selected the menu shown in front of you will appear.\n\n";
    private const string TOWERDEFENCEINTROFOUR = "The following expands on what the menu options mean:\n"
                                              + "1. Move Tower: This will let you move a tower (only between rounds).\n"
                                              + "2. Repair Tower: Towers with no durability remaining can't attack, make sure to repair towers proactively.\n"
                                              + "3. Destroy Tower: This option will destroy the tower and refund you a portion of the tower's cost.\n"
                                              + "4. Deselect Tower: Will deselect the tower.";
    private const string TOWERDEFENCEINTROFIVE = "The goal of a tower defence is to prevent the enemy units from reaching the base at the end. "
                                              + "Choose your towers carefully, and use them to ensure the enemy forces will not reach your base. "
                                              + "There will be three phases with time between for moving your towers if desired. Each phase consists of "
                                              + "three waves. \nTo assist with your view of the map these yellow cubes are above in the sky. If you target one "
                                              + "it will change your camera's position to that location.";
    private const string TOWERDEFENCEINTROSIX = "When you continue from this screen you will have a period of time to place your initial towers before the first wave "
                                              + "begins to spawn. \n\nIf you have any additional questions about how to play you should ask the researcher now.\n\n"
                                              + "Once you hit continue the tower defence will begin.";

    // Periphery Final text
    private const string PEERIPHERYFINALINTROONE = "The following part will display a menu preview to your left and right. All you need to do is consider how having "
                                                + "these or a similar type of menu preview would influence how you use the menus.";
    private const string PERIPHERYFINALTASKTEXT = "Try revealing a menu by looking left and right. Just to get a feel of this different configuration with the menu previews.\n\n"
                                                + "When you are ready you can hit continue to answer one final question before the use of the Oculus Rift is complete.";


    // Sensitivity Test Questions
    private const string SENSITIVITYQUESTIONINTRO = "You have now completed a variety of different combinations using the different sensitivity settings. "
                                                  + "The following three questions will ask you to indicate how effective you felt each of the sensitivites were.";
    private const string SENSITIVITYQUESTIONONE = "On a scale of 1 to 5, where 1 is low and 5 is high, how effective was the low sensitivity?";
    private const string SENSITIVITYQUESTIONTWO = "On a scale of 1 to 5, where 1 is low and 5 is high, how effective was the medium sensitivity?";
    private const string SENSITIVITYQUESTIONTHREE = "On a scale of 1 to 5, where 1 is low and 5 is high, how effective was the high sensitivity?";

    // Tower Construction Questions
    private const string CONSTRUCTIONQUESTIONONE = "On a scale of 1 to 5, where 1 is not useful, and 5 is very useful, how useful was the periphery vision menu for"
                                                 + " completing the construction task considering that it was untimed?";
    
    // Tower Defence Questions
    private const string TOWERDEFENCEQUESTIONONE = "On a scale of 1 to 5, where 1 is not useful, and 5 is very useful, how useful was the periphery vision menu for"
                                                 + " completing the tower defence task considering that it was partially timed?";

    // Periphery Final Questions
    private const string PERIPHERYQUESTIONONE = "On a scale of 1 to 5, where 1 is not useful, and 5 is very useful, how useful do you find the addition of the periphery "
                                                + "indicators for knowing when a menu is available?";

    public struct GameStateMenuDef
    {
        public GameState gameState;
        public string menuDefLeft, menuDefRight, menuDefOtherTitle, menuDefOtherBody;
        public int showMenu;

        public GameStateMenuDef(GameState gameState, string menuDefLeft, string menuDefRight, string menuDefOtherTitle, string menuDefOtherBody, int showMenu)
        {
            this.gameState = gameState;
            this.menuDefLeft = menuDefLeft;
            this.menuDefRight = menuDefRight;
            this.menuDefOtherTitle = menuDefOtherTitle;
            this.menuDefOtherBody = menuDefOtherBody;
            this.showMenu = showMenu; // 0 = none, 1 = info screen in main menu, 2 = question in main menu, 3 = info screen in tower defence, 4 = question screen in tower defence
        }
    }

    public List<List<GameStateMenuDef>> stateStrings = new List<List<GameStateMenuDef>> {
        // MainMenu
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.MainMenu,"","","","",0),
            new GameStateMenuDef(GameState.MainMenu_Intro01,"","","Main Menu: Introduction 1/3",MAINMENUINTROONE,1),
            new GameStateMenuDef(GameState.MainMenu_Intro02,"","","Main Menu: Introduction 2/3",MAINMENUINTROTWO,1),
            new GameStateMenuDef(GameState.MainMenu_Intro03,"","","Main Menu: Introduction 3/3",MAINMENUINTROTHREE,1),
            new GameStateMenuDef(GameState.MainMenu_WaitForBegin,MAINMENUWAITFORBEGINMENU,MAINMENUWAITFORBEGINMENU,"","",0),
        },
        
        // MenuSensitivityTest
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.MenuSensitivityTest,"","","","",0),
            new GameStateMenuDef(GameState.MenuSensitivityTest_Intro1,"","","Menu Sensitivity Test: Introduction 1/2",SENSITIVITYINTROONE,1),
            new GameStateMenuDef(GameState.MenuSensitivityTest_Intro2,"","","Menu Sensitivity Test: Introduction 2/2",SENSITIVITYINTROTWO,1),
            new GameStateMenuDef(GameState.MenuSensitivityTest_Test,SENSITIVITYTESTMENULEFT,SENSITIVITYTESTMENURIGHT,"","",0),
            new GameStateMenuDef(GameState.MenuSensitivityTest_QuestionIntro,"","","Menu Sensitivity Test: Questions",SENSITIVITYQUESTIONINTRO,1),
            new GameStateMenuDef(GameState.MenuSensitivityTest_Question1,"","","User Response Question 1/3 (Low Sensitivity)",SENSITIVITYQUESTIONONE,2),
            new GameStateMenuDef(GameState.MenuSensitivityTest_Question2,"","","User Response Question 2/3 (Med Sensitivity)",SENSITIVITYQUESTIONTWO,2),
            new GameStateMenuDef(GameState.MenuSensitivityTest_Question3,"","","User Response Question 3/3 (High Sensitivity)",SENSITIVITYQUESTIONTHREE,2)
        },

        // Tower Construction
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.TowerConstruction,"","","","",0),
            new GameStateMenuDef(GameState.TowerConstruction_Intro1,"","","Tower Construction: Introduction 1/2",CONSTRUCTIONINTROONE,1),
            new GameStateMenuDef(GameState.TowerConstruction_Intro2,"","","Tower Construction: Introduction 2/2",CONSTRUCTIONINTROTWO,1),
            new GameStateMenuDef(GameState.TowerConstruction_Test1,TOWERCOMPONENTMENU, TOWERCOMPONENTMENU,"","",0),
            new GameStateMenuDef(GameState.TowerConstruction_Test2,TOWERCOMPONENTMENU, TOWERCOMPONENTMENU,"","",0),
            new GameStateMenuDef(GameState.TowerConstruction_Test3,TOWERCOMPONENTMENU, TOWERCOMPONENTMENU,"","",0),
            new GameStateMenuDef(GameState.TowerConstruction_Test4,TOWERCOMPONENTMENU, TOWERCOMPONENTMENU,"","",0),
            new GameStateMenuDef(GameState.TowerConstruction_Question1,"","","User Response Question",CONSTRUCTIONQUESTIONONE,2)
        },

        // Tower Defence
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.TowerDefence,"","","","",0),
            new GameStateMenuDef(GameState.TowerDefence_Intro01,"","","Tower Defence: Introduction 1/6",TOWERDEFENCEINTROONE,1),
            new GameStateMenuDef(GameState.TowerDefence_Intro02,"","","Tower Defence: Introduction 2/6",TOWERDEFENCEINTROTWO,1),
            new GameStateMenuDef(GameState.TowerDefence_Intro03,"","","Tower Defence: Introduction 3/6",TOWERDEFENCEINTROTHREE,1),
            new GameStateMenuDef(GameState.TowerDefence_Intro04,"","","Tower Defence: Introduction 4/6",TOWERDEFENCEINTROFOUR,1),
            new GameStateMenuDef(GameState.TowerDefence_Intro05,"","","Tower Defence: Introduction 5/6",TOWERDEFENCEINTROFIVE,1),
            new GameStateMenuDef(GameState.TowerDefence_Intro06,"","","Tower Defence: Introduction 6/6",TOWERDEFENCEINTROSIX,1),
            new GameStateMenuDef(GameState.TowerDefence_Test,TOWERDEFENCEMENULEFT,TOWERDEFENCEMENURIGHT,"","",0),
            new GameStateMenuDef(GameState.TowerDefence_Question1,"","","User Response Question",TOWERDEFENCEQUESTIONONE,2)
        },

        // Tower Defence
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.PeripheryHover,"","","","",0),
            new GameStateMenuDef(GameState.PeripheryHover_Intro01,"","","Periphery Preview Introduction 1/1",PEERIPHERYFINALINTROONE,1),
            new GameStateMenuDef(GameState.PeripheryHover_Test,FINALPERIPHERYMENULEFT,FINALPERIPHERYMENURIGHT,"Periphery Preview Task",PERIPHERYFINALTASKTEXT,1),
            new GameStateMenuDef(GameState.PeripheryHover_Question1,"","","User Response Question",PERIPHERYQUESTIONONE,2),
        },

        // Complete
        new List<GameStateMenuDef>{
            new GameStateMenuDef(GameState.Complete,"","","Testing Complete","Thank you for participating!",1)
        },
    };


    public GameSuperState gameSuperState = GameSuperState.MainMenu;
    public GameState gameState = GameState.MainMenu;

    public ReplayEngine replayEngine;
    public CameraBehaviour camRef;

	// Use this for initialization
	void Start () {
        peripheryBehaviour = gameObject.GetComponent<PeripheryBehaviour>();
        replayEngine = gameObject.GetComponent<ReplayEngine>();
        camRef = gameObject.GetComponent<CameraBehaviour>();
        for(int i = 0; i < questionResponses.Length; i++)
        {
            questionResponses[i] = 0;
        }

        extraDataRecorder = ExtraDataRecorder.getSingleton();
        timeEventLog = extraDataRecorder.getDataCollection("Time Log");
        timeEventLog.logData("Timestamp,GameSuperState,GameState,timeSinceLastState", false);
        questionEventLog = extraDataRecorder.getDataCollection("User Response Questions");
        questionEventLog.logData("Timestamp,Question,Response", false);

        int setSkipTo = 0;
        bool success = Settings.parseIntSetting("SkipToStage", ref setSkipTo);
        if(success)
        {
            skipToState = setSkipTo;
        }

        int gameSettingVersion = GAMEVERSION;
        success = Settings.parseIntSetting("GameVersion", ref gameSettingVersion);
        if (success)
        {
            GAMEVERSION = gameSettingVersion;
        }
	}
	
	// Update is called once per frame
	public void update(float deltaTime)
    {
        // disable all states until they are needed.
        if (firstUpdate)
        {
            infoMenuBehaviour = menuInformation.GetComponent<InformationMenuBehaviour>();
            questionMenuBehaviour = menuQuestion.GetComponent<QuestionMenuBehaviour>();

            for (int i = 1; i < stateObjects.Count; i++)
            {
                stateObjects[i].SetActive(false);
            }
            firstUpdate = false;
        }
        timeSinceLastState += deltaTime;

        if (replayEngine.checkKey(KeyCode.F1))
        {
            applyAutoSave();
        }

        // Push into next state after one frame of beginning.
        if (((int)gameState) % 100 == 0)
        {
            nextState();
        }

        infoMenuBehaviour.update(deltaTime);
        questionMenuBehaviour.update(deltaTime);

        if (infoMenuBehaviour.menuAction == -1 || peripheryBehaviour.menuBehaviour.menuAction == -1 || questionMenuBehaviour.menuAction == -1)
        {
            testStoreQuestionResponse();
            nextState();
        }
        else if (replayEngine.checkKey(KeyCode.F12))//Input.GetKeyDown(KeyCode.F12))
        {
            //print("KeyCode F12");
            questionMenuBehaviour.menuResult = -2;
            testStoreQuestionResponse();
            nextState();
        }
        else if ((int)gameState < skipToState)
        {
            nextState();
        }
        else if(gameState == GameState.TowerDefence_Test && camRef.levelManager.levelState == LevelManager.LEVELSTATE.Complete)
        {
            nextState();
        }
	}

    public void nextState()
    {
        if (gameSuperState == GameSuperState.Complete)
        {
            timeSinceLastState = 0;
            return;
        }
        // Hide the menu to make it disappear between skips using the shortcut keys
        if (infoMenuBehaviour != null)
        {
            infoMenuBehaviour.menuActive = false;
        }
        if (questionMenuBehaviour != null)
        {
            questionMenuBehaviour.menuActive = false;
        }

        timeEventLog.logData(gameSuperState + "," + gameState + "," + timeSinceLastState,true);
        timeSinceLastState = 0;

        int curState = (int)gameState;
        int curStateSuper = (curState / 100); // truncate last two digits
        int gameStateIndex;
        if (curState % 100 + 1 > maxStateOfEach[curStateSuper])
        {
            stateObjects[curStateSuper].SetActive(false);
            curStateSuper += 1;
            gameStateIndex = 0;
            gameState = (GameState)(curStateSuper * 100);
            gameSuperState = (GameSuperState)(curStateSuper * 100);
            stateObjects[curStateSuper].SetActive(true);

            replayEngine.autoSave.setProperty("GameState", (int)gameState + "");
            if(gameState == GameState.TowerConstruction)
            {
                replayEngine.autoSave.setProperty("SensitivityTaskOrder", camRef.menuSensitivityTest.serialiseSensitivityData(camRef.menuSensitivityTest.taskOrderData));
            }
            else if(gameState == GameState.TowerDefence)
            {
                if (replayEngine.replayMode != ReplayEngine.ReplayMode.Replay)
                {
                    replayEngine.autoSave.setProperty("ConstructionPreferences", camRef.towerConstructionManager.serialiseTowerConfig());
                }

                camRef.towerConstructionManager.loadFromAutoSave();                
            }
            AutoSave.saveAutoSave(replayEngine.autoSave);
        }
        else {
            gameState++;
            gameStateIndex = ((int)gameState) % 100;
        }

        peripheryBehaviour.leftMenuDef = stateStrings[curStateSuper][gameStateIndex].menuDefLeft;
        peripheryBehaviour.rightMenuDef = stateStrings[curStateSuper][gameStateIndex].menuDefRight;

        if (stateStrings[curStateSuper][gameStateIndex].showMenu == 1)
        {
            if (infoMenuBehaviour == null)
            {
                infoMenuBehaviour = menuInformation.GetComponent<InformationMenuBehaviour>();
            }

            infoMenuBehaviour.showMenu(stateStrings[curStateSuper][gameStateIndex].menuDefOtherTitle, stateStrings[curStateSuper][gameStateIndex].menuDefOtherBody);
            menuInformation.transform.position = menuPositionMainMenu.position;
        }
        else if (stateStrings[curStateSuper][gameStateIndex].showMenu == 2)
        {
            if (questionMenuBehaviour == null)
            {
                questionMenuBehaviour = menuQuestion.GetComponent<QuestionMenuBehaviour>();
            }

            questionMenuBehaviour.showMenu(stateStrings[curStateSuper][gameStateIndex].menuDefOtherTitle, stateStrings[curStateSuper][gameStateIndex].menuDefOtherBody);
            menuQuestion.transform.position = menuPositionMainMenu.position;
        }

        if(gameState == GameState.TowerDefence_Test)
        {
            camRef.setCameraToSnap(camRef.towerDefenceFirstSnap, camRef.towerDefenceCameraLookAt);
            // Trigger a proper periphery tower menu update
            camRef.levelManager.addCurrency(0);
            camRef.levelManager.nextState();
        }
        else if(gameState == GameState.TowerDefence_Question1)
        {
            camRef.setCameraToSnap(camRef.firstCameraSnapNode, camRef.otherModesCameraLookAt);
        }
        
        if(gameState != GameState.MenuSensitivityTest)
        {
            peripheryBehaviour.setSensitivity(PeripheryBehaviour.Sensitivity.Medium);
        }

        ErrorLog.logData("New State: " + gameState, true);
        //print("New State: " + gameState);
    }

    private void testStoreQuestionResponse()
    {
        if (questionMenuBehaviour.menuAction != -1)
        {
            return;
        }

        switch(gameState)
        {
            case GameState.MenuSensitivityTest_Question1:
                questionResponses[0] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.MenuSensitivityTest_Question1: " + questionResponses[0], false);
                questionEventLog.logData(gameState + "," + questionResponses[0], true);
                break;
            case GameState.MenuSensitivityTest_Question2:
                questionResponses[1] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.MenuSensitivityTest_Question2: " + questionResponses[1], false);
                questionEventLog.logData(gameState + "," + questionResponses[1], true);
                break;
            case GameState.MenuSensitivityTest_Question3:
                questionResponses[2] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.MenuSensitivityTest_Question3: " + questionResponses[2], false);
                questionEventLog.logData(gameState + "," + questionResponses[2], true);
                break;
            case GameState.TowerConstruction_Question1:
                questionResponses[3] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.TowerConstruction_Question1: " + questionResponses[3], false);
                questionEventLog.logData(gameState + "," + questionResponses[3], true);
                break;
            case GameState.TowerDefence_Question1:
                questionResponses[4] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.TowerDefence_Question1: " + questionResponses[4], false);
                questionEventLog.logData(gameState + "," + questionResponses[4], true);
                break;
            case GameState.PeripheryHover_Question1:
                questionResponses[5] = questionMenuBehaviour.menuResult+1;
                ErrorLog.logData("GameState.PeripheryHover_Question1: " + questionResponses[5], false);
                questionEventLog.logData(gameState + "," + questionResponses[5], true);
                break;
        }
    }

    public void applyAutoSave()
    {
        replayEngine.useLastAutoSave();
        string s = replayEngine.autoSave.getPropertyValue((replayEngine.replayMode == ReplayEngine.ReplayMode.Replay) ? "SkipUsed":"GameState");
        ErrorLog.logData("Trying to parse data: " + s, true);
        //print("Trying to parse data: " + s);
        if (s.Length == 0)
        {
            // Can't apply because there is no jump state
            //print("Can't jump to state because no state was supplied by auto save.");
            ErrorLog.logData("Can't jump to state because no state was supplied by auto save.", true);
            return;
        }
        int i = int.Parse(s);
        i = i - i % 100;
        skipToState = i;

        if (skipToState > (int)gameState)
        {
            replayEngine.autoSave.setProperty("SkipUsed", skipToState + "");
            AutoSave.saveAutoSave(replayEngine.autoSave);
        }
    }
}
