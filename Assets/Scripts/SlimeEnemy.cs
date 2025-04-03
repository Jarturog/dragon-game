using System;
using UnityEngine;

public class SlimeEnemy : Enemy 
{
    public static GameObject gameObjectToInstantiate;

    private void Awake() {
        gameObjectToInstantiate = GameObject.FindWithTag("SlimeEnemy");
        if (gameObjectToInstantiate == null) {
            Debug.LogError("UEP: slime is null");
        }

        GetComponent<Enemy>().enabled = false;
    }
}