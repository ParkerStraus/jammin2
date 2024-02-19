using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    [SerializeField] private MainGameHandler MH;

    [SerializeField] private List<string[]> actionLists = new List<string[]>();
    [SerializeField] private GameObject currentPlayer;
    [SerializeField] private GameObject copyObject;
    [SerializeField] private List<GameObject> currentCopys;
    [SerializeField] private Coroutine[] copyRoutines = new Coroutine[5];
    [SerializeField] private GameObject[] SpawnPoints;

    [SerializeField] public float ActionRefreshRate;

    [SerializeField] public float TimeLimit;
    [SerializeField] public float TimeSet;
    [SerializeField] public bool TimeCountDown;
    [SerializeField] public float TimeShortest;

    [SerializeField] public GameObject[] itemObj;
    [SerializeField] public bool[] itemCollection = {false, false, false, false, false};
    [SerializeField] public bool[] copiesFinished = { false, false, false, false, false };

    [SerializeField] private bool Paradoxed = false;

    [Header("Sound Effects")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] SoundEffects;

    
    // Start is called before the first frame update
    void Start()
    {
        currentPlayer = GameObject.Find("Player");
        MH = GameObject.Find("Main Camera").GetComponent<MainGameHandler>();
        MH.PrepareCamera(currentPlayer);
        StartCoroutine(Introduction());

        TimeSet = MH.GetTime() - MH.GetDifficulty();

        TimeLimit = TimeSet;
        TimeCountDown = false;
        TimeShortest = TimeSet;
    }

    IEnumerator Introduction()
    {
        MH.CueSound(1);
        MH.TextCue("PLAYER READY", 1.5f);
        yield return new WaitForSeconds(1.5f);
        MH.TextCue("LEVEL "+MH.GetLevel(), 1.5f);

        yield return new WaitForSeconds(1.5f);
        audioSource.Play();
        currentPlayer.GetComponent<Player>().SetEnable(true);
        //Start Game
        TimeCountDown = true;
    }

    void CompleteGame()
    {
        audioSource.Stop();
        currentPlayer.SetActive(false);
        for(int i  = 0; i < currentCopys.Count; i++)
        {
            Destroy(currentCopys[i]);
        }
        MH.StartCoroutine(MH.LevelComplete(TimeShortest, currentCopys.Count + 1));
    }

    // Update is called once per frame
    void Update()
    {
        if (TimeCountDown)
        {
            TimeLimit -= Time.deltaTime;

        }
        if(TimeLimit <= 0 && Paradoxed == false)
        {
            Paradoxed = true;
            TimeCountDown = false;
            TimeLimit = 0;
            StartCoroutine(TimesUp());
        }
        MH.InGameTextSet(TimeLimit);
        if(TimeShortest > TimeLimit)
        {
            TimeShortest = TimeLimit;
        }
    }

    IEnumerator PerformAction(GameObject copyOBJ, string[] actions, int step)
    {
        int Offset = 0;
        
        Vector2 pos = copyOBJ.transform.position;
        float TimeCurrent = 0.0f;
        if (step == 0)
        {
            string[] Initcoordinates = actions[step].Split('_')[1].Split('|');
            copyOBJ.transform.position = float.Parse(Initcoordinates[0].Trim()) * Vector2.right + float.Parse(Initcoordinates[1].Trim()) * Vector2.up;
            Offset += 1;
        }
        else if (actions[step].Substring(0, 1) == "p")
        {
            //Move event
            string[] coordinates = actions[step].Split('_')[1].Split('|');
            Vector2 des = float.Parse(coordinates[0].Trim()) * Vector2.right + float.Parse(coordinates[1].Trim()) * Vector2.up;
            copyOBJ.GetComponent<PlayerAnim>().CurrentVelocity = des - pos;
            while (ActionRefreshRate > TimeCurrent)
            {
                copyOBJ.transform.position = Vector2.Lerp(pos, des, TimeCurrent/ ActionRefreshRate);
                TimeCurrent += Time.deltaTime;
                yield return new WaitForEndOfFrame();
            }
            copyOBJ.transform.position = des;
            Offset += 1;
        }
        if (actions[step + Offset].Substring(0, 1) == "c")
        {
            //Collection event
            Debug.Log("Item Collected");
            CollectItem(int.Parse(actions[step + Offset].Substring(2, 1)), true);
            Offset += 1;
        }
        if (actions[step + Offset].Substring(0, 1) == "p")
        {
            //Go the next move event if there is
            StartCoroutine(PerformAction(copyOBJ, actions, step + Offset));
        }
        if(actions[step + Offset].Substring(0, 1) == "e")
        {
            copyOBJ.SetActive(false);
        }
        yield return new WaitForEndOfFrame();

    }

    public void CollectItem(int item, bool CopyTake)
    {
        if (itemCollection[item] == true && CopyTake == true && Paradoxed == false)
        {
            Paradox();
        }
        else
        {

            itemCollection[item] = true;
            itemObj[item].SetActive(false);
        }
    }

    public void ItemMsg(int item)
    {
        string Msg = "";
        switch (item)
        {
            case 0:
                Msg = "POWER SUPPLY GET";
                break;
            case 1:
                Msg = "PROCESSOR GET";
                break;
            case 2:
                Msg = "RAM GET";
                break;
            case 3:
                Msg = "KEYBOARD GET";
                break;
            case 4:
                Msg = "GRAPHICS CARD GET";
                break;
        }
        MH.TextCue(Msg, 1.5f);
        MH.AddPoints(250);
        audioSource.PlayOneShot(SoundEffects[2]);
    }

    public void Paradox()
    {
        
        Paradoxed = true;
        currentPlayer.GetComponent<Player>().SetEnable(false);
        for(int i = 0; i < currentCopys.Count; i++)
        {
            StopCoroutine(copyRoutines[i]);
        }
        StartCoroutine(ParadoxEvent());
    }

    IEnumerator ParadoxEvent()
    {
        TimeCountDown = false;
        audioSource.Pause();
        MH.ToggleFilter(0);
        MH.TextCue("PARADOX", 3f);
        audioSource.Stop();
        audioSource.PlayOneShot(SoundEffects[1]);
        yield return new WaitForSeconds(3.25f);
        for(int i = 0; i < currentCopys.Count; i++)
        {
            Destroy(currentCopys[i]);
        }
        MH.StartCoroutine(MH.LostLife());
    }
    IEnumerator TimesUp()
    {
        currentPlayer.GetComponent<Player>().SetEnable(false);
        audioSource.Pause();
        MH.ToggleFilter(0);
        MH.TextCue("TIME'S UP", 3f);
        audioSource.Stop();
        audioSource.PlayOneShot(SoundEffects[1]);
        yield return new WaitForSeconds(3.25f);
        for (int i = 0; i < currentCopys.Count; i++)
        {
            Destroy(currentCopys[i]);
        }
        MH.StartCoroutine(MH.LostLife());
    }



    IEnumerator TimeOverload()
    {
        currentPlayer.GetComponent<Player>().SetEnable(false);
        audioSource.Pause();
        MH.ToggleFilter(0);
        MH.TextCue("LOOP OVERLOAD", 3f);
        audioSource.Stop();
        audioSource.PlayOneShot(SoundEffects[1]);
        yield return new WaitForSeconds(3.25f);
        for (int i = 0; i < currentCopys.Count; i++)
        {
            Destroy(currentCopys[i]);
        }
        MH.StartCoroutine(MH.LostLife());
    }

    public void AddNewLoop(string[] actions)
    {
        
        for(int i = 0; i < itemObj.Length; i++)
        {
            itemCollection[i] = false;
            itemObj[i].SetActive(true);
        }
        actionLists.Add(actions);
        StopAllCoroutines();
        //check if all actions acquired the items
        for (int i = 0; i < actionLists.Count; i++)
        {
            for(int j = 0; j < actionLists[i].Length; j++)
            {

                if (actionLists[i][j].Substring(0, 1) == "c")
                {
                    //Collection event
                    Debug.Log("Item Collected");
                    int itemCol = int.Parse(actionLists[i][j].Substring(2, 1));
                    itemCollection[itemCol] = true;
                }
            }
        }

        //Raise flag if not all items are collected
        bool NoCollectFlag = false;
        for (int i = 0; i < itemObj.Length; i++)
        {
            if (itemCollection[i] == false)
            {
                NoCollectFlag = true;
            }

        }
        if( NoCollectFlag)
        {

            //Reset all collections
            for (int i = 0; i < itemObj.Length; i++)
            {
                itemCollection[i] = false;
                itemObj[i].SetActive(true);
            }
            StartCoroutine(LoopCreation());
            //Spawn Player in new spawnpoint
        }
        else
        {
            //LevelComplete
            CompleteGame();
        }

    }

    IEnumerator LoopCreation()
    {
        TimeCountDown = false;
        if (currentCopys.Count >= 4)
        {
            StartCoroutine(TimeOverload());
        }
        else
        {
            //Reset Timer
            MH.ToggleFilter(1);
            TimeLimit = TimeSet;

            //Spawn Character in
            currentPlayer.transform.position = SpawnPoints[actionLists.Count].transform.position;
            //Hide Character
            currentPlayer.SetActive(false);
            //Create animation

            audioSource.PlayOneShot(SoundEffects[0]);
            MH.TextCue("LOOP " + (currentCopys.Count + 2), 2f);
            yield return new WaitForSeconds(2f);
            for (int i = 0; i < currentCopys.Count; i++)
            {
                Destroy(currentCopys[i]);
            }
            currentCopys.Clear();
            for (int i = 0; i < actionLists.Count; i++)
            {
                var copy = Instantiate(copyObject);
                currentCopys.Add(copy);
                copyRoutines[i] = StartCoroutine(PerformAction(copy, actionLists[i], 0));
            }
            currentPlayer.SetActive(true);
            TimeCountDown = true;
        }

    }
}
