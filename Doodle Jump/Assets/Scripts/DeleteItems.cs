using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeleteItems : MonoBehaviour
{
    private Collider delete;

    void Start()
    {
        delete = GetComponent<Collider>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag != "Player")
        {
            Destroy(other.gameObject);
        }
    }

}
