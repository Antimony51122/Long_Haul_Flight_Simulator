﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// the attribute here will automatically bring a Nav Mesh Agent when attached to an object
[RequireComponent(typeof(NavMeshAgent))]
public class NavAgentExample : MonoBehaviour {
	// Inspector Assigned Variable
	public AIWaypointNetwork WaypointNetwork = null;

	public int  CurrentIndex = 0;
	public bool HasPath      = false;
	public bool PathPending  = false;
	public bool PathStale    = false;

	public NavMeshPathStatus PathStatus = NavMeshPathStatus.PathInvalid;

	// Jump Curve used for later off mesh link manipulations
	public AnimationCurve JumpCurve = new AnimationCurve();

	// Private Members (cache reference to it later)
	private NavMeshAgent _navAgent = null;

	// Use this for initialization
	void Start() {
		// Cache NavMeshAgent Reference
		_navAgent = GetComponent<NavMeshAgent>();

		// This section for fun
		// Turning off auto-update, then only the cylinder is moving
		// while the mesh is not updating
		//_navAgent.updatePosition = false;
		//_navAgent.updateRotation = false;

		// If not valid Waypoint Network has been assigned then return
		if (WaypointNetwork == null) return;

		// prototyping to test the navigation works
		//if (WaypointNetwork.Waypoints[CurrentIndex] != null) {
		//	// due that `Waypoints` is a transform, we fetch the position of that
		//	_navAgent.destination = WaypointNetwork.Waypoints[CurrentIndex].position;
		//}

		SetNextDestination(false);
	}

	// Update is called once per frame
	void Update() {
		// Copy NavMeshAgents state into inspector visible variables
		HasPath     = _navAgent.hasPath;
		PathPending = _navAgent.pathPending;
		PathStale   = _navAgent.isPathStale;
		PathStatus  = _navAgent.pathStatus;


		// If agent is on an offmesh link then perform a jump
		if (_navAgent.isOnOffMeshLink) {
			StartCoroutine(Jump(1.0f));
			return;
		}

		// If we don't have a path and one isn't pending then set the next waypoint as the target,
		// otherwise if path is stale regenerate path
		// when encountering partial path, reach the closet viable position and reject the waypoint and find a new path
		// if uncomment the last or statement, will completely ignore the partial path
		// _navAgent.stoppingDistance for judging the threshold of jumping
		// stoppingDistance property is in nav mesh agent inspector, representing the distance away from the destination considering traversed
		if ((_navAgent.remainingDistance <= _navAgent.stoppingDistance && !PathPending) ||
			PathStatus == NavMeshPathStatus.PathInvalid /*|| 
			PathStatus == NavMeshPathStatus.PathPartial*/) {
			SetNextDestination(true);
		} else if (_navAgent.isPathStale) {
			// check whether the path is stale, meaning no longer valid or optimal
			// current index won't be incremented
			// reset the same waypoint and regenerate a path for the current waypoint
			SetNextDestination(false);
		}
	}

	void SetNextDestination(bool increment) {
		// If no network return
		if (!WaypointNetwork) return;

		// Calculate how much the current waypoint index needs to be incremented
		int incStep = increment ? 1 : 0;

		Transform nextWaypointTransform = null;

		int nextWaypoint = (CurrentIndex + incStep >= WaypointNetwork.Waypoints.Count) ? 0 : CurrentIndex + incStep;
		nextWaypointTransform = WaypointNetwork.Waypoints[nextWaypoint];

		// Assuming we have a valid waypoint transform
		if (nextWaypointTransform != null) {
			// Update the current waypoint index, assign its position as the NavMeshAgents Destination and then return
			CurrentIndex          = nextWaypoint;
			_navAgent.destination = nextWaypointTransform.position;
			return;
		}

		// We did not find a valid waypoint in the list for this iteration
		// In the next iteration of the loop, having another pop up
		CurrentIndex++;
	}

	// --------------------------------------------------------------------------------------
	// Name	: Jump
	// DESC	: Manual OffMeshLInk traversal using an Animation Curve to control agent height.
	// --------------------------------------------------------------------------------------
	IEnumerator Jump(float duration) {
		// create a loop that interpolate between the positions of agents currently at the end point of the off link 

		// Get the current OffMeshLink data
		OffMeshLinkData data = _navAgent.currentOffMeshLinkData;

		// Start Position is agent current position rather the start position property of the nav mesh link
		Vector3 startPos = _navAgent.transform.position;

		// End position is fetched from OffMeshLink data and adjusted for base-offset of agent
		Vector3 endPos = data.endPos + (_navAgent.baseOffset * Vector3.up);

		// Used to keep track of time
		float time = 0.0f; // set to 0 to begin with, modified later


		// Keeo iterating for the passed duration
		while (time <= duration) {
			// Calculate normalized time
			float t = time / duration;

			// Lerp between start position and end position and adjust height based on evaluation of t on Jump Curve
			// Lerp: Linearly interpolates between two vectors.
			_navAgent.transform.position = Vector3.Lerp(startPos, endPos, t) + (JumpCurve.Evaluate(t) * Vector3.up);

			// Accumulate time and yield each frame
			// make sure yield from the coroutine because we wish to only yield for a single frame
			// and then have another iteration of the while loop executed next frame
			time += Time.deltaTime;
			yield return null;
		}

		// All done so inform the agent it can resume control to next target
		_navAgent.CompleteOffMeshLink();
	}
}