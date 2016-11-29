﻿using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Tango;
using KDTree;

public class TouchController : MonoBehaviour {
	public MenuController brickMenu;
	public Text debug; //For testing purposes
	public int neighborCountThreshold = 15;
	public float neighborDistanceThreshold = .05f;
	public float planeDistanceThreshold = 0.02f;
	public Surface surfaceTemplate;
	public TangoPointCloud tangoPointCloud;
	//Helper class for kdTree
	public class Point : MonoBehaviour
	{
		public double[] doublePosition;
		public Vector3 vectorPosition;
		public bool visited = false;
		public Point(Vector3 point) {
			doublePosition = new double[3]{point.x, point.y, point.z};
			vectorPosition = point;
		}
	}

	/*DEBUGGING SURFACE FINDING*/
	public Surface surface;
	private TangoPointCloud surfacePC;
	private Vector2 surfaceTP;
	private Plane surfacePlane;
	private Vector3 surfacePlaneCenter;
	public void updateNeighborDistanceThreshold(float newValue) {
		neighborDistanceThreshold = newValue;
		updateDebug ();
		updateSurface ();
	}
	public void updateNeighborCountThreshold(float newValue) {
		neighborCountThreshold = (int) newValue;
		updateDebug ();
		updateSurface ();
	}
	private void updateSurface() {
		surface.Recreate(FindSurfaceVertices (surfacePlane, surfacePlaneCenter, surfacePC));
	}
	private void updateDebug() {
		debug.text = "Count = " + neighborCountThreshold + " and Distance = " + neighborDistanceThreshold;
	}

	void Update () {
		if (Input.touchCount > 0)
		{	
			Touch touch = Input.GetTouch (0);
			if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject (touch.fingerId)) {
				if (!CreateSurface (touch.position, tangoPointCloud)) {
					debug.text = "Unable to find a surface. Please try again.";
				}
			}
		}
//		if (Input.GetMouseButtonDown (0)) {
//			if (!CreateSurface (Input.mousePosition)) {
//				depthText.text = "Unable to find a surface. Please try again.";
//			}
//		}
	}

	private bool CreateSurface(Vector2 touch, TangoPointCloud pc) {
		Vector3 planeCenter;
		Plane plane;
		if (pc.FindPlane (Camera.main, touch, out planeCenter, out plane)) {
			var surfaceVertices = FindSurfaceVertices (plane, planeCenter, pc);
			if (surfaceVertices.Count != 0) {
				surfacePC = pc; //For debugging
				surfaceTP = touch;
				surfacePlane = plane;
				surfacePlaneCenter = planeCenter;
				surface = Instantiate (surfaceTemplate) as Surface;
				surface.Create (surfaceVertices, plane, planeCenter, brickMenu.GetCurrentMaterial ());
				return true;
			}
		}
		return false;
	}

	private List<Vector3> FindSurfaceVertices(Plane plane, Vector3 planeCenter, TangoPointCloud pc) {
		var verticesOnSurface = new List<Vector3> ();

		//Step One: Narrow point cloud to points on the plane
		var verticesOnPlane = new List<Vector3>();
		for (int i = 0; i < pc.m_pointsCount; i++) {
			var p = pc.m_points [i];
			if (Mathf.Abs (plane.GetDistanceToPoint (p)) <= planeDistanceThreshold) {
				verticesOnPlane.Add (p);
			}
		}

		//Step Two: Create a kd-tree of the points on the plane
		var kdTree = new KDTree<Point>(3);
		foreach (var vertex in verticesOnPlane) {
			var point = new Point (vertex);
			kdTree.AddPoint(point.doublePosition, point);
		}

		//Step Three: Breadth-first-style search for points on surface.
		var pointsToCheck = new UniqueQueue<Point>();
		pointsToCheck.Enqueue (new Point(planeCenter));
		while (!pointsToCheck.isEmpty()) {
			var currentPoint = pointsToCheck.Dequeue ();
			var neighbors = kdTree.NearestNeighbors(currentPoint.doublePosition, neighborCountThreshold, neighborDistanceThreshold);
			int neighborCount = 0;
			while (neighbors.MoveNext()) {
				if (!neighbors.Current.visited) {
					pointsToCheck.Enqueue (neighbors.Current);
					neighbors.Current.visited = true;
				}
				neighborCount++;
			}
			if (neighborCount >= neighborCountThreshold) {
				verticesOnSurface.Add (currentPoint.vectorPosition);
			}
		}

		debug.text = "There are " + verticesOnPlane.Count + " vertices on the plane and " + verticesOnSurface.Count + " vertices on the surface.";
		return verticesOnSurface;
	}

}
