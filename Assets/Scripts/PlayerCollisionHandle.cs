using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using ZhouSoftware;
public class PlayerCollisionHandle : MonoBehaviour
{
    public InfiniteCorridor infiniteCorridor;


    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other){
            if (other.CompareTag("Front")){
                infiniteCorridor.OnFrontEnter();
            }
            else if (other.CompareTag("Rear")){
                infiniteCorridor.OnRearEnter();
            }
            else if (other.CompareTag("Entrance")){
                infiniteCorridor.OnEntranceEnter(other);
            }
            else if (other.CompareTag("Door")){
                infiniteCorridor.EndGame();
            }
            else if (other.CompareTag("RearEntrance")){
                infiniteCorridor.OnRearEntrance();
            }
            else if (other.CompareTag("SpecialRear")){
                infiniteCorridor.OnSpecialRearEnter();
            }
            else if (other.CompareTag("Uturn")){
                infiniteCorridor.OnUturn();
            }
            else if (other.CompareTag("ReEnter")){
                infiniteCorridor.OnReEnter(other);
            }
          
        }
}
