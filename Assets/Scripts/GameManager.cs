using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using Assets.Scripts.Inputs;
using Assets.Scripts.EntityClasses;

public class GameManager : MonoBehaviour
{
	public static GameManager instance = null;

	#region Things I Plug in Unity
#pragma warning disable 0649
	[SerializeField] private Tilemap coralTileMap;
	[SerializeField] private Tilemap groundTileMap;
	[SerializeField] private Tilemap substrataTileMap;
	[SerializeField] private Tilemap substrataOverlayTileMap;
	[SerializeField] private Tilemap algaeTileMap;
	[SerializeField] private GameObject popupCanvas;
	[SerializeField] private GameObject endGameScreen;
	[SerializeField] private int level;
	[SerializeField] private int boardSize;
	[SerializeField] private InputHandler _inputHandler;
#pragma warning restore 0649
	public int Level => level;
	#endregion
	#region Components of GameObjects
	Grid grid;
	GameEnd endGameScript;
	PopupScript popupScript;
	private void InitializeComponents()
	{
		grid = GameObject.Find("Grid").GetComponent<Grid>();
		endGameScript = endGameScreen.GetComponent<GameEnd>();
		popupScript = popupCanvas.GetComponent<PopupScript>();
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
	public GlobalContainer globalVarContainer;
	public CoralDataContainer coralBaseData;
	public SubstrataDataContainer substrataDataContainer;
	public AlgaeDataContainer algaeDataContainer;
	#endregion
	#region Global Changing Values
	public int fishIncome = 0;
	private float cfTotalProduction = 0;
	private float hfTotalProduction = 0;
	public CountdownTimer gameTimer;
	// dirty flags
	private List<int> dirtyReadyCoralsPerType;
	private List<int> dirtyTotalCoralsPerType;
	private List<NursingCoral>[] growingCorals;
	private CountdownTimer disasterTimer;
	private CountdownTimer climateChangeTimer;
	public bool climateChangeHasWarned;
	public bool climateChangeHasHappened;
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
	public CountdownTimer shovelTimer;
	#endregion

	#region Event Stuff
	public class QueueStatusChangedEventArgs : EventArgs
	{
		public int coralType;
		public int coralReady;
		public int coralTotal;
	}
	public event EventHandler<QueueStatusChangedEventArgs> QueueStatusChanged;

	public class SelectionStatusChangedEventArgs : EventArgs
	{
		public int coralType;
	}
	public event EventHandler<SelectionStatusChangedEventArgs> SelectionStatusChanged;
	
	public class FeedbackTextStatusEventArgs : EventArgs
	{
		public string feedbackTextContent;
		public float feedbackTextDuration;
	}
	public event EventHandler<FeedbackTextStatusEventArgs> FeedbackTextStatus;

	public class CoralCompositionStatusChangedEventArgs : EventArgs
	{
		public int branchingTotal;
		public int encrustingTotal;
		public int massiveTotal;
	}
	public event EventHandler<CoralCompositionStatusChangedEventArgs> CoralCompositionChanged;
	#endregion

	#region Generic Helper Functions
	private Vector3Int GetMouseGridPosition() //input
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Vector3 worldPoint = ray.GetPoint(-ray.origin.z / ray.direction.z);
		Vector3Int position = grid.WorldToCell(worldPoint);
		return position;
	}
	#endregion
	#region Supposed to be States
	public int selectedCoral = 0;
	public bool shovelChosen = false;
	#endregion
	#region Game-Specific Helper Functions
	private string GetTypeOfTileBase(TileBase tileBase)
	{
		string[] tokens = tileBase.name.Split('_');
		if (tokens.Length == 0)
		{
			return "ERROR"; // maybe throw next time?
		}
		return tokens[0];
	}

	private int GetIndexOfTileBaseHelper(TileBase[] array, TileBase tileBase)
	{
		int idx = -1;
		for (int i = 0; i < array.Length; ++i)
		{
			if (array[i].name == tileBase.name)
			{
				idx = i;
				break;
			}
		}
		return idx;
	}

	// __FIX: please for the love of god fix this steaming pile of shit
	private int GetIndexOfTileBase(TileBase tileBase)
	{
		int ans = -1;
		// corals
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.coralTileBases00, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.coralTileBases01, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.coralTileBases02, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.coralDeadTileBases00, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.coralDeadTileBases01, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.coralDeadTileBases02, tileBase));
		// algae
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.algaeTileBases00, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.algaeTileBases01, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.algaeTileBases02, tileBase));
		// substrata
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.substrataTileBases, tileBase));
		// toxic
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.toxicTileBases, tileBase));
		// edge
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.edgeGroundTileBases, tileBase));
		ans = Math.Max(ans, GetIndexOfTileBaseHelper(GameAssets.instance.edgeSubstrataTileBases, tileBase));
		return ans;
	}
	// assumes it's an entity
	private string GetGrowthLevelOfEntity(TileBase tileBase)
	{
		if (tileBase.name.Contains("young"))
		{
			return "young";
		} 
		else if (tileBase.name.Contains("mid"))
		{
			return "mid";
		}
		else if (tileBase.name.Contains("mature"))
		{
			return "mature";
		}
		else
		{
			return "ERROR"; // throw exception?
		}
	}
	// assumes it's a coral
	private TileBase ConvertAliveCoralToDead(TileBase tileBase)
	{
		if (GetGrowthLevelOfEntity(tileBase) == "young")
		{
			return GameAssets.instance.coralDeadTileBases00[GetIndexOfTileBase(tileBase)];
		}
		else if (GetGrowthLevelOfEntity(tileBase) == "mid")
		{
			return GameAssets.instance.coralDeadTileBases01[GetIndexOfTileBase(tileBase)];
		}
		else if (GetGrowthLevelOfEntity(tileBase) == "mature")
		{
			return GameAssets.instance.coralDeadTileBases02[GetIndexOfTileBase(tileBase)];
		}
		else
		{
			return null; // if for some reason something went wrong
		}
	}
	public void ChangeCoral(int select)
	{
		selectedCoral = select;
		shovelChosen = false;
		SelectionStatusChanged?.Invoke(this, new SelectionStatusChangedEventArgs
		{
			coralType = select
		});
	}
	public void SelectShovel()
	{
		selectedCoral = 3;
		shovelChosen = true;
		SelectionStatusChanged?.Invoke(this, new SelectionStatusChangedEventArgs
		{
			coralType = 3
		});
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
	public float GetSoonestToMatureCoralPercent(int type)
	{
		float best = 0.0f;
		foreach (NursingCoral coral in growingCorals[type])
		{
			best = Math.Max(best, coral.timer.percentComplete);
		}
		return best;
	}
	public float GetClimateChangeProgress()
	{
		return climateChangeTimer.percentComplete;
	}
	/*
	 * Programmer note: by nature of the new system, the ready coral is ALWAYS in front of the list
	 */
	private int GetIndexOfReadyCoral(int type)
	{
		int index = -1;
		for (int i = 0; i < GetCoralsPerType(type); i++)
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
		int type = GetIndexOfTileBase(tileBase);
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
		int type = GetIndexOfTileBase(tileBase);
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
			coralTypeNumbers[GetIndexOfTileBase(cell.TileBase)]++;
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
		endGameScript.ResetEndScreen();
		print("loading XML data...");
		globalVarContainer = GlobalContainer.Load("GlobalsXML");
		substrataDataContainer = SubstrataDataContainer.Load("SubstrataXML");
		coralBaseData = CoralDataContainer.Load("CoralDataXML");
		algaeDataContainer = AlgaeDataContainer.Load("AlgaeDataXML");
		totalCoralTypes = coralBaseData.corals.Count;
		print("XML data loaded");
		print("initializing tiles...");
		coralTypeNumbers = new List<int>();
		for (int i = 0; i < totalCoralTypes; ++i)
		{
			coralTypeNumbers.Add(0); // __WHAT: used to be Add(i)... but i think it should be Add(0)
		}
		InitializeTiles();
		print("initialization done");
		gameTimer = new CountdownTimer(globalVarContainer.globals[level].maxGameTime);
		disasterTimer = new CountdownTimer(30f); // make into first 5 mins immunity
		climateChangeTimer = new CountdownTimer(globalVarContainer.globals[level].timeUntilClimateChange);
		climateChangeHasWarned = false;
		climateChangeHasHappened = false;
		economyMachine = new EconomyMachine(10f, 0f, 5f, 15);
		timeUntilEnd = new CountdownTimer(globalVarContainer.globals[level].timeUntilEnd); //(60f);
		gameIsWon = false;
		timeToKillCorals = false;
		print("level is " + globalVarContainer.globals[level].level);
	}

	private void InitializeTiles()
	{
		// instantiation
		coralCells = new Dictionary<Vector3Int, CoralCellData>();
		substrataCells = new Dictionary<Vector3Int, int>();
		algaeCells = new Dictionary<Vector3Int, AlgaeCellData>();
		growingCorals = new List<NursingCoral>[totalCoralTypes];
		dirtyReadyCoralsPerType = new List<int>();
		dirtyTotalCoralsPerType = new List<int>();
		for (int i = 0; i < totalCoralTypes; i++)
		{
			growingCorals[i] = new List<NursingCoral>();
			dirtyReadyCoralsPerType.Add(0);
			dirtyTotalCoralsPerType.Add(0);
		}
		markedToDieCoral = new List<Vector3Int>();

		// initialization
		// Setting the substrata data
		foreach (Vector3Int pos in substrataTileMap.cellBounds.allPositionsWithin)
		{
			if (!substrataTileMap.HasTile(pos)) continue; // does this case even happen???
			if (!Utility.WithinBoardBounds(pos, boardSize) && substrataTileMap.GetTile(pos).name.Contains("edge"))
			{
				substrataTileMap.SetTile(pos, null);
			}
			else
			{
				TileBase currentTB = substrataTileMap.GetTile(pos);
				if (GetTypeOfTileBase(currentTB) == "toxic")
				{
					HashSet<Vector3Int> toxicSpread = Utility.Spread(pos, 2);
					foreach (Vector3Int toxicPos in toxicSpread)
					{
						substrataOverlayTileMap.SetTile(toxicPos, GameAssets.instance.toxicOverlay);
					}
				}
				else
				{
					substrataCells.Add(pos, substrataDataContainer.substrata[GetIndexOfTileBase(currentTB)].groundViability);
				}
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

		CoralCompositionChanged?.Invoke(this, new CoralCompositionStatusChangedEventArgs
		{
			branchingTotal = coralTypeNumbers[0],
			encrustingTotal = coralTypeNumbers[1],
			massiveTotal = coralTypeNumbers[2]
		});

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

		shovelTimer = new CountdownTimer(120f);
	}
	// BAND-AID
	private bool game7ForceEvent1 = false;
	private bool game7ForceEvent2 = false;

	private void Start()
	{
		// __FIX__ MAKE INTO GLOBALS?
		// InvokeRepeating(nameof(UpdateFishData), 0f, 1.0f);
		InvokeRepeating(nameof(UpdateAllAlgae), 1.0f, 1.0f);
		InvokeRepeating(nameof(UpdateAllCoral), 2.0f, 2.0f);
		InvokeRepeating(nameof(KillCorals), 1.0f, 3.0f);
	}

	// __DECOMPOSE: a lot of things can be isolated into their own functions rather than their own regions
	void Update()
	{
		if (GameEnd.gameHasEnded) return;
		if (PauseScript.GamePaused) return;

		if (Input.GetKeyDown(KeyCode.Backspace))
		{
			EndTheGame("abc");
		}

		_inputHandler.HandleInput();

		UpdateFishOutput();
		DisasterUpdates();
		UpdateNursingCorals();
		UpdateShovelTimer();

		if (Level == 7)
		{
			if (Math.Abs(gameTimer.percentComplete - 0.33) < 0.001 && !game7ForceEvent1)
			{
				RandomDisaster(1);
				game7ForceEvent1 = true;
			}
			else if (Math.Abs(gameTimer.percentComplete - 0.66) < 0.001 && !game7ForceEvent2)
			{
				RandomDisaster(2);
				game7ForceEvent2 = true;
			}
		}

		gameTimer.updateTime();
		if (gameTimer.isDone())
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

	public void StopTime()
	{
		Time.timeScale = 0f;
	}

	public void ResumeTime()
	{
		Time.timeScale = 1f;
	} 

	private void UpdateShovelTimer()
	{
		shovelTimer.updateTime();
	}

	private void UpdateNursingCorals()
	{
		for (int type = 0; type < totalCoralTypes; type++)
		{
			foreach (NursingCoral coralTmp in growingCorals[type])
			{
				coralTmp.timer.updateTime();
			}
			int currentReadyCoralsOfType = GetReadyCoralsPerType(type);
			int currentTotalCoralsOfType = GetCoralsPerType(type);
			if (dirtyReadyCoralsPerType[type] != currentReadyCoralsOfType || dirtyTotalCoralsPerType[type] != currentTotalCoralsOfType)
			{
				dirtyReadyCoralsPerType[type] = currentReadyCoralsOfType;
				dirtyTotalCoralsPerType[type] = currentTotalCoralsOfType;
				QueueStatusChanged?.Invoke(this, new QueueStatusChangedEventArgs
				{
					coralType = type,
					coralReady = currentReadyCoralsOfType,
					coralTotal = currentTotalCoralsOfType
				});
			}
		}
	}

	private void DisasterUpdates()
	{
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
	}

	private void EndTheGame(string s)
	{
		endGameScript.FinalStatistics(fishIncome, Utility.ConvertTimetoMS(gameTimer.currentTime));
		endGameScript.SetCongrats((gameIsWon ? GameAssets.instance.gameWinWordArt : GameAssets.instance.gameLoseWordArt));
		endGameScript.EndMessage(s);
		endGameScript.GameEndReached();
	}

	/*
	 * Computes for the prosperity state of the fish just for display.
	 */
	public int FishProsperityState()
	{
		if (hfTotalProduction >= cfTotalProduction)
		{
			return 1;
		} 
		else
		{
			return 0;
		}
	}

	// __DECOMPOSE
	// __ECONOMY__
	#region Algae Updates
	private void UpdateAllAlgae()
	{
		UpdateAlgaeSurvivability();
		UpdateAlgaePropagation();
	}

	// _FIX: decouple visual from this shit!!!
	private void UpdateAlgaeSurvivability()
	{
		List<Vector3Int> keys = new List<Vector3Int>(algaeCells.Keys);
		foreach (Vector3Int key in keys)
		{
			if (algaeCells[key].maturity <= 25)
			{
				algaeCells[key].addMaturity(1);
				int idx = GetIndexOfTileBase(algaeCells[key].TileBase);
				if (algaeCells[key].maturity >= 25 && algaeCells[key].TileBase.name != GameAssets.instance.algaeTileBases02[idx].name)
				{
					algaeTileMap.SetTile(key, GameAssets.instance.algaeTileBases02[idx]);
					algaeCells[key].TileBase = GameAssets.instance.algaeTileBases02[idx];
				}
				else if (algaeCells[key].maturity >= 15 && algaeCells[key].TileBase.name != GameAssets.instance.algaeTileBases01[idx].name)
				{
					algaeTileMap.SetTile(key, GameAssets.instance.algaeTileBases01[idx]);
					algaeCells[key].TileBase = GameAssets.instance.algaeTileBases01[idx];
				}
				else if (algaeCells[key].maturity >= 15 && algaeCells[key].TileBase.name != GameAssets.instance.algaeTileBases00[idx].name)
				{
					algaeTileMap.SetTile(key, GameAssets.instance.algaeTileBases00[idx]);
					algaeCells[key].TileBase = GameAssets.instance.algaeTileBases00[idx];
				}
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
							foreach (Vector3Int neighbor in surrounding)
							{
								if (coralCells.ContainsKey(neighbor))
								{
									randNum -= coralCells[neighbor].maturity / 3;
								}
							}
							if (randNum < 60) continue;
						}
						// AddAlgaeOnMap(localPlace, algaeCells[key].TileBase, 0);
						AddAlgaeOnMap(localPlace, GameAssets.instance.algaeTileBases00[GetIndexOfTileBase(algaeCells[key].TileBase)], 0);
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

	public void GrowCoral(int type)
	{
		bool spaceInNursery = GetCoralsInNursery() < globalVarContainer.globals[level].maxSpaceInNursery;
		if (spaceInNursery)
		{
			growingCorals[type].Add(new NursingCoral(coralBaseData.corals[type].name, new CountdownTimer(coralBaseData.corals[type].growTime)));
		}
		else if (!spaceInNursery)
		{
			FeedbackTextStatus?.Invoke(this, new FeedbackTextStatusEventArgs
			{
				feedbackTextContent = "Nursery is at maximum capacity.",
				feedbackTextDuration = globalVarContainer.globals[level].feedbackDelayTime
			});
		}
	}

	/*
	 * removing coral
	 * */
	public void PlantCoral(int type)
	{
		Vector3Int position = GetMouseGridPosition();
		int readyNum = GetReadyCoralsPerType(type);
		int loadedNum = GetCoralsPerType(type);
		if (!Utility.WithinBoardBounds(position, boardSize))
		{
			FeedbackTextStatus?.Invoke(this, new FeedbackTextStatusEventArgs
			{
				feedbackTextContent = "Can't put corals out of bounds!",
				feedbackTextDuration = globalVarContainer.globals[level].feedbackDelayTime
			});
		}
		else if (coralTileMap.HasTile(position))
		{
			FeedbackTextStatus?.Invoke(this, new FeedbackTextStatusEventArgs
			{
				feedbackTextContent = "Can't put corals on top of other corals!",
				feedbackTextDuration = globalVarContainer.globals[level].feedbackDelayTime
			});
		}
		else if (algaeTileMap.HasTile(position))
		{
			FeedbackTextStatus?.Invoke(this, new FeedbackTextStatusEventArgs
			{
				feedbackTextContent = "Can't put corals on top of algae! The coral will die!",
				feedbackTextDuration = globalVarContainer.globals[level].feedbackDelayTime
			});
		}
		else if (substrataOverlayTileMap.HasTile(position))
		{
			FeedbackTextStatus?.Invoke(this, new FeedbackTextStatusEventArgs
			{
				feedbackTextContent = "This is a toxic tile! Corals won't survive here.",
				feedbackTextDuration = globalVarContainer.globals[level].feedbackDelayTime
			});
		}
		else if ((substrataTileMap.HasTile(position) || substrataCells.ContainsKey(position)) && readyNum > 0)
		{
			int tempIdx = GetIndexOfReadyCoral(type);
			if (tempIdx != -1)
			{
				growingCorals[type].RemoveAt(tempIdx);
			}
			AddCoralOnMap(position, GameAssets.instance.coralTileBases00[type], 0);
			CoralCompositionChanged?.Invoke(this, new CoralCompositionStatusChangedEventArgs
			{
				branchingTotal = coralTypeNumbers[0],
				encrustingTotal = coralTypeNumbers[1],
				massiveTotal = coralTypeNumbers[2]
			});
		}
		else if (readyNum == 0 && loadedNum - readyNum > 0)
		{
			float minTime = 3600f;
			// go find the quickest to finish coral
			foreach (NursingCoral coral in growingCorals[type])
			{
				minTime = Math.Min(minTime, coral.timer.currentTime);
			}
			string t = "Soonest to mature coral of this type has " + Utility.ConvertTimetoMS(minTime) + " time left.";
			FeedbackTextStatus?.Invoke(this, new FeedbackTextStatusEventArgs
			{
				feedbackTextContent = t,
				feedbackTextDuration = globalVarContainer.globals[level].feedbackDelayTime
			});
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
				int idx = GetIndexOfTileBase(coralCells[key].TileBase);
				if (coralCells[key].maturity >= 25 && coralCells[key].TileBase.name != GameAssets.instance.coralTileBases02[idx].name)
				{
					coralTileMap.SetTile(key, GameAssets.instance.coralTileBases02[idx]);
					coralCells[key].TileBase = GameAssets.instance.coralTileBases02[idx];
				}
				else if (coralCells[key].maturity >= 15 && coralCells[key].TileBase.name != GameAssets.instance.coralTileBases01[idx].name)
				{
					coralTileMap.SetTile(key, GameAssets.instance.coralTileBases01[idx]);
					coralCells[key].TileBase = GameAssets.instance.coralTileBases01[idx];
				}
				else if (coralCells[key].maturity >= 15 && coralCells[key].TileBase.name != GameAssets.instance.coralTileBases00[idx].name)
				{
					coralTileMap.SetTile(key, GameAssets.instance.coralTileBases00[idx]);
					coralCells[key].TileBase = GameAssets.instance.coralTileBases00[idx];
				}
				if (!economyMachine.CoralWillSurvive(coralCells[key], substrataCells[key], miscFactors - coralSurvivabilityDebuff, groundTileMap.GetTile(key).name))
				{
					// setting data
					markedToDieCoral.Add(key);
					coralTileMap.SetTile(key, ConvertAliveCoralToDead(coralCells[key].TileBase));
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
			if (!coralCells.ContainsKey(key)) continue;
			RemoveCoralOnMap(key);
		}
		CoralCompositionChanged?.Invoke(this, new CoralCompositionStatusChangedEventArgs
		{
			branchingTotal = coralTypeNumbers[0],
			encrustingTotal = coralTypeNumbers[1],
			massiveTotal = coralTypeNumbers[2]
		});
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
						// AddCoralOnMap(localPlace, coralCells[key].TileBase, 0);
						AddCoralOnMap(localPlace, GameAssets.instance.coralTileBases00[GetIndexOfTileBase(coralCells[key].TileBase)], 0);
						CoralCompositionChanged?.Invoke(this, new CoralCompositionStatusChangedEventArgs
						{
							branchingTotal = coralTypeNumbers[0],
							encrustingTotal = coralTypeNumbers[1],
							massiveTotal = coralTypeNumbers[2]
						});
					}
				}
			}
		}
	}
	#endregion
	#region Disasters
	// __MAGIC_NUMBER: so many magic numbers
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
						coralTileMap.SetTile(removePos, ConvertAliveCoralToDead(coralCells[removePos].TileBase));
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
			substrataTileMap.SetTile(pos, GameAssets.instance.toxicTileBases[UnityEngine.Random.Range(0, GameAssets.instance.toxicTileBases.Length)]);
			HashSet<Vector3Int> toxicSpread = Utility.Spread(pos, 2);
			foreach (Vector3Int toxicPos in toxicSpread)
			{
				if (coralCells.ContainsKey(toxicPos))
				{
					markedToDieCoral.Add(toxicPos); // __TIMING__
					// this is replacing the asset with a dying coral sprite
					coralTileMap.SetTile(toxicPos, ConvertAliveCoralToDead(coralCells[toxicPos].TileBase));
				}
				if (algaeCells.ContainsKey(toxicPos))
				{
					RemoveAlgaeOnMap(toxicPos);
				}
				substrataOverlayTileMap.SetTile(toxicPos, GameAssets.instance.toxicOverlay);
			}
			popupScript.makeEvent(1);
		}
	}
	// __UNUSED
	public void QueueShovel()
	{
		if (!shovelTimer.isDone())
		{
			FeedbackTextStatus?.Invoke(this, new FeedbackTextStatusEventArgs
			{
				feedbackTextContent = "Currently queueing a shovel! Can't queue multiple shovels at once.",
				feedbackTextDuration = 2f
			});
		}
	}
	public void ShovelArea()
	{
		if (!shovelTimer.isDone()) return;
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
			shovelTimer.reset();
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
	private void UpdateFishOutput()
	{
		economyMachine.UpdateHFCF(hfTotalProduction, cfTotalProduction);
		fishIncome = (int)Math.Round(economyMachine.GetTotalFish(coralTypeNumbers));
	}
}
