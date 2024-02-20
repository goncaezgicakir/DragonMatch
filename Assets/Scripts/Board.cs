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
        public int fillYOffset = 10;
        public float fillMoveTime = 0.5f;
        public GameObject tileNormalPrefab;
        public GameObject tileObstaclePrefab;
        public GameObject[] gamePiecesPrefabs;
        public GameObject adjacentBombPrefab;
        public GameObject columnBombPrefab;
        public GameObject rowBombPrefab;

        public StartingObject[] startingTiles;
        public StartingObject[] startingGamePieces;

        //private variables
        Tile[,] m_allTiles;
        GamePiece[,] m_allGamePieces;
        Tile m_clickedTile;
        Tile m_targetTile;
        GameObject m_clickedTileBomb;
        GameObject m_targetTileBomb;
        bool m_playerSwitchingEnabled = true;
        ParticleManager m_particleManager;

        //container for obstacle tile management
        [System.Serializable]
        public class StartingObject
        {
            public GameObject prefab;
            public int x;
            public int y;
            public int z;
        }

        //main
        void Start () 
        {
            //set all tiles
            m_allTiles = new Tile[width,height];
            //set all game pieces
            m_allGamePieces = new GamePiece[width, height];
            //set particle manager for effects
            m_particleManager = GameObject.FindWithTag("ParticleManager").GetComponent<ParticleManager>();

            SetupTiles();
            SetupGamePieces();
            SetupCamera();  
            //TODO: make constant
            FillBoard(fillYOffset, fillMoveTime);
        }

        //method to set the camera on center to see board properly
        void SetupCamera(){

            Camera.main.transform.position = new Vector3( (float)(width - 1) / 2f, (float)(height - 1) /2f, -10f);
            float aspectRatio = (float)Screen.width / (float)Screen.height;
            float verticalSize = (float)height / 2f + (float)borderSize;
            float horizontalSize = ( (float)width / 2f + (float)borderSize ) / aspectRatio;

            Camera.main.orthographicSize = (verticalSize > horizontalSize) ? verticalSize : horizontalSize;
        }

        //method to create obstacle and normal tiles randomly
        void MakeTiles(GameObject prefab, int x, int y, int z=0)
        {   
            if(prefab != null && IsWithinBounds(x, y))
            {       
                GameObject tile = Instantiate(prefab, new Vector3(x, y, z), Quaternion.identity) as GameObject;
                tile.name = "Tile (" + x + "," + y + ")";
                m_allTiles[x, y] = tile.GetComponent<Tile>();
                tile.transform.parent = transform;
                m_allTiles[x, y].Init(x, y, this);
            }

        }

        //method to create game piece
        void MakeGamePiece(GameObject prefab, int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {
            if (prefab != null && IsWithinBounds(x, y))
            {
                prefab.GetComponent<GamePiece>().Init(this);
                PlaceGamePiece(prefab.GetComponent<GamePiece>(), x, y);

                if (falseYOffset != 0)
                {
                    prefab.transform.position = new Vector3(x, y + falseYOffset, 0);
                    prefab.GetComponent<GamePiece>().Move(x, y, moveTime);
                }

                prefab.transform.parent = transform;
            }

        }

        //method to create bomb
        GameObject MakeBomb(GameObject prefab, int x, int y)
        {
            if(prefab != null && IsWithinBounds(x,y))
            {
                GameObject bomb = Instantiate(prefab, new Vector3(x, y, 0), Quaternion.identity) as GameObject;
                bomb.GetComponent<Bomb>().Init(this);
                bomb.GetComponent<Bomb>().SetCoord(x,y);
                bomb.transform.parent = transform;
                return bomb;
            }     

            return null;       
        }
        
        //method to set tile coordinates
        void SetupTiles()
        {
            //set obstacle tiles
            foreach(StartingObject sTile in startingTiles)
            {
                if(sTile != null)
                {
                    MakeTiles(sTile.prefab, sTile.x, sTile.y, sTile.z);
                }

            }

            //set other tiles 
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {

                    if(m_allTiles[i,j] == null)
                    {
                        MakeTiles(tileNormalPrefab, i, j);
                    }
                   
                }
            }
        }

        //method to set defaultly wanted starting game pieces   
        void SetupGamePieces()
        {
            foreach (StartingObject sPiece in startingGamePieces)
            {
                if(sPiece != null)
                {
                    GameObject piece = Instantiate(sPiece.prefab, new Vector3(sPiece.x, sPiece.y, 0), Quaternion.identity) as GameObject;
                    MakeGamePiece(piece, sPiece.x, sPiece.y, fillYOffset, fillMoveTime);
                }
            }
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

        //method to create a random game piece at the given coordinate
        GamePiece FillRandomAt(int x, int y, int falseYOffset = 0, float moveTime = 0.1f)
        {   
            if(IsWithinBounds(x,y))
            {
                //creates a ranom game piece
                GameObject randomPiece = Instantiate(GetRandomGamePiece(), Vector3.zero, Quaternion.identity) as GameObject;
                MakeGamePiece(randomPiece, x, y, falseYOffset, moveTime);
                return randomPiece.GetComponent<GamePiece>();
            }
            return null;
        }
            
        //method to determine is there a free matched game pieces while filling the board randomly
        bool HasMatchOnFill(int x, int y, int minLength = 3)
        {
            //only check downwards and left since we are filling the board at that direction
            List<GamePiece> leftMatches = FindMatches(x, y, new Vector2(-1,0), minLength);
            List<GamePiece> downwardsMatches = FindMatches(x, y, new Vector2(0, -1), minLength);

            if (leftMatches == null)
            {
                leftMatches = new List<GamePiece>();
            }

            if (downwardsMatches == null)
            {
                downwardsMatches = new List<GamePiece>();
            }

            return(leftMatches.Count > 0 || downwardsMatches.Count > 0);
        }

        //methof to fill the board with random GamePiece objects
        void FillBoard(int falseYOffset = 0, float moveTime = 0.1f)
        {   
            //TODO: make it constant 
            int maxIteration = 100;
            int iteration = 0;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {

                    if (m_allGamePieces[i, j] == null && m_allTiles[i,j].tileType != TileType.Obstacle)
                    {
                        //create a random game piece
                        GamePiece piece = FillRandomAt(i, j, falseYOffset, moveTime);
                        iteration = 0;
                        //if there is a match while filling randomly
                        //clear the game piece and create another game piece
                        while (HasMatchOnFill(i, j))
                        {
                            ClearGamePieceAt(i, j);
                            piece = FillRandomAt(i, j, falseYOffset, moveTime);
                            iteration++;

                            if (iteration >= maxIteration)
                            {
                                Debug.Log("break while loop - filling randomly--------------------------------");
                                break;
                            }
                        }           
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
                //Debug.Log("Clicked tile: " + tile.name);
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

            if (m_playerSwitchingEnabled)
            {
                //get pieces
                GamePiece clickedPiece = m_allGamePieces[m_clickedTile.xIndex, m_clickedTile.yIndex];
                GamePiece targetPiece = m_allGamePieces[m_targetTile.xIndex, m_targetTile.yIndex];            

                if (targetPiece != null && clickedPiece != null)
                {
                    //move pieces
                    clickedPiece.Move(targetTile.xIndex, targetTile.yIndex, swapTime);
                    targetPiece.Move(clickedTile.xIndex, clickedTile.yIndex, swapTime);
                        
                    //waits for swap time befor highlighting matches
                    //delay
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
                    //if there is a match, clear those game pieces from board
                    else
                    {
                        //waits for swap time befor highlighting matches
                        //delay
                        yield return new WaitForSeconds(swapTime);

                        //drop a new bomb if there is more than 4 game pieces are matched
                        Vector2 swapDirection = new Vector2(targetTile.xIndex - clickedTile.xIndex,targetTile.yIndex - clickedTile.yIndex);
                        m_clickedTileBomb = DropBomb(clickedTile.xIndex, clickedTile.yIndex, swapDirection, clickedPieceMatches);
                        m_targetTileBomb = DropBomb(targetTile.xIndex, targetTile.yIndex, swapDirection, targetPieceMatches);

                        //change the clicked bomb's color
                        if(m_clickedTileBomb != null && targetPiece != null)
                        {
                            GamePiece clickedBombPiece = m_clickedTileBomb.GetComponent<GamePiece>();
                            clickedBombPiece.ChangeColor(targetPiece);
                        }

                        //change the target bomb's color
                        if(m_targetTileBomb != null && clickedPiece != null)
                        {
                            GamePiece targetBombPiece = m_targetTileBomb.GetComponent<GamePiece>();
                            targetBombPiece.ChangeColor(clickedPiece);
                        }

                        //clear and refill the board after a match
                        ClearAndRefillBoard(clickedPieceMatches.Union(targetPieceMatches).ToList());
                    }
                }
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

                if (nextPiece == null)
                {
                    break;
                }
                else
                {
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
            List<GamePiece> upwardMatches = FindMatches(startX, startY, new Vector2(0, 1), 2);
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

        //method to find matches with its coordinate values and returning as a combined matches list 
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

        //method to find all matches in the given list and returning as a combined matches list 
        List<GamePiece> FindMatchesAt(List<GamePiece> gamePieces, int minLength = 3)
        {
            List<GamePiece> matches = new List<GamePiece>();

            foreach (GamePiece piece in gamePieces)
            {
                matches = matches.Union(FindMatchesAt(piece.xIndex, piece.yIndex, minLength)).ToList();
            }

            return matches;
        }

        //method to find all matches in the board
        List<GamePiece> FindAllMatches()
        {
            List<GamePiece> combinedMatches = new List<GamePiece>();

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    //find the match at the current coordinate
                    List<GamePiece> matches = FindMatchesAt(i, j);
                    //add the match to combined match list
                    combinedMatches = combinedMatches.Union(matches).ToList();
                }
            }

            return combinedMatches;
        }

        //method to find matches are corner in order to drop a bomb
        bool IsCornerMatch(List<GamePiece> gamePieces)
	{
		bool vertical = false;
		bool horizontal = false;
		int xStart = -1;
		int yStart = -1;

		foreach (GamePiece piece in gamePieces)
		{
			if (piece !=null)
			{
				if (xStart == -1 || yStart == -1)
				{
					xStart = piece.xIndex;
					yStart = piece.yIndex;
					continue;
				}

				if (piece.xIndex != xStart && piece.yIndex == yStart)
				{
					horizontal = true;
				}

				if (piece.xIndex == xStart && piece.yIndex != yStart)
				{
					vertical = true;
				}
			}
		}

		return (horizontal && vertical);

	}

        //method to remove highlight on the given coordinated tile
        void HighlightTileOff(int x, int y)
        {
            if(m_allTiles[x,y].tileType != TileType.Breakable)
            {
                SpriteRenderer spriteRenderer = m_allTiles[x,y].GetComponent<SpriteRenderer>();
                spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, 0);
            }
        }

        //method to highlight on the given coordinated tile
        void HighlightTileOn(int x, int y, Color col)
        {
            if(m_allTiles[x,y].tileType != TileType.Breakable)
            {
                SpriteRenderer spriteRenderer = m_allTiles[x, y].GetComponent<SpriteRenderer>();
                spriteRenderer.color = col;
            }
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

        //method to highlight matched tiles
        void HighlightPieces(List<GamePiece> gamePieces)
        {
            foreach(GamePiece piece in gamePieces)
            {
                if(piece != null)
                {
                    HighlightTileOn(piece.xIndex, piece.yIndex, piece.GetComponent<SpriteRenderer>().color);
                }
            }
        }

        //method to clear the game piece at the given coordinate
        void ClearGamePieceAt(int x, int y)
        {
            GamePiece pieceToClear = m_allGamePieces[x, y];

            if (pieceToClear != null)
            {
                m_allGamePieces[x, y] = null;
                Destroy(pieceToClear.gameObject);
            }

            //HighlightTileOff(x, y);
        }

        //method to clear the game pieces at the given list
        void ClearGamePieceAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    ClearGamePieceAt(piece.xIndex, piece.yIndex);

                    //call the clear effect
                    if (m_particleManager != null)
                    {
                        m_particleManager.ClearPieceFXAt(piece.xIndex, piece.yIndex);
                    }
                }
            }
        }

        //method to break the tile at the given coordinates
        void BreakTileAt(int x, int y)
        {
            Tile tileToBreak = m_allTiles[x, y];

            if (tileToBreak != null && tileToBreak.tileType == TileType.Breakable)
            {
                //call the break effect
                //we need to call this previously, because BreakTile method decrements the breakableValue
                if (m_particleManager != null)
                {
                    m_particleManager.BreakPieceFXAt(tileToBreak.breakableValue, x, y, 0);
                }

                tileToBreak.BreakTile();
            }

        }

        //method to break tiles at the given list
        void BreakTileAt(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                if(piece != null)
                {
                    //break tiles
                    BreakTileAt(piece.xIndex, piece.yIndex);
                }
            }
        }

        //method to collapse column when there is an empty space at the board
        List<GamePiece> CollapseColumn(int column, float collapseTime = 0.1f)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();

            for (int i = 0; i < height; i++)
            {
                //found an empty space
                if (m_allGamePieces[column, i] == null  && m_allTiles[column, i].tileType != TileType.Obstacle)
                {
        
                    for (int j = i + 1; j < height; j++)
                    {   
                        //found the first not empty tile
                        if (m_allGamePieces[column, j] != null)
                        {
                            //move this game piece to the firstly found empty space (i)
                            m_allGamePieces[column, j].Move(column, i, collapseTime * (j-i));
                            m_allGamePieces[column, i] = m_allGamePieces[column, j];
                            m_allGamePieces[column, i].SetCoord(column, i);

                            //store the moved pieces
                            if (!movingPieces.Contains(m_allGamePieces[column, i]))
                            {
                                movingPieces.Add(m_allGamePieces[column, i]);
                            }

                            m_allGamePieces[column, j] = null;
                            break;
                        }
                    }
                }
            }
            return movingPieces;
        }

        //method to collapse column when there is an empty space at the board
        List<GamePiece> CollapseColumn(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<int> columnsToCollapse = GetColumns(gamePieces);

            foreach (int column in columnsToCollapse)
            {
                movingPieces = movingPieces.Union(CollapseColumn(column)).ToList();
            }

            return movingPieces;    
        }

        //method to get list of column numbers
        List<int> GetColumns(List<GamePiece> gamePieces)
        {
            List<int> columns = new List<int>();

            foreach(GamePiece piece in gamePieces)
            {
                if(!columns.Contains(piece.xIndex))
                {
                    columns.Add(piece.xIndex);
                }
            }
            return columns;
        }

        //method to clear all game pieces on the board
        void ClearBoard()
        {
            for (int i=0; i<width; i++)
            {
                for(int j=0; j<height; j++)
                {
                    ClearGamePieceAt(i, j);
                }
            }
        }
       
        //method to check all moving pieces are collapsed
        bool isCollapsed(List<GamePiece> gamePieces)
        {
            foreach (GamePiece piece in gamePieces)
            {
                if (piece != null)
                {
                    //check the game pieces's y locationa and internal location difference 
                    //to see it is collapsed or not
                    if (piece.transform.position.y - (float)piece.yIndex > 0.001f)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
       
        //method to clear and refill the game board
        void ClearAndRefillBoard(List<GamePiece> gamePieces)
        {
           StartCoroutine(ClearAndRefillBoardRoutine(gamePieces));
          
        }

        //method to clear and refill the game board routine
        IEnumerator ClearAndRefillBoardRoutine(List<GamePiece> gamePieces)
        {
            m_playerSwitchingEnabled = false;
            List<GamePiece> matches = gamePieces;

            do
            {
                //clear and collapse
                //wait till the ClearAndRefillBoardRoutine method completed
                yield return StartCoroutine(ClearAndCollapseRoutine(matches));
                //short delay
                yield return null;

                //refill the board
                yield return StartCoroutine(RefillRoutine());
                matches = FindAllMatches();
                //delay
                yield return new WaitForSeconds(0.2f);

            } while(matches.Count != 0);

            m_playerSwitchingEnabled = true;
        }
            
        //method to clear and collapse columns routine recursively
        IEnumerator ClearAndCollapseRoutine(List<GamePiece> gamePieces)
        {
            List<GamePiece> movingPieces = new List<GamePiece>();
            List<GamePiece> matches = new List<GamePiece>();

            //highlight the matched pieces
            //HighlightPieces(gamePieces);

            //short delay
            yield return new WaitForSeconds(0.2f);

            bool isFinished = false;

            while (!isFinished)
            {
                //add the bombed game pieces to default ones clear them too
                List<GamePiece> bombedPieces = GetBombedPieces(gamePieces);
                gamePieces = gamePieces.Union(bombedPieces).ToList();

                ClearGamePieceAt(gamePieces);
                BreakTileAt(gamePieces);

                //add newly droped bombs to the game pieces array
                if(m_clickedTileBomb != null)
                {
                    ActivateBomb(m_clickedTileBomb);
                    m_clickedTileBomb = null;
                }
                if(m_targetTileBomb != null)
                {
                    ActivateBomb(m_targetTileBomb);
                    m_targetTileBomb = null;
                }


                //delay
                yield return new WaitForSeconds(0.25f);
                movingPieces = CollapseColumn(gamePieces);

                //waits till all of the columns are collapsed
                while (!isCollapsed(movingPieces))
                {
                    //short delay
                    yield return null;
                }

                //delay
                yield return new WaitForSeconds(0.2f);

                matches = FindMatchesAt(movingPieces);

                if (matches.Count == 0)
                {
                    isFinished = true;
                    break;
                }
                else
                {
                    //wait till ClearAndCollapseRoutine is completed
                    yield return StartCoroutine(ClearAndCollapseRoutine(matches));
                }
            }

            //short delay
            yield return null;  
        }

        //method to refill the board after a match
        IEnumerator RefillRoutine()
        {
            FillBoard(fillYOffset, fillMoveTime);
            //short delay
            yield return null;
        }

        //method to get all game pieces int the specified row
        List<GamePiece> GetRowPieces(int row)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for(int i=0; i<width; i++)
            {
                if(m_allGamePieces[i, row] != null)
                {
                    gamePieces.Add(m_allGamePieces[i,row]);
                }
            }

            return gamePieces;
        }

        //method to get all game pieces int the specified column
        List<GamePiece> GetColumnPieces(int column)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for(int i=0; i<height; i++)
            {
                if(m_allGamePieces[column,i] != null)
                {
                    gamePieces.Add(m_allGamePieces[column,i]);
                }
            }

            return gamePieces;
        }

        //method to get all adjacent game pieces int the specified coordinates and offset  
        List<GamePiece> GetAdjacentPieces(int x, int y, int offset=1)
        {
            List<GamePiece> gamePieces = new List<GamePiece>();

            for(int i=x-offset; i<=x+offset; i++)
            {
                for(int j=y-offset; j<=y+offset; j++)
                {
                    if(IsWithinBounds(i,j))
                    {
                        gamePieces.Add(m_allGamePieces[i,j]);
                    }
                }
            }

            return gamePieces;
        }

        //method to get all need to be cleared game pieces while there is a bomb
        List<GamePiece> GetBombedPieces(List<GamePiece> gamePieces)
        {
            List<GamePiece> allPiecesToClear = new List<GamePiece>();

            foreach(GamePiece piece in gamePieces)
            {
                if(piece != null)
                {
                    List<GamePiece> piecesToClear = new List<GamePiece>(); //only for the bomb's needed to be cleared game pieces
                    Bomb bomb = piece.GetComponent<Bomb>();

                    //if the game pieces is a bomb, add its relevant needed to be cleared pieces to the list
                    if(bomb != null)
                    {
                        switch (bomb.bombType)
                        {
                            case BombType.Row:
                                piecesToClear = GetRowPieces(bomb.yIndex);
                                break;

                            case BombType.Column:
                                piecesToClear = GetColumnPieces(bomb.xIndex);
                                break;

                            case BombType.Adjacent:
                                piecesToClear = GetAdjacentPieces(bomb.xIndex, bomb.yIndex, 1);
                                break;

                            case BombType.Color:
                                //TODO: add method
                                break;
                        }

                        allPiecesToClear = allPiecesToClear.Union(piecesToClear).ToList(); //unite this bomb's needed to be cleared pieces to all
                    }
                }
            }

            return allPiecesToClear;
        }

        //method to drop a bomb on board after more than 4 game pieces matched
        //according to match type droped bomb type will be set
        GameObject DropBomb (int x, int y, Vector2 swapDirection, List<GamePiece> gamePieces)
        {
            GameObject bomb = null;

            if (gamePieces.Count >= 4)
            {
                //if it is corner match, then adjacent bomb will be dropped
                if (IsCornerMatch(gamePieces))
                {
                    if (adjacentBombPrefab !=null)
                    {
                        bomb = MakeBomb(adjacentBombPrefab, x, y);
                    }
                }
                else
                {   
                    //if it is row match, then row bomb will be dropped
                    if (swapDirection.x != 0)
                    {
                        if (rowBombPrefab !=null)
                        {
                            bomb = MakeBomb(rowBombPrefab, x, y);
                        }

                    }
                    else
                    {            
                        //if it is column match, then column bomb will be dropped
                        if (columnBombPrefab !=null)
                        {
                            bomb = MakeBomb(columnBombPrefab, x, y);
                        }
                    }
                }
            }
            return bomb;
        }

        //method to activate bomb by adding it to game pieces array
        void ActivateBomb(GameObject bomb)
        {
            int x = (int) bomb.transform.position.x;
            int y = (int) bomb.transform.position.y;

            if (IsWithinBounds(x,y))
            {
                m_allGamePieces[x,y] = bomb.GetComponent<GamePiece>();
            }
        }
    }

