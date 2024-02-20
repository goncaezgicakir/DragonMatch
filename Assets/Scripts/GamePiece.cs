using System.Linq.Expressions;
using UnityEngine;
using System.Collections;

public class GamePiece : MonoBehaviour {

    //public variables
    public int xIndex;
    public int yIndex;
    public InterpType interpolation = InterpType.SmootherStep;
    public MatchValue matchValue;
    
    //private variables
    Board m_board;
    bool m_isMoving = false;

    //interpolation types
    public enum InterpType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    };

    //game piece match value types
    /*public enum MatchValue
    {
        Yellow,
        Blue,
        Indigo,
        Green,
        DarkGreen,
        Red,
        Lilac,
        Wild

    }*/

     public enum MatchValue
    {
        BlueEgg,
        GreenEgg,
        IndigoEgg,
        LilacEgg,
        OrangeEgg,
        PinkEgg,
        PurpleEgg,
        None
    }

	//main
	void Start () {
	
	}

    //updates frames
    void Update () 
    {
        
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move((int)transform.position.x + 2, (int) transform.position.y, 0.5f);

        }

        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move((int)transform.position.x - 2, (int) transform.position.y, 0.5f);
        }
    }
	
    //method to init board
    public void Init(Board board){

        m_board = board;
    }

    //method to set coordinates of the game piece
    public void SetCoord(int x, int y){

        xIndex = x;
        yIndex = y;
    }

    //method to move the game piece to specified coordinate
    public void Move(int destX, int destY, float timeToMove)
    {
        if(!m_isMoving)
        {
            StartCoroutine(MoveRoutine(new Vector3(destX, destY, 0), timeToMove));
        }
    }

    //method to handle moving process of the game piece
    IEnumerator MoveRoutine(Vector3 destination, float timeToMove)
    {
        Vector3 startPosition = transform.position;

        bool reachedDestination = false;

        float elapsedTime = 0f;

        m_isMoving = true;

        while (!reachedDestination)
        {

            //if we are close enough to destination
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                reachedDestination = true;

                if (m_board != null)
                {
                    m_board.PlaceGamePiece(this, (int)destination.x, (int)destination.y);
                }
                break;
            }
            
            //total running time
            elapsedTime += Time.deltaTime;
            
            // calculate the Lerp value
            float t = Mathf.Clamp(elapsedTime / timeToMove, 0f, 1f);
            
            //set the interpolation type
            switch (interpolation)
            {
                case InterpType.Linear:
                    break;
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;
                case InterpType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterpType.SmoothStep:
                    t = t*t*(3 - 2*t);
                    break;
                case InterpType.SmootherStep:
                    t =  t*t*t*(t*(t*6 - 15) + 10);
                    break;
            }

            
            //move the game piece
            transform.position = Vector3.Lerp(startPosition, destination, t);

            //wait until next frame
            yield return null;
        }

        m_isMoving = false;
    }

    public void ChangeColor(GamePiece pieceToMatch)
    {
        SpriteRenderer rendererToChange = GetComponent<SpriteRenderer>();

        if(pieceToMatch != null)
        {
            SpriteRenderer rendererToMatch = GetComponent<SpriteRenderer>();

            if(rendererToMatch != null && rendererToChange != null)
            {
                
                switch(pieceToMatch.matchValue)
                {
                    case MatchValue.BlueEgg:
                        rendererToChange.color = new Color32(128, 229, 255, 255);
                        break;
                    case MatchValue.GreenEgg:
                        rendererToChange.color = new Color(36, 143, 36);
                        break;
                    case MatchValue.IndigoEgg:
                        rendererToChange.color = new Color(0, 0, 204, 255);
                        break;
                    case MatchValue.LilacEgg:
                        rendererToChange.color = new Color(191, 128, 255, 255);
                        break;
                    case MatchValue.OrangeEgg:
                        rendererToChange.color = new Color(255, 128, 0, 255);
                        break;
                    case MatchValue.PinkEgg:
                        rendererToChange.color = new Color(204, 0, 102, 255);
                        break;
                    case MatchValue.PurpleEgg:
                        rendererToChange.color = new Color(204, 51, 255, 255);
                        break;
                    default:
                        break;
                }
            }
        }

        matchValue = pieceToMatch.matchValue;
    }


}
