using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [SerializeField] private GameObject target = null;
    
    // Update is called once per frame
    void Update()
    {
        if (target == null) 
        {
            target = GameObject.FindWithTag("Player");
        }

        this.transform.position = new Vector3(target.transform.position.x, target.transform.position.y, this.transform.position.z);
    }
}
