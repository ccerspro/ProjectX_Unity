using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


namespace ZhouSoftware{
    //This is the new CorridorGenerator script designed to replace the old one（InfiniteCorridor). It is for the Project X
    /*
    Dev Log:
        5/28/2025:
            Start working on a new separate doorWall prefab that will be used in the game.
        6/8/2025:
            Attempt to maintain 3 doorwall objects during runtime. The doorwalls will be separated from the corridorList.
            The doorwall will be instantiated in the Start() method and will be used to represent the door in the game.
            Modularized doorwall completed.
    */
    //
    public class EndlessCorridor : MonoBehaviour
    {
        List<GameObject> corridorList = new List<GameObject>();
        [SerializeField] private float corridorLength = 35;
        [SerializeField] private float corridorWidth = 5;
        [SerializeField] GameObject normalPrefab;
        [SerializeField] List<GameObject> anomalyPrefab;
        //a collider object represent the boundary in the game. Used as trigger. Need to be tagged by this script.
        [SerializeField] GameObject boundaryPrefab;
        //list of sign objects that will be use in game
        [SerializeField] List<GameObject> signList;
        private GameObject Front;
        private GameObject Rear;
        private GameObject SpecialRear;
        private GameObject RearEntrance;
        private GameObject Uturn;// not used as of now
        private Collider EntranceHolder;
        private int level;
        //buffer holder for direction
        private int direction = 1;
        //hold the reference to the current sign object
        private GameObject roomSign;
        private GameObject NormalRear;
        [SerializeField] GameObject doorWallPrefab;
        private List<GameObject> doorWallList = new List<GameObject>(3);
        //Hold the reference to the doorholder object. Will be used as the parent of all the doorwall objects.
        [SerializeField] private GameObject doorHolder;


        private Vector3 entransLocation;
        private GameObject ReEnter;

        //offset variable. Can be adjust accordingly
        [SerializeField] private float FrontOffset = 32;
        [SerializeField] private float RearOffset = 6;
        [SerializeField] private Vector3 doorOffset = new Vector3(-5, 2, 30);
        [SerializeField] private Vector3 signOffset = new Vector3(3.87f, 3, 0);
        [SerializeField] private Vector3 reverseOffset = new Vector3(6.38f, 0, -8.21f);


