﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Tango;

public class TouchController : MonoBehaviour {
	public Text depthText; // For testing purposes
	public MenuController menu;
	public TangoPointCloud pc;
	public float planeDistanceThreshold = 0.1f;
	public Surface surfaceTemplate;

	void Update () {
		if (Input.touchCount > 0)
		{	
			Touch touch = Input.GetTouch (0);
			if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject (touch.fingerId)) {
				if (!CreateSurface (touch.position)) {
					depthText.text = "Unable to find a surface. Please try again.";
				}
			}
		}
		if (Input.GetMouseButtonDown (0)) {
			if (!CreateSurface (Input.mousePosition)) {
				depthText.text = "Unable to find a surface. Please try again.";
			}
		}
	}

	private bool CreateSurface(Vector2 touch) {
		List<Vector3> vertices = new List<Vector3> ();
		Vector3 planeCenter;
		Plane plane;
		if (pc.FindPlane (Camera.main, touch, out planeCenter, out plane)) {
			for (int i = 0; i < pc.m_pointsCount; i++) {
				Vector3 p = pc.m_points [i];
				if (Mathf.Abs(plane.GetDistanceToPoint(p)) <= planeDistanceThreshold) {
//					vertices.Add(Vector3.ProjectOnPlane(p, plane.normal));
					vertices.Add(p);
				}
			}
			depthText.text = "Found " + vertices.Count + " vertices.";
			Surface surf = Instantiate(surfaceTemplate) as Surface;
			surf.Create(vertices, plane, planeCenter, menu.GetCurrentMaterial());
			return true;
		}
		return false;
	}

	void PositionBricks(Vector2 touchCoordinates) {
		Vector3 planeCenter;
		Vector3 forward;
		Vector3 up;
		Plane plane;
		if (pc.FindPlane (Camera.main, touchCoordinates, out planeCenter, out plane)) {
			up = plane.normal;
			float angle = Vector3.Angle (up, Camera.main.transform.forward);
			depthText.text = "angle with normal is " + angle + " degrees.";
			if (angle < 175) {
				Vector3 right = Vector3.Cross(up, Camera.main.transform.forward).normalized;
				forward = Vector3.Cross(right, up).normalized;
			} else {
				// Normal is nearly parallel to camera look direction, the cross product would have too much
				// floating point error in it.
				forward = Vector3.Cross(up, Camera.main.transform.right);
			}
			GameObject brickpic = GameObject.CreatePrimitive (PrimitiveType.Plane);
			brickpic.transform.localScale *= 0.05f;
			brickpic.transform.position = planeCenter;
			brickpic.transform.rotation = Quaternion.LookRotation(forward, up);
			brickpic.GetComponent<Renderer> ().material = menu.GetCurrentMaterial();
		} else {
			depthText.text = "No plane in sight...";
		}
		//		float x = (float)(touchCoordinates.x / Screen.width);
		//		float y = (float)(touchCoordinates.y / Screen.height);
		//		float z = pointCloud.m_overallZ;
		//		brickpic.transform.position = Camera.main.ViewportToWorldPoint(new Vector3(x, y, z));
	}

}
