using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RamailoGames;


public class GameManager : MonoBehaviour
{
    public bool drawGizmos;

    #region Tmp Text

    public TMP_Text GameOverScoreText;
    public TMP_Text GameOverhighscoreText;
    public TMP_Text gamePlayScoreText;
    public TMP_Text gamePlayhighscoreText;

    #endregion

    #region Singleton
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
    }

    #endregion

    #region Transforms

    #endregion

    #region Prefabs

    #endregion

    #region List of objects

    #endregion


    #region Private Serialized Fields
    [SerializeField] int score;

    #endregion

    #region Private Fields

    bool paused;
    float startTime;

    #endregion

    #region Public Fields

    #endregion

    #region MonoBehaviour Functions

    // Start is called before the first frame update
    void Start()
    {
        score = 0;
        ScoreAPI.GameStart((bool s) => {
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    #endregion

    #region Public Functions
    
    public void PauseGame()
    {
        
        Time.timeScale = 0;
        paused = true;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        paused = false;
    }

    public void AddScore(int amount)
    {
        score += amount ;
        gamePlayScoreText.text = score.ToString();
        setHighScore(gamePlayhighscoreText);
    }

    #endregion

    #region Private Functions
    void GameOver()
    {
        PauseGame();
        UIManager.instance.SwitchCanvas(UIPanelType.GameOver);
        UIManager.instance.SwitchCanvas(UIPanelType.GameOver);
        GameOverScoreText.text =score.ToString();
        int playTime = (int)(Time.unscaledTime - startTime);
        ScoreAPI.SubmitScore(score, playTime, (bool s, string msg) => { });
        GetHighScore();
    }

    

    void setHighScore(TMP_Text highscroreTextUI)
    {
        ScoreAPI.GetData((bool s, Data_RequestData d) => {
            if (s)
            {
                if (score >= d.high_score)
                {
                    highscroreTextUI.text = score.ToString();

                }
                else
                {
                    highscroreTextUI.text = d.high_score.ToString();
                }

            }
        });
    }
    


    void GetHighScore()
    {
        ScoreAPI.GetData((bool s, Data_RequestData d) => {
            if (s)
            {
                if (score >= d.high_score)
                {
                    GameOverhighscoreText.text = score.ToString();

                }
                else
                {
                    GameOverhighscoreText.text =d.high_score.ToString();
                }

            }
        });

    }

    #endregion

    #region Coroutines

    #endregion
}
