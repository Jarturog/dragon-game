using UnityEngine;

public class MageEnemy : Enemy
{
    public static GameObject gameObjectToInstantiate;

    private void Awake() {
        gameObjectToInstantiate = GameObject.FindWithTag("MageEnemy");
        if (gameObjectToInstantiate == null) {
            Debug.LogError("UEP: mage is null");
        }
        
        GetComponent<Enemy>().enabled = false;
    }
}
