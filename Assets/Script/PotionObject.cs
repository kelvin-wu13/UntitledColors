using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PotionObject : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        gameObject.layer = LayerMask.NameToLayer("Potion");
    }
}
