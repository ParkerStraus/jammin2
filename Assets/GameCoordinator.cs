using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;

public class GameCoordinator : MonoBehaviour
{
    [SerializeField] private MainGameHandler MH;

    [SerializeField] private List<string[]> actionLists = new List<string[]>();
    [SerializeField] private GameObject currentPlayer;
    [SerializeField] private GameObject copyObject;
    [SerializeField] private List<GameObject> currentCopys;
    [SerializeField] private GameObject[] SpawnPoints;

    [SerializeField] public float ActionRefreshRate;

    [SerializeField] public GameObject[] itemObj;
    [SerializeField] public bool[] itemCollection = {false, false, false, false, false};
    // Start is called before the first frame update
    void Start()
    {
        currentPlayer = GameObject.Find("Player");
        MH = GameObject.Find("Main Camera").GetComponent<MainGameHandler>();
        MH.PrepareCamera(currentPlayer);
        StartCoroutine(Introduction());


    }

    IEnumerator Introduction()
    {
        MH.CueSound(1);
        MH.TextCue("PLAYER 1", 1.5f);
        yield return new WaitForSeconds(1.5f);
        MH.TextCue("LEVEL "+MH.GetLevel(), 1.5f);
        //Start Game
    }

    IEnumerator CompleteGame()
    {
        MH.CueSound(2);
        MH.TextCue("STORE RANSACKED", 3f);
        yield return new WaitForSeconds(3.1f);
    }

    // Update is called once per frame
    void Update()
    {
        
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
            CollectItem(int.Parse(actions[step + Offset].Substring(2, 1)));
            Offset += 1;
        }
        if (actions[step + Offset].Substring(0, 1) == "p")
        {
            //Go the next move event if there is
            StartCoroutine(PerformAction(copyOBJ, actions, step + Offset));
        }
        yield return new WaitForEndOfFrame();

    }

    public void CollectItem(int item)
    {
        itemCollection[item] = true;
        itemObj[item].SetActive(false);
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
        //Spawn Player in new spawnpoint
        currentPlayer.transform.position = SpawnPoints[actionLists.Count].transform.position;
        for (int i = 0; i < currentCopys.Count; i++)
        {
            Destroy(currentCopys[i]);
        }
        currentCopys.Clear();
        for(int i = 0; i < actionLists.Count; i++)
        {
            var copy = Instantiate(copyObject);
            currentCopys.Add(copy);
            StartCoroutine(PerformAction(copy, actionLists[i], 0));
        }
    }
}
