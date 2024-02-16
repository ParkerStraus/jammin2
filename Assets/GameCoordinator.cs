using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameCoordinator : MonoBehaviour
{
    [SerializeField] private List<string[]> actionLists = new List<string[]>();
    [SerializeField] private GameObject copyObject;
    [SerializeField] private List<GameObject> currentCopys;

    [SerializeField] public float ActionRefreshRate;
    // Start is called before the first frame update
    void Start()
    {
        
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
            Offset += 1;
        }
        if (actions[step + Offset].Substring(0,1) == "e")
        {
            //Exit Event
            yield return new WaitForEndOfFrame();
            Offset += 1;
        }
        if (actions[step + Offset].Substring(0, 1) == "p")
        {
            //Go the next move event if there is
            StartCoroutine(PerformAction(copyOBJ, actions, step+Offset));
        }

    }

    public void AddNewLoop(string[] actions)
    {
        actionLists.Add(actions);
        StopAllCoroutines();
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
