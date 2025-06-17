using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleController : MonoBehaviour
{
    public GameObject square;
    public GameObject puzzle;
    public float puzzleDisplacement = 0.75f;
    // Start is called before the first frame update
    void Awake()
    {
        //put square in proper place based on new stretch
        transform.localScale = new Vector3(2.36585365853f * ((float)Screen.width / (float)Screen.height), transform.localScale.y, transform.localScale.z);
        square.transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(.25f, 0, 0)).x, square.transform.position.y, square.transform.position.z);
        puzzle.transform.position = new Vector3(Camera.main.ViewportToWorldPoint(new Vector3(puzzleDisplacement, 0, 0)).x, puzzle.transform.position.y, puzzle.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
