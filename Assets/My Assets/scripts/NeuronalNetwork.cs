using UnityEngine;
using System.Collections;

public class NeuronalNetwork : MonoBehaviour {

	float[] vectorAlerta = new float[]{0,1085,719};
	float[] vectorMigrar = new float[]{-1250,295,1000,0};
	float[] vectorReproducir = new float[]{400,-138,-157,-17};



	public void printVector(float[] vector){
		foreach (float v in vector) {
			Debug.Log (v);
			Debug.Log ("; ");
		}
		//Debug.Log ();
	}
	
	public float[] returnArray(float[,] vector, int index){
		float[] newArray = new float[vector.GetLength (1)];
		for (int i=0; i<newArray.Length; i++) {
			newArray[i] = vector[index,i];
		}
		return newArray;
	}
	
	public float[] fillVector(float[] vector, float value){
		for (int i=0; i<vector.Length; i++) {
			vector[i] = value;
		}
		return vector;
	}
	public float[] sumVector(float[] vector1, float[] vector2){
		float[] vectorR = new float[vector1.Length];
		if (vector1.Length == vector2.Length) {
			for (int i=0; i<vector1.Length; i++) {
				vectorR[i] = vector1 [i] + vector2 [i];
			}
		} else {
			Debug.Log("Error en el suma de vectores, vectores de distintos tamaños");
			return vectorR;
		}
		return vectorR;
	}
	
	public float dotProduct(float[] vector1, float[] vector2){
		float result = 0;
		if (vector1.Length == vector2.Length) {
			for (int i=0; i<vector1.Length; i++) {
				result += vector1 [i] * vector2 [i];
			}
		} else {
			Debug.Log("Error en el producto punto, vectores de distintos tamaños");
			return 0;
		}
		return result;
	}
	
	public float[] multipleVectorXScalar(float[] vector1, float escalar){
		float[] vectorR = new float[vector1.Length];
		for (int i=0; i<vector1.Length; i++) {
			vectorR[i] = vector1 [i] * escalar;
		}
		return vectorR;
	}
	
	public int charValue(float value){
		if (value > 0)
			return 1;
		else
			return -1;
	}

	public float evaluateAlert(float[] vector){	
		return evaluate (this.vectorAlerta, vector);
	}

	public float evaluateMigrate(float[] vector){	
		return evaluate (this.vectorMigrar, vector);
	}

	public float evaluateReproduce(float[] vector){	
		return evaluate (this.vectorReproducir, vector);
	}

	public float evaluate(float[] vectorW, float[] vector){ //for usage		
		if (charValue (dotProduct (vectorW, vector)) > 0 )
			return 1f;
		else
			return -1f;
	}

	public float evaluate(float[] vectorW, float[] vector, int result){ // for trainning		
		if (charValue (dotProduct (vectorW, vector)) == result)
			return 1f;
		else
			return -1f;
	}
	
	public float[] trainning(float[,] vector, int[] results){
		float[] vectorW = new float[vector.GetLength (1)];
		vectorW = fillVector (vectorW, 0);
		float[] currentArray;
		float y = 0; //Scalar Factor +1 or -1
		bool vectorWChange = false;
		int counter = 0;
		while (true) {
			for(int i=0; i<6; i++){
				currentArray=returnArray(vector,i);
				if(evaluate(vectorW,currentArray,results[i] ) == -1f) {
					y = results[i];
					vectorW=sumVector(vectorW,multipleVectorXScalar(currentArray,y));
					//Debug.Log("new vectorW: ");
					//printVector(vectorW);
					vectorWChange = true;
				}
				//Thread.Sleep(100);
			}
			if(!vectorWChange)
				break;
			else
				vectorWChange=false;
			counter++;
			if(counter>vector.GetLength(0)*10){ // no linear composition
				Debug.Log("Alerta, clasificacion no lineal");
				break;
			}
		}
		//printVector (vectorW);
		return vectorW;
		
	}
}
