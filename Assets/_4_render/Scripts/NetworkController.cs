using UnityEngine;
using System.Net.Sockets;
using System.Collections;
using System.Diagnostics;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;

public class NetworkController
{

    private string hostIP;
    private int port;
    private Socket socket;

    private int timeout;

    private string result;

    public NetworkController(string hostIP, int port, int timeout = 10)
    {
        this.hostIP = hostIP;
        this.port = port;
        this.timeout = timeout;
    }

    public bool Connect()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        Stopwatch s = new Stopwatch();
        s.Start();

        while (s.Elapsed < TimeSpan.FromSeconds(timeout))
        {
            try
            {
                socket.Connect(this.hostIP, this.port);
            }
            catch (SocketException)
            {

            }

            if (socket.Connected)
            {
                return true;
            }
        }

        return false;
    }

    public bool SendScene(string filePath)
    {
        try
        {
            socket.SendFile(filePath);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public bool ReceiveResult()
    {

        try
        {
            byte[] rB = new byte[99999];
            int rec = socket.Receive(rB);
            byte[] data = new byte[rec];
            Array.Copy(rB, data, rec);
            result = Encoding.ASCII.GetString(data);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }

    }

    public void getOutput(ref Output output)
    {
        //if not found or error
        if (result.IndexOf("<NF>") != -1 || result.IndexOf("<ER>") != -1)
        {
            output = null;
            return;
        }

        int st, ed;
        string[] s;

        //this.result = "<BOX><DR>-0.259000,0.001000,-0.004000,-0.966000</DR><OR>0.255000,0.150000,-0.033000,-0.955000</OR><ICP>0.708150,-0.429461,-0.560458,0.400947,0.897959,-0.181472,0.581198,-0.096203,0.808069</ICP><SC>0.000199,0.051519,0.691524</SC><DCO>0.018203,-0.124923,0.093536</DCO></BOX>";
        //get object type

        if (result.IndexOf("<BOX>") != -1)
        {
            output.type = 1;
        }
        //get DR
        st = result.IndexOf("<DR>") + 4;
        ed = result.IndexOf("</DR>") - st;
        s = result.Substring(st, ed).Split(',');
        output.DR = new Quaternion(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
        //get OR
        st = result.IndexOf("<OR>") + 4;
        ed = result.IndexOf("</OR>") - st;
        s = result.Substring(st, ed).Split(',');
        output.OR = new Quaternion(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]), float.Parse(s[3]));
        //get SC
        st = result.IndexOf("<SC>") + 4;
        ed = result.IndexOf("</SC>") - st;
        s = result.Substring(st, ed).Split(',');
        output.SC = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
        //get DCO
        st = result.IndexOf("<DCO>") + 5;
        ed = result.IndexOf("</DCO>") - st;
        s = result.Substring(st, ed).Split(',');
        output.DCO = new Vector3(float.Parse(s[0]), float.Parse(s[1]), float.Parse(s[2]));
        //get ICP
        st = result.IndexOf("<ICP>") + 5;
        ed = result.IndexOf("</ICP>") - st;
        s = result.Substring(st, ed).Split(',');
        Matrix4x4 m = new Matrix4x4();
        for (int r = 0; r < 3; r++)
        {
            m.SetRow(r, new Vector4(float.Parse(s[(r * 3) + 0]), float.Parse(s[(r * 3) + 1]), float.Parse(s[(r * 3) + 2]), 0));
        }
        m.SetRow(3, new Vector4(0, 0, 0, 1f));
        output.ICP = m;
    }

    public string getResult()
    {
        return result;
    }

    public bool ShutdownSend()
    {
        try
        {
            socket.Shutdown(SocketShutdown.Send);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public bool ShutdownReceive()
    {
        try
        {
            socket.Shutdown(SocketShutdown.Receive);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public bool Close()
    {
        try
        {
            socket.Close();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}
