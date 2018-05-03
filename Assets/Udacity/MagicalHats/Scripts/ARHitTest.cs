using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;

public class ARHitTest : MonoBehaviour {
	public Camera ARCamera; //the Virtual Camera used for AR
	public GameObject hitPrefab; //prefab we place on a hit test
	public GameObject rabbitPrefab;

	private List<GameObject> spawnedObjects = new List<GameObject>(); //array used to keep track of spawned objects
	private List<GameObject> spawnedRabbits = new List<GameObject>();
	private GameObject rabHat; 

	/// <summary>
	/// Function that is called on 
	/// NOTE: HIT TESTS DON'T WORK IN ARKIT REMOTE
	/// </summary>
	public void SpawnHitObject() {
		ARPoint point = new ARPoint { 
			x = 0.5f, //do a hit test at the center of the screen
			y = 0.5f
		};

		// prioritize result types
		ARHitTestResultType[] resultTypes = {
			//ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, // if you want to use bounded planes
			//ARHitTestResultType.ARHitTestResultTypeExistingPlane, // if you want to use infinite planes 
			ARHitTestResultType.ARHitTestResultTypeFeaturePoint // if you want to hit test on feature points
		}; 

		foreach (ARHitTestResultType resultType in resultTypes) {
			if (HitTestWithResultType (point, resultType)) {
				return;
			}
		}
	}

	bool HitTestWithResultType (ARPoint point, ARHitTestResultType resultTypes) {
		List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface ().HitTest (point, resultTypes);
		if (hitResults.Count > 0) {
			foreach (var hitResult in hitResults) {
				//TODO: get the position and rotations to spawn the hat
				Vector3 pos = UnityARMatrixOps.GetPosition (hitResult.worldTransform);
				Quaternion rotation = UnityARMatrixOps.GetRotation (hitResult.worldTransform) * Quaternion.AngleAxis(180, Vector3.up);
				spawnedObjects.Add( Instantiate (hitPrefab, pos, rotation) ); // in order to use for shuffling
				return true;
			}
		}
		return false;
	}

	// Fixed Update is called once per frame
	void FixedUpdate () {
		if (Input.GetMouseButtonDown(0)) { //this works with touch as well as with a mouse
			RemoveObject (Input.mousePosition);
		}
	}

	public void RemoveObject(Vector2 point) {
		//TODO: Raycast from the screen point into the virtual world and see if we hit anything
		//if we do, then check to see if it is part of the spawnedObjects array
		//if so, then delete the object we raycast hit
		RaycastHit hit;
		if (Physics.Raycast (ARCamera.ScreenPointToRay (point), out hit)) {
			GameObject item = hit.collider.transform.parent.gameObject; //parent is what is stored in our area;
			if (item == rabHat) {
				Vector3 pos = rabHat.transform.position;
				Quaternion rotation = rabHat.transform.rotation;
				spawnedRabbits.Add( Instantiate (rabbitPrefab, pos, rotation) );
			}
			item.transform.position += new Vector3(0f, 0.1f, 0f);

			if (spawnedRabbits.Remove (item) ) { //make sure to remove the hat from the array for consistancy
				Destroy (item);
			}
			//if (spawnedObjects.Remove (item) ) { //make sure to remove the hat from the array for consistancy
				//Destroy (item);
			//}
		}
	}
		
	/// <summary>
	/// NOTE: A Function To Be Called When the Shuffle Button is pressed
	/// </summary>
	public void Shuffle(){
		StartCoroutine( ShuffleTime ( Random.Range(5, 10)) );

		rabHat = spawnedObjects[Random.Range(0, spawnedObjects.Count)];

	}
		
	/// <summary>
	/// NOTE: A Co-routine that shuffles 
	/// </summary>
	IEnumerator ShuffleTime(int numSuffles) {
		//TODO:
		//iterate numShuffles times
		//pick two hats randomly from spawnedObject and call the Co-routine Swap with their Transforms
		for (int i = 0; i < numSuffles; i++) {
			GameObject randFrom = spawnedObjects[Random.Range(0, spawnedObjects.Count)];
			GameObject randTo = spawnedObjects[Random.Range(0, spawnedObjects.Count)];
			yield return StartCoroutine(Swap(randFrom.transform, randTo.transform, .5f));
		}
	}

	IEnumerator Swap(Transform item1, Transform item2, float duration){
		//Lerp the position of item1 and item2 so that they switch places
		//the transition should take "duration" amount of time
		//Optional: try making sure the hats do not collide with each other
		float t = 0;
		Vector3 startPos = item1.position;
		Vector3 endPos = item2.position;
		while (t < duration) {
			t += Time.deltaTime;
			item1.position = Vector3.Lerp (startPos, endPos, t / duration);
			item2.position = Vector3.Lerp (endPos, startPos, t / duration);
			yield return null;
		}
	}
}