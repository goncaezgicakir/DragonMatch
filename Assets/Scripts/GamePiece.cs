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
    public enum MatchValue
    {
        Yellow,
        Blue,
        Magenta,
        Indigo,
        Green,
        Teal,
        Red,
        Cyan,
        Wild

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


}
