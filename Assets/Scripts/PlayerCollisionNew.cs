using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZhouSoftware;

public class PlayerCollisionNew : MonoBehaviour
{
    public EndlessCorridor endlessCorridor;
   

    // Update is called once per frame
    private void OnTriggerEnter(Collider other){
            if (other.CompareTag("Front")){
                endlessCorridor.OnFrontEnter();
            }
            else if (other.CompareTag("Rear")){
                endlessCorridor.OnRearEnter();
            }
            else if (other.CompareTag("Entrance")){
                endlessCorridor.OnEntranceEnter(other);
            }
            else if (other.CompareTag("Door")){
                endlessCorridor.EndGame();
            }
            else if (other.CompareTag("RearEntrance")){
                endlessCorridor.OnRearEntrance();
            }
            else if (other.CompareTag("SpecialRear")){
                endlessCorridor.OnSpecialRearEnter();
            }
            else if (other.CompareTag("Uturn")){
                endlessCorridor.OnUturn();
            }
            else if (other.CompareTag("ReEnter")){
                endlessCorridor.OnReEnter(other);
            }
          
        }
}
