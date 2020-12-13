using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GameAssets : MonoBehaviour
{

    // Internal instance reference
    private static GameAssets _instance;

    // Instance reference
    public static GameAssets instance
    {
        get
        {
            if (_instance == null) _instance = Instantiate(Resources.Load<GameAssets>("GameAssets"));
            return _instance;
        }
    }


    // All references
    // GameManager
    // make this better with a design pattern maybe!!
    public TileBase[] coralTileBases00;
    public TileBase[] coralTileBases01;
    public TileBase[] coralTileBases02;
    public TileBase[] coralDeadTileBases00;
    public TileBase[] coralDeadTileBases01;
    public TileBase[] coralDeadTileBases02;
    public TileBase[] groundTileBases;
    public TileBase[] algaeTileBases00;
    public TileBase[] algaeTileBases01;
    public TileBase[] algaeTileBases02;
    public TileBase[] substrataTileBases;
    public TileBase[] toxicTileBases;
    public TileBase algaeEdgeTileBase;
    public TileBase[] edgeSubstrataTileBases;
    public TileBase[] edgeGroundTileBases;
    public TileBase toxicOverlay;
    public Sprite gameWinWordArt;
    public Sprite gameLoseWordArt;

    // PopupScript
    public Sprite defaultPopupSprite;
    public Sprite toxicWasteDisasterSprite;
    public Sprite touristsDisasterSprite;
    public Sprite bombingDisasterSprite;
    public Sprite climateChangeDisasterSprite;
    public Sprite normalButtonSprite;
    public Sprite climateButtonSprite;

}
