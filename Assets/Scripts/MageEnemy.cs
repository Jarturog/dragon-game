using UnityEngine;

public class MageEnemy : Enemy
{
    new void Start() {
        base.Start();
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = Color.blue;
    }
}
