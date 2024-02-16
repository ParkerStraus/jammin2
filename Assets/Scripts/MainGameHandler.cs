using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameHandler : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TMP_Text p1_Text;
    [SerializeField] private TMP_Text p2_Text;
    [SerializeField] private TMP_Text highscore_Text;
    [SerializeField] private TMP_Text credits_Text;
    [SerializeField] private TMP_Text gameOver_Text;


    [Header("Common Values")]
    [SerializeField] private int Player1 = 999999;
    [SerializeField] private int Player2 = 999999;
    [SerializeField] private int HighScore = 100000;
    [SerializeField] private int credits = 0;
    [SerializeField] private bool InGame;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioClip Coin_In;
    [SerializeField] private AudioClip Coin_Mech;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        PrepareGame();

        GetGameInput();
        SetText();
    }

    void PrepareGame()
    {
        if (Input.GetButtonDown("1P"))
        {
            if (!InGame && credits >= 1)
            {
                Player1 = 0;
                Player2 = 0;
                InGame = true;
                credits -= 1;
                SceneManager.LoadSceneAsync(1,LoadSceneMode.Additive);
            }
        }
    }

    void GetGameInput()
    {
        if (Input.GetButtonDown("InsertCoin"))
        {
            StartCoroutine(InsertCredits());
        }
    }

    IEnumerator InsertCredits()
    {
        CueSound(Coin_Mech);
        yield return new WaitForSeconds(1.11f);
        CueSound(Coin_In);

        credits += 1;
    }



    void SetText()
    {
        p1_Text.text = Player1.ToString();
        p2_Text.text = Player2.ToString();
        highscore_Text.text = HighScore.ToString();
        if(InGame == false)
        {
            gameOver_Text.enabled = true;
            if(credits == 0)
            {
                credits_Text.text = "INSERT COINS";
            }
            else
            {
                if(credits == 1) credits_Text.text = "PRESS FOR 1 PLAYER";
                else credits_Text.text = "PRESS FOR 2 PLAYER";
            }
        }
        else
        {
            gameOver_Text.enabled = false;
            credits_Text.enabled = false;

        }
    }

    public void AddPointsP1(int points)
    {
        Player1 += points;
    }
    public void AddPointsP2(int points)
    {
        Player2 += points;

    }

    public void StartGame()
    {
        InGame = true;
    }

    public void EndGame()
    {
        InGame = false;
    }


    public void CueSound(AudioClip sound)
    {
        AudioSource.PlayOneShot(sound);
    }
}
