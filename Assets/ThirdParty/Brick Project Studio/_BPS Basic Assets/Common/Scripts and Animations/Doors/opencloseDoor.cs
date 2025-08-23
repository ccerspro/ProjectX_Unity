using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZhouSoftware;

namespace SojaExiles

{
	public class opencloseDoor : MonoBehaviour
	{

		public Animator openandclose;
		public bool open;
		public Transform Player;

        void OnEnable()
        {
            if (PlayerLocator.TryGet(out Player) == false)
			{
				PlayerLocator.OnAvailable += HandlePlayerAvailable;
			}
        }
		
		void OnDisable()
		{
			PlayerLocator.OnAvailable -= HandlePlayerAvailable;
		}

		private void HandlePlayerAvailable(Transform t)
		{
			Player = t;
			PlayerLocator.OnAvailable -= HandlePlayerAvailable; // one-shot
		}

        void Start()
		{
			open = false;
		}

		public void handleDoor()
		{
			if (open)
			{
				StartCoroutine(closing());
			}
			else
			{
				StartCoroutine(opening());
			}
		}


		IEnumerator opening()
		{
			print("you are opening the door");
			openandclose.Play("Opening");
			open = true;
			yield return new WaitForSeconds(.5f);
		}

		IEnumerator closing()
		{
			print("you are closing the door");
			openandclose.Play("Closing");
			open = false;
			yield return new WaitForSeconds(.5f);
		}


	}
}