using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

public class SlimeSpringGenerator : MonoBehaviour
{
    List<Transform> SlimePoints = new List<Transform>();
    List<SpringJoint2D> CircumferenceSpringJoints = new List<SpringJoint2D>();
    List<SpringJoint2D> InteriorSpringJoints = new List<SpringJoint2D>();


    public float CircumferenceFrequencency = 4f;
    public float InteriorFrequency = 2f;

    //dampness slider
    [ShowInInspector, PropertyRange(0, 10)]
    public float CircumferenceDampingRatio {
        get { return CircumferenceDampingRatio; }
        set
        {
            CircumferenceDampingRatio = value;
            // Update all existing spring joints with the new damping ratio
            foreach (var point in SlimePoints)
            {
                var springs = point.GetComponents<SpringJoint2D>();
                foreach (var spring in springs)
                {
                    spring.dampingRatio = CircumferenceDampingRatio;
                }
            }
        }
    }

    [ShowInInspector, PropertyRange(0, 10)]
    public float InteriorDampingRatio
    {
        get { return InteriorDampingRatio; }
        set
        {
            InteriorDampingRatio = value;
            // Update all existing spring joints with the new damping ratio
            foreach (var point in SlimePoints)
            {
                var springs = point.GetComponents<SpringJoint2D>();
                foreach (var spring in springs)
                {
                    spring.dampingRatio = InteriorDampingRatio;
                }
            }
        }
    }


    [Button]
    public void GenerateStaticSlimePhysics()
    {
        SlimePoints.Clear();
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

        // Ensure each point has a Rigidbody2D
        foreach (var point in SlimePoints)
        {
            if (point.GetComponent<Rigidbody2D>() == null)
            {
                point.gameObject.AddComponent<Rigidbody2D>();
            }
        }

        // Create two small spring joints for each circumference point connecting them to each other (2D)
        for (int i = 0; i < SlimePoints.Count; i++)
        {
            var rb = SlimePoints[i].GetComponent<Rigidbody2D>();

            SpringJoint2D spring1 = SlimePoints[i].gameObject.AddComponent<SpringJoint2D>();
            spring1.connectedBody = SlimePoints[(i - 1 + SlimePoints.Count) % SlimePoints.Count].GetComponent<Rigidbody2D>();
            spring1.frequency = CircumferenceFrequencency;
            spring1.dampingRatio = 0.1f;

            SpringJoint2D spring2 = SlimePoints[i].gameObject.AddComponent<SpringJoint2D>();
            spring2.connectedBody = SlimePoints[(i + 1) % SlimePoints.Count].GetComponent<Rigidbody2D>();
            spring2.frequency = CircumferenceFrequencency;
            spring2.dampingRatio = 0.1f;
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
                    spring.dampingRatio = 0.05f;
                }
            }
        }
    }


    [Button]
    public void GenerateDynamicSlimePhysics()
    {
        SlimePoints.Clear();
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

        // Ensure each point has a Rigidbody2D
        foreach (var point in SlimePoints)
        {
            if (point.GetComponent<Rigidbody2D>() == null)
            {
                point.gameObject.AddComponent<Rigidbody2D>();
            }
        }

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
                    spring.dampingRatio = 0.05f;

                    CircumferenceSpringJoints.Add(spring);
                }
            }
        }

    }


}
