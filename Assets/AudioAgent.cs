using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioAgent : MonoBehaviour
{
    public Material matForFoodAgent;
    public Material matNoFood;

    public GameObject[] agentLegs;
    public GameObject agentTail;

    public float maxVelocity = 3.0f;
    /// <summary>
    /// The size of the hungry agent vision in meters used for spherecasting. 
    /// </summary>
    public float hungryAgentVisionSize = 5.0f;
    /// <summary>
    /// The size of the food agent vision in meters used for spherecasting.
    /// This is intentionally very large to give them an advantage as 
    /// they are usually outnumbered 
    /// </summary>
    public float foodAgentVisionSize = 10.0f;

    /// <summary>
    /// The step speed, currently increased for prey agents
    /// </summary>
    public float stepSpeed = 1.0f;

    /// <summary>
    /// The % chance out of 100 of the agent being food.
    /// </summary>
    public float chanceOfAgentBeingFood = 10.0f;

    private bool isAgentFood;

    private List<Renderer> renderers;
    private bool playingAudio = false;
    private pxStrax synth;

    private Quaternion desiredHeadingDirection;
    private new Rigidbody rigidbody;
    private void InitAgent()
    {
        // get all the stuff
        renderers = new List<Renderer>();
        renderers.Add(GetComponent<Renderer>());
        rigidbody = GetComponent<Rigidbody>();
        synth = GetComponent<pxStrax>();
        if (synth == null)
        {
            gameObject.AddComponent<pxStrax>();
            synth = GetComponent<pxStrax>();
        }

        // put the agent somewhere randomly
        RotateAgent(-10.0f, 10.0f);
        transform.position = new Vector3(
            Random.Range(-10.0f, 10.0f),
            Random.Range(-10.0f, 10.0f),
            Random.Range(-10.0f, 10.0f)
        );

        // predator or prey
        isAgentFood = Random.Range(0.0f, 100.0f) < chanceOfAgentBeingFood ? true : false;

        // setup agent types
        if (isAgentFood)
        {
            gameObject.tag = "agent-food";
            stepSpeed *= 4.0f;
            synth.sustain = true;
            synth.release = 0.01f;
            // configure body
            Destroy(agentTail);

            foreach (GameObject leg in agentLegs)
            {
                renderers.Add(leg.GetComponent<Renderer>());
            }

            foreach (Renderer rend in renderers)
            {
                rend.material = matForFoodAgent;
            }

        }
        else
        {
            // prey agent
            gameObject.tag = "agent-hungry";
            synth.sustain = false;
            synth.release = 0.02f;

            // configure body
            foreach (GameObject leg in agentLegs)
            {
                Destroy(leg);
            }
            renderers.Add(agentTail.GetComponent<Renderer>());
            foreach (Renderer rend in renderers)
            {
                rend.material = matNoFood;
            }

        }

    }

    private void RotateAgent(float min, float max)
    {
        Quaternion newRotation = Quaternion.Euler(
            Random.Range(min, max),
            Random.Range(min, max),
            Random.Range(min, max)
        );
        desiredHeadingDirection = newRotation;
    }

    private void StepAgent()
    {

        // slowly bring down the attack 
        // as some agents get assigned higher attack when they see their goal
        if (synth.attack > 0.01f)
        {
            synth.attack -= 0.01f;
        }


        // look for food
        Ray visionRay = new Ray(transform.position, transform.forward);

        string goalTag = isAgentFood ? "agent-hungry" : "agent-food";
        float nearestDistance = Mathf.Infinity;
        Vector3 nearestDesiredAgentPosition = Vector3.zero;
        bool hasAgentFoundGoal = false;

        RaycastHit[] hits;
        if (isAgentFood)
        {
            hits = Physics.SphereCastAll(visionRay, foodAgentVisionSize);
        }
        else
        {
            hits = Physics.SphereCastAll(visionRay, hungryAgentVisionSize);
        }

        // go through what the agent saw
        foreach (RaycastHit hitItem in hits)
        {
            // look for the nearest opposite agent type
            if (hitItem.transform.gameObject.tag == goalTag)
            {
                hasAgentFoundGoal = true;
                float distance = Vector3.Distance(transform.position, hitItem.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestDesiredAgentPosition = hitItem.transform.position;
                }

            }
        }

        // head either towards or away goal
        Vector3 newLookDirection = Vector3.zero;
        if (hasAgentFoundGoal)
        {
            if (isAgentFood)
            {
                // look away from the predator
                newLookDirection = transform.position - nearestDesiredAgentPosition;
                // prey sounds
                // agent is being been eaten, freak out here
                if (nearestDistance < 4.0f)
                {
                    synth.release = Mathf.Abs(nearestDistance - 4.0f) / 4.0f + 0.01f;
                    synth.KeyOn(14);
                    playingAudio = true;
                }
                else
                {
                    float note = nearestDistance;
                    note += 50.0f;
                    note = note < 127 ? note : 127;
                    if (playingAudio == false)
                    {
                        synth.KeyOn(note);
                        synth.attack = note / 127.0f + 0.01f;
                        synth.envelope = note / 127.0f + 0.1f;
                        synth.release = 0.01f;
                        playingAudio = true;
                    }
                    else
                    {
                        synth.KeyOff();
                        playingAudio = false;
                    }
                }
            }
            else
            {
                // look toward the food
                newLookDirection = nearestDesiredAgentPosition - transform.position;
                // make the hungry agents change color if they've found food
                foreach (Renderer rend in renderers)
                {
                    float emission = rend.material.GetFloat("_Emission");
                    emission = emission < 1.0f ? emission + 0.01f : emission;
                    rend.material.SetFloat("_Emission", emission);
                }
                // predator sounds
                float distance = Vector3.Distance(transform.position, nearestDesiredAgentPosition);
                float note = distance;
                note += 50.0f;
                note = note < 127 ? note : 127;
                if (playingAudio == false)
                {
                    synth.KeyOn(note);
                    playingAudio = true;
                }
                else
                {
                    synth.KeyOff();
                    playingAudio = false;
                }
            }
            // update heading direciton for both agents
            desiredHeadingDirection = Quaternion.LookRotation(newLookDirection, transform.up);
        }

        // randomly look elsewhere for goal
        if (hasAgentFoundGoal == false)
        {
            synth.KeyOff();
            playingAudio = false;
            RotateAgent(-55.0f, 55.0f);

            if(isAgentFood==false){
                // make the hungry agents change color back if there's no food
                foreach (Renderer rend in renderers)
                {
                    float emission = rend.material.GetFloat("_Emission");
                    emission = emission > 0.0f ? emission - 0.01f : emission;
                    rend.material.SetFloat("_Emission", emission);
                }
            }
        }

        // Look ahead and rotate to avoid exiting the camera view
        if (CheckAheadForOffscreen())
        {
            Vector3 directionToMoveTowards = Vector3.zero - transform.position;
            desiredHeadingDirection = Quaternion.LookRotation(directionToMoveTowards, transform.up);
        }

        // apply the desired rotation
        rigidbody.rotation = Quaternion.Slerp(rigidbody.rotation, desiredHeadingDirection, 0.05f);

        Debug.DrawLine(transform.position, transform.position + transform.forward * 10.0f, Color.red);

        // prevent speeding off out of control
        // importantly, agents need their rigid body to have drag enabled or this will never/rarely reapply forces
        if (rigidbody.velocity.magnitude < maxVelocity)
        {
            Debug.DrawLine(transform.position, transform.position + transform.forward * 20.0f, Color.blue);

            if (CheckAheadForOffscreen())
            {
                // slow down if you're headed offscreen, you little jerks
                rigidbody.velocity = rigidbody.velocity * 0.75f;
            }
            else
            {
                // move hungry agents faster if they found food
                float moveBonus = 1.0f;
                if (hasAgentFoundGoal && isAgentFood == false)
                {
                    moveBonus = 1.55f;
                }

                rigidbody.AddForce(transform.forward * stepSpeed * moveBonus, ForceMode.Impulse);
            }
        }



    }

    private bool CheckAheadForOffscreen()
    {
        bool offscreenAhead = false;

        Vector3 directionVec = rigidbody.velocity;
        // check the direction the agent is facing rather than the direction its moving
        // if its come to a stop near the edge
        if (rigidbody.velocity.magnitude < 0.1f)
        {
            directionVec = transform.forward;
        }

        Vector3 viewPortPoint = Camera.main.WorldToViewportPoint(
            transform.position + directionVec * Camera.main.farClipPlane * 0.1f
        );
        if (viewPortPoint.x < 0.001f || viewPortPoint.x > 1.001f ||
            viewPortPoint.y < 0.01f || viewPortPoint.y > 1.001f ||
            viewPortPoint.z < Camera.main.nearClipPlane || viewPortPoint.z > Camera.main.farClipPlane
           )
        {
            offscreenAhead = true;
        }
        return offscreenAhead;
    }

    private void Awake()
    {
        InitAgent();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        StepAgent();
        //if (Time.frameCount % 12 == 0)
        //{
        //    StepAgent();
        //}
    }

}
