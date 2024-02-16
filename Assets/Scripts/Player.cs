using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEditor.Rendering;
using UnityEngine;

public class Player : MonoBehaviour
{

    [Header("Game specific variables")]
    [SerializeField] private GameCoordinator gc;

    [Header("Movement")]
    [SerializeField] private float MoveSpeed;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Vector2 moveVector;
    [SerializeField] private Vector2 moveRealized;

    //This will be the recordings of all the different event the player will do
    //Ex. Position: p_123.4|123.4 interpolate copy object to x: 123.4 y: 123.4
    //    CollectItem: c_2 Collect the second item in the list.
    //    Exit Level: e exit the scene
    [Header("Action Recording")]
    [SerializeField] private List<string> ActionList = new List<string>();
    [SerializeField] private bool CollectFlag;
    [SerializeField] private int CollectIndex = 0;

    //To keep with consistency and fluidity keep the record threshold around a quarter of a second: 0.25;
    [SerializeField] private float RecordTimer;
    [SerializeField] private float RecordThreshold;
    // Start is called before the first frame update
    void Start()
    {
        ActionList.Add("p_" + transform.position.x + "|" + transform.position.y);
        gc = GameObject.Find("MainHandler").GetComponent<GameCoordinator>();
        RecordThreshold = gc.ActionRefreshRate;
    }

    // Update is called once per frame
    void Update()
    {
        Move();
        if(RecordTimer >= RecordThreshold)
        {
            ActionList.Add("p_" + transform.position.x + "|" + transform.position.y);
            //record actions and position to Action List
            if(CollectFlag)
            {
                ActionList.Add("c_" + CollectIndex);
            }
            RecordTimer -= RecordThreshold;
        }
        RecordTimer += Time.deltaTime;

        DebugScript();
    }

    private void Move()
    {
        moveVector = Vector2.zero;
        if(Input.GetAxisRaw("Vertical") > 0.1)
        {
            moveVector += Vector2.up;
        }
        else if(Input.GetAxisRaw("Vertical") < -0.1)
        {
            moveVector += Vector2.down;
        }


        if (Input.GetAxisRaw("Horizontal") > 0.1)
        {
            moveVector += Vector2.right;
        }
        else if (Input.GetAxisRaw("Horizontal") < -0.1)
        {
            moveVector += Vector2.left;
        }

        moveRealized = moveVector  * MoveSpeed;
    }

    void DebugScript()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            EndLoop();
        }
    }

    void EndLoop()
    {
        ActionList.Add("e");
        gc.AddNewLoop(ActionList.ToArray());
        ActionList.Clear();
        ActionList.Add("p_" + transform.position.x + "|" + transform.position.y);
    }

    private void FixedUpdate()
    {
        rb.velocity = moveRealized;
    }
}
