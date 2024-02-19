using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainGameHandler : MonoBehaviour
{

    [Header("UI Intro Screen")]
    [SerializeField] private GameObject i_Logo;
    [SerializeField] private TMP_Text i_Desc;

    [Header("UI Elements/Camera")]
    [SerializeField] private TMP_Text p1_Text;
    [SerializeField] private TMP_Text p2_Text;
    [SerializeField] private TMP_Text cue_Text;
    [SerializeField] private TMP_Text highscore_Text;
    [SerializeField] private TMP_Text credits_Text;
    [SerializeField] private TMP_Text gameOver_Text;
    [SerializeField] private TMP_Text storyText;
    [SerializeField] private TMP_Text livesText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private GameObject VirCam;

    [Header("UI Results")]
    [SerializeField] private GameObject r_PCIMAGE;
    [SerializeField] private TMP_Text r_ResultTitle;
    [SerializeField] private TMP_Text r_ResultRow;
    [SerializeField] private TMP_Text r_Time;
    [SerializeField] private TMP_Text r_FastBonus;
    [SerializeField] private TMP_Text r_Loop;
    [SerializeField] private TMP_Text r_LoopBonus;

    [Header("Common Values")]
    [SerializeField] private int Level;
    [SerializeField] private int Level2;
    [SerializeField] private int Player1 = 999999;
    [SerializeField] private int Lives1 = 3;
    [SerializeField] private int Player2 = 999999;
    [SerializeField] private int HighScore = 100000;
    [SerializeField] private int credits = 0;
    [SerializeField] private bool InGame;
    [SerializeField] private bool GameOverOverride;
    [SerializeField] private int TimeSet;
    [SerializeField] private int TimeDifficult;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource AudioSource;
    [SerializeField] private AudioClip Coin_In;
    [SerializeField] private AudioClip Coin_Mech;
    [SerializeField] private AudioClip[] MusicCues;
    [SerializeField] private AudioClip[] SoundEffects;

    // Start is called before the first frame update
    void Start()
    {
        Level = -1;
        TimeDifficult = -2 * (Level / 3);
    }

    // Update is called once per frame
    void Update()
    {
        PrepareGame();

        GetGameInput();
        SetText();

        if(InGame == true && Player1 > HighScore)
        {
            HighScore = Player1;
        }
        else if (InGame == true && Player2 > HighScore)
        {
            HighScore = Player2;
        }
        TimeDifficult = -2 * (Level / 3);

        if (Input.GetButtonDown("Escape"))
        {
            Application.Quit();   
        }
    }

    void PrepareGame()
    {
        if (Input.GetButtonDown("1P"))
        {
            if (!InGame && credits >= 1)
            {
                TimeDifficult = 0;
                Player1 = 0;
                Player2 = 0;
                InGame = true;
                credits -= 1;
                Level = 0;
                StartCoroutine(GameIntro());
            }
        }
    }

    IEnumerator GameIntro()
    {
        storyText.gameObject.SetActive(true);
        CueSound(MusicCues[0]);
        yield return new WaitForSeconds(14);

        storyText.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.25f);
        //Animate Screen
        LoadLevel(0);
        yield return null;
    }


    void LoadLevel(int Level)
    {

        SceneManager.LoadScene(1+(Level%3), LoadSceneMode.Additive);

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

    public int GetDifficulty()
    {
        return TimeDifficult;
    }

    public int GetTime()
    {
        return TimeSet;
    }

    void SetText()
    {
        p1_Text.text = Player1.ToString();
        highscore_Text.text = HighScore.ToString();
        if(InGame == false)
        {
            if (credits == 0)
            {
                credits_Text.text = "INSERT COINS";
            }
            else
            {
                if(credits == 1) credits_Text.text = "PRESS 1 TO START PLAYING";
            }
            timeText.enabled = false;
            livesText.enabled = false;
            i_Logo.SetActive(true);
            i_Desc.gameObject.SetActive(true);
            credits_Text.rectTransform.localPosition = new Vector2(0, -80);
            if (credits >= 1)
            {
                i_Logo.SetActive(false);
                i_Desc.gameObject.SetActive(false);
                credits_Text.rectTransform.localPosition = new Vector2(0,0);
            }
            gameOver_Text.gameObject.SetActive(false);
            credits_Text.gameObject.SetActive(true);

        }
        else
        {
            if (!GameOverOverride)
            {
                gameOver_Text.gameObject.SetActive(false);
            }
            else
            {
                gameOver_Text.gameObject.SetActive(true);

            }
            credits_Text.gameObject.SetActive(false);
            timeText.enabled = true;
            livesText.enabled = true;
            livesText.text = "LIVES: " + Lives1.ToString("D2");
            i_Logo.SetActive(false);
            i_Desc.gameObject.SetActive(false);

        }
    }

    public void InGameTextSet(float Time)
    {
        timeText.text = "TIME=" + ((int)Mathf.Ceil(Time)).ToString("D2");
    }

    public void AddPoints(int points)
    {
        Player1 += points;
    }

    public void ToggleFilter(int filter)
    {
        switch(filter) {
            case 0:
                GetComponent<Animator>().CrossFade("Camera PARADOX", 0, 0);
                break;
            case 1:
                GetComponent<Animator>().CrossFade("Camera Warp", 0, 0);
                break;
        }
    }

    public void StartGame()
    {
        InGame = true;
        Lives1 = 3;
    }

    public void EndGame()
    {
        InGame = false;
        Level = -1;
    }

    public int GetLevel()
    {
        return Level+1;
    }


    public IEnumerator LevelComplete(float time, int loops)
    {
        yield return new WaitForSeconds(1f);
        SceneManager.UnloadScene(1 + Level%3);
        CueSound(2);
        r_PCIMAGE.SetActive(true);
        TextCue("PC COMPLETE!", 1.5f);
        yield return new WaitForSeconds(1.5f);
        TextCue("3000 POINT FOR COMPLETION", 1.5f);
        AddPoints(3000);
        yield return new WaitForSeconds(2.25f);
        r_PCIMAGE.SetActive(false);
        //Show Results
        r_ResultTitle.gameObject.SetActive(true);
        r_ResultRow.gameObject.SetActive(true);
        //display time
        yield return new WaitForSeconds(0.75f);
        r_Time.gameObject.SetActive(true);
        //Time Display
        r_Time.text = "TIME: ";
        yield return new WaitForSeconds(0.75f);
        int CurrentValue = 0;
        r_Time.text = "TIME: " + CurrentValue;
        while (CurrentValue < (int)Mathf.Ceil(time))
        {
            CurrentValue++;
            CueSound(SoundEffects[0]);

            r_Time.text = "TIME: " + CurrentValue;
            yield return new WaitForSeconds(0.125f);
        }
        yield return new WaitForSeconds(1f);

        //Display Time bonus
        if((int)Mathf.Ceil(time) >= 10)
        {
            r_FastBonus.gameObject.SetActive(true);
            AddPoints(1500);
            CueSound(SoundEffects[1]);
        }
        yield return new WaitForSeconds(1f);



        //Loop Display
        r_Loop.gameObject.SetActive(true);
        r_Loop.text = "LOOP: ";
        yield return new WaitForSeconds(0.75f);

        CurrentValue = 0;
        r_Loop.text = "LOOP: " + CurrentValue;
        while (CurrentValue < loops)
        {
            CurrentValue++;
            CueSound(SoundEffects[0]);

            r_Loop.text = "LOOP: " + CurrentValue;
            yield return new WaitForSeconds(0.125f);
        }
        yield return new WaitForSeconds(1f);

        //Display Loop bonus
        if ((int)Mathf.Ceil(loops) <= 3)
        {
            if((int)Mathf.Ceil(loops) == 1)
            {
                r_LoopBonus.gameObject.SetActive(true);
                r_LoopBonus.text = "PERFECT SWEEP: 3500";
                AddPoints(3500);
                CueSound(SoundEffects[1]);
            }
            else
            {

                r_LoopBonus.gameObject.SetActive(true);
                r_LoopBonus.text = "LOOP BONUS: 1000";
                AddPoints(1000);
                CueSound(SoundEffects[1]);
            }
        }

        yield return new WaitForSeconds(2f);
        r_ResultTitle.gameObject.SetActive(false);
        r_ResultRow.gameObject.SetActive(false);
        r_Time.gameObject.SetActive(false);
        r_FastBonus.gameObject.SetActive(false);
        r_Loop.gameObject.SetActive(false);
        r_LoopBonus.gameObject.SetActive(false);

        //Display loop amount
        Level++;
        TimeDifficult = 2 * (Level / 3);
        LoadLevel(Level);
    }

    public void TextCue(string text, float time)
    {
        StartCoroutine(textCue(text, time));

    }

    IEnumerator textCue(string text, float time)
    {
        cue_Text.text = text;
        cue_Text.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        cue_Text.gameObject.SetActive(false);


    }

    public void PrepareCamera(GameObject player)
    {
        VirCam.GetComponent<CinemachineVirtualCamera>().Follow = player.transform;
        VirCam.transform.position = player.transform.position;
    }

    public IEnumerator LostLife()
    {
        
            Lives1--;
            SceneManager.UnloadScene(1 + Level);
            if (Lives1 == 0)
            {
                GameOverOverride = true;
                CueSound(3);
                yield return new WaitForSeconds(5.5f);
                GameOverOverride = false;
                EndGame();

            }
            else
            {

                yield return new WaitForSeconds(0.4f);
                SceneManager.LoadScene(1 + Level, LoadSceneMode.Additive);
            }
        
    }

    public void CueSound(AudioClip sound)
    {
        AudioSource.PlayOneShot(sound);
    }

    public void CueSound(int Sound)
    {
        AudioSource.PlayOneShot(MusicCues[Sound]);
    }
}
