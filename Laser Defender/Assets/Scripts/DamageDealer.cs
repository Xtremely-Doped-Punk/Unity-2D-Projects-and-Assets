using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageDealer : MonoBehaviour
{
    [SerializeField] int damage = 10;
    [SerializeField] bool spin = false;
    [SerializeField] int SpinIncr = 360;
    public enum DamageType   { Player, Enemy, };
    /* this can be implement also by LAYER-COLLISION-MATRIX
    under Physics2D properties by add seperate layers for - 
    Player, Enemy, Plyr-Proj, Enmy-Proj. In layer collision matrix
    we can easily define which layers reaction to colliders events
    of other layer respectively...    */

    [SerializeField] DamageType DamageWho;
    public int DmgAmt()     { return damage; } // Damage Amount
    public DamageType DmgTyp() { return DamageWho; } // Damage Amount
    public void Hit() 
    {
        if (gameObject.TryGetComponent<Enemy>(out var enemy))
        {
            if (enemy.GetHealth() > 0)
                FindObjectOfType<EnemySpawner>().DecrementEnemyCount();
        }
        Destroy(gameObject);
    }
    // whenever obj like bullet (having class DamageDealer) hits another entity
    // then the bullet is to be destroyed
    void Update()
    {
        if (spin)
            transform.Rotate(0, 0, SpinIncr * Time.deltaTime);
    }
}
