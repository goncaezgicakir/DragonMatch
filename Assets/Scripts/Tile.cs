using UnityEngine;
using System.Collections;


//enum for TileTypes
public enum TileType
{
    Normal,
    Obstacle,
    Breakable
}

[RequireComponent(typeof(SpriteRenderer))]
public class Tile : MonoBehaviour {

    //public variables
    public int xIndex;
    public int yIndex;
    public TileType tileType = TileType.Normal;
    public int breakableValue = 0;
    public Sprite[] breakableSprites;
    public Color normalColor;

    //private variables
    Board m_board;
    SpriteRenderer m_spriteRenderer;

    //main
	void Start () {
	
	}
	
    void Awake()
    {   
        m_spriteRenderer = GetComponent<SpriteRenderer>();
    }

    //method to init tile to board
    public void Init(int x, int y, Board board){
    
        xIndex = x;
        yIndex = y;
        m_board = board;

        //if the tile is breakable set the tile's sprite
        if (tileType == TileType.Breakable)
        {
            if (breakableSprites[breakableValue] != null)
            {
                m_spriteRenderer.sprite = breakableSprites[breakableValue];
            }
        }
    }

    //method to call mouse click on board
    void OnMouseDown()
    {
        if (m_board != null)
        {
            m_board.ClickTile(this);
        }
    }

    //method to call mouse drag on board
    void OnMouseEnter()
    {
        if (m_board != null)
        {
            m_board.DragToTile(this);
        }
    }

    //method to call mouse release on board
    void OnMouseUp()
    {
        if (m_board != null)
        {
            m_board.ReleaseTile();
        }
    }

    //method to break the tile
    public void BreakTile()
    {
        //if tile is not breakable, returns immediately
        if (tileType != TileType.Breakable)
        {
            return;
        }

        //call break tile coroutine to process 
        StartCoroutine(BreakTileCoroutine());
    }

    //method to break the brekable tile
    IEnumerator BreakTileCoroutine()
    {   
        //get the breakable value (it could be 0,1 or 2)
        //decreases it by one AFTER using it
        breakableValue--;
        breakableValue = Mathf.Clamp(breakableValue, 0, breakableValue);

        //delay
        yield return new WaitForSeconds(0.25f);

        //set the tile's sprite 
        if (breakableSprites[breakableValue] != null)
        {
            m_spriteRenderer.sprite = breakableSprites[breakableValue];
        }

        //if there no breakability left, tile types is set to Normal
        if (breakableValue == 0)
        {
            tileType = TileType.Normal;
            m_spriteRenderer.color = normalColor;
        }
    }
}
