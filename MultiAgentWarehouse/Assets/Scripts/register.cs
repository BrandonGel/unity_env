using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.SideChannels;


public class RegisterStringLogSideChannel : MonoBehaviour
{

    StringLogSideChannel stringChannel;

    public string getIncomingMsg()
    {
        return stringChannel.receivedString;
    }

    public bool checkNewMsg()
    {
        return stringChannel.isNewMsg ;
    }

    public void resetIncomingMsg()
    {
        stringChannel.receivedString = "";
        stringChannel.isNewMsg = false;
    }

    public void readIncomingMsg()
    {
        string msg = getIncomingMsg();
        string[] msg_split = msg.Split(' ');
        string parameter = msg_split[0];
        float[] values = new float[msg_split.Length - 1];
        for (int ii = 1; ii < msg_split.Length; ii++)
        {
            values[ii - 1] = float.Parse(msg_split[ii]);
        }
    }

    public void Awake()
    {
        // We create the Side Channel
        stringChannel = new StringLogSideChannel();

        // When a Debug.Log message is created, we send it to the stringChannel
        Application.logMessageReceived += stringChannel.SendDebugStatementToPython;

        // The channel must be registered with the SideChannelManager class
        SideChannelManager.RegisterSideChannel(stringChannel);
    }

    public void OnDestroy()
    {
        // De-register the Debug.Log callback
        Application.logMessageReceived -= stringChannel.SendDebugStatementToPython;
        if (Academy.IsInitialized){
            SideChannelManager.UnregisterSideChannel(stringChannel);
        }
    }

    public void Update()
    {
        if (!checkNewMsg())
        {
            return;
        }
        readIncomingMsg();
        resetIncomingMsg();
    }
}