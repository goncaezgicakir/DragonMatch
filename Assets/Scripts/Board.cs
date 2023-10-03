using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Board : MonoBehaviour {

    //public variables
    public int width;
    public int height;
    public int borderSize;
    public float swapTime = 0.5f;

    public GameObject tilePrefab;
    public GameObject[] gamePiecesPrefabs;


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
        HighlightMatches();
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

    //method to switching game piece to get a match
    void SwitchTiles(Tile clickedTile, Tile targetTile)
    {
        StartCoroutine(SwitchTilesRoutine(clickedTile, targetTile));
    }

    //method to switching game pieces and highlighting if it is a match
    IEnumerator SwitchTilesRoutine(Tile clickedTile, Tile targetTile)
    {   
        //get pieces
        GamePiece clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];
        GamePiece targetPiece = m_allGamePieces[m_targetTile.xIndex, m_targetTile.yIndex];            

        if(targetPiece != null && clickedPiece != null)
        {
            //move pieces
            clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                
            //waits for swap time befor highlighting matches
            yield return new WaitForSeconds(swapTime);

            //find matches at the new coordinates of the tiles
            List<GamePiece> clickedPieceMatches = FindMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            List<GamePiece> targetPieceMatches = FindMatchesAt(targetTile.xIndex, targetTile.yIndex);

            //if there is no match, move the pieces back to their old coordinates
            if (targetPieceMatches.Count == 0 & clickedPieceMatches.Count == 0)
            {
                clickedPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                targetPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
            }

            //waits for swap time befor highlighting matches
            yield return new WaitForSeconds(swapTime);

            //highlight tiles if there is a match
            HighlightMatchesAt(clickedTile.xIndex, clickedTile.yIndex);
            HighlightMatchesAt(targetTile.xIndex, targetTile.yIndex);
        }
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

    //method to find matched game pieces
    List<GamePiece> FindMatches(int startX, int startY, Vector2 searchDirection, int minLength = 3)
    {
        List<GamePiece> matches = new List<GamePiece>();
        GamePiece startPiece = null;

        if(IsWithinBounds(startX, startY))
        {
            startPiece = m_allGamePieces[startX, startY];
        }

        //add the first piece to matches list
        if (startPiece != null)
        {
            matches.Add(startPiece);
        }
        else
        {
            return null;
        }

        int nextX;
        int nextY;

        //max value of the board length
        int maxValue = (width > height) ? width : height;

        for (int i = 1; i < maxValue - 1; i++)
        {
            //get the next piece coordinates
            nextX = startX + (int)Mathf.Clamp(searchDirection.x, -1, 1) * i;
            nextY = startY + (int)Mathf.Clamp(searchDirection.y, -1, 1) * i;

            if (!IsWithinBounds(nextX, nextY))
            {
                break;
            }

            //get the next piece
            GamePiece nextPiece = m_allGamePieces[nextX, nextY];

            //add this piece to matches if it is identical with the previous one and not already added to matches list 
            if (nextPiece.matchValue == startPiece.matchValue && !matches.Contains(nextPiece))
            {
                matches.Add(nextPiece);
            }
            else
            {
                break;
            }
        }

        //return the macthes list
        if (matches.Count >= minLength)
        {
            return matches;
        }

        //no matched pieces, return null
        return null;
    }

    //method to find vertically matched game pieces
    List<GamePiece> FindVerticalMatches(int startX, int startY, int minLength = 3)
    {   
        //find upward matches by moving +1 on y axis
        List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);
        //find downward matches by moving -1 on y axis
        List<GamePiece> downwardMatches = FindMatches(startX, startY, new Vector2(0, -1), 2);

        if (upwardMatches == null)
        {
            upwardMatches = new List<GamePiece>();
        }

        if (downwardMatches == null)
        {
            downwardMatches = new List<GamePiece>();
        }

        /*
        //combine matches lists
        foreach (GamePiece piece in downwardMatches)
        {
            if (!upwardMatches.Contains(piece))
            {
                upwardMatches.Add(piece);
            }
        }

        //if there is enough matches return the matches list, else null
        return (upwardMatches.Count >= minLength) ? upwardMatches : null;
        */

        //combine matches lists
        var combinedMatches = upwardMatches.Union(downwardMatches).ToList();
        //if there is enough matches return the matches list, else null

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    //method to find horizontally matched game pieces
    List<GamePiece> FindHorizontalMatches(int startX, int startY, int minLength = 3)
    {   
        //find left matches by moving -1 on x axis
        List<GamePiece> leftMatches = FindMatches(startX, startY, new Vector2(-1, 0), 2);
        //find right matches by moving +11 on x axis
        List<GamePiece> rightMatches = FindMatches(startX, startY, new Vector2(1, 0), 2);

        if (leftMatches == null)
        {
            leftMatches = new List<GamePiece>();
        }

        if (rightMatches == null)
        {
            rightMatches = new List<GamePiece>();
        }

        //combine matches lists
        var combinedMatches = rightMatches.Union(leftMatches).ToList();
        //if there is enough matches return the matches list, else null

        return (combinedMatches.Count >= minLength) ? combinedMatches : null;
    }

    //method to find matches and returning as a combined matches list 
    List<GamePiece> FindMatchesAt(int x, int y, int minLength = 3)
    {
        List<GamePiece> horizMatches = FindHorizontalMatches(x,y,minLength);
        List<GamePiece> vertMatches = FindVerticalMatches(x,y,minLength);

        if (horizMatches == null)
        {
            horizMatches = new List<GamePiece>();
        }

        if (vertMatches == null)
        {
            vertMatches = new List<GamePiece>();
        }
        var combinedMatches = horizMatches.Union(vertMatches).ToList();
        return combinedMatches;
    }

    //method to remove highlight on the given coordinated tile
    void HighlightTileOff(int x, int y)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
    }

    //method to highlight on the given coordinated tile
    void HighlightTileOn(int x, int y, Color col)
    {
        SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
        spriteRenderer.color = col;
    }

    //method to find matched tiles and highlight them
    void HighlightMatchesAt(int x, int y)
    {
        HighlightTileOff(x, y);
        var combinedMatches = FindMatchesAt(x, y);

        if (combinedMatches.Count > 0)
        {
            foreach (GamePiece piece in combinedMatches)
            {
                HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
            }
        }
    }

    //method to higlight all matched tiles at the hole board
    void HighlightMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                HighlightMatchesAt(i, j);

            }
        }
    }
        

}