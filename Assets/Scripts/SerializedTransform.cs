using System;
using UnityEngine;

[Serializable]
public class SerializedTransform
{
    [Tooltip("The position of the transform")]
    public float[] position = new float[3];
    [Tooltip("The rotation of the transform")]
    public float[] rotation = new float[4];
    [Tooltip("The scale of the transform")]
    public float[] scale = new float[3];

    /// <summary>
    /// Create a new serialized transform from a transform
    /// </summary>
    /// <param name="transform">The transform to be serialised</param>
    public SerializedTransform(Transform transform)
    {
        position[0] = transform.position.x;
        position[1] = transform.position.y;
        position[2] = transform.position.z;

        rotation[0] = transform.rotation.w;
        rotation[1] = transform.rotation.x;
        rotation[2] = transform.rotation.y;
        rotation[3] = transform.rotation.z;

        scale[0] = transform.localScale.x;
        scale[1] = transform.localScale.y;
        scale[2] = transform.localScale.z;

    }
}
