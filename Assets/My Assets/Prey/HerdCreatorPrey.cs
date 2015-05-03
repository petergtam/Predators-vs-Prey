//------------------------------------------------------------------------------
// <auto-generated>
//     Este código fue generado por una herramienta.
//     Versión de runtime:4.0.30319.17929
//
//     Los cambios en este archivo podrían causar un comportamiento incorrecto y se perderán si
//     se vuelve a generar el código.
// </auto-generated>
//------------------------------------------------------------------------------
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Assets.My_Assets
{
	public class HerdCreatorPrey : MonoBehaviour
	{
		private int K;
		private double EPS = 0.0001;
		private float timeToGo;
		Prey[] elements;
		public HerdCreatorPrey()
		{
			K = 5;
			timeToGo = 0.0F;
		}
		public HerdCreatorPrey( int k){
			K = k;
			timeToGo = 0.0F;
		}
		private float distance(Vector3 p1, Vector3 p2 ){
			return (p1.x - p2.x) * (p1.x - p2.x)  
				+ (p1.z - p2.z) * (p1.z - p2.z);
		}
		void Start(){
			Debug.Log("Creating Prey Herds");
			this.createHerds();
			Debug.Log ("Done Prey Herds");
		}

		private void fillHerd(Prey p, int herd,Dictionary<int,int> preyVisited){
			if (preyVisited.ContainsKey(p.GetInstanceID()))
				return;
			preyVisited.Add(p.GetInstanceID(),herd);
			
			//Por cada objeto encontrado revisa si es un arbol y añada su cantidad de comida
			for (int i = 0; i < elements.Length; i++) {
				Prey vecino = elements[i];
				if( distance( p.transform.position, vecino.transform.position ) <= p.comRange * p.comRange ){ 
					fillHerd( vecino, herd, preyVisited);
				}
			}
		}

		
		void Update(){
			if (Time.fixedTime < timeToGo) {
				return;
			}
			timeToGo = Time.fixedTime + 5.0F;
			//Debug.Log ("Iniciando actualizacion de herds - Prey");
			Dictionary<int,int> preyVisited = new Dictionary<int, int> ();
			int[] curCluster = new int[elements.Count()];
			elements = GameObject.FindObjectsOfType<Prey>();
			int nElements = elements.Length;
			int nHerd = 0;
			for (int i=0; i<nElements; i++) {
				Prey curPrey = elements[i];
				if( !preyVisited.ContainsKey(curPrey.GetInstanceID())){
					fillHerd(curPrey,nHerd++,preyVisited);
				}
			}
			//Debug.Log ("Flood Fill terminado");
			nHerd++;
			//Debug.Log ("Creando listas de herds");

			 List<List<Prey>> herds = new List<List<Prey>> ();
			for (int i=0; i<nHerd; i++) {
				herds.Add( new List<Prey>() );
			}
			for (int i=0; i<nElements; i++) {
				int herdId = preyVisited[elements[i].GetInstanceID()];
				herds[herdId].Add(elements[i]);
				elements[i].herdid  = herdId;
			}
			//Debug.Log ("Creadas listas de herds");
			//Debug.Log ("Iniciando seleccion de lideres y asignacion de manadas");
			for( int i=0;i<nHerd;i++){
				if(herds[i].Count == 0 ) continue;
				Prey element = herds[i][0];
				List<GameObject> herdList = new List<GameObject>();
				for( int j=0;j<herds[i].Count;j++){
					herdList.Add(herds[i][j].gameObject);
				}
				for( int j=0;j<herds[i].Count;j++){
					herds[i][j].herd = herdList;
				}
				//element.getNewLeader(herds[i]);
			}
			//Debug.Log ("Herds creadas y todo listo");
		}


		public void createHerds(){
			elements = GameObject.FindObjectsOfType<Prey>();
			int nElements = elements.Count ();
			int[] curCluster = new int[elements.Count()];
			for (int i=0; i<nElements; i++) {
				curCluster[i] = i; // We assign each element to it's cluster. Making sure this way to at least
									//run 1 time the assignment step
			}
			// Make sure we can create the K clusters,
			// If we have less than K elements then we only can create
			// Number of elements clusters, each cluster with 1 element.
			if (nElements < K) {
				K = nElements;
			}

			// Create K Herds, and K objects with the points where elements live
			// Then populate the K herds using K Means.
			List<List<Prey>> best = new List<List<Prey>> ();
			float bestMSE = 100000000.0F;
			List<List<Prey>> herds = new List<List<Prey>> ();
			List<Vector3> means = new List<Vector3> ();
			float minX=1000000.0F, maxX=0.0F, minY=1000000.0F, maxY=0.0F;
			for (int i=0; i<nElements; i++) {
				Vector3 pos = elements[i].transform.position;
				if( pos.x < minX ) minX = pos.x;
				if( pos.x > maxX ) maxX = pos.x;
				if( pos.z < minY ) minY = pos.z;
				if( pos.z > maxY ) maxY = pos.z;
			}
			for(int T=0;T<100;T++){

				herds = new List<List<Prey>> ();
				means = new List<Vector3> ();
				curCluster = new int[elements.Count()];
				for (int i=0; i<nElements; i++) {
					curCluster[i] = i; // We assign each element to it's cluster. Making sure this way to at least
					//run 1 time the assignment step
				}
				for (int i=0; i<K; i++) {
					herds.Add( new List<Prey>() );
					means.Add(new Vector3(
						(float) Random.Range ((int)minX,(int)maxX),
						15.0F,
						(float) Random.Range ((int)minY,(int)maxY)
								) 
					          );
				}
				float MSE = 0.0F;
				float MSEPREV = 1.0F;
				int epoch = 0;
				while ( Mathf.Abs (MSE - MSEPREV ) > EPS && epoch < 100000) { //Each time we find a change on the assignment
					MSEPREV = MSE;
					MSE = 0.0F;
					epoch++;
					//Empty the clusters
					for (int i=0; i<K; i++) {
						herds [i] = new List<Prey> ();
					}
					//We iterate to create the K clusters
					for (int i=0; i<nElements; i++) { //For each element
						int bestCluster = 0; // Keep the bestCluster
						for (int j=1; j<K; j++) { // For each cluster
							if (distance (elements [i].transform.position, means [j]) < distance (elements [i].transform.position, means [bestCluster])) { //bestCluster
								bestCluster = j;
							}
						}
						if (curCluster [i] != bestCluster) {
							curCluster [i] = bestCluster; // Assign Cluster maps ;)
							herds [bestCluster].Add (elements [i]); // Add the element to the cluster.
						}else{
							herds[curCluster[i]].Add (elements[i]);
						}
					}
					// Now update the clusters
					for (int i=0; i<K; i++) {
						float newX = 0;
						float newY = 0;
						float newZ = 0;
						for (int j=0; j<herds[i].Count; j++) {
							newX += herds [i] [j].transform.position.x;
							newY += herds [i] [j].transform.position.y;
							newZ += herds [i] [j].transform.position.z;
						}
						int tot = herds [i].Count == 0 ? 1 : herds [i].Count; //Avoid division by 0;
						
						means[i] = new Vector3(newX/(float)tot, means[i].y, newZ / (float)tot);
						for( int j=0;j<herds[i].Count;j++){
							MSE += distance(herds[i][j].transform.position, means[i]);
						}
					}
				}
				if( MSE < bestMSE ){
					bestMSE = MSE;
					best = herds;
					for(int i=0;i<nElements;i++){
						elements[i].herdid = curCluster[i];
					}
				}
			}
			herds = best;
			//Finally we iterate each of the final clusters and select the leader of the elements.
			for( int i=0;i<K;i++){
				if(herds[i].Count == 0 ) continue;
				Prey element = herds[i][0];
				List<GameObject> herdList = new List<GameObject>();
				for( int j=0;j<herds[i].Count;j++){
					herdList.Add(herds[i][j].gameObject);
				}
				for( int j=0;j<herds[i].Count;j++){
					herds[i][j].herd = herdList;
				}
				element.getNewLeader(herds[i]);
			}
		}
	}
}

