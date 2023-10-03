using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

    //public variables
    public int xIndex;
    public int yIndex;

    //private variables
    Board m_board;

    //main
	void Start () {
	
	}
	
    //method to init tile to board
    public void Init(int x, int y, Board board){
    
        xIndex = x;
        yIndex = y;
        m_board = board;
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
}
