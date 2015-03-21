using UnityEngine;
using System.Collections;

public class NodesController : MonoBehaviour {

	public int pathNodeCount;

	// Use this for initialization
	void Start () {
		pathNodeCount = GameObject.FindGameObjectsWithTag("Node").Length;
	}

	public GameObject getNeartestNode(Vector3 pos){
		GameObject min, temp;
		float minDistance, tempDistance;

		//Busca el nodo mas cercano, inicializa con 1
		min = GameObject.Find ("pathNode1");
		minDistance = Vector3.Distance (pos, min.transform.position);

		//Recorre los nodos buscando al mas cercano
		for (int i = 2; i <= pathNodeCount; i++) {
			temp = GameObject.Find ("pathNode" + i);
			tempDistance = Vector3.Distance (pos, temp.transform.position);
			if( tempDistance < minDistance ){
				min = temp;
				minDistance = tempDistance;
			}
		}
		return min;
	}

	public GameObject[] getNeighbors(GameObject n){
		return  n.GetComponent<PathNode> ().getNeighbors();
	}
}
