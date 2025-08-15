using UnityEngine;
using UnityEngine.SceneManagement;
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
            else if (other.CompareTag("End")){
                endlessCorridor.EndGame();
            }
            else if (other.CompareTag("RearEntrance")){
                endlessCorridor.OnRearEntrance();
            }
            else if (other.CompareTag("SpecialRear")){
                endlessCorridor.OnSpecialRearEnter();
            }
            else if (other.CompareTag("ReEnter")){
                endlessCorridor.OnReEnter(other);
            }
            else if (other.CompareTag("Enemy") || other.CompareTag("DeathZone")){
                //reload the scene
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);      
            }
          
        }
}