        void Start()
        {
            Debug.unityLogger.logEnabled = true;
            //initialize 3 sections
            for (int i = 0; i < 3; i++)
            {
                GameObject currentCorridor = Instantiate(normalPrefab, new Vector3(-corridorWidth * i, 0, corridorLength * i), Quaternion.Euler(0, 90 - direction * 90, 0));
                corridorList.Add(currentCorridor);
                GameObject currentDoorWall = Instantiate(doorWallPrefab, new Vector3(-corridorWidth * i - doorOffset.x * direction, doorOffset.y, corridorLength * i - doorOffset.z * direction), Quaternion.Euler(0, 90 - 90 * direction, 0));
                doorWallList.Add(currentDoorWall);
            }
            //create Front and Rear act as trigger collider. And create roomSign to indicate level. Assign them to current section
            Vector3 current = corridorList[1].transform.position;
            Front = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z + FrontOffset * direction), Quaternion.Euler(90, 0, 0));
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z - RearOffset * direction), Quaternion.Euler(90, 0, 0));
            Front.tag = "Front";
            Rear.tag = "Rear";
            Front.transform.SetParent(corridorList[1].transform);
            Rear.transform.SetParent(corridorList[1].transform);
            level = 0; //start at level 0
        }

        void Update()
        {
            //Debug.Log($"Current level: {level}");
            //Debug.Log($"Current direction: {direction}");

            foreach(GameObject doorWall in doorWallList)
            {
                doorWall.transform.SetParent(doorHolder.transform);
            }
        }



        //player trigger Front boundary. Generate 1 new section and destroy 1 current section. Shift the corridorList
        public void OnFrontEnter()
        {
            Debug.Log("OnFrontEnter");
            //check for the tag of current section. When normal increment the level
            if (corridorList[1].CompareTag("NormalSection"))
            {
                if (level < 8)
                {
                    level++;
                }
            }
            //when player go forward in anomalySection. Set the level back to 0
            else if (corridorList[1].CompareTag("AnomalySection"))
            {
                level = 0;
            }

            //Destory corridors in the end of oppsite direction. And destroy the old boundary and class sign
            Destroy(corridorList[0]);
            Destroy(Front);
            Destroy(Rear);
            Destroy(roomSign);
            corridorList[0] = corridorList[1];
            corridorList[1] = corridorList[2];

            
            //shift the corridors in the scene
            //generate a new corridor in the front
            Vector3 current = corridorList[1].transform.position;
            GameObject currentCorridor;
            //50% chance to instantiate an anomalyPrefab
            if (UnityEngine.Random.value > 0.5f)
            {
                currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            }
            else
            {
                int anomalyIndex = UnityEngine.Random.Range(0, anomalyPrefab.Count);
                currentCorridor = Instantiate(anomalyPrefab[anomalyIndex], new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            }

            //add the new corridor to the list
            corridorList[2] = currentCorridor;
            //current = corridorList[1].transform.position;

            Front = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z + FrontOffset * direction), Quaternion.Euler(90, 0, 0));
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z - RearOffset * direction), Quaternion.Euler(90, 0, 0));
            Front.tag = "Front";
            Rear.tag = "Rear";
            Front.transform.SetParent(corridorList[1].transform);
            Rear.transform.SetParent(corridorList[1].transform);

            //DoorwallList management
            Destroy(doorWallList[0]);
            doorWallList[0] = doorWallList[1];
            doorWallList[1] = doorWallList[2];
            GameObject currentDoorWall = Instantiate(doorWallPrefab, new Vector3(current.x - corridorWidth * direction - doorOffset.x * direction, doorOffset.y, current.z + corridorLength * direction - doorOffset.z * direction), Quaternion.Euler(0, 90 - 90 * direction, 0));
            doorWallList[2] = currentDoorWall;
        }

        public void OnRearEnter()
        {
            Debug.Log("OnRearEnter");
            //Destrou the old clasSign and Front&Rear boundary
            Destroy(roomSign);
            Destroy(Rear);
            Destroy(Front);
            //Destroy the further away corridor in front and shift the list
            Destroy(corridorList[2]);
            corridorList[2] = corridorList[1];
            corridorList[1] = corridorList[0];
            level = 0;

            if (EntranceHolder != null)
            {
                EntranceHolder.enabled = true;
            }

            //create new corridor append to rear and add to list
            Vector3 current = corridorList[1].transform.position;
            GameObject currentCorridor;
            currentCorridor = Instantiate(normalPrefab, new Vector3(current.x + corridorWidth * direction, 0, current.z - corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            corridorList[0] = currentCorridor;

            Front = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z + FrontOffset * direction), Quaternion.Euler(90, 0, 0));
            Front.tag = "Front";
            Front.transform.SetParent(corridorList[1].transform);
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z - RearOffset * direction), Quaternion.Euler(90, 0, 0));
            Rear.tag = "Rear";
            Rear.transform.SetParent(corridorList[1].transform);

            //roomSign = Instantiate(signList[level], new Vector3(current.x + 3.87f * direction, current.y + 3, current.z), Quaternion.Euler(90, 0 - 90 * direction, 0));
            roomSign = Instantiate(signList[level], current + signOffset, Quaternion.Euler(0, 0 + 90 * direction, 0));
            roomSign.transform.SetParent(corridorList[1].transform);

            //DoorWallList management
            Destroy(doorWallList[2]);
            doorWallList[2] = doorWallList[1];
            doorWallList[1] = doorWallList[0];
            GameObject currentDoorWall = Instantiate(doorWallPrefab, new Vector3(current.x + corridorWidth * direction - doorOffset.x * direction, doorOffset.y, current.z - corridorLength * direction - doorOffset.z * direction), Quaternion.Euler(0, 90 - 90 * direction, 0));
            doorWallList[0] = currentDoorWall;
        }

        public void OnEntranceEnter(Collider Entrance)
        {
            Debug.Log($"OnEntrance enter level {level}");
            Vector3 current = corridorList[1].transform.position;

            //replacing the old 2 corridors

            //Destroy(Entrance);

            Entrance.enabled = false;
            EntranceHolder = Entrance;
            entransLocation = Entrance.gameObject.transform.position;

            //Destroy(corridorList[0]);
            if (Entrance.transform.parent.CompareTag("AnomalySection"))
            {
                SpecialRear = Instantiate(boundaryPrefab, new Vector3(entransLocation.x, entransLocation.y, entransLocation.z - 4 * direction), Quaternion.Euler(90, 0, 0));
                //SpecialRear = Instantiate(boundaryPrefab, new Vector3(current.x + 6 * direction, current.y, current.z - 16 * direction), Quaternion.Euler(90, 90, 0));
                SpecialRear.tag = "SpecialRear";
                SpecialRear.transform.SetParent(corridorList[1].transform);
                Destroy(Rear);
            }
            else
            {
                NormalRear = Instantiate(boundaryPrefab, new Vector3(entransLocation.x, entransLocation.y, entransLocation.z - 4 * direction), Quaternion.Euler(90, 0, 0));
                NormalRear.tag = "NormalRear";
                NormalRear.transform.SetParent(corridorList[1].transform);
            }
            if (roomSign == null)
            {
                Vector3 signPos = new Vector3(current.x + signOffset.x * direction, current.y + signOffset.y, current.z + signOffset.z * direction);

                roomSign = Instantiate(signList[level], signPos, Quaternion.Euler(0, 0 + 90 * direction, 0));
                roomSign.transform.SetParent(corridorList[1].transform);
            }
        }


        //handle scenario while player pass the special but turn back before RearEntrance
        public void OnReEnter(Collider c)
        {
            Debug.Log("OnReEnter");
            Destroy(ReEnter);

            //corridorList[1].tag = "NormalSection";
            Vector3 current = corridorList[1].transform.position;
            Destroy(RearEntrance);
            level = 0;
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z - RearOffset * direction), Quaternion.Euler(90, 0, 0));
            Rear.tag = "Rear";
            Front = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z + FrontOffset * direction), Quaternion.Euler(90, 0, 0));
            Front.tag = "Front";
            Rear.transform.SetParent(corridorList[1].transform);
            roomSign = Instantiate(signList[level], current + signOffset, Quaternion.Euler(0, 0 + 90 * direction, 0));
            roomSign.transform.SetParent(corridorList[1].transform);

            Destroy(corridorList[0]);
            corridorList[0] = Instantiate(normalPrefab, new Vector3(current.x + corridorWidth * direction, 0, current.z - corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));

        }

        //call when player turn back on anomalySection
        public void OnSpecialRearEnter()
        {
            Debug.Log("OnspecialRear");

            ReEnter = Instantiate(boundaryPrefab, new Vector3(entransLocation.x - 1 * direction, entransLocation.y, entransLocation.z), Quaternion.Euler(90, 0, 0));
            ReEnter.tag = "ReEnter";
            ReEnter.transform.SetParent(corridorList[1].transform);
            //reverse the direction
            direction *= -1;
            //Destory corridors in the end of oppsite direction. And destroy the old boundary
            Destroy(Front);
            Destroy(SpecialRear);
            Destroy(roomSign);
            //create a Uturn collider in case the player decide to go back

            //shift the corridors in the scene
            Destroy(corridorList[0]);
            //corridorList [0] = corridorList[2];
            //generate a new corridor in the rear
            Vector3 current = corridorList[1].transform.position;
            GameObject currentCorridor;


            //Uturn = Instantiate(boundaryPrefab, new Vector3(current.x + 5 * direction * -1, current.y, current.z - 16 * direction * -1), Quaternion.Euler(90, 90, 0));
            //Uturn.transform.SetParent(corridorList[0].transform);

            // Instantiate two new corridor section in reversed direction          
            //if reach the level 8. Delete the door in next section to allow player to enter the room
            Vector3 corridorPos = new Vector3(current.x - reverseOffset.x * direction, current.y, current.z - reverseOffset.z * direction);
            if (level >= 7)
            {
                level = 8;
                currentCorridor = Instantiate(normalPrefab, corridorPos, Quaternion.Euler(0, 90 - direction * 90, 0));
                corridorList[0] = currentCorridor;
            }
            else if (level < 7)
            {
                level++;
                if (UnityEngine.Random.value > 0.5f)
                {
                    currentCorridor = Instantiate(normalPrefab, corridorPos, Quaternion.Euler(0, 90 - direction * 90, 0));
                }
                else
                {
                    int anomalyIndex = UnityEngine.Random.Range(0, anomalyPrefab.Count);
                    currentCorridor = Instantiate(anomalyPrefab[anomalyIndex], corridorPos, Quaternion.Euler(0, 90 - direction * 90, 0));
                }
                corridorList[0] = currentCorridor;
            }

            //generate another corridor beyond current one.
            current = corridorList[0].transform.position;
            Front = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z + FrontOffset * direction), Quaternion.Euler(90, 0, 0));
            Front.tag = "Front";
            Front.transform.SetParent(corridorList[0].transform);
            RearEntrance = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z - 1 * direction), Quaternion.Euler(90, 0, 0));
            RearEntrance.tag = "RearEntrance";
            RearEntrance.transform.SetParent(corridorList[1].transform);


            direction *= -1;
        }

        //call when player entered a new reversed section. Destroy RearEntrance and instantiate Rear
        public void OnRearEntrance()
        {
            Debug.Log("OnRearEntrance");
            direction *= -1;
            Destroy(RearEntrance);

            //Destroy(corridorList[0]);
            //corridorList[0] = corridorList[1];
            //corridorList[1] = corridorList[2];

            Destroy(corridorList[2]);
            GameObject temp = corridorList[1];
            corridorList[1] = corridorList[0];
            corridorList[0] = temp;
            Vector3 current = corridorList[1].transform.position;
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x, current.y, current.z - RearOffset * direction), Quaternion.Euler(90, 0, 0));
            Rear.tag = "Rear";
            Rear.transform.SetParent(corridorList[1].transform);

            //generate new front most corridor
            GameObject currentCorridor;
            if (level == 8)
            {
                currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
                corridorList[2] = currentCorridor;
            }

            else if (level <= 7)
            {
                if (UnityEngine.Random.value > 0.5f)
                {
                    currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
                }
                else
                {
                    int anomalyIndex = UnityEngine.Random.Range(0, anomalyPrefab.Count);
                    currentCorridor = Instantiate(anomalyPrefab[anomalyIndex], new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
                }
                corridorList[2] = currentCorridor;
            }



        }

        public void EndGame()
        {
            Debug.Log("Exiting the game...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
#else
                    Application.Quit(); // Quit the application
#endif
        }


        GameObject FindChildWithTag(Transform parent, string tag)
        {
            // Loop through all children of the parent
            foreach (Transform child in parent)
            {
                if (child.CompareTag(tag))
                {
                    return child.GetChild(0).gameObject; // Return the child if it has the specified tag
                }
            }
            return null; // Return null if no matching child is found
        }

    }
}