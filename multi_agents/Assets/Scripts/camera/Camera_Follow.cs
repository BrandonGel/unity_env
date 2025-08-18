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
            directionVector = Vector3.forward;
            directionVector = directionVector/directionVector.magnitude;
            newpos = Vector3.zero;
        }

        void Update()
        {
            if (players != null)
            {
                playerIndex = Mathf.Clamp(playerIndex,0,Mathf.Max(0,players.Length-1));
                GameObject player = players[playerIndex];
                Quaternion rotation = Quaternion.Euler(-pitchAngle, yawAngle, 0);
                newpos = rotation*directionVector;

                transform.rotation = Quaternion.LookRotation(-newpos);
                transform.position = player.transform.position + offsetDistance*newpos;
            }
        }
    }
}