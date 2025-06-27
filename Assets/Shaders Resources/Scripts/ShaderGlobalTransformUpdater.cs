using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class ShaderGlobalTransformUpdater : MonoBehaviour
{
    [SerializeField]
    string globalPositionReference;
    [SerializeField]
    string globalScaleReference;
    [SerializeField]
    string globalRotationReference;

    private void Update()
    {
        Shader.SetGlobalVector(globalPositionReference, transform.position);
        Shader.SetGlobalVector(globalScaleReference, transform.localScale);
        Shader.SetGlobalVector(globalRotationReference, transform.eulerAngles);
    }
}
