using UnityEngine;
using System.Collections;

public class Scene
{

    public int status;
    public string filePath;
    public Vector3 devicePosition;
    public Quaternion deviceRotation;
    public Output output;

    /*
     *  status
     *  0 == waiting
     *  1 == done
     *  -1 == fail
     * 
     */

    public Scene(string filePath, Vector3 devicePosition, Quaternion deviceRotation)
    {
        this.status = 0;
        this.filePath = filePath;
        this.devicePosition = devicePosition;
        this.deviceRotation = deviceRotation;
        this.output = new Output();
    }

}
