﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Tango;
using KDTree;

public class NewTouchController : MonoBehaviour {
	public Text debug;
//	public MenuController brickMenu;
	public NewSurface surfaceTemplate;
	public TangoPointCloud tangoPointCloud;
	private Vector3 topLeft;
	private Vector3 bottomRight;
	private bool isDragging;
	public GameObject line;
	private bool hasStartPoint;

	/* USEFUL FOR DEBUGGING */
//	void Start() {
//		var center = new Vector3(2, 3, 5);
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
					topLeft = closestPoint;
					hasStartPoint = true;
				}
				ExtendLine (closestPoint);
				bottomRight = closestPoint;
			}
			if (touch.phase == TouchPhase.Ended) {
				line.SetActive(false);
				if (hasStartPoint) {
					hasStartPoint = false;
					CreateSurface ();
				}
			}
		}
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

	private bool CreateSurface() {
		var diagonal = topLeft - bottomRight;
		if (diagonal.magnitude < 0.01f) {
			return false; //Surface not big enough; could also be a UI tap
		}
		Vector3 planeCenter;
		Plane plane;

		float lerpOffset = -0.25f;
		float lerpAmount = 0.5f;
		Vector3 center = Vector3.Lerp (topLeft, bottomRight, lerpAmount);
		//Try to find the plane
		while (!tangoPointCloud.FindPlane (Camera.main, Camera.main.WorldToScreenPoint(center), out planeCenter, out plane)) {
			if (Mathf.Abs (lerpOffset) >= 0.5) {
				debug.text = "No surface found. Please try again.";
				return false;
			} else if (lerpOffset > 0) {
				lerpOffset += 0.1f;
				lerpOffset *= -1;
			}
			lerpAmount = 0.5f + lerpOffset;
			center = Vector3.Lerp (topLeft, bottomRight, lerpAmount);
		}
		debug.text = "Found plane at " + lerpAmount;
		NewSurface surface = Instantiate (surfaceTemplate) as NewSurface;
		surface.Create (plane, topLeft, bottomRight, planeCenter);
		return true;
	}

}
