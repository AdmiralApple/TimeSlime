using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Collections;

public class SlimeSpringGenerator : MonoBehaviour
{
    public List<Transform> SlimePoints = new List<Transform>();
    public List<Rigidbody2D> SlimeRigidbodies = new List<Rigidbody2D>();
    List<SpringJoint2D> CircumferenceSpringJoints = new List<SpringJoint2D>();
    List<float> CircumferenceOriginalFrequencies = new List<float>();
    List<SpringJoint2D> InteriorSpringJoints = new List<SpringJoint2D>();

    [SerializeField]
    private PhysicsMaterial2D slimePhysicsMaterial;

    [SerializeField]
    private float slimeLinearDrag = 2f;

    [SerializeField]
    private float wallStickiness = 12f;

    [SerializeField]
    private float circumferenceDampingRatio = 0.1f;

    [SerializeField]
    private float circumferenceFrequencyMultiplier = 1f;

    [SerializeField]
    private float interiorDampingRatio = 0.05f;

    public float CircumferenceFrequencency = 4f;
    public float InteriorFrequency = 2f;
    readonly ContactPoint2D[] contactBuffer = new ContactPoint2D[8];

    //dampness slider
    [ShowInInspector, PropertyRange(0, 1)]
    public float CircumferenceDampingRatio {
        get { return circumferenceDampingRatio; }
        set
        {
            circumferenceDampingRatio = value;
            // Update all existing spring joints with the new damping ratio
            foreach (var point in SlimePoints)
            {
                var springs = point.GetComponents<SpringJoint2D>();
                foreach (var spring in springs)
                {
                    spring.dampingRatio = circumferenceDampingRatio;
                }
            }
        }
    }

    [ShowInInspector, PropertyRange(0.0001, 1)]
    public float CircumferenceFrequencyMultiplier
    {
        get { return circumferenceFrequencyMultiplier; }
        set
        {
            circumferenceFrequencyMultiplier = value;
            SetFrequencyMultiplier();


        }
    }
    void SetFrequencyMultiplier()
    {
        for (var i = 0; i < CircumferenceSpringJoints.Count; i++)
        {
            var spring = CircumferenceSpringJoints[i];
            var originalFrequency = CircumferenceOriginalFrequencies[i];
            spring.frequency = originalFrequency * circumferenceFrequencyMultiplier;
        }
    }

    [ShowInInspector, PropertyRange(0, 1)]
    public float InteriorDampingRatio
    {
        get { return interiorDampingRatio; }
        set
        {
            interiorDampingRatio = value;
            // Update all existing spring joints with the new damping ratio
            foreach (var point in SlimePoints)
            {
                var springs = point.GetComponents<SpringJoint2D>();
                foreach (var spring in springs)
                {
                    spring.dampingRatio = interiorDampingRatio;
                }
            }
        }
    }

    void ConfigurePointPhysics()
    {
        SlimeRigidbodies.Clear();
        foreach (var point in SlimePoints)
        {
            var rb = point.GetComponent<Rigidbody2D>();
            if (rb == null) rb = point.gameObject.AddComponent<Rigidbody2D>();
            rb.linearDamping = slimeLinearDrag;
            SlimeRigidbodies.Add(rb);

            var collider = point.GetComponent<Collider2D>();
            if (collider == null) collider = point.gameObject.AddComponent<CircleCollider2D>();
            if (slimePhysicsMaterial != null) collider.sharedMaterial = slimePhysicsMaterial;
        }
    }

    void FixedUpdate()
    {
        ApplyContactFriction();
    }

    private void Start()
    {
        GenerateDynamicSlimePhysics();
        SetFrequencyMultiplier();
        StartCoroutine(DisableAutoDistance());
    }
    //courroutine

    IEnumerator DisableAutoDistance()
    {
        yield return new WaitForSeconds(0.1f); // Wait a short time to ensure all joints are initialized

        DisableAutoDistanceConfiguration();
    }


    [Button]
    public void GenerateStaticSlimePhysics()
    {
        SlimePoints.Clear();
        SlimeRigidbodies.Clear();
        CircumferenceSpringJoints.Clear();
        CircumferenceOriginalFrequencies.Clear();
        InteriorSpringJoints.Clear();
        foreach (Transform child in transform)
        {
            SlimePoints.Add(child);
        }

        // Place each point equidistant away around this object (2D: x/y only)
        for (int i = 0; i < SlimePoints.Count; i++)
        {
            float angle = i * Mathf.PI * 2 / SlimePoints.Count;
            Vector3 newPos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 2; // z = 0 for 2D
            SlimePoints[i].localPosition = newPos;
        }

        ConfigurePointPhysics();

        // Create two small spring joints for each circumference point connecting them to each other (2D)
        for (int i = 0; i < SlimePoints.Count; i++)
        {
            SpringJoint2D spring1 = SlimePoints[i].gameObject.AddComponent<SpringJoint2D>();
            spring1.connectedBody = SlimePoints[(i - 1 + SlimePoints.Count) % SlimePoints.Count].GetComponent<Rigidbody2D>();
            spring1.frequency = CircumferenceFrequencency;
            spring1.dampingRatio = circumferenceDampingRatio;

            SpringJoint2D spring2 = SlimePoints[i].gameObject.AddComponent<SpringJoint2D>();
            spring2.connectedBody = SlimePoints[(i + 1) % SlimePoints.Count].GetComponent<Rigidbody2D>();
            spring2.frequency = CircumferenceFrequencency;
            spring2.dampingRatio = circumferenceDampingRatio;
        }

        // Create larger spring joints for each circumference point connecting them to the other points that are not directly connected to them (2D)
        for (int i = 0; i < SlimePoints.Count; i++)
        {
            for (int j = 0; j < SlimePoints.Count; j++)
            {
                if (j != i && j != (i - 1 + SlimePoints.Count) % SlimePoints.Count && j != (i + 1) % SlimePoints.Count)
                {
                    SpringJoint2D spring = SlimePoints[i].gameObject.AddComponent<SpringJoint2D>();
                    spring.connectedBody = SlimePoints[j].GetComponent<Rigidbody2D>();
                    spring.frequency = InteriorFrequency;
                    spring.dampingRatio = interiorDampingRatio;
                }
            }
        }
    }


    [Button]
    public void GenerateDynamicSlimePhysics()
    {
        SlimePoints.Clear();
        SlimeRigidbodies.Clear();
        CircumferenceSpringJoints.Clear();
        CircumferenceOriginalFrequencies.Clear();
        InteriorSpringJoints.Clear();
        foreach (Transform child in transform)
        {
            SlimePoints.Add(child);
        }

        // Place each point equidistant away around this object (2D: x/y only)
        for (int i = 0; i < SlimePoints.Count; i++)
        {
            float angle = i * Mathf.PI * 2 / SlimePoints.Count;
            Vector3 newPos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * 2; // z = 0 for 2D
            SlimePoints[i].localPosition = newPos;
        }

        ConfigurePointPhysics();

        // Create a spring joing between each point with a frequency based on their current distanance from each other (2D)
        for (int i = 0; i < SlimePoints.Count; i++)
        {
            for (int j = 0; j < SlimePoints.Count; j++)
            {
                if (j != i)
                {
                    SpringJoint2D spring = SlimePoints[i].gameObject.AddComponent<SpringJoint2D>();
                    spring.connectedBody = SlimePoints[j].GetComponent<Rigidbody2D>();
                    float distance = Vector3.Distance(SlimePoints[i].position, SlimePoints[j].position);
                    //spring.frequency = Mathf.Max(0.1f, 10f / distance); // Inverse relationship with distance
                    spring.frequency = distance; // Inverse relationship with distance
                    spring.dampingRatio = interiorDampingRatio;
                    spring.enableCollision = true;


                    //spring.autoConfigureConnectedAnchor = true;
                    //spring.autoConfigureDistance = false;
                    //spring.anchor = SlimePoints[i].position;
                    //spring.connectedAnchor = SlimePoints[j].position;

                    CircumferenceSpringJoints.Add(spring);
                    CircumferenceOriginalFrequencies.Add(spring.frequency);
                }
            }
        }

    }

    [Button]
    void DisableAutoDistanceConfiguration()
    {
        foreach (var spring in CircumferenceSpringJoints)
        {
            spring.autoConfigureDistance = false;
            //spring.distance = Vector3.Distance(spring.transform.position, spring.connectedBody.transform.position);
        }
    }

    void ApplyContactFriction()
    {
        if (wallStickiness <= 0f) return;

        foreach (var rb in SlimeRigidbodies)
        {
            if (rb == null) continue;

            var contactCount = rb.GetContacts(contactBuffer);
            if (contactCount == 0) continue;

            for (int i = 0; i < contactCount; i++)
            {
                var normal = contactBuffer[i].normal;
                var tangent = new Vector2(-normal.y, normal.x);

                var tangentVelocity = Vector2.Dot(rb.linearVelocity, tangent);
                var dampedTangent = Mathf.Lerp(tangentVelocity, 0f, wallStickiness * Time.fixedDeltaTime);
                rb.linearVelocity += tangent * (dampedTangent - tangentVelocity);
            }
        }
    }
    [Button]
    void RemovePointParent()
    {
        for(int i = transform.childCount - 1; i >= 0; i--)
        {
            transform.GetChild(i).parent = null;
        }
    }



}
