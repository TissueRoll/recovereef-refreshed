using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;

	#region Things I Plug in Unity
#pragma warning disable 0649
	[SerializeField] private CameraFollow cameraFollow;
	[SerializeField] private Tilemap coralTileMap;
	[SerializeField] private Tilemap groundTileMap;
	[SerializeField] private Tilemap substrataTileMap;
	[SerializeField] private Tilemap substrataOverlayTileMap;
	[SerializeField] private Tilemap algaeTileMap;
	[SerializeField] private TileBase[] coralTileBases;
	[SerializeField] private TileBase[] coralDeadTileBases;
	[SerializeField] private TileBase[] groundTileBases;
	[SerializeField] private TileBase[] algaeTileBases;
	[SerializeField] private TileBase[] substrataTileBases;
	[SerializeField] private TileBase[] toxicTileBases;
	[SerializeField] private TileBase algaeEdgeTileBase;
	[SerializeField] private GameObject fishDisplay;
	[SerializeField] private GameObject fishImage;
	[SerializeField] private GameObject timeLeft;
	[SerializeField] private GameObject feedbackText;
	[SerializeField] private GameObject CNC;
	//[SerializeField] private GameObject[] CoralOptions;
	[SerializeField] private TileBase toxicOverlay;
	[SerializeField] private GameObject popupCanvas;
	//[SerializeField] private Sprite emptyRack;
	//[SerializeField] private Sprite[] smallRack;
	//[SerializeField] private Sprite[] bigRack;
	[SerializeField] private GameObject endGameScreen;
	[SerializeField] private Sprite gameWinWordArt;
	[SerializeField] private Sprite gameLoseWordArt;
	[SerializeField] private GameObject ccTimerImage;
	[SerializeField] private GameObject ccOverlay;
	[SerializeField] private int level;
	[SerializeField] private int boardSize;
