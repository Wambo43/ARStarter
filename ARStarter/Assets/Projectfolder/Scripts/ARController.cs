using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleARCore;

public class ARController : MonoBehaviour {

    public GameObject GridPref;
    public GameObject Portal;
    public GameObject ARCamera;

    //We will fill this with the planes that ARCore detected in the current frame
    private List<TrackedPlane> m_NewTrackedPlanes = new List<TrackedPlane>();
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //Check ARCore session status
        if (Session.Status != SessionStatus.Tracking)
        {
            return;
        }

        ProcessNewPlanes();

        ProcessTouches();
    }

    void ProcessNewPlanes()
    {
        //The following function will fill m_NewTrackPlanes with planes that ARCore detected in the current frame
        Session.GetTrackables<TrackedPlane>(m_NewTrackedPlanes, TrackableQueryFilter.New);

        //Instaitiate a Grid for each TrackedPLane in m_NewTrackedPlanes
        for (int i = 0; i < m_NewTrackedPlanes.Count; ++i)
        {
            GameObject Grid = Instantiate(GridPref, Vector3.zero, Quaternion.identity, transform);

            //This funktion will set the Position of grid and modify the vertices of the attached mesh
            Grid.GetComponent<GridVisualiser>().Initialize(m_NewTrackedPlanes[i]);
        }
    }

    void ProcessTouches()
    {
        //Check if the used touches the screen
        Touch touch;

        if (Input.touchCount <= 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
        {
            return;
        }

        //Let's now check if the user touched any of the tracked planes
        TrackableHit hit;
        TrackableHitFlags raycastFilter = TrackableHitFlags.PlaneWithinBounds | TrackableHitFlags.PlaneWithinPolygon;
        if (Frame.Raycast(touch.position.x, touch.position.y, raycastFilter, out hit))
        {
            SetSelectedPlane(hit);
        }
    }

    void SetSelectedPlane(TrackableHit hit)
    {
        //Let's now place the portal on the tracked plane we touched

        //Endable the Portal
        Portal.SetActive(true);

        //Create a new Anchor
        Anchor anchor = hit.Trackable.CreateAnchor(hit.Pose);

        //Set the position of the Portal to be the smae as the hit postion
        Portal.transform.position = new Vector3(hit.Pose.position.x, hit.Pose.position.y + (Portal.transform.localScale.y / 2), hit.Pose.position.z);
        Portal.transform.rotation = hit.Pose.rotation;

        //we want the portal to face the camera
        Vector3 cameraPositon = ARCamera.transform.position;

        //the portal should only rotate arround the y axis
        cameraPositon.y = hit.Pose.position.y;

        //Rotate the portal to face the camera
        Portal.transform.LookAt(cameraPositon, Portal.transform.up);

        //ARCore will keep understanding the world and update the anchors accordingly hence we need to attach our portal to the anchor
        Portal.transform.parent = anchor.transform;
    }
}
