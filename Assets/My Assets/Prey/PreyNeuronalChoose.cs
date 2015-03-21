using UnityEngine;
using System.Collections;

public class PreyNeuronalChoose : MonoBehaviour {

	private NodesController nodes;
	private NeuronalNetwork neural;
	public struct NeuralReturn
	{
		public GameObject node;
		public float score;
	}


	///public int 
	//Node Food
	//Node allays
	//Alert
	//Migration
	public float reproduce(){
		init ();
		GameObject node = nodes.getNeartestNode (gameObject.transform.position);
		float food = node.GetComponent<PathNode>().getPlants ();
		float members = node.GetComponent<PathNode>().getPrays ();
		float a = alert ();
		NeuralReturn m = migrate();
		float[] input = { food, members, a, m.score };

		return neural.evaluateAlert ( input );
	}

	public float alert(){
		init ();
		GameObject node = nodes.getNeartestNode (gameObject.transform.position);
		float food = node.GetComponent<PathNode>().getPlants ();
		float members = node.GetComponent<PathNode>().getPrays ();
		GameObject[] neigh = nodes.getNeighbors ( node );
		float enemies = 0;
		for (int i = 0; i < neigh.Length; i++) {
			enemies += neigh[i].GetComponent<PathNode>().getPredators();
		}
		float[] input = { food, members, enemies };
		return neural.evaluateAlert ( input );
	}

	public NeuralReturn migrate(){
		init ();
		GameObject node = nodes.getNeartestNode (gameObject.transform.position);
		float food = node.GetComponent<PathNode>().getPlants ();
		float members = node.GetComponent<PathNode>().getPrays ();
		GameObject[] neigh = nodes.getNeighbors (node);
		GameObject best = null;
		float bestScore = 0;

		for (int i = 0; i < neigh.Length; i++) {
			float neEnemies = neigh[i].GetComponent<PathNode>().getPredators();
			float neFood = neigh[i].GetComponent<PathNode>().getPlants();

			float[] input = {food, members, neFood, neEnemies};
			float score = neural.evaluateMigrate( input );
			if ( best == null ){
				best = neigh[i];
				bestScore = score;
			}else if ( bestScore < score ){
				bestScore = score;
				best = neigh[i];
			}
		}
		NeuralReturn r = new NeuralReturn();
		r.node = best;
		r.score = bestScore;
		return r;
	}





	private void setNodesController(){
		nodes  = GameObject.Find ("Global").GetComponent<NodesController> ();
	}
	
	private void setNeuralNetwork(){
		neural = GameObject.Find ("Global").GetComponent<NeuronalNetwork> ();
	}

	private void init(){
		if( nodes == null )
			setNodesController();
		if( neural == null )
			setNeuralNetwork();
	}
}