#pragma warning restore 0649
	#endregion
	#region Components of GameObjects
	Grid grid;
	TMPro.TextMeshProUGUI fishDisplayText;
	UnityEngine.UI.Image fishImageImage;
	TMPro.TextMeshProUGUI timeLeftText;
	TMPro.TextMeshProUGUI ccTimerText;
	ClimateChangeTimer ccTimer;
	//UnityEngine.UI.Image[,] coralIndicators;
	//UnityEngine.UI.Image[,] coralRackIndicators;
	TMPro.TextMeshProUGUI cncText;
	GameEnd endGameScript;
	PopupScript popupScript;
	TMPro.TextMeshProUGUI feedbackTextText;
	private void InitializeComponents()
	{
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		fishDisplayText = fishDisplay.GetComponent<TMPro.TextMeshProUGUI>();
		fishImageImage = fishImage.GetComponent<UnityEngine.UI.Image>();
		timeLeftText = timeLeft.GetComponent<TMPro.TextMeshProUGUI>();
		ccTimerText = ccTimerImage.transform.Find("CCTimeLeft").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
		ccTimer = ccTimerImage.GetComponent<ClimateChangeTimer>();
		cncText = CNC.GetComponent<TMPro.TextMeshProUGUI>();
		endGameScript = endGameScreen.GetComponent<GameEnd>();
		popupScript = popupCanvas.GetComponent<PopupScript>();
		feedbackTextText = feedbackText.GetComponent<TMPro.TextMeshProUGUI>();
	}
	#endregion
	#region Data Structures for the Game
	private Dictionary<Vector3Int, CoralCellData> coralCells;
	private Dictionary<Vector3Int, int> substrataCells;
	private Dictionary<Vector3Int, AlgaeCellData> algaeCells;
	#endregion
	#region Global Unchanging Values
	private Vector3Int[,] hexNeighbors = new Vector3Int[,] {
		{new Vector3Int(1,0,0), new Vector3Int(0,-1,0), new Vector3Int(-1,-1,0), new Vector3Int(-1,0,0), new Vector3Int(-1,1,0), new Vector3Int(0,1,0)},
		{new Vector3Int(1,0,0), new Vector3Int(1,-1,0), new Vector3Int(0,-1,0), new Vector3Int(-1,0,0), new Vector3Int(0,1,0), new Vector3Int(1,1,0)}
	};
	GlobalContainer globalVarContainer;
	CoralDataContainer coralBaseData;
	SubstrataDataContainer substrataDataContainer;
	AlgaeDataContainer algaeDataContainer;
	#endregion
	#region Global Changing Values
	private float zoom;
	private Vector3 cameraFollowPosition;
	private Vector3 savedCameraPosition;
	private bool edgeScrollingEnabled = false;
	private int selectedCoral = 0;
	private int fishIncome = 0;
	private float cfTotalProduction = 0;
	private float hfTotalProduction = 0;
	private CountdownTimer tempTimer;
	private List<NursingCoral>[] growingCorals;
	private CountdownTimer disasterTimer;
	private CountdownTimer climateChangeTimer;
	private bool climateChangeHasWarned;
	private bool climateChangeHasHappened;
	private int coralPropagationDebuff = 0;
	private int coralSurvivabilityDebuff = 0;
	private EconomyMachine economyMachine;
	private CountdownTimer timeUntilEnd;
	private bool gameIsWon;
	private List<Vector3Int> markedToDieCoral;
	private bool timeToKillCorals;
	private List<int> coralTypeNumbers;
	private Vector2 resolution;
	private Utility utility;
	private int totalCoralTypes = 3;
	#endregion

	#region Generic Helper Functions
	private Vector3Int GetMouseGridPosition()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
		Vector3Int position = grid.WorldToCell(worldPoint);
		return position;
	}
	#endregion
	#region Game-Specific Helper Functions
	private int FindIndexOfEntityFromType(string code, string type)
	{
		int index = -1;
		if (type == "coral")
		{
			for (int i = 0; i < coralBaseData.corals.Count; i++)
			{
				if (Regex.IsMatch(coralBaseData.corals[i].name, code))
				{
					index = i;
					break;
				}
			}
		}
		else if (type == "algae")
		{
			for (int i = 0; i < algaeDataContainer.algae.Count; i++)
			{
				if (Regex.IsMatch(algaeDataContainer.algae[i].name, code))
				{
					index = i;
					break;
				}
			}
		}
		else if (type == "substrata")
		{
			for (int i = 0; i < substrataDataContainer.substrata.Count; i++)
			{
				if (Regex.IsMatch(substrataDataContainer.substrata[i].name, code))
				{
					index = i;
					break;
				}
			}
		}
		if (index == -1)
			print("ERROR: Entity not found");
		return index;
	}
	private int FindIndexOfEntityFromName(string nameOfTileBase)
	{
		int index = -1;
		if (Regex.IsMatch(nameOfTileBase, ".*coral.*"))
		{
			for (int i = 0; i < coralBaseData.corals.Count; i++)
			{
				if (Regex.IsMatch(nameOfTileBase, ".*coral_" + coralBaseData.corals[i].name + ".*"))
				{
					index = i;
					break;
				}
			}
		}
		else if (Regex.IsMatch(nameOfTileBase, ".*algae.*"))
		{
			for (int i = 0; i < algaeDataContainer.algae.Count; i++)
			{
				if (Regex.IsMatch(nameOfTileBase, ".*algae_" + algaeDataContainer.algae[i].name + ".*"))
				{
					index = i;
					break;
				}
			}
		}
		else if (Regex.IsMatch(nameOfTileBase, ".*substrata.*"))
		{
			for (int i = 0; i < substrataDataContainer.substrata.Count; i++)
			{
				if (Regex.IsMatch(nameOfTileBase, ".*substrata_" + substrataDataContainer.substrata[i].name + ".*"))
				{
					index = i;
					break;
				}
			}
		}
		if (index == -1)
			print("ERROR: Entity not found");
		return index;
	}
	public void ChangeCoral(int select) => selectedCoral = select;
	private int GetCoralsPerType(int type)
	{
		// TODO
		int result = 0; 
		result = growingCorals[type].Count; // assumes the structure changes in size
		for (int i = 0; i < growingCorals[type].Count; i++)
		{
			if (growingCorals[type][i] != null)
				result += 1;
		}
		return result;
	}
	private int GetCoralsInNursery()
	{
		int coralsInNursery = 0;
		for (int i = 0; i < totalCoralTypes; i++)
		{
			coralsInNursery += GetCoralsPerType(i);
		}
		return coralsInNursery;
	}
	private int GetReadyCoralsPerType(int type)
	{
		// TODO
		// get ready corals per type: if (growingCorals[type][i].timer.isDone())
		int ready = 0;
		ready = growingCorals[type].Count;
		for (int i = 0; i < growingCorals[type].Count; i++)
		{
			if (growingCorals[type][i] == null)
				continue;
			if (growingCorals[type][i].timer.isDone())
				ready += 1;
		}
		return ready;
	}
	private int GetIndexOfReadyCoral(int type)
	{
		int index = -1;
		for (int i = 0; i < growingCorals[type].Count; i++)
		{
			if (growingCorals[type][i] == null)
				continue;
			if (index == -1 && growingCorals[type][i].timer.isDone())
				index = i;
		}
		return index;
	}
	#endregion

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else if (instance != this)
		{
			Destroy(gameObject);
		}
		resolution = new Vector2(Screen.width, Screen.height);
		InitializeComponents();
		endGameScript.resetEndScreen();
		print("loading XML data...");
		globalVarContainer = GlobalContainer.Load("GlobalsXML");
		substrataDataContainer = SubstrataDataContainer.Load("SubstrataXML");
		coralBaseData = CoralDataContainer.Load("CoralDataXML");
		algaeDataContainer = AlgaeDataContainer.Load("AlgaeDataXML");
		totalCoralTypes = coralBaseData.corals.Count;
		zoom = globalVarContainer.globals[level].zoom;
		print("XML data loaded");
		print("initializing tiles...");
		coralTypeNumbers = new List<int>();
		for (int i = 0; i < totalCoralTypes; ++i)
		{
			coralTypeNumbers.Add(i);
		}
		InitializeTiles();
		print("initialization done");
		tempTimer = new CountdownTimer(globalVarContainer.globals[level].maxGameTime);
		disasterTimer = new CountdownTimer(30f); // make into first 5 mins immunity
		climateChangeTimer = new CountdownTimer(globalVarContainer.globals[level].timeUntilClimateChange);
		climateChangeHasWarned = false;
		climateChangeHasHappened = false;
		utility = new Utility();
		economyMachine = new EconomyMachine(10f, 0f, 5f, 15);
		timeUntilEnd = new CountdownTimer(60f);
		gameIsWon = false;
		timeToKillCorals = false;
		ccTimerImage.transform.Find("CCTimeLeft").gameObject.SetActive(false);
		ccOverlay.SetActive(false);
		print("level is " + globalVarContainer.globals[level].level);
		InitializeGame();
	}


	private void InitializeTiles()
	{
		// instantiation
		coralCells = new Dictionary<Vector3Int, CoralCellData>();
		substrataCells = new Dictionary<Vector3Int, int>();
		algaeCells = new Dictionary<Vector3Int, AlgaeCellData>();
		growingCorals = new List<NursingCoral>[totalCoralTypes];
		for (int i = 0; i < totalCoralTypes; i++)
		{
			growingCorals[i] = new List<NursingCoral>() { null, null, null, null };
		}
		markedToDieCoral = new List<Vector3Int>();

		// initialization
		// Setting the substrata data
		foreach (Vector3Int pos in substrataTileMap.cellBounds.allPositionsWithin)
		{
			Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
			if (!substrataTileMap.HasTile(localPlace)) continue;
			if (!utility.WithinBoardBounds(localPlace, boardSize))
			{
				substrataTileMap.SetTile(localPlace, null);
				continue;
			}
			TileBase currentTB = substrataTileMap.GetTile(localPlace);
			int idx = FindIndexOfEntityFromName(currentTB.name);
			if (idx == -1)
			{ // UNKNOWN TILE; FOR NOW TOXIC
				HashSet<Vector3Int> toxicSpread = utility.Spread(localPlace, 2);
				foreach (Vector3Int toxicPos in toxicSpread)
				{
					substrataOverlayTileMap.SetTile(toxicPos, toxicOverlay);
				}
			}
			else
			{
				substrataCells.Add(localPlace, substrataDataContainer.substrata[idx].groundViability);
			}
		}

		for (int i = boardSize + 1; i <= boardSize + 5; i++)
		{
			for (int j = -boardSize - 5; j <= boardSize + 5; j++)
			{
				substrataOverlayTileMap.SetTile(new Vector3Int(j, i, 0), algaeEdgeTileBase);
				substrataOverlayTileMap.SetTile(new Vector3Int(j, -i, 0), algaeEdgeTileBase);
				substrataOverlayTileMap.SetTile(new Vector3Int(i, j, 0), algaeEdgeTileBase);
				substrataOverlayTileMap.SetTile(new Vector3Int(-i, j, 0), algaeEdgeTileBase);
			}
		}

		// Setting the tiles in the tilemap to the coralCells dictionary
		foreach (Vector3Int pos in coralTileMap.cellBounds.allPositionsWithin)
		{
			Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
			if (!groundTileMap.HasTile(localPlace)) continue;
			if (!coralTileMap.HasTile(localPlace)) continue;
			if (!substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace) || !utility.WithinBoardBounds(localPlace, boardSize))
			{
				coralTileMap.SetTile(localPlace, null);
				continue;
			}
			TileBase currentTB = coralTileMap.GetTile(localPlace);
			CoralCellData cell = new CoralCellData(
				localPlace,
				coralTileMap,
				currentTB,
				26,
				coralBaseData.corals[FindIndexOfEntityFromName(currentTB.name)]
			);
			cfTotalProduction += cell.coralData.cfProduction;
			hfTotalProduction += cell.coralData.hfProduction;
			coralTypeNumbers[FindIndexOfEntityFromName(currentTB.name)]++;
			coralCells.Add(cell.LocalPlace, cell);
		}

		foreach (Vector3Int pos in algaeTileMap.cellBounds.allPositionsWithin)
		{
			Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
			if (!groundTileMap.HasTile(localPlace)) continue;
			if (!algaeTileMap.HasTile(localPlace)) continue;
			if (!substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || !utility.WithinBoardBounds(localPlace, boardSize))
			{
				algaeTileMap.SetTile(localPlace, null);
				continue;
			}
			TileBase currentTB = algaeTileMap.GetTile(localPlace);
			AlgaeCellData cell = new AlgaeCellData(
				localPlace,
				algaeTileMap,
				currentTB,
				26,
				algaeDataContainer.algae[FindIndexOfEntityFromName(currentTB.name)]
			);
			hfTotalProduction += cell.algaeData.hfProduction;
			algaeCells.Add(cell.LocalPlace, cell);
		}
	}

	private void InitializeGame()
	{
		fishDisplayText.text = "Fish Income: 0";
		if (hfTotalProduction >= cfTotalProduction)
		{
			fishImageImage.color = utility.green;
		}
		else
		{
			fishImageImage.color = utility.red;
		}
		UpdateFishData();
		timeLeftText.text = utility.ConvertTimetoMS(tempTimer.currentTime);
	}

	private void Start()
	{
		// sets the cameraFollowPosition to the default 
		cameraFollowPosition = cameraFollow.transform.position;
		cameraFollow.Setup(() => cameraFollowPosition, () => zoom);
		cameraFollow.enabled = true;
		// __FIX__ MAKE INTO GLOBALS?
		InvokeRepeating(nameof(UpdateFishData), 0f, 1.0f);
		InvokeRepeating(nameof(UpdateAllAlgae), 1.0f, 1.0f);
		InvokeRepeating(nameof(UpdateAllCoral), 2.0f, 2.0f);
		InvokeRepeating(nameof(KillCorals), 1.0f, 3.0f);
	}

	void Update()
	{
		if (GameEnd.gameHasEnded)
		{
			return;
		}

		if (PauseScript.GamePaused)
		{
			return;
		}

		// updateFishData();

		// if (resolution.x != Screen.width || resolution.y != Screen.height) {

		//     resolution.x = Screen.width;
		//     resolution.y = Screen.height;
		// }

		#region Disaster Happenings
		disasterTimer.updateTime();
		if (disasterTimer.isDone())
		{
			disasterTimer = new CountdownTimer(60f);
			RandomDisaster();
		}

		if (!climateChangeTimer.isDone())
		{
			climateChangeTimer.updateTime();
			ccTimerText.text = utility.ConvertTimetoMS(climateChangeTimer.currentTime);
		}
		if (!climateChangeHasWarned && climateChangeTimer.currentTime <= climateChangeTimer.timeDuration * (2.0 / 3.0))
		{
			climateChangeHasWarned = true;
			ccTimerImage.transform.Find("CCTimeLeft").gameObject.SetActive(true);
			ccTimer.climateChangeIsHappen();
			popupScript.makeEvent(0, "Climate Change is coming! Scientists have predicted that our carbon emmisions will lead to devastating damages to sea life in a few years! This could slow down the growth of coral reefs soon...");
		}
		else if (climateChangeHasWarned && !climateChangeHasHappened && climateChangeTimer.isDone())
		{
			climateChangeHasHappened = true;
			ccOverlay.SetActive(true);
			popupScript.makeEvent(0, "Climate Change has come! Scientists have determined that the increased temperature and ocean acidity has slowed down coral growth! We have to make a greater effort to coral conservation and rehabilitation!");
			ApplyClimateChange();
		}
		#endregion

		#region Keyboard Shortcuts
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			GrowCoral(0);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			GrowCoral(1);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			GrowCoral(2);
		}

		if (Input.GetKeyDown(KeyCode.F1) || Input.GetKeyDown(KeyCode.Z))
		{
			ChangeCoral(0);
		}
		else if (Input.GetKeyDown(KeyCode.F2) || Input.GetKeyDown(KeyCode.X))
		{
			ChangeCoral(1);
		}
		else if (Input.GetKeyDown(KeyCode.F3) || Input.GetKeyDown(KeyCode.C))
		{
			ChangeCoral(2);
		}
		#endregion

		bool rb = Input.GetMouseButtonDown(1);
		if (rb)
		{
			if (!PlantCoral(selectedCoral))
			{
				// feedbackDialogue("Cannot plant coral onto the reef", 1);
			}
		}

		#region Screen Movement
		// movement of screen
		if (Input.GetKeyDown(KeyCode.Space))
		{
			edgeScrollingEnabled = !edgeScrollingEnabled;
			FeedbackDialogue("Edge scrolling is " + (edgeScrollingEnabled ? "enabled!" : "disabled!"), globalVarContainer.globals[level].feedbackDelayTime);
		}
		#endregion

		//for (int i = 0; i < 6; i++)
		//{
		//    for (int j = 0; j < globalVarContainer.globals[level].maxSpacePerCoral; j++)
		//    {
		//        Sprite currentPositionSprite = coralRackIndicators[i, j].sprite;
		//        if (growingCorals[i][j] == null)
		//        {
		//            coralIndicators[i, j].color = progressNone;
		//            if (currentPositionSprite.name != emptyRack.name)
		//                coralRackIndicators[i, j].sprite = emptyRack;
		//            continue;
		//        }
		//        growingCorals[i][j].timer.updateTime();
		//        bool currentCoralDone = growingCorals[i][j].timer.isDone();
		//        if (!currentCoralDone)
		//            coralIndicators[i, j].color = Color.Lerp(progressZero, progressIsDone, growingCorals[i][j].timer.percentComplete);
		//        else if (currentCoralDone && coralIndicators[i, j].color != progressDefinitelyDone)
		//            coralIndicators[i, j].color = progressDefinitelyDone;
		//        if (currentCoralDone && currentPositionSprite.name != bigRack[i].name)
		//            coralRackIndicators[i, j].sprite = bigRack[i];
		//        else if (!currentCoralDone && currentPositionSprite.name != smallRack[i].name)
		//            coralRackIndicators[i, j].sprite = smallRack[i];
		//    }
		//}

		cncText.text = GetCoralsInNursery() + "/" + globalVarContainer.globals[level].maxSpaceInNursery + " SLOTS LEFT";

		tempTimer.updateTime();
		timeLeftText.text = utility.ConvertTimetoMS(tempTimer.currentTime);
		if (tempTimer.isDone())
		{
			EndTheGame("The reef could not recover...");
		}

		if (fishIncome >= globalVarContainer.globals[level].goal)
		{
			timeUntilEnd.updateTime();
		}
		else
		{
			timeUntilEnd.reset();
		}

		if (timeUntilEnd.isDone())
		{
			gameIsWon = true;
			EndTheGame("You have recovered the reef!");
		}
	}

	private void EndTheGame(string s)
	{
		endGameScript.finalStatistics(fishIncome, utility.ConvertTimetoMS(tempTimer.currentTime));
		endGameScript.setCongrats((gameIsWon ? gameWinWordArt : gameLoseWordArt));
		endGameScript.endMessage(s);
		endGameScript.gameEndReached();
	}

	private void FeedbackDialogue(string text, float time) => StartCoroutine(ShowMessage(text, time));

	IEnumerator ShowMessage(string text, float time)
	{
		feedbackTextText.text = text;
		feedbackTextText.enabled = true;
		yield return new WaitForSeconds(time);
		feedbackTextText.enabled = false;
	}

	private void UpdateFishData()
	{
		if (GameEnd.gameHasEnded || PauseScript.GamePaused)
			return;
		UpdateFishOutput();

		fishDisplayText.text = "Fish Income: " + fishIncome;
		if (hfTotalProduction >= cfTotalProduction)
		{
			if (economyMachine.isAverageGood())
			{
				fishImageImage.color = utility.gold;
			}
			else
			{
				fishImageImage.color = utility.green;
			}
		}
		else
		{
			fishImageImage.color = utility.red;
		}
	}

	// __ECONOMY__
	#region Algae Updates
	private void UpdateAllAlgae()
	{
		UpdateAlgaeSurvivability();
		UpdateAlgaePropagation();
	}

	private void UpdateAlgaeSurvivability()
	{
		List<Vector3Int> keys = new List<Vector3Int>(algaeCells.Keys);
		foreach (Vector3Int key in keys)
		{
			if (algaeCells[key].maturity <= 25)
			{
				algaeCells[key].addMaturity(1);
			}
			HashSet<Vector3Int> coralsAround = utility.Spread(key, 1);
			int weightedCoralMaturity = 0;
			foreach (Vector3Int pos in coralsAround)
			{
				if (coralCells.ContainsKey(pos))
				{
					weightedCoralMaturity += coralCells[pos].maturity;
				}
			}
			if (!economyMachine.algaeWillSurvive(algaeCells[key], substrataCells[key], -3 * weightedCoralMaturity / 2 + coralSurvivabilityDebuff))
			{
				algaeTileMap.SetTile(key, null);
				algaeCells.Remove(key);
			}
		}
	}

	private void UpdateAlgaePropagation()
	{
		// handles propagation
		// basically get list of keys, then propagate
		// https://www.redblobgames.com/grids/hexagons/
		// note: unity is using inverted odd-q; switch x and y then baliktad
		// each algae has a random propagation chance; generate a random num to roll chance

		List<Vector3Int> keys = new List<Vector3Int>(algaeCells.Keys);
		foreach (Vector3Int key in keys)
		{
			if (algaeCells[key].maturity > 25)
			{ // propagate only if "mature"
				for (int i = 0; i < totalCoralTypes; i++)
				{
					if (economyMachine.algaeWillPropagate(algaeCells[key], coralPropagationDebuff, groundTileMap.GetTile(key).name))
					{
						Vector3Int localPlace = key + hexNeighbors[key.y & 1, i];
						if (!groundTileMap.HasTile(localPlace)) continue;
						if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace)) continue;
						if (substrataOverlayTileMap.HasTile(localPlace)) continue;
						if (!utility.WithinBoardBounds(localPlace, boardSize)) continue;
						if (algaeTileMap.HasTile(localPlace) || algaeCells.ContainsKey(localPlace)) continue;
						// __ECONOMY__ __FIX__ MANUAL OVERRIDE TO CHECK IF ALGAE CAN TAKE OVER
						if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace))
						{
							int randNum = UnityEngine.Random.Range(0, 101);
							HashSet<Vector3Int> surrounding = utility.Spread(localPlace, 1);
							CoralCellData temp;
							foreach (Vector3Int tempLocation in surrounding)
							{
								if (coralCells.TryGetValue(tempLocation, out temp))
								{
									randNum -= coralCells[tempLocation].maturity / 3;
								}
							}
							if (randNum < 60) continue;
						}
						// adding algae
						AlgaeCellData cell = new AlgaeCellData(
							localPlace,
							algaeTileMap,
							algaeCells[key].TileBase,
							0,
							algaeDataContainer.algae[FindIndexOfEntityFromName(algaeCells[key].TileBase.name)]
						);
						hfTotalProduction += cell.algaeData.hfProduction;
						algaeCells.Add(cell.LocalPlace, cell);
						algaeTileMap.SetTile(cell.LocalPlace, cell.TileBase);
						// delete coral under algae
						if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace))
						{
							coralTileMap.SetTile(localPlace, null);
							hfTotalProduction -= coralCells[localPlace].coralData.hfProduction;
							cfTotalProduction -= coralCells[localPlace].coralData.cfProduction;
							coralTypeNumbers[FindIndexOfEntityFromName(coralCells[localPlace].TileBase.name)]--;
							coralCells.Remove(localPlace);
						}
					}
				}
			}
		}

	}
	#endregion

	#region Coral Updates
	private void UpdateAllCoral()
	{
		UpdateCoralSurvivability();
		UpdateCoralPropagation();
	}

	private void GrowCoral(int type)
	{
		bool spaceInNursery = GetCoralsInNursery() < globalVarContainer.globals[level].maxSpaceInNursery;
		if (spaceInNursery)
		{
			// add it to the list of growing corals
			// growingCorals[type][nullIdx] = new NursingCoral(coralBaseData.corals[type].name, new CountdownTimer(coralBaseData.corals[type].growTime));
		}
		else if (!spaceInNursery)
		{
			FeedbackDialogue("Nursery is at maximum capacity.", globalVarContainer.globals[level].feedbackDelayTime);
		}
	}

	private bool PlantCoral(int type)
	{
		bool successful = false;
		Vector3Int position = GetMouseGridPosition();
		int readyNum = GetReadyCoralsPerType(type);
		int loadedNum = GetCoralsPerType(type);
		if (!utility.WithinBoardBounds(position, boardSize))
		{
			FeedbackDialogue("Can't put corals out of bounds!", globalVarContainer.globals[level].feedbackDelayTime);
		}
		else if (coralTileMap.HasTile(position))
		{
			FeedbackDialogue("Can't put corals on top of other corals!.", globalVarContainer.globals[level].feedbackDelayTime);
		}
		else if (algaeTileMap.HasTile(position))
		{
			FeedbackDialogue("Can't put corals on top of algae! The coral will die!", globalVarContainer.globals[level].feedbackDelayTime);
		}
		else if (substrataOverlayTileMap.HasTile(position))
		{
			FeedbackDialogue("This is a toxic tile! Corals won't survive here.", globalVarContainer.globals[level].feedbackDelayTime);
		}
		else if ((substrataTileMap.HasTile(position) || substrataCells.ContainsKey(position)) && readyNum > 0)
		{
			successful = true;
			int tempIdx = GetIndexOfReadyCoral(type);
			NursingCoral tempCoral = null;
			if (tempIdx != -1)
			{
				tempCoral = growingCorals[type][tempIdx];
				growingCorals[type][tempIdx] = null;
			}
			CoralCellData cell = new CoralCellData(
				position,
				coralTileMap,
				coralTileBases[type],
				0,
				coralBaseData.corals[type]
			);
			coralCells.Add(position, cell);
			cfTotalProduction += coralCells[position].coralData.cfProduction;
			hfTotalProduction += coralCells[position].coralData.hfProduction;
			coralTypeNumbers[type]++;
			coralTileMap.SetTile(position, coralTileBases[type]);
		}
		else if (readyNum == 0 && loadedNum - readyNum > 0)
		{
			float minTime = 3600f;
			// go find the quickest to finish coral
			string t = "Soonest to mature coral of this type has " + utility.ConvertTimetoMS(minTime) + " time left.";
			FeedbackDialogue(t, globalVarContainer.globals[level].feedbackDelayTime);
		}

		return successful;
	}
	private void UpdateCoralSurvivability()
	{
		List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
		foreach (Vector3Int key in keys)
		{
			if (coralCells[key].maturity <= 25)
			{
				// check adj corals
				// miscFactors aka the amount of corals around it influences how much more they can add to the survivability of one
				// how much they actually contribue can be varied; change the amount 0.01f to something that makes more sense
				int miscFactors = 0;
				// __FIX__ MAYBE USE SPREAD?
				for (int i = 0; i < totalCoralTypes; i++)
					if (coralCells.ContainsKey(key + hexNeighbors[key.y & 1, i]))
						miscFactors += coralCells[key + hexNeighbors[key.y & 1, i]].maturity / 5;
				coralCells[key].addMaturity(1);
				if (!economyMachine.coralWillSurvive(coralCells[key], substrataCells[key], miscFactors - coralSurvivabilityDebuff, groundTileMap.GetTile(key).name))
				{
					// setting data
					coralTileMap.SetTile(key, coralDeadTileBases[FindIndexOfEntityFromName(coralCells[key].TileBase.name)]);
					markedToDieCoral.Add(key);
				}
			}
		}
	}

	private void KillCorals()
	{
		timeToKillCorals = !timeToKillCorals;
		if (!timeToKillCorals)
			return;
		foreach (Vector3Int key in markedToDieCoral)
		{
			if (!coralCells.ContainsKey(key))
				continue;
			coralTileMap.SetTile(key, null);
			hfTotalProduction -= coralCells[key].coralData.hfProduction;
			cfTotalProduction -= coralCells[key].coralData.cfProduction;
			coralTypeNumbers[FindIndexOfEntityFromName(coralCells[key].TileBase.name)]--;
			coralCells.Remove(key);
		}
		markedToDieCoral.Clear();
	}

	private void UpdateCoralPropagation()
	{
		List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
		foreach (Vector3Int key in keys)
		{
			if (coralCells[key].maturity > 25)
			{ // propagate only if "mature"
				for (int i = 0; i < totalCoralTypes; i++)
				{
					if (economyMachine.coralWillPropagate(coralCells[key], -coralPropagationDebuff, groundTileMap.GetTile(key).name))
					{
						Vector3Int localPlace = key + hexNeighbors[key.y & 1, i];
						if (!groundTileMap.HasTile(localPlace)) continue;
						if (!substrataTileMap.HasTile(localPlace) || !substrataCells.ContainsKey(localPlace) || substrataOverlayTileMap.HasTile(localPlace)) continue;
						if (!utility.WithinBoardBounds(localPlace, boardSize)) continue;
						if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || algaeTileMap.HasTile(localPlace)) continue;
						CoralCellData cell = new CoralCellData(
							localPlace,
							coralTileMap,
							coralCells[key].TileBase,
							0,
							coralBaseData.corals[FindIndexOfEntityFromName(coralCells[key].TileBase.name)]
						);
						cfTotalProduction += cell.coralData.cfProduction;
						hfTotalProduction += cell.coralData.hfProduction;
						coralTypeNumbers[FindIndexOfEntityFromName(coralCells[key].TileBase.name)]++;
						coralCells.Add(cell.LocalPlace, cell);
						coralTileMap.SetTile(cell.LocalPlace, cell.TileBase);
					}
				}
			}
		}
	}
	#endregion
	#region Disasters
	private void RandomDisaster(int forceEvent = 0)
	{
		// chance: 1/50
		// random: random area selection
		// toxic: center must be a coral
		int t = UnityEngine.Random.Range(1, 51);
		if (t == 21 || forceEvent == 1)
		{
			if (coralCells.Count > 15)
			{
				Vector3Int pos = coralCells.ElementAt(UnityEngine.Random.Range(0, coralCells.Count)).Key;
				HashSet<Vector3Int> removeSpread = utility.Spread(pos, 2);
				foreach (Vector3Int removePos in removeSpread)
				{
					if (coralCells.ContainsKey(removePos))
					{
						markedToDieCoral.Add(removePos); // __TIMING__
						coralTileMap.SetTile(removePos, coralDeadTileBases[FindIndexOfEntityFromName(coralCells[removePos].TileBase.name)]);
					}
				}
				popupScript.makeEvent(UnityEngine.Random.Range(2, 4));
			}
		}
		else if (t == 42 || forceEvent == 2)
		{
			Vector3Int pos = new Vector3Int(UnityEngine.Random.Range(-boardSize, boardSize + 1), UnityEngine.Random.Range(-boardSize, boardSize + 1), 0);
			if (substrataCells.ContainsKey(pos))
				substrataCells.Remove(pos);
			substrataTileMap.SetTile(pos, toxicTileBases[UnityEngine.Random.Range(0, toxicTileBases.Length)]);
			HashSet<Vector3Int> toxicSpread = utility.Spread(pos, 2);
			foreach (Vector3Int toxicPos in toxicSpread)
			{
				if (coralCells.ContainsKey(toxicPos))
				{
					markedToDieCoral.Add(toxicPos); // __TIMING__
					coralTileMap.SetTile(toxicPos, coralDeadTileBases[FindIndexOfEntityFromName(coralCells[toxicPos].TileBase.name)]);
				}
				if (algaeCells.ContainsKey(toxicPos))
				{
					algaeCells.Remove(toxicPos);
					algaeTileMap.SetTile(toxicPos, null);
				}
				substrataOverlayTileMap.SetTile(toxicPos, toxicOverlay);
			}
			popupScript.makeEvent(1);
		}
	}
	// __ECONOMY__
	private void ApplyClimateChange()
	{
		coralPropagationDebuff = 5;
		coralSurvivabilityDebuff = 5;
	}
	#endregion

	// __ECONOMY__
	#region Misc Updates
	private void UpdateFishOutput()
	{
		economyMachine.updateHFCF(hfTotalProduction, cfTotalProduction);
		fishIncome = (int)Math.Round(economyMachine.getTotalFish(coralTypeNumbers));
	}
	#endregion

	#region Camera Movement and Zoom
	private void MoveCameraWASD(float moveAmount)
	{
		if (Input.GetKey(KeyCode.W))
		{
			cameraFollowPosition.y += moveAmount * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.A))
		{
			cameraFollowPosition.x -= moveAmount * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.S))
		{
			cameraFollowPosition.y -= moveAmount * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.D))
		{
			cameraFollowPosition.x += moveAmount * Time.deltaTime;
		}
	}

	private void MoveCameraMouseEdge(float moveAmount, float edgeSize)
	{
		if (Input.mousePosition.x > Screen.width - edgeSize)
		{
			cameraFollowPosition.x += moveAmount * Time.deltaTime;
		}
		if (Input.mousePosition.x < edgeSize)
		{
			cameraFollowPosition.x -= moveAmount * Time.deltaTime;
		}
		if (Input.mousePosition.y > Screen.height - edgeSize)
		{
			cameraFollowPosition.y += moveAmount * Time.deltaTime;
		}
		if (Input.mousePosition.y < edgeSize)
		{
			cameraFollowPosition.y -= moveAmount * Time.deltaTime;
		}
	}

	private void ZoomKeys(float zoomChangeAmount)
	{
		if (Input.GetKey(KeyCode.Q))
		{
			zoom -= zoomChangeAmount * Time.deltaTime;
		}
		if (Input.GetKey(KeyCode.E))
		{
			zoom += zoomChangeAmount * Time.deltaTime;
		}

		if (Input.mouseScrollDelta.y > 0)
		{
			zoom -= zoomChangeAmount;
		}
		if (Input.mouseScrollDelta.y < 0)
		{
			zoom += zoomChangeAmount;
		}

		zoom = Mathf.Clamp(zoom, 5f, 30f);
	}

	private void ClampCamera()
	{
		cameraFollowPosition = new Vector3(
			Mathf.Clamp(cameraFollowPosition.x, -90.0f, 90.0f),
			Mathf.Clamp(cameraFollowPosition.y, -150.0f, 150.0f),
			cameraFollowPosition.z
		);
	}
	#endregion
}
