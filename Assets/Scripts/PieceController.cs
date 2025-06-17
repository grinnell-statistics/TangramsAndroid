using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* 
 * Class: PieceController
 * Purpose: Implements the controls of selected piece
 */
public class PieceController : MonoBehaviour
{
    public static float acceptableError = 0.5F;
    public static KeyCode flipKey = KeyCode.S;
    public static KeyCode rotateCWKey = KeyCode.A;
    public static KeyCode rotateCCWKey = KeyCode.D;
    


    public Vector3 startPosition;
    public GameObject pattern;
    public bool weirdFlips;
    public bool parallelogram;
    public Color ogCol;


    private bool isActive;
    public bool isSnapped;
    private Vector3 displacementVector;
    private bool isFlipped;
    public static Vector2[] outline;
    private Vector2[] myBounds;
    private Vector2 center;
    public static Vector2 outlineCenter;
    private static int layer = 2;
    private static GameObject[] order;
    [HideInInspector]
    public int myPosition;
    public Button flipButton;
    public Button CWButton;
    public Button CCWButton;

    private Quaternion startRotation;
    private int myTouch;

    public int pieceNumber;
 

    // Start is called before the first frame update
    void Start()
    {
        //Input.multiTouchEnabled = false;
        startRotation = transform.rotation;
        DataManager.gameData.numClicks = 0;
        isActive = false;
        isFlipped = false;
        isSnapped = false;
        transform.localPosition = startPosition;
        if (order == null)
            order = new GameObject[7];
        //outline is the vertices of the puzzle's collider
        //for the purposes of snapping pieces into place

        //each piece tests if it is within a short distance of 
        //a vertex of the puzzle and if so, snaps into place
        if (outline == null)
        {
            //this only gets an unscaled collection of points that are centered at 0,0
            //so we need to adjust by both the scale and the center of the puzzle
            outline = pattern.GetComponent<PolygonCollider2D>().GetPath(0);
            outlineCenter = pattern.transform.position;
            for (int i = 0; i < outline.Count(); i++)
            {
                outline[i] *= pattern.transform.lossyScale;
                outline[i] += outlineCenter;
            }
        }
        //Draw shape
        /*
        for(int i=0; i<outline.Count()-1; i++)
        {
            Debug.DrawLine(outline[i], outline[i + 1], Color.blue, 15);
        }
        Debug.DrawLine(outline[outline.Count() - 1], outline[0], Color.blue, 15);
        */
        //gets the vertex set of the piece's collider
        myBounds = GetComponent<PolygonCollider2D>().GetPath(0);
        //sorts the pieces so they stack
        foreach (var renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingOrder = layer;
        }
        order[layer / 10] = gameObject;
        myPosition = layer / 10;
        layer += 10;
        /* 
         //for testing, moves all pieces to correct locations
        if (layer < 62)
            return;
        for(int i=0; i<order.Length; i++)
        {
            foreach (Puzzle p in pattern.GetComponentsInChildren<Puzzle>())
            {
                if (p.pieceNum == order[i].GetComponent<PieceController>().pieceNumber)
                {
                    order[i].transform.position = p.transform.position;
                    order[i].transform.rotation = p.transform.rotation;
                }
            }
        }
        */
    }
    private void ColorChange()
    {
        foreach (SpriteRenderer renderer in order[6].GetComponentsInChildren<SpriteRenderer>())
        {
            if (renderer.gameObject.name.Contains("PieceTriangle"))
                renderer.color = new Color(144, 0, 185, 100);  // Colors it higlighed purple 
        }
    }


    private void ColorRevert()
    {
        foreach (SpriteRenderer renderer in order[6].GetComponentsInChildren<SpriteRenderer>())
        {
            if (renderer.gameObject.name.Contains("PieceTriangle"))
                renderer.color = ogCol;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ColorChange();
        //resets stacking so that array accesses based on layers work
        //more than one time
        if (layer!=2)
            layer = 2;
        //there are 7 pieces and at most one is active
        //if there is nothing to do why stick around
        if (!isActive)
            return;
        //keeps the piece displaced from the mouse by the same
        //amount that it was when it was clicked on
        //basically we don't want the pieces to snap to the mouse when picked up
        Touch myFinger = Input.GetTouch(0);
        foreach(Touch t in Input.touches)
        {
            if (t.fingerId == myTouch)
                myFinger = t;
        }
        transform.position = Camera.main.ScreenToWorldPoint(myFinger.position)-displacementVector;
        if (Input.GetKeyDown(flipKey))
        {
            Flip();
            DataManager.gameData.numClicks++;
        }
        //this is only difficult because when the shape is flipped we have to rotate the other way
        //to rotate the desired direction
        if ((Input.GetKeyDown(rotateCWKey) && !isFlipped) || (Input.GetKeyDown(rotateCCWKey) && isFlipped))
        {
            transform.Rotate(Vector3.forward, 45);
            DataManager.gameData.numClicks++;
        }
        if ((Input.GetKeyDown(rotateCCWKey) && !isFlipped) || (Input.GetKeyDown(rotateCWKey) && isFlipped))
        {
            transform.Rotate(Vector3.forward, -45);
            DataManager.gameData.numClicks++;
        }
    }

    private void OnMouseDown()
    {
        DataManager.gameData.numClicks++;
        PickUp();
    }

    private void OnMouseUp()
    {
        Touch myFinger = Input.GetTouch(0);
        foreach (Touch t in Input.touches)
        {
            if (t.fingerId == myTouch)
                myFinger = t;
        }
        if (myFinger.phase == TouchPhase.Ended)
        {
            Drop();
            isActive = false;
        }
    }


    private void PickUp()
    {
        isSnapped = false;
        isActive = true;
        displacementVector = Camera.main.ScreenToWorldPoint(Input.GetTouch(0).position) - transform.position;
        myTouch = Input.GetTouch(0).fingerId;
        //sort the pieces again, since this one is obviously on top
        ColorRevert();
        for (int i = myPosition; i<6; i++)
        {
            order[i] = order[i + 1];
            order[i].GetComponent<PieceController>().myPosition = i;
            foreach (SpriteRenderer renderer in order[i].GetComponentsInChildren<SpriteRenderer>())
            {
                renderer.sortingOrder = 10*i+2;
            }
        }
        order[6] = gameObject;
        myPosition = 6;
        foreach (SpriteRenderer renderer in GetComponentsInChildren<SpriteRenderer>())
        {
            renderer.sortingOrder = 62;
        }
        //end sort
    }

    private void Drop()
    {
        
        List<Vector3> differences = new List<Vector3>();
        Vector2[] myCurrentBounds = GetBounds();
        //points on the collider
        foreach(Vector2 point in myCurrentBounds)
        {
            //points on the house (or other shape)
            foreach (Vector2 point2 in outline)
            {
                //close enough to snap
                if (Vector2.Distance(point, point2) < acceptableError)
                {
                    differences.Add(point - point2);
                }
            }
        }
        isSnapped = false;
        if (differences.Count > 0)
        {
            isSnapped = true;
            //snap into place
            transform.position -= differences[0];
        }
    }

    public Vector2[] GetBounds()
    {
        return GetBounds(1);
    }

    public Vector2[] GetBounds(float scale)
    {
        center = transform.position; //center of the object
        Vector2[] myTransformedBounds = new Vector2[myBounds.Count()]; // outer points
        int i = 0;//for indexing
        foreach(var point in myBounds)
        {
            float theta = transform.rotation.eulerAngles.x - transform.rotation.eulerAngles.z;//rotation angle
            theta *= Mathf.Deg2Rad;
            if (!isFlipped)
                theta = -theta;//confirm correct rotation angle, in Radians
            float y = transform.lossyScale.y*point.y; //x and y values
            float x = transform.lossyScale.x*point.x;
            if (parallelogram && isFlipped)
                y = -y;
            myTransformedBounds[i] = new Vector2(x*Mathf.Cos(theta)-y*Mathf.Sin(theta), x*Mathf.Sin(theta) + y*Mathf.Cos(theta));//matrix math for rotations
            myTransformedBounds[i] *= scale;
            myTransformedBounds[i] += center; //Put the bounds on the object
            i++;

        }
        // draw collider
        /*
        Debug.DrawLine(myTransformedBounds[0], myTransformedBounds[1], Color.blue, 2);
        Debug.DrawLine(myTransformedBounds[1], myTransformedBounds[2], Color.blue, 2);
        if(myTransformedBounds.Count()==3)
            Debug.DrawLine(myTransformedBounds[2], myTransformedBounds[0], Color.blue, 2);
        else
        {
            Debug.DrawLine(myTransformedBounds[2], myTransformedBounds[3], Color.blue, 2);
            Debug.DrawLine(myTransformedBounds[3], myTransformedBounds[0], Color.blue, 2);
        }
        */
        return myTransformedBounds;
    }

    private void Flip()
    {
        ColorChange();
        //the medium triangle is a pain because it rotates weird
        //so here it is
        if (weirdFlips)
        {
            transform.Rotate(0, 0, 180);
            return;
        }
        //on all the pieces we have to make sure that the border does not end up in
        //front of the color so we have to move the colored triangles forward (or backwards)
        foreach (Transform child in GetComponentsInChildren<Transform>())
        {
            if(child.name.Contains("Piece"))
            {
                child.localPosition = new Vector3(child.localPosition.x, child.localPosition.y, -child.localPosition.z);
            }
                
        }
        transform.Rotate(180, 0, 0);
        isFlipped = !isFlipped;
    }
    //hello

    public void Restart()
    {
        foreach(GameObject piece in order)
        {
            piece.transform.localPosition = piece.GetComponent<PieceController>().startPosition;
            piece.transform.rotation = piece.GetComponent<PieceController>().startRotation;
        }
    }

    public void Rotate(int degrees)
    {
        ColorChange();
        DataManager.gameData.numClicks++;
        order[6].transform.Rotate(Vector3.forward, order[6].GetComponent<PieceController>().isFlipped ? -degrees : degrees);
        order[6].GetComponent<PieceController>().Drop();
    }

    public void FlipTop()
    {
        ColorChange();
        DataManager.gameData.numClicks++;
        order[6].GetComponent<PieceController>().Flip();
        order[6].GetComponent<PieceController>().Drop();
    }
}
