using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using GoogleARCore;

public class ARController : MonoBehaviour
{

    private List<TrackedPlane> m_NewTrackedPlanes = new List<TrackedPlane>();

    public GameObject GridPref;
    public GameObject PrefabObject;
    public GameObject ARCamera;

    public GameObject text;

    // Use this for initialization
    void Start()
    {
        //disable the Gameobject
        //PrefabObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        //check ARCore Session status
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        ProcessNewPlanes();

        ProcessTouches();
    }

    // The Plane creator
    void ProcessNewPlanes()
    {
        //The following function will fill m_NewTrackPlanes with planes that ARCore detected in the current frame
        Session.GetTrackables<TrackedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

        //Instaitiate a Grid for each TrackedPLane in m_NewTrackedPlanes
        for (int i = 0; i < m_NewTrackedPlanes.Count; ++i)
        {
            GameObject Grid = Instantiate(GridPref, Vector3.zero, Quaternion.identity, transform);

            //This funktion will set the Position of grid and modify the vertices of the attached mesh
            Grid.GetComponent<PlaneVisualizer>().Initialize(m_NewTrackedPlanes[i]);
        }
    }

    // Touch Process 
    void ProcessTouches()
    {
        //Check if the used touches the screen
        Touch touch;

        if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        // Raycast against the location the player touched to search for planes.
        TrackableHit hit;
        //TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinPolygon | TrackableHitFlags.FeaturePointWithSurfaceNormal;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinPolygon;
        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {           
            SetGameObject(hit);
        }
    }

    // Set the Gameobjekt of the Plane we touched
    void SetGameObject(TrackableHit hit)
    {
        //Enable the GameObject
        PrefabObject.SetActive(true);

        //Create a new Anchor
        Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

        //Set the position of the PrefabObject to be the smae as the hit postion
        PrefabObject.transform.position = new Vector3(hit.Pose.position.x, hit.Pose.position.y + (PrefabObject.transform.localScale.y / 2 ), hit.Pose.position.z);
        PrefabObject.transform.rotation = hit.Pose.rotation;

        //we want the portal to face the camera
        Vector3 cameraPositon = ARCamera.transform.position;

        //the portal should only rotate arround the y axis
        cameraPositon.y = hit.Pose.position.y;

        //Rotate the portal to face the camera
        PrefabObject.transform.LookAt(cameraPositon, PrefabObject.transform.up);

        //the object should rotate left by 180 degrees
        Quaternion newRotation = Quaternion.AngleAxis(180, Vector3.up);
        PrefabObject.transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, .05f);

        text.GetComponent<Text>().text = "tricker fur z-achse " + hit.Pose.position.z;

        //ARCore will keep understanding the world and update the anchors accordingly hence we need to attach our portal to the anchor
        PrefabObject.transform.parent = anchor.transform;

    }

    /// <summary>
    /// Actually quit the application.
    /// </summary>
    private void _DoQuit()
    {
        Application.Quit();
    }
}
