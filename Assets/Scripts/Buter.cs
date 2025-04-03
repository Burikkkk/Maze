using System;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

public class Buter : MonoBehaviour
{
    public static int Amount;
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Olivka"))
        {
            Amount--;
            // убрать и будут смешно падать
            Destroy(gameObject);

            // это вернуть, чтобы оливки смешно падали и убрать скрипт в оливках
            Destroy(other.gameObject);

            var rb = GetComponent<Rigidbody2D>();
            var ai = GetComponent<AIPath>();
            var seeker = GetComponent<Seeker>();
            var aiDestination = GetComponent<AIDestinationSetter>();
            var spriteRenderer = GetComponent<SpriteRenderer>();


            rb.freezeRotation = false;
            Destroy(ai);
            Destroy(aiDestination);
            Destroy(seeker);
            spriteRenderer.color = Color.gray;
            gameObject.layer = LayerMask.NameToLayer("NoCollision");

            Destroy(this);
        }
    }
}
