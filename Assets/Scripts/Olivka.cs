using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Olivka : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        // убрать чтобы смешно падали
        Destroy(gameObject);
    }
}
