using UnityEngine;
using System.Collections;

public class Plant : MonoBehaviour {

	public float hp;
	public float flesh;
	private float regenRange;
	private int maxFruit;

	void Start(){
        hp = 0f;
	    flesh = 1f;
		
        //Fija velicidad de cresimiento y la cantidad maxima de alimento que puede tener
		regenRange = Random.Range (5, 20f);
		maxFruit = Random.Range(150, 200);

		//Si el arbol no nace tocando el suelo, revisa si hay algo debajo de el y se pone en ese lugar
		RaycastHit hit;
		if (Physics.Raycast (transform.position, -Vector3.up, out hit )) {
			float distanceToGround = hit.distance;
			Vector3 p = transform.position;
			p.y -= distanceToGround;
			transform.position = p;
		}
		//Inicia corrutina de crecimiento
		StartCoroutine ("treeGrow");

	}

	// Update is called once per frame
	void Update () {
		//Si no tiene Alimento, muere
		if ( this.flesh <= 0 )
			Destroy( gameObject );

		//Si no esta en el maximo, creece
		if (this.flesh <= maxFruit)
            this.flesh += regenRange * Time.deltaTime; //TODO: Cambiar a funcion logistica

	}

	IEnumerator treeGrow(){
		while (true) {
			//Cambia el tamaño del arbol, de acuerdo a la cantidad de alimento que tiene
			float size = flesh / maxFruit;
			gameObject.transform.localScale = new Vector3 (size, size, size);
			yield return new WaitForSeconds( 3 );
		}
	}
}
