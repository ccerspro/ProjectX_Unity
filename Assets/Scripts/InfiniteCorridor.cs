using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Rendering;

namespace ZhouSoftware{
    public class InfiniteCorridor : MonoBehaviour
    {
        List<GameObject> corridorList = new List<GameObject>();
        [SerializeField] private float corridorLength = 60;
        [SerializeField] private float corridorWidth = 32;
        [SerializeField] GameObject normalPrefab;
        [SerializeField] List<GameObject> anomalyPrefab;
        [SerializeField] GameObject boundaryPrefab;
        [SerializeField] List<GameObject> numberSign;
        private GameObject Front;
        private GameObject Rear;
        private GameObject SpecialRear;
        private GameObject RearEntrance;
        private GameObject Uturn;
        private Collider EntranceHolder;
        private int level;
        //buffer holder for direction
        private int direction = 1;
        private GameObject classSign;
        private GameObject NormalRear;
        private Vector3 entransLocation;
        private GameObject ReEnter;
         void Start()
        {
            Debug.unityLogger.logEnabled = true;
            //initialize 3 sections
            for (int i = 0; i < 3; i++){
                GameObject currentCorridor = Instantiate(normalPrefab, new Vector3(-corridorWidth * i, 0, corridorLength * i), Quaternion.Euler(0, 90 - direction * 90, 0));
               
                corridorList.Add(currentCorridor);
            }
            //create Front and Rear act as trigger collider. And create classSign to indicate level. Assign them to current section
            Vector3 current = corridorList[1].transform.position;
            Front = Instantiate(boundaryPrefab, new Vector3(current.x - 14 * direction, current.y, current.z + (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x + 14 * direction, current.y, current.z - (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            classSign = Instantiate(numberSign[0], new Vector3(current.x + 3.87f, current.y + 3, current.z), Quaternion.Euler(90, -90, 0));
            Front.tag = "Front";
            Rear.tag = "Rear";
            Front.transform.SetParent(corridorList[1].transform);
            Rear.transform.SetParent(corridorList[1].transform);
            classSign.transform.SetParent(corridorList[1].transform);
            level = 0; //start at level 0
        }

        //player trigger Front boundary. Generate 1 new section and destroy 1 current section. Shift the corridorList
        public void OnFrontEnter() {
            Debug.Log("OnFrontEnter");
            //check for the tag of current section. When normal increment the level
            if (corridorList[1].CompareTag("NormalSection")){
                if (level < 8){
                    level++;
                }
            }
            //when player go forward in anomalySection. Set the level back to 0
            else if (corridorList[1].CompareTag("AnomalySection")){
                level = 0;
            } 

            //Destory corridors in the end of oppsite direction. And destroy the old boundary and class sign
            Destroy(corridorList[0]);
            Destroy(Front);
            Destroy(Rear);
            Destroy(classSign);
            corridorList[0] = corridorList[1];
            corridorList[1] = corridorList[2];

            //if the current section is level 8 and normal section. Then delete the door so player can exit the game
            if(level == 8 && corridorList[1].CompareTag("NormalSection")){
                DeleteDoor(corridorList[1], "Door");
            }
            //shift the corridors in the scene
            //generate a new corridor in the front
            Vector3 current = corridorList[1].transform.position;
            GameObject currentCorridor;
            //50% chance to instantiate an anomalyPrefab
            if (UnityEngine.Random.value > 0.5f){
                currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            } else{
                int anomalyIndex = UnityEngine.Random.Range(0, anomalyPrefab.Count);
                currentCorridor = Instantiate(anomalyPrefab[anomalyIndex], new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            }
            
            //add the new corridor to the list
            corridorList[2] = currentCorridor;
            //current = corridorList[1].transform.position;

            Front = Instantiate(boundaryPrefab, new Vector3(current.x - 14 * direction, current.y, current.z + (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x + 14 * direction, current.y, current.z - (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            //classSign = Instantiate(numberSign[level], new Vector3(current.x + 3.87f * direction, current.y + 3, current.z), Quaternion.Euler(90, 0 - 90 * direction, 0));
            Front.tag = "Front";
            Rear.tag = "Rear";
            Front.transform.SetParent(corridorList[1].transform);
            Rear.transform.SetParent(corridorList[1].transform);
            //classSign.transform.SetParent(corridorList[1].transform);
        }

        public void OnRearEnter() {
            Debug.Log("OnRearEnter");
            //Destrou the old clasSign and Front&Rear boundary
            Destroy(classSign);
            Destroy(Rear);
            Destroy(Front);
            //Destroy the further away corridor in front and shift the list
            Destroy(corridorList[2]);
            corridorList[2] = corridorList[1];
            corridorList[1] = corridorList[0];
            level = 0;

            if (EntranceHolder != null){
                EntranceHolder.enabled = true;
            }

            //create new corridor append to rear and add to list
            Vector3 current = corridorList[1].transform.position;
            GameObject currentCorridor;
            currentCorridor = Instantiate(normalPrefab, new Vector3(current.x + corridorWidth * direction, 0, current.z - corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            corridorList[0] = currentCorridor;

            Front = Instantiate(boundaryPrefab, new Vector3(current.x - 14 * direction, current.y, current.z + (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            Front.tag = "Front";
            Front.transform.SetParent(corridorList[1].transform);
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x + 14 * direction, current.y, current.z - (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            Rear.tag = "Rear";
            Rear.transform.SetParent(corridorList[1].transform);

            classSign = Instantiate(numberSign[level], new Vector3(current.x + 3.87f * direction, current.y + 3, current.z), Quaternion.Euler(90, 0 - 90 * direction, 0));
            classSign.transform.SetParent(corridorList[1].transform);
        }

        public void OnEntranceEnter(Collider Entrance){
            Debug.Log($"OnEntrance enter level {level}");
            Vector3 current = corridorList[1].transform.position;

            //replacing the old 2 corridors
            
            //Destroy(Entrance);

            Entrance.enabled = false;
            EntranceHolder = Entrance;
            entransLocation = Entrance.gameObject.transform.position;
            
            //Destroy(corridorList[0]);
            if (Entrance.transform.parent.CompareTag("AnomalySection")){
                SpecialRear = Instantiate(boundaryPrefab, new Vector3(entransLocation.x + 4 * direction, entransLocation.y, entransLocation.z), Quaternion.Euler(90,90,0));
                //SpecialRear = Instantiate(boundaryPrefab, new Vector3(current.x + 6 * direction, current.y, current.z - 16 * direction), Quaternion.Euler(90, 90, 0));
                SpecialRear.tag = "SpecialRear";
                SpecialRear.transform.SetParent(corridorList[1].transform);
                Destroy(Rear);
            } else {
                NormalRear = Instantiate(boundaryPrefab, new Vector3(entransLocation.x + 4 * direction, entransLocation.y, entransLocation.z), Quaternion.Euler(90,90,0));
                NormalRear.tag = "NormalRear";
                NormalRear.transform.SetParent(corridorList[1].transform);
            }
            if (classSign == null){
                classSign = Instantiate(numberSign[level], new Vector3(current.x + 3.87f * direction, current.y + 3, current.z), Quaternion.Euler(90, 0 - 90 * direction, 0));
                classSign.transform.SetParent(corridorList[1].transform);
            }
            
            
        }

        //handle scenario while player pass the special but turn back before RearEntrance
        public void OnReEnter(Collider c){
            Debug.Log("OnReEnter");
            Destroy(ReEnter);

            //corridorList[1].tag = "NormalSection";
            Vector3 current = corridorList[1].transform.position;
            Destroy(RearEntrance);
            level = 0;
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x + 14 * direction, current.y, current.z - (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            Rear.tag = "Rear";
            Rear.transform.SetParent(corridorList[1].transform);
            classSign = Instantiate(numberSign[level], new Vector3(current.x + 3.87f * direction, current.y + 3, current.z), Quaternion.Euler(90, 0 - 90 * direction, 0));
            classSign.transform.SetParent(corridorList[1].transform);

            Destroy(corridorList[0]);
            corridorList[0] = Instantiate(normalPrefab, new Vector3(current.x + corridorWidth * direction, 0, current.z - corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));

        }

        //call when player turn back on anomalySection
        public void OnSpecialRearEnter()
        {
            Debug.Log("OnspecialRear");

            ReEnter = Instantiate(boundaryPrefab, new Vector3(entransLocation.x - 1 * direction, entransLocation.y, entransLocation.z), Quaternion.Euler(90,90,0));
            ReEnter.tag = "ReEnter";
            ReEnter.transform.SetParent(corridorList[1].transform);           
            //reverse the direction
            direction *= -1;
           //Destory corridors in the end of oppsite direction. And destroy the old boundary
            Destroy(Front);
            Destroy(SpecialRear);
            Destroy(classSign);
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
            if(level >= 7){
                level = 8;
                currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
                //DeleteDoor(currentCorridor, "Door");
                corridorList[0] = currentCorridor;
            }
            else if (level < 7){
                level++;
                if (UnityEngine.Random.value > 0.5f){
                currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            } else{
                int anomalyIndex = UnityEngine.Random.Range(0, anomalyPrefab.Count);
                currentCorridor = Instantiate(anomalyPrefab[anomalyIndex], new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            }
            corridorList[0] = currentCorridor;
            }

            //generate another corridor beyond current one.
            current = corridorList[0].transform.position;
            Front = Instantiate(boundaryPrefab, new Vector3(current.x - 14 * direction, current.y, current.z + (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            Front.tag = "Front";
            Front.transform.SetParent(corridorList[0].transform);
            RearEntrance = Instantiate(boundaryPrefab, new Vector3(current.x + 14 * direction , current.y, current.z - (corridorLength / 2) * direction), Quaternion.Euler(90, 0, 0));
            RearEntrance.tag = "RearEntrance";
            RearEntrance.transform.SetParent(corridorList[1].transform);
            //classSign = Instantiate(numberSign[level], new Vector3(current.x + 3.87f * direction, current.y + 3, current.z), Quaternion.Euler(90, 0 - 90 * direction, 0));
            //classSign.transform.SetParent(corridorList[2].transform);

            
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
            corridorList [0] = temp;
            Vector3 current = corridorList[1].transform.position;
            Rear = Instantiate(boundaryPrefab, new Vector3(current.x + 14 * direction, current.y, current.z - (corridorLength / 2 + 1) * direction), Quaternion.Euler(90, 0, 0));
            Rear.tag = "Rear";
            Rear.transform.SetParent(corridorList[1].transform);

            //generate new front most corridor
            GameObject currentCorridor;
            if(level == 8){
                currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
                corridorList[2] = currentCorridor;
                DeleteDoor(corridorList[1], "Door");
            }

            else if (level <= 7){
                if (UnityEngine.Random.value > 0.5f){
                currentCorridor = Instantiate(normalPrefab, new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            } else{
                int anomalyIndex = UnityEngine.Random.Range(0, anomalyPrefab.Count);
                currentCorridor = Instantiate(anomalyPrefab[anomalyIndex], new Vector3(current.x - corridorWidth * direction, 0, current.z + corridorLength * direction), Quaternion.Euler(0, 90 - direction * 90, 0));
            }
            corridorList[2] = currentCorridor;
            }

            //classSign = Instantiate(numberSign[level], new Vector3(current.x + 3.87f * direction, current.y + 3, current.z), Quaternion.Euler(90, 0 - 90 * direction, 0));
            //classSign.transform.SetParent(corridorList[2].transform);


        }

        public void EndGame(){
            Debug.Log("Exiting the game...");
            #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
            #else
                    Application.Quit(); // Quit the application
            #endif
        }

        public void OnUturn(){

        }

         void DeleteDoor(GameObject parentObject, string tag)
        {
            // Check if the parent object exists
            if (parentObject == null)
            {
                Debug.LogError("Parent object is null!");
                return;
            }

            // Find the child object with the specified tag
            GameObject child = FindChildWithTag(parentObject.transform, tag);
            Destroy(child);

            if (child != null)
            {
                
                Destroy(child);
            }
            else
            {
                Debug.LogError($"No child with tag '{tag}' found in the prefab.");
            }
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
