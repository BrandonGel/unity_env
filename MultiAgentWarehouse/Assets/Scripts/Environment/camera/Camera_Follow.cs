using System;
using UnityEngine;


namespace multiagent.camera
{
    public class Camera_Follow : MonoBehaviour
    {
        [Range(0,360)][SerializeField] public float yawAngle = 0; // rotate y axis
        [Range(-90,90)][SerializeField] public float pitchAngle = 0; // rotate x axis
        [SerializeField] public float offsetDistance =10;
        [SerializeField] public int playerIndex = 0;
        Vector3 directionVector;
        Vector3 newpos;
        public GameObject[] players = null;
        Vector3 startPosition;

        public void getPlayers(GameObject[] players)
        {
            this.players = players;
        }
        void Start()
        {
            players = null;
            directionVector = Vector3.right;
            directionVector = directionVector/directionVector.magnitude;
            newpos = Vector3.zero;
            startPosition = transform.position;
        }

        void Update()
        {
            if (players != null && players.Length > 0)
            {
                if (GetComponent<Camera>().enabled && Input.GetKeyDown(KeyCode.Tab)) {
                    playerIndex +=1;
                    playerIndex %= players.Length;
                }
                
                // float yawOffset = players[playerIndex].transform.localRotation.eulerAngles.y +180f;
                float yawOffset = 90f;

                if (Input.GetKey(KeyCode.A))
                {
                    yawAngle += -0.5f;
                }
                else if (Input.GetKey(KeyCode.D))
                {
                    yawAngle += 0.5f;
                }
                if(Input.GetKey(KeyCode.W))
                {
                    pitchAngle += 0.5f;
                }
                else if(Input.GetKey(KeyCode.S))
                {
                    pitchAngle += -0.5f;
                }
                float scrollInput = Input.GetAxis("Mouse ScrollWheel");
                if(scrollInput < 0)
                {
                    offsetDistance += 0.5f;
                }
                else if(scrollInput > 0)
                {
                    offsetDistance += -0.5f;
                }
                if (Input.GetKey(KeyCode.R))
                {
                    yawAngle = 0;
                    pitchAngle = 90f;
                    offsetDistance = 10;
                }
                if (Input.GetMouseButton(1))
                {
                    // Get mouse movement deltas
                    float mouseX = Input.GetAxis("Mouse X");
                    float mouseY = Input.GetAxis("Mouse Y");

                    // Apply rotation to the transform
                    // Rotate around the Y-axis for horizontal movement (mouseX)
                    // Rotate around the X-axis for vertical movement (mouseY)
                    transform.Rotate(Vector3.up, mouseX * 0.5f, Space.World); 
                    transform.Rotate(Vector3.right, -mouseY * 0.5f, Space.Self); // Invert mouseY for intuitive up/down rotation
                    yawAngle = transform.rotation.eulerAngles.y;
                    pitchAngle = transform.rotation.eulerAngles.x;
                }
                

                yawAngle %= 360;
                pitchAngle = Mathf.Clamp(pitchAngle,-89.9f,89.9f);
                offsetDistance = Mathf.Clamp(offsetDistance,0f,100f);

                playerIndex = Mathf.Clamp(playerIndex,0,Mathf.Max(0,players.Length-1));
                GameObject player = players[playerIndex];
                Quaternion rotation = Quaternion.Euler(0, (yawOffset+yawAngle)%360f, pitchAngle);
                newpos = rotation*directionVector;
                transform.position = player.transform.position + offsetDistance*newpos;
                transform.rotation = Quaternion.LookRotation(-newpos,Vector3.up);

            }
        }
    }
}