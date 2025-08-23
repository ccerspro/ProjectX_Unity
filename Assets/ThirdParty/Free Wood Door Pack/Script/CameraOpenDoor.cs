using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CameraDoorScript
{
public class CameraOpenDoor : MonoBehaviour {
	public float DistanceOpen=3;
	public GameObject text;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		RaycastHit hit;
			if (Physics.Raycast(transform.position, transform.forward, out hit, DistanceOpen))
			{
				if (hit.transform.GetComponent<DoorScript.Door>())
				{
					//text.SetActive (true);
					AimManager.I?.SetFocusedAim();
					if (Input.GetMouseButtonDown(0)) //(Input.GetKeyDown(KeyCode.E))
						hit.transform.GetComponent<DoorScript.Door>().OpenDoor();
				}
				else if (hit.transform.GetComponent<SojaExiles.opencloseDoor>() || hit.transform.GetComponent<SojaExiles.opencloseDoor1>())
				{
					AimManager.I?.SetFocusedAim();
					if (Input.GetMouseButtonDown(0)) //(Input.GetKeyDown(KeyCode.E))
					{
						if (hit.transform.GetComponent<SojaExiles.opencloseDoor>())
							hit.transform.GetComponent<SojaExiles.opencloseDoor>().handleDoor();
						if (hit.transform.GetComponent<SojaExiles.opencloseDoor1>())
							hit.transform.GetComponent<SojaExiles.opencloseDoor1>().handleDoor();
					}
				}
				else
				{
					//text.SetActive (false);
					AimManager.I?.SetDefaultAim();
				}
			}
			else
			{
				//text.SetActive (false);
				AimManager.I?.SetDefaultAim();
		}
	}
}
}
