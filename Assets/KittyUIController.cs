using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KittyUIController : MonoBehaviour
{
    public GameObject m_kitten;
    public GameObject door;
    public GameObject realKitty;
    private GameObject virtualPlane;
    private Vector3 vertualPlaneNormal;
    private TangoPointCloud m_pointCloud;
    private bool point1Ready;
    private bool point2Ready;
    private bool planeReady;
    public bool modelPlaced;
    private Vector2 positionA;
    private Vector2 positionB;
    private int cornerCount;
    private List<Vector3> corners;

    private bool doorPlaced;

    void Start()
    {
        m_pointCloud = FindObjectOfType<TangoPointCloud>();
        cornerCount = 0;
        corners = new List<Vector3>();
        //Input.gyro.enabled = true;
    }

void Update()
    {
        if (Input.touchCount == 1)
        {
            // Trigger place kitten function when single touch ended.
            Vector3 touchPosition = Input.GetTouch(0).position;
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Ended)
            {
                //if (!point1Ready)
                //{
                //    positionA = t.position;
                //    point1Ready = true;

                //} else if (!point2Ready)
                //{
                //    positionB = t.position;
                //    point2Ready = true;
                //}
                //if (point1Ready && point2Ready && !modelPlaced)
                //{
                //    PlaceKitten(positionA, positionB);
                //    modelPlaced = true;
                //}
                if (!planeReady)
                {
                    PlaceKitten(t.position);
                } else
                {
                    if (cornerCount<4)
                    {
                        SetCorners(touchPosition);
                    }
                    if (cornerCount == 4 && !doorPlaced)
                    {
                        PlaceDoor();
                    }
                }

            }
        }
    }

    void PlaceKitten(Vector2 touchPosition)
    {
        // Find the plane.
        Camera cam = Camera.main;
        Vector3 planeCenter;
        Plane plane;
        if (!m_pointCloud.FindPlane(cam, touchPosition, out planeCenter, out plane))
        {
            Debug.Log("cannot find plane.");
            return;
        }

        GameObject realKittyAugmented = null;
        // Place kitten on the surface, and make it always face the camera.
        if (Vector3.Angle(plane.normal, Vector3.up) < 30.0f)
        {
            //horizontal and sloped plane behaviour
            Vector3 up = plane.normal;
            Vector3 right = Vector3.Cross(plane.normal, cam.transform.forward).normalized;
            Vector3 forward = Vector3.Cross(right, plane.normal).normalized;
            if (realKittyAugmented == null)
            {
                realKittyAugmented = Instantiate(realKitty, planeCenter, Quaternion.LookRotation(forward, up));
            }
        }
        else
        {
            //Debug.Log("surface is too steep for kitten to stand on.");
            //Verticle plane behaviour 
            Vector3 up = plane.normal;
            vertualPlaneNormal = plane.normal;
            Vector3 right = Vector3.Cross(plane.normal, cam.transform.forward).normalized;
            Vector3 forward = Vector3.Cross(right, plane.normal).normalized;
            virtualPlane = Instantiate(m_kitten, planeCenter, Quaternion.LookRotation(forward, up));
            //instantiatedObject.transform.Rotate(Input.gyro.gravity);// = (Input.gyro.gravity);
            planeReady = true;
        }
    }

    void SetCorners(Vector3 touchPosition)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(touchPosition);
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 hitPosition = hit.point;
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = hitPosition;
            sphere.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
            cornerCount++;
            corners.Add(hitPosition);
        }
    }

    void PlaceDoor()
    {
        //GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject placedDoor = Instantiate(door);
        Vector3 topLeft = corners.ToArray()[0];
        Vector3 topRight = corners.ToArray()[1];
        Vector3 bottomRight = corners.ToArray()[2];
        Vector3 bottomLeft = corners.ToArray()[3];

        Vector3 topLine = topRight - topLeft;
        float doorWidth = Vector3.Distance(topLeft, topRight);
        float doorHeight = Vector3.Distance(topLeft, bottomLeft);
        placedDoor.transform.position = new Vector3(topLeft.x + doorWidth/2, topLeft.y - doorHeight/2, topLeft.z);
        //placedDoor.transform.Rotate(vertualPlaneNormal);
        //Vector3 up = vertualPlaneNormal;
        //Vector3 right = Vector3.Cross(vertualPlaneNormal, Camera.main.transform.forward).normalized;
        //Vector3 forward = Vector3.Cross(right, vertualPlaneNormal).normalized;
        //Quaternion lookRotation = Quaternion.LookRotation(forward, up);
        //placedDoor.transform.rotation = new Quaternion(lookRotation.x,lookRotation.y, placedDoor.transform.rotation.z, placedDoor.transform.rotation.w);//Quaternion.LookRotation(forward, up);//Quaternion.Euler(vertualPlaneNormal);
        float step = 100 * Time.deltaTime;
        placedDoor.transform.rotation = Quaternion.LookRotation(Vector3.RotateTowards(placedDoor.transform.forward, vertualPlaneNormal, step, 0.0f));

        //door.transform.position = topLeft;
        placedDoor.transform.localScale = new Vector3(doorWidth, doorHeight, 0.01f);
        //placedDoor.transform.position. = doorWidth / 2;
        //door.transform.Rotate(topLine);
        //door.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Destroy(virtualPlane);
        doorPlaced = true;
    }

}