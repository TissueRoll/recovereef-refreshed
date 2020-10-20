using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Assets : MonoBehaviour
{

    // Internal instance reference
    private static Assets _instance;

    // Instance reference
    public static Assets instance
    {
        get
        {
            if (_instance == null) _instance = Instantiate(Resources.Load<Assets>("GameAssets"));
            return _instance;
        }
    }


    // All references
    // GameManager
    public TileBase[] coralTileBases;
    public TileBase[] coralDeadTileBases;
    public TileBase[] groundTileBases;
    public TileBase[] algaeTileBases;
    public TileBase[] substrataTileBases;
    public TileBase[] toxicTileBases;
    public TileBase algaeEdgeTileBase;
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
