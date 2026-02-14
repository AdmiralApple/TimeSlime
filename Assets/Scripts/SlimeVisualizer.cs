using UnityEngine;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
public class SlimeVisualizer : MonoBehaviour
{
    [SerializeField] SlimeSpringGenerator slimeSpringGenerator;
    SpriteShapeController shape;

    void Awake()
    {
        if (shape == null) shape = GetComponent<SpriteShapeController>();
    }

    void LateUpdate()
    {
        if (slimeSpringGenerator == null || shape == null) return;
        SyncShapePointCount();

        for (var i = 0; i < slimeSpringGenerator.SlimePoints.Count; i++)
        {
            // SpriteShape expects positions in local space of the controller
            var localPos = transform.InverseTransformPoint(slimeSpringGenerator.SlimePoints[i].position);
            shape.spline.SetPosition(i, localPos);
        }
    }

    void SyncShapePointCount()
    {
        var desiredCount = slimeSpringGenerator.SlimePoints.Count;
        if (shape.spline.GetPointCount() == desiredCount) return;

        shape.spline.Clear();
        for (var i = 0; i < desiredCount; i++)
        {
            var localPos = transform.InverseTransformPoint(slimeSpringGenerator.SlimePoints[i].position);
            shape.spline.InsertPointAt(i, localPos);
            shape.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
        }
    }
}
