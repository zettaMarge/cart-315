using UnityEngine;

public class JumpAttackCollisionDetect : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        transform.parent.GetComponent<AttackManagerC>().OnChildTriggerCollision(true, other);
    }

    private void OnTriggerExit(Collider other)
    {
        transform.parent.GetComponent<AttackManagerC>().OnChildTriggerCollision(false, other);
    }
}
