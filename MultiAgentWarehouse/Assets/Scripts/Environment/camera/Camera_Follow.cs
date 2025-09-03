using UnityEngine;


namespace multiagent.camera
{
    public class Camera_Follow : MonoBehaviour
    {
        [Range(0,360)][SerializeField] public float yawAngle = 0; // rotate y axis
        [Range(0,360)][SerializeField] public float pitchAngle = 0; // rotate x axis
        [SerializeField] public float offsetDistance;
        [SerializeField] public int playerIndex = 0;
        Vector3 directionVector;
        Vector3 newpos;
        public GameObject[] players = null;

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
        }

        void Update()
        {
            if (players != null)
            {
                if (GetComponent<Camera>().enabled && Input.GetKeyDown(KeyCode.Tab)) {
                    playerIndex +=1;
                    playerIndex %= players.Length;
                }
                
                float yawOffset = players[playerIndex].transform.localRotation.eulerAngles.y +180f;

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
                yawAngle %= 360;
                pitchAngle %= 360;


                playerIndex = Mathf.Clamp(playerIndex,0,Mathf.Max(0,players.Length-1));
                GameObject player = players[playerIndex];
                Quaternion rotation = Quaternion.Euler(0, (yawOffset+yawAngle)%360f, pitchAngle);
                newpos = rotation*directionVector;

                transform.rotation = Quaternion.LookRotation(-newpos);
                transform.position = player.transform.position + offsetDistance*newpos;
            }
        }
    }
}