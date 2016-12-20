﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Tango;
using KDTree;

public class NewTouchController : MonoBehaviour {
	public Text debug;
	public GameObject line;
	public NewSurface surfaceTemplate;
	public TangoPointCloud tangoPointCloud;
	private Vector3 firstCorner;
	private Vector3 oppositeCorner;
	private bool hasStartPoint = false;

	/* USEFUL FOR DEBUGGING */
//	void Start() {
//		var center = new Vector3(1, 1, 1);
//		var plane = new Plane (Quaternion.Euler(30, 60, 70) * -Vector3.forward, center);
//		NewSurface surface = Instantiate (surfaceTemplate) as NewSurface;
//		surface.Create (plane, center + new Vector3(1, 1, 1), center + new Vector3(-1, -1, -1), center);
//	}

	void Update () {
		if (Input.touchCount > 0)
		{	
			Touch touch = Input.GetTouch (0);
			int closestPointIndex = tangoPointCloud.FindClosestPoint (Camera.main, touch.position, 500);
			Vector3 closestPoint = tangoPointCloud.m_points [closestPointIndex];
			if (closestPointIndex != -1) {
				if (!hasStartPoint) {
					StartLine (closestPoint);
					firstCorner = closestPoint;
					hasStartPoint = true;
				}
				ExtendLine (closestPoint);
				oppositeCorner = closestPoint;
			}
			if (touch.phase == TouchPhase.Ended) {
				line.SetActive(false);
				if (hasStartPoint) {
					hasStartPoint = false;
					HandleTouch (touch.position);
				}
			}
		}
		/* USEFUL FOR DEBUGGING */
//		if (Input.GetMouseButtonDown(0)) {
//		}
	}

	private void StartLine(Vector3 start)
	{
		line.transform.position = start;
		LineRenderer lr = line.GetComponent<LineRenderer>();
		lr.SetPosition(0, start);
		lr.SetPosition(1, start);
		line.SetActive(true);
	}

	private void ExtendLine(Vector3 end) {
		LineRenderer lr = line.GetComponent<LineRenderer>();
		lr.SetPosition(1, end);
	}

	private void HandleTouch(Vector2 position) {
		var diagonal = firstCorner - oppositeCorner;
		if (diagonal.magnitude < 0.1f) { //Surface not big enough; could also be a UI tap
			if (!TrySelectSurface (position)) {
				debug.text = "No surface selected.";
			}
		} else {
			CreateSurface ();
		}
	}

	private bool CreateSurface() {
		Vector3 planeCenter;
		Plane plane;

		float lerpOffset = -0.25f; //Must be negative to start
		float lerpAmount = 0.5f;
		Vector3 center = Vector3.Lerp (firstCorner, oppositeCorner, lerpAmount);
		if (!tangoPointCloud.FindPlane (Camera.main, Camera.main.WorldToScreenPoint(center), out planeCenter, out plane)) {
			debug.text = "No surface found. Please try again.";
			return false;
		}
		NewSurface surface = Instantiate (surfaceTemplate) as NewSurface;
		surface.Create (plane, firstCorner, oppositeCorner, planeCenter);
		return true;
	}

	private bool TrySelectSurface(Vector2 touch) {
		//Check if you hit a UI element (http://answers.unity3d.com/questions/821590/unity-46-how-to-raycast-against-ugui-objects-from.html)
		var pointer = new PointerEventData(EventSystem.current);
		pointer.position = touch;
		var results = new List<RaycastResult> ();
		EventSystem.current.RaycastAll(pointer, results);
		if (results.Count == 0) {
			//Check if you hit a surface
			RaycastHit hit;
			var ray = Camera.main.ScreenPointToRay (touch);
			var layerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
			if (Physics.Raycast (ray.origin, ray.direction, out hit, layerMask)) {
				NewSurface selected = hit.collider.gameObject.GetComponent<NewSurface> ();
				if (selected != null) {
					selected.SelectSurface ();
					return true;
				}
			}
		}
		return false;
	}

}
