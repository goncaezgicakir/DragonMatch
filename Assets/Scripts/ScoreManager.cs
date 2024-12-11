using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScoreManager : Singleton<ScoreManager> {

    //private variables
    int m_currentScore = 0;
	int m_counterValue = 0; //score to count up really fast to current scote
	int m_increment = 5; //increment amount for counting up fastly

    //public variables
    public Text scoreText;
	public float countTime = 1f;


	void Start () {
	
	}

	//method to update score value on the UI
	public void UpdateScoreText(int scoreValue)
	{

		if (scoreText != null)
		{
			scoreText.text = scoreValue.ToString();
		}
	}

	//method to increment score by given value
	public void AddScore(int value)
	{
		m_currentScore += value;
		StartCoroutine(CountScoreRoutine());
	}

    //method to increment previous score by counting really fast on UI till reaching the new current score
    IEnumerator CountScoreRoutine()
	{
		int iterations = 0;

		while ((m_counterValue < m_currentScore) && (iterations < 100000))
		{
			m_counterValue += m_increment;
			UpdateScoreText(m_counterValue);
			iterations++;
			yield return null;
		}

		m_counterValue = m_currentScore;
		UpdateScoreText (m_currentScore);

	}
}
