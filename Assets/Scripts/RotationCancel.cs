using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotationCancel : MonoBehaviour
{
    /*
        Cancels the rotation made by the parent
    */

    // Update is called once per frame
    void Update()
    {
        this.transform.rotation = Quaternion.identity;
    }
}
