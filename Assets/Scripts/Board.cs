using UnityEngine;
using System.Collections;

public class Board : MonoBehaviour {

    //public variables
    public int width;
    public int height;
    public int borderSize;

    public GameObject tilePrefab;
    public GameObject[] gamePiecesPrefabs;

    public float swapTime = 0.5f;

    //private variables
    Tile[,] m_allTiles;
    GamePiece[,] m_allGamePieces;

    Tile m_clickedTile;
    Tile m_targetTile;

    //main
    void Start () 
    {
        m_allTiles = new Tile[width,height];
        m_allGamePieces = new GamePiece[width, height];

        SetupTiles();
        SetupCamera();  
        FillRandom();
    }
        
    //method to set tile coordinates
    void SetupTiles()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject tile = Instantiate (tilePrefab, new Vector3(i, j, 0), Quaternion.identity) as GameObject;

                tile.name = "Tile (" + i + "," + j + ")";

                m_allTiles [i,j] = tile.GetComponent<Tile>();

                tile.transform.parent = transform;

                m_allTiles[i, j].Init(i, j, this);
            }
        }
    }

    //method to set the camera on center to see board properly
    void SetupCamera(){

        Camera.main.transform.position = new Vector3( (float)(width - 1) / 2f, (float)(height - 1) /2f, -10f);
        float aspectRatio = (float)Screen.width / (float)Screen.height;
        float verticalSize = (float)height / 2f + (float)borderSize;
        float horizontalSize = ( (float)width / 2f + (float)borderSize ) / aspectRatio;

        Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
    
    }


    //method to get a random GamePiece 
    GameObject GetRandomGamePiece(){

        int randomIdx = Random.Range(0, gamePiecesPrefabs.Length);

        if(gamePiecesPrefabs[randomIdx] == null){
        
            Debug.LogWarning("BOARD:" + randomIdx + "does not contain a valid GamePiece prefab!");
        }

        return gamePiecesPrefabs[randomIdx];
    }


    //method to place the GamePiece object to specified (x,y) coordinate
    public void PlaceGamePiece(GamePiece gamePiece, int x, int y){

        if(gamePiece == null){

            Debug.LogWarning("Invalig GamePiece!");
            return;
        }

        //set the position and rotation of the gamePiece
        gamePiece.transform.position = new Vector3(x, y, 0);
        gamePiece.transform.rotation = Quaternion.identity;

        //update the current gamePiece on the tile
        if(IsWithinBounds(x,y))
        {
            m_allGamePieces[x, y] = gamePiece;
        }

        //set gamePiece's new coord values
        gamePiece.SetCoord(x, y);

    }


    //method to check gamePieces still in range
    bool IsWithinBounds(int x, int y)
    {
        return (x >= 0 && x < width && y >= 0 && y < height); 
    }
        
    //methof to fill the board with random GamePiece objects
    void FillRandom()
    {   
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                GameObject randomPiece = Instantiate (GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;

                if (randomPiece != null)
                {
                    randomPiece.GetComponent<GamePiece>().Init(this);
                    PlaceGamePiece(randomPiece.GetComponent<GamePiece>(), i, j);
                    randomPiece.transform.parent = transform;
                }
            }
        }
    }

    //method to handle mouse click
    public void ClickTile(Tile tile)
    {
        if (m_clickedTile == null )
        {
            m_clickedTile = tile;
            Debug.Log("Clicked tile: " + tile.name);
        }
    }

    //method to handle mouse drag
    public void DragToTile(Tile tile)
    {
        if (m_clickedTile != null && IsNextTo(tile, m_clickedTile))
        {
            m_targetTile = tile;
        }
    }

    //method to handle mouse releaase
    public  void ReleaseTile()
    {
        if (m_clickedTile != null && m_targetTile != null)
        {
            SwitchTiles(m_clickedTile, m_targetTile);
        }

        //reset the tile values at the end of switching
        m_clickedTile = null;
        m_targetTile = null;
    }

    //method to switching game pieces
    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        GamePiece clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];
        GamePiece targetPiece = m_allGamePieces[m_targetTile.xIndex, m_targetTile.yIndex];            

        clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
        targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);

    }

    //method to determine if the two tile is next to each other
    bool IsNextTo(Tile start, Tile end)
    {
        if (Mathf.Abs(start.xIndex - end.xIndex) == 1 && start.yIndex == end.yIndex)
        {
            return true;
        }

        if (Mathf.Abs(start.yIndex - end.yIndex) == 1 && start.xIndex == end.xIndex)
        {
            return true;
        }

        return false;
    }



}