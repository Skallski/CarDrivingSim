using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ValueTesterScript : MonoBehaviour
{
    public float testValue = 0f;

    public static float testValueStatic = 0f;

    private void Update()
    {
        testValueStatic = testValue;
    }
}
