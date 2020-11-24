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
	[SerializeField] private CodeMonkey.MonoBehaviours.CameraFollow cameraFollow;
	[SerializeField] private Tilemap coralTileMap;
	[SerializeField] private Tilemap groundTileMap;
	[SerializeField] private Tilemap substrataTileMap;
	[SerializeField] private Tilemap substrataOverlayTileMap;
	[SerializeField] private Tilemap algaeTileMap;
	[SerializeField] private GameObject fishDisplay;
	[SerializeField] private GameObject fishImage;
	[SerializeField] private GameObject timeLeft;
	// [SerializeField] private GameObject feedbackText;
	// [SerializeField] private GameObject CNC;
	[SerializeField] private GameObject popupCanvas;
	[SerializeField] private GameObject endGameScreen;
	// [SerializeField] private GameObject ccTimerImage;
	// [SerializeField] private GameObject ccOverlay;
	[SerializeField] private int level;
	[SerializeField] private int boardSize;
#pragma warning restore 0649
	#endregion
	#region Components of GameObjects
	Grid grid;
	TMPro.TextMeshProUGUI fishDisplayText;
	UnityEngine.UI.Image fishImageImage;
	TMPro.TextMeshProUGUI timeLeftText;
	// TMPro.TextMeshProUGUI ccTimerText;
	// ClimateChangeTimer ccTimer;
	// TMPro.TextMeshProUGUI cncText;
	GameEnd endGameScript;
	PopupScript popupScript;
	// TMPro.TextMeshProUGUI feedbackTextText;
	private void InitializeComponents()
	{
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		fishDisplayText = fishDisplay.GetComponent<TMPro.TextMeshProUGUI>();
		fishImageImage = fishImage.GetComponent<UnityEngine.UI.Image>();
		timeLeftText = timeLeft.GetComponent<TMPro.TextMeshProUGUI>();
		// ccTimerText = ccTimerImage.transform.Find("CCTimeLeft").gameObject.GetComponent<TMPro.TextMeshProUGUI>();
		// ccTimer = ccTimerImage.GetComponent<ClimateChangeTimer>();
		// cncText = CNC.GetComponent<TMPro.TextMeshProUGUI>();
		endGameScript = endGameScreen.GetComponent<GameEnd>();
		popupScript = popupCanvas.GetComponent<PopupScript>();
		// feedbackTextText = feedbackText.GetComponent<TMPro.TextMeshProUGUI>();
	}
	#endregion
	#region Data Structures for the Game
	private Dictionary<Vector3Int, CoralCellData> coralCells;
	private Dictionary<Vector3Int, int> substrataCells;
	private Dictionary<Vector3Int, AlgaeCellData> algaeCells;
	#endregion
	#region Global Unchanging Values
	private readonly Vector3Int[,] hexNeighbors = new Vector3Int[,] {
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
	private int totalCoralTypes = 3;
	private bool shovelChosen = false;
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
	// __STYLE: maybe use some kind of .Find(delegate/funcptr) thing to make this cleaner
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
	// __STYLE: maybe use some kind of .Find(delegate/funcptr) thing to make this cleaner
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
	public void ChangeCoral(int select)
	{
		selectedCoral = select;
		shovelChosen = false;
	}
	public void SelectShovel()
	{
		selectedCoral = 3;
		shovelChosen = true;
	}
	private int GetCoralsPerType(int type)
	{
		return growingCorals[type].Count;
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
		int ready = 0;
		foreach (NursingCoral growingCoral in growingCorals[type])
		{
			if (growingCoral.timer.isDone())
				ready++;
		}
		return ready;
	}
	/*
	 * Programmer note: by nature of the new system, the ready coral is ALWAYS in front of the list
	 */
	private int GetIndexOfReadyCoral(int type)
	{
		int index = -1;
		for (int i = 0; i < growingCorals[type].Count; i++)
		{
			if (index == -1 && growingCorals[type][i].timer.isDone())
				index = i;
		}
		return index;
	}
	/*
	 * Adds an algae on the map if possible
	 */
	private void AddAlgaeOnMap(Vector3Int position, TileBase tileBase, int maturity)
	{
		int type = FindIndexOfEntityFromName(tileBase.name);
		AlgaeCellData cell = new AlgaeCellData(position, algaeTileMap, tileBase, maturity, algaeDataContainer.algae[type]);
		hfTotalProduction += cell.algaeData.hfProduction;
		algaeCells.Add(position, cell);
		algaeTileMap.SetTile(position, tileBase);
	}

	/*
	 * Adds a coral on the map if possible
	 */
	private void AddCoralOnMap(Vector3Int position, TileBase tileBase, int maturity)
	{
		int type = FindIndexOfEntityFromName(tileBase.name);
		CoralCellData cell = new CoralCellData(position, coralTileMap, tileBase, maturity, coralBaseData.corals[type]);
		cfTotalProduction += cell.coralData.cfProduction;
		hfTotalProduction += cell.coralData.hfProduction;
		coralTypeNumbers[type]++;
		coralCells.Add(position, cell);
		coralTileMap.SetTile(position, tileBase);
	}

	/*
	 * Removes algae on map if it exists
	 */
	private void RemoveAlgaeOnMap(Vector3Int position)
	{
		if (algaeCells.ContainsKey(position) && algaeTileMap.HasTile(position))
		{
			AlgaeCellData cell = algaeCells[position];
			hfTotalProduction -= cell.algaeData.hfProduction;
			algaeCells.Remove(position);
			algaeTileMap.SetTile(position, null);
		}
	}

	/*
	 * Removes coral on map if it exists
	 */
	private void RemoveCoralOnMap(Vector3Int position)
	{
		if (coralCells.ContainsKey(position) && coralTileMap.HasTile(position))
		{
			CoralCellData cell = coralCells[position];
			cfTotalProduction -= cell.coralData.cfProduction;
			hfTotalProduction -= cell.coralData.hfProduction;
			coralTypeNumbers[FindIndexOfEntityFromName(cell.TileBase.name)]++;
			coralCells.Remove(position);
			coralTileMap.SetTile(position, null);
		}
	}

	private bool SpaceIsAvailable(Vector3Int position)
	{
		if (!groundTileMap.HasTile(position)) return false;
		if (!(substrataTileMap.HasTile(position) && substrataCells.ContainsKey(position))) return false;
		if (substrataOverlayTileMap.HasTile(position)) return false;
		if (!Utility.WithinBoardBounds(position, boardSize)) return false;
		return true;
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
			coralTypeNumbers.Add(0); // __WHAT: used to be Add(i)... but i think it should be Add(0)
		}
		InitializeTiles();
		print("initialization done");
		tempTimer = new CountdownTimer(globalVarContainer.globals[level].maxGameTime);
		disasterTimer = new CountdownTimer(30f); // make into first 5 mins immunity
		climateChangeTimer = new CountdownTimer(globalVarContainer.globals[level].timeUntilClimateChange);
		climateChangeHasWarned = false;
		climateChangeHasHappened = false;
		economyMachine = new EconomyMachine(10f, 0f, 5f, 15);
		timeUntilEnd = new CountdownTimer(60f);
		gameIsWon = false;
		timeToKillCorals = false;
		// ccTimerImage.transform.Find("CCTimeLeft").gameObject.SetActive(false);
		// ccOverlay.SetActive(false);
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
			growingCorals[i] = new List<NursingCoral>();
		}
		markedToDieCoral = new List<Vector3Int>();

		// initialization
		// Setting the substrata data
		foreach (Vector3Int pos in substrataTileMap.cellBounds.allPositionsWithin)
		{
			if (!substrataTileMap.HasTile(pos)) continue; // does this case even happen???
			if (!Utility.WithinBoardBounds(pos, boardSize))
			{
				substrataTileMap.SetTile(pos, null);
			}
			else
			{
				// __FIX: hacky way of doing it
				TileBase currentTB = substrataTileMap.GetTile(pos);
				int idx = FindIndexOfEntityFromName(currentTB.name);
				if (idx == -1)
				{ // UNKNOWN TILE; FOR NOW TOXIC
					HashSet<Vector3Int> toxicSpread = Utility.Spread(pos, 2);
					foreach (Vector3Int toxicPos in toxicSpread)
					{
						substrataOverlayTileMap.SetTile(toxicPos, Assets.instance.toxicOverlay);
					}
				}
				else
				{
					substrataCells.Add(pos, substrataDataContainer.substrata[idx].groundViability);
				}
			}
		}

		// assigning the borders of the level
		for (int i = boardSize + 1; i <= boardSize + 5; i++)
		{
			for (int j = -boardSize - 5; j <= boardSize + 5; j++)
			{
				substrataOverlayTileMap.SetTile(new Vector3Int(j, i, 0), Assets.instance.algaeEdgeTileBase);
				substrataOverlayTileMap.SetTile(new Vector3Int(j, -i, 0), Assets.instance.algaeEdgeTileBase);
				substrataOverlayTileMap.SetTile(new Vector3Int(i, j, 0), Assets.instance.algaeEdgeTileBase);
				substrataOverlayTileMap.SetTile(new Vector3Int(-i, j, 0), Assets.instance.algaeEdgeTileBase);
			}
		}

		// Setting the tiles in the tilemap to the coralCells dictionary
		foreach (Vector3Int pos in coralTileMap.cellBounds.allPositionsWithin)
		{
			if (!coralTileMap.HasTile(pos)) continue; // does this case even happen???
			if (!SpaceIsAvailable(pos))
			{
				coralTileMap.SetTile(pos, null);
			}
			else
			{
				AddCoralOnMap(pos, coralTileMap.GetTile(pos), 26);
			}
		}

		foreach (Vector3Int pos in algaeTileMap.cellBounds.allPositionsWithin)
		{
			if (!algaeTileMap.HasTile(pos)) continue; // does this case even happen???
			if (!SpaceIsAvailable(pos) || coralCells.ContainsKey(pos))
			{
				algaeTileMap.SetTile(pos, null);
			}
			else
			{
				AddAlgaeOnMap(pos, algaeTileMap.GetTile(pos), 26);
			}
		}
	}

	private void InitializeGame()
	{
		fishDisplayText.text = "Fish Income: 0";
		if (hfTotalProduction >= cfTotalProduction)
		{
			fishImageImage.color = Utility.green;
		}
		else
		{
			fishImageImage.color = Utility.red;
		}
		UpdateFishData();
		timeLeftText.text = Utility.ConvertTimetoMS(tempTimer.currentTime);
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
			// ccTimerText.text = Utility.ConvertTimetoMS(climateChangeTimer.currentTime);
		}
		if (!climateChangeHasWarned && climateChangeTimer.currentTime <= climateChangeTimer.timeDuration * (2.0 / 3.0))
		{
			climateChangeHasWarned = true;
			// ccTimerImage.transform.Find("CCTimeLeft").gameObject.SetActive(true);
			// ccTimer.climateChangeIsHappen();
			popupScript.makeEvent(0, "Climate Change is coming! Scientists have predicted that our carbon emmisions will lead to devastating damages to sea life in a few years! This could slow down the growth of coral reefs soon...");
		}
		else if (climateChangeHasWarned && !climateChangeHasHappened && climateChangeTimer.isDone())
		{
			climateChangeHasHappened = true;
			// ccOverlay.SetActive(true);
			popupScript.makeEvent(0, "Climate Change has come! Scientists have determined that the increased temperature and ocean acidity has slowed down coral growth! We have to make a greater effort to coral conservation and rehabilitation!");
			ApplyClimateChange();
		}
		#endregion

		#region Keyboard Shortcuts
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			GrowCoral(0);
			ChangeCoral(0);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2))
		{
			GrowCoral(1);
			ChangeCoral(1);
		}
		else if (Input.GetKeyDown(KeyCode.Alpha3))
		{
			GrowCoral(2);
			ChangeCoral(2);
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

		if (Input.GetKeyDown(KeyCode.R))
		{
			SelectShovel();
		}
		#endregion

		bool rb = Input.GetMouseButtonDown(1);
		if (rb)
		{
			if (shovelChosen)
			{
				ShovelArea();
			}
			else
			{
				PlantCoral(selectedCoral);
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

		MoveCameraWASD(25f);
		if (edgeScrollingEnabled) MoveCameraMouseEdge(25f, 10f);
		ZoomKeys(1.0f);
		ClampCamera();

		for (int type = 0; type < totalCoralTypes; type++)
		{
			foreach (NursingCoral coralTmp in growingCorals[type])
			{
				coralTmp.timer.updateTime();
			}
		}

		// cncText.text = GetCoralsInNursery() + "/" + globalVarContainer.globals[level].maxSpaceInNursery + " SLOTS LEFT";

		tempTimer.updateTime();
		timeLeftText.text = Utility.ConvertTimetoMS(tempTimer.currentTime);
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
		endGameScript.finalStatistics(fishIncome, Utility.ConvertTimetoMS(tempTimer.currentTime));
		endGameScript.setCongrats((gameIsWon ? Assets.instance.gameWinWordArt : Assets.instance.gameLoseWordArt));
		endGameScript.endMessage(s);
		endGameScript.gameEndReached();
	}

	private void FeedbackDialogue(string text, float time) => StartCoroutine(ShowMessage(text, time));

	IEnumerator ShowMessage(string text, float time)
	{
		// feedbackTextText.text = text;
		// feedbackTextText.enabled = true;
		Debug.Log(text);
		yield return new WaitForSeconds(time);
		// feedbackTextText.enabled = false;
	}

	private void UpdateFishData()
	{
		if (GameEnd.gameHasEnded || PauseScript.GamePaused)
			return;
		UpdateFishOutput();

		fishDisplayText.text = "Fish Income: " + fishIncome;
		if (hfTotalProduction >= cfTotalProduction)
		{
			if (economyMachine.IsAverageGood())
			{
				fishImageImage.color = Utility.gold;
			}
			else
			{
				fishImageImage.color = Utility.green;
			}
		}
		else
		{
			fishImageImage.color = Utility.red;
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
			HashSet<Vector3Int> coralsAround = Utility.Spread(key, 1);
			int weightedCoralMaturity = 0;
			foreach (Vector3Int pos in coralsAround)
			{
				if (coralCells.ContainsKey(pos))
				{
					weightedCoralMaturity += coralCells[pos].maturity;
				}
			}
			if (!economyMachine.AlgaeWillSurvive(algaeCells[key], substrataCells[key], -3 * weightedCoralMaturity / 2 + coralSurvivabilityDebuff))
			{
				RemoveAlgaeOnMap(key);
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
			// __MAGIC_NUMBER
			if (algaeCells[key].maturity > 25)
			{ // propagate only if "mature"
				for (int i = 0; i < totalCoralTypes; i++)
				{
					if (economyMachine.AlgaeWillPropagate(algaeCells[key], coralPropagationDebuff, groundTileMap.GetTile(key).name))
					{
						Vector3Int localPlace = key + hexNeighbors[key.y & 1, i];
						if (!SpaceIsAvailable(localPlace)) continue;
						if (algaeTileMap.HasTile(localPlace) || algaeCells.ContainsKey(localPlace)) continue;
						// __ECONOMY__ __FIX__ MANUAL OVERRIDE TO CHECK IF ALGAE CAN TAKE OVER
						if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace))
						{
							int randNum = UnityEngine.Random.Range(0, 101);
							HashSet<Vector3Int> surrounding = Utility.Spread(localPlace, 1);
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
						AddAlgaeOnMap(localPlace, algaeCells[key].TileBase, 0);
						// delete coral under algae
						RemoveCoralOnMap(localPlace);
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
			growingCorals[type].Add(new NursingCoral(coralBaseData.corals[type].name, new CountdownTimer(coralBaseData.corals[type].growTime)));
		}
		else if (!spaceInNursery)
		{
			FeedbackDialogue("Nursery is at maximum capacity.", globalVarContainer.globals[level].feedbackDelayTime);
		}
	}

	/*
	 * removing coral
	 * */
	private void PlantCoral(int type)
	{
		Vector3Int position = GetMouseGridPosition();
		int readyNum = GetReadyCoralsPerType(type);
		int loadedNum = GetCoralsPerType(type);
		if (!Utility.WithinBoardBounds(position, boardSize))
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
			int tempIdx = GetIndexOfReadyCoral(type);
			NursingCoral tempCoral = null;
			if (tempIdx != -1)
			{
				tempCoral = growingCorals[type][tempIdx];
				growingCorals[type].RemoveAt(tempIdx);
			}
			AddCoralOnMap(position, Assets.instance.coralTileBases[type], 0);
		}
		else if (readyNum == 0 && loadedNum - readyNum > 0)
		{
			float minTime = 3600f;
			// go find the quickest to finish coral
			for (int i = 0; i < growingCorals[type].Count; i++)
			{
				minTime = Math.Min(minTime, growingCorals[type][i].timer.currentTime);
			}
			string t = "Soonest to mature coral of this type has " + Utility.ConvertTimetoMS(minTime) + " time left.";
			FeedbackDialogue(t, globalVarContainer.globals[level].feedbackDelayTime);
		}
	}
	private void UpdateCoralSurvivability()
	{
		List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
		foreach (Vector3Int key in keys)
		{
			// __MAGIC_NUMBER
			if (coralCells[key].maturity <= 25)
			{
				// check adj corals
				// miscFactors aka the amount of corals around it influences how much more they can add to the survivability of one
				// how much they actually contribue can be varied; change the amount 0.01f to something that makes more sense
				int miscFactors = 0;
				HashSet<Vector3Int> neighbors = Utility.Spread(key, 1);
				foreach (Vector3Int neighbor in neighbors)
				{
					if (key != neighbor && coralCells.ContainsKey(neighbor))
						miscFactors += coralCells[neighbor].maturity / 5;
				}
				coralCells[key].addMaturity(1);
				if (!economyMachine.CoralWillSurvive(coralCells[key], substrataCells[key], miscFactors - coralSurvivabilityDebuff, groundTileMap.GetTile(key).name))
				{
					// setting data
					coralTileMap.SetTile(key, Assets.instance.coralDeadTileBases[FindIndexOfEntityFromName(coralCells[key].TileBase.name)]);
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
			RemoveCoralOnMap(key);
		}
		markedToDieCoral.Clear();
	}

	private void UpdateCoralPropagation()
	{
		List<Vector3Int> keys = new List<Vector3Int>(coralCells.Keys);
		foreach (Vector3Int key in keys)
		{
			// __MAGIC_NUMBER
			if (coralCells[key].maturity > 25)
			{ // propagate only if "mature"
				for (int i = 0; i < totalCoralTypes; i++)
				{
					if (economyMachine.CoralWillPropagate(coralCells[key], -coralPropagationDebuff, groundTileMap.GetTile(key).name))
					{
						Vector3Int localPlace = key + hexNeighbors[key.y & 1, i];
						if (!SpaceIsAvailable(localPlace)) continue;
						if (coralTileMap.HasTile(localPlace) || coralCells.ContainsKey(localPlace) || algaeTileMap.HasTile(localPlace)) continue;
						AddCoralOnMap(localPlace, coralCells[key].TileBase, 0);
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
				HashSet<Vector3Int> removeSpread = Utility.Spread(pos, 2);
				foreach (Vector3Int removePos in removeSpread)
				{
					if (coralCells.ContainsKey(removePos))
					{
						markedToDieCoral.Add(removePos); // __TIMING__
						coralTileMap.SetTile(removePos, Assets.instance.coralDeadTileBases[FindIndexOfEntityFromName(coralCells[removePos].TileBase.name)]);
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
			substrataTileMap.SetTile(pos, Assets.instance.toxicTileBases[UnityEngine.Random.Range(0, Assets.instance.toxicTileBases.Length)]);
			HashSet<Vector3Int> toxicSpread = Utility.Spread(pos, 2);
			foreach (Vector3Int toxicPos in toxicSpread)
			{
				if (coralCells.ContainsKey(toxicPos))
				{
					markedToDieCoral.Add(toxicPos); // __TIMING__
					coralTileMap.SetTile(toxicPos, Assets.instance.coralDeadTileBases[FindIndexOfEntityFromName(coralCells[toxicPos].TileBase.name)]);
				}
				if (algaeCells.ContainsKey(toxicPos))
				{
					RemoveAlgaeOnMap(toxicPos);
				}
				substrataOverlayTileMap.SetTile(toxicPos, Assets.instance.toxicOverlay);
			}
			popupScript.makeEvent(1);
		}
	}
	private void ShovelArea()
	{
		Vector3Int position = GetMouseGridPosition();
		if (!Utility.WithinBoardBounds(position, boardSize))
		{
			// not allowed
		}
		else if (coralTileMap.HasTile(position))
		{
			// not allowed
		}
		else if (algaeTileMap.HasTile(position))
		{
			// allowed
			HashSet<Vector3Int> spread = Utility.Spread(position, 2);
			foreach (Vector3Int location in spread)
			{
				if (algaeTileMap.HasTile(location))
				{
					RemoveAlgaeOnMap(location);
				}
			}
		}
		else
		{
			// not allowed
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
		economyMachine.UpdateHFCF(hfTotalProduction, cfTotalProduction);
		fishIncome = (int)Math.Round(economyMachine.GetTotalFish(coralTypeNumbers));
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

		zoom = Mathf.Clamp(zoom, 1f, 5f);
	}

	private void ClampCamera()
	{
		cameraFollowPosition = new Vector3(
			Mathf.Clamp(cameraFollowPosition.x, -11.25f, 11.25f),
			Mathf.Clamp(cameraFollowPosition.y, -18.75f, 18.75f),
			cameraFollowPosition.z
		);
	}
	#endregion
}
