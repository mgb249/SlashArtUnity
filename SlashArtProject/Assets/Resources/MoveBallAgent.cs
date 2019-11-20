using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
public class MoveBallAgent : Agent
{

    protected Rigidbody rBody;

    protected bool canJump = false;

    protected bool canBoost = false;
    protected int speed = 5;
    protected float _current_boost = 4f;  //Boost multiplier that gradualt fades 

    public GameObject target;
    public GameObject ladder;

    protected GameObject current_nearest;
    protected float distance_nearest = 100f;



    public float spawnRange;

    private float updateTimer = 0;

    public bool ignoreNeighbors = true;

    // Start is called before the first frame update
    void Start()
    {
        this.tag = "ball";
        this.rBody = this.GetComponent<Rigidbody>();

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void FixedUpdate()
    {
        updateTimer += Time.fixedDeltaTime;
    }




    Vector3 jump(float action, Vector3 controlSignal)
    {


        Vector3 jumpSignal = Vector3.zero;

  

        if (rBody.velocity.y <= 0)
        {
            if (canJump)
            {
                if (action > 0)
                {
                    jumpSignal.y  = action * (1.0f / speed) * 300f;
                    rBody.AddForce(jumpSignal * speed);
                }
            }
        }
        return controlSignal;
    }

    Vector3 boost(float action, Vector3 controlSignal)
    {
        Vector3 boostSignal = Vector3.zero;
        boostSignal.x = controlSignal.x;
        boostSignal.z = controlSignal.z;

        if (action > 0)
        {
            if (Mathf.Abs(rBody.velocity.x) < 5f)
            {
                if (Mathf.Abs(rBody.velocity.z) < 5f)
                {
                    if (canJump)
                    {
                        canBoost = true;
                    }
                }
            }

            if (canBoost)
            {
                rBody.AddForce(boostSignal * 4f, ForceMode.Impulse);
                canBoost = false;
            }
        }
        return controlSignal;
    }




    void setJump()
    {
        canJump = true;
    }

    void stopJump()
    {
        canJump = false;
    }



    private void OnCollisionExit(Collision collision)
    {
        stopJump();

    }




    List<Vector3> aggregateWorldKnowledge()
    {



        List<Vector3> allData = new List<Vector3>();

        Vector3 nearestNeighborDistance = Vector3.one * 100f;
        Vector3 nearestNeighborVelocity = Vector3.zero;
        if (this.current_nearest != null)
        {
            nearestNeighborDistance = this.gameObject.transform.position - this.current_nearest.transform.position;
            nearestNeighborVelocity = this.gameObject.GetComponent<Rigidbody>().velocity;

        }
        Vector3 ladderPos = this.ladder.transform.localPosition;
        Vector3 goalPos = this.target.transform.localPosition;
        Vector3 myVelocity = this.rBody.velocity;
        Vector3 goalDis = this.gameObject.transform.position - this.target.transform.position;

        allData.Add(nearestNeighborDistance);
        allData.Add(nearestNeighborVelocity);
        allData.Add(ladderPos);
        allData.Add(goalPos);
        allData.Add(myVelocity);
        allData.Add(goalDis);

        return allData;



    }


    // Spawn ball around the ladder
    Vector3 getSpawnLocation()
    {

        Vector3 startPos = GameObject.Find("SpawnPoint").transform.position;

        var x = startPos.x + Random.Range(-spawnRange, spawnRange);
        var y = 4f;
        var z = startPos.z + Random.Range(-spawnRange, spawnRange);

        var position = new Vector3(x, y, z);

        //print("where am I spawning : " + position);
        // print("where is the ladder : " + ladderPos);

        // print("position is at " + position);

        return position;
    }
    //This is purely to reset the agent. Spawn it somewhere random and give it an initial velocity of 0
    public override void AgentReset()
    {
        var distanceToTarget = Vector3.Distance(this.transform.position,
                                                   target.transform.position);

        if (this.transform.position.y < 0)
        {
            // If the Agent fell, zero its momentum
            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.position = getSpawnLocation();

            //this.transform.localPosition =  new Vector3(0, 0.5f, -6f);
        }



            this.rBody.angularVelocity = Vector3.zero;
            this.rBody.velocity = Vector3.zero;
            this.transform.position = getSpawnLocation();
       

    }

    public override void CollectObservations()
    {

        List<Vector3> aggregated = aggregateWorldKnowledge();

        foreach (Vector3 v in aggregated)
        {
            AddVectorObs(v);
        }


    }
    public void Act(float[] vectorAction)
    {



        //Movement 
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];

        this.rBody.AddForce(controlSignal * speed);
        //JUMP 
        controlSignal = jump(vectorAction[2], controlSignal);
        controlSignal = boost(vectorAction[3], controlSignal);

       



    }
    protected void _setCloserNeighbor(GameObject g)

    {

        if (ignoreNeighbors == false) { //if you want to pay attention to neighbors
            if (g.tag == this.tag)
            {
                if (this.current_nearest == null)
                {
                    this.current_nearest = g;
                }
                else
                {
                    float dist = Vector3.Distance(g.transform.position, this.transform.position);
                    if (dist < distance_nearest)
                    {
                        this.distance_nearest = dist;
                        this.current_nearest = g;
                    }
                }

            }
        }
    }

    private void OnTriggerStay(Collider collision)
    {

        _setCloserNeighbor(collision.transform.gameObject);
    }
    private void OnCollisionStay(Collision collision)
    {

        if (this.transform.position.y > collision.transform.position.y)
        {
            setJump();
        }
    }





    public override void AgentAction(float[] vectorAction, string textAction)



    {


      
            Act(vectorAction);
            
    




        var distanceToTarget = Vector3.Distance(this.transform.position,
                                                   ladder.transform.position);

        if (this.transform.position.y < 0)
        {
            Done();

        }
        Debug.Log(distanceToTarget);
        if (distanceToTarget < 25.3f)
        {
            //print("WTF");
            Done();
        }

    }
}
