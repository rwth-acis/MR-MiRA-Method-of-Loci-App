using System;
using UnityEngine;

[Serializable]
public class SerializedTransform
{
    [Tooltip("The position of the transform")]
    public float[] _position = new float[3];
    [Tooltip("The rotation of the transform")]
    public float[] _rotation = new float[4];
    [Tooltip("The scale of the transform")]
    public float[] _scale = new float[3];

    /// <summary>
    /// Create a new serialized transform from a transform
    /// </summary>
    /// <param name="transform">The transform to be serialised</param>
    public SerializedTransform(Transform transform)
    {
        _position[0] = transform.position.x;
        _position[1] = transform.position.y;
        _position[2] = transform.position.z;

        _rotation[0] = transform.rotation.w;
        _rotation[1] = transform.rotation.x;
        _rotation[2] = transform.rotation.y;
        _rotation[3] = transform.rotation.z;

        _scale[0] = transform.localScale.x;
        _scale[1] = transform.localScale.y;
        _scale[2] = transform.localScale.z;

    }
}
