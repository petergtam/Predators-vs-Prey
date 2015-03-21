using UnityEngine;
using System.Collections;

public class snapshot : MonoBehaviour {
	private bool vis = false;
	// Use this for initialization
	void Start () {
		GameObject[] trees = GameObject.FindGameObjectsWithTag ("Tree");
		Debug.Log ("trees:" + trees.Length);
	}
	
	// Update is called once per frame
	void Update () {
		GameObject[] trees = GameObject.FindGameObjectsWithTag ("Tree");
		for (int i = 0; i < trees.Length; i++) {
			trees[i].renderer.enabled = this.vis;
		}
		this.vis = !this.vis;
	}


}
