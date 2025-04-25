using UnityEngine;

public class Boss1Enemy: Enemy
{
    new void Start() {
        base.Start();
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null) renderer.material.color = Color.red;
    }
    protected override float AttackDistance => 0.5f;
    protected override float FleeDistance => 0f;
}
