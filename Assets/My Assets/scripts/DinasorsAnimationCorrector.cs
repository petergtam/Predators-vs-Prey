using UnityEngine;
using System.Collections;

public class DinasorsAnimationCorrector : MonoBehaviour {

	/*Este archivo es creado para que la velocidad de la animacion de los dinosaurios sea cuerente con su velocidad de movimiento */

	/*
	 * Tyranosaurio
	 * Velocidad de movimiento (animation.magnitud) en animacion x1
	 * WALK = 1.5
	 * RUN = 4
	 */


	private float walk = 0;
	private float run = 0;
	public string type = "";
	private int state;
	private enum States {idle, running, walking, die, attack};


	//Inicializacion:
	//Fija la velocidad de la animacion de acduerdo al modelo
	void Start () {
		if (type == "") {
			type = gameObject.name;
		}

		if (type == "tiranosaurus") {
			walk = 1.5f;
			run = 4f;
		}

		if (type == "velociraptor") {
			walk = 1f;
			run = 4f;
		}

		if (type == "ankylosaurus") {
			walk = .31f;
			run = 1.3f;
		}

		if (type == "stegosaurus") {
			walk = .55f;
			run = 1.5f;
		}

	
	}
	
	// Update is called once per frame
	void Update () {
		if (state == (int)States.die) {		//Si esta muerto, no hacer nada
			return;
		}
		if (state == (int)States.attack) {
			return;
		}

		if (walk != 0) {
			//Obtiene la velocidad de movimiento actual 
			Vector3 velocity = GetComponent<NavMeshAgent> ().velocity;
			//Se esta moviendo
			if (isMoving (velocity)) {
					
					//Si esta corriendo
					if (run <= velocity.magnitude) {
							float speedRate = velocity.magnitude / run * (0.3f / transform.localScale.x);
							if( state != (int) States.running ){
								animation.CrossFade ("run");
								state = (int) States.running;
							}
							animation ["run"].speed = speedRate;
							
					//Si esta caminando
					} else {
							float speedRate = velocity.magnitude / walk * (0.3f / transform.localScale.x);
							if( state != (int) States.walking ){
								animation.CrossFade ("walk");
								state = (int) States.walking;
							}
							animation ["walk"].speed = speedRate;
					}
			}else{
				if( state != (int) States.idle ){
					animation.CrossFade ("idle");
					animation["walk"].speed = 1;
					state = (int) States.idle;
				}
			}	
		}
	}

	bool isMoving(Vector3 velocity){
		if (velocity == Vector3.zero)
			return false;
		return true;
	}


	/*
	 * 	Funcion que actiava la animacion de muerte
	 */
	public void die(){
		state = (int)States.die;
		NavMeshAgent nav = GetComponent<NavMeshAgent>();

		//Detiene el movimiento del agente
		nav.Stop (true);
		nav.destination = transform.position;
		nav.velocity = Vector3.zero;

		//Fija la animacion
		animation["die"].wrapMode = WrapMode.Once;
		animation.CrossFade ("die");
	}

	public void attack(){
		NavMeshAgent nav = GetComponent<NavMeshAgent>();
		state = (int)States.attack;
		/*nav.Stop (true);
		nav.destination = transform.position;
		nav.velocity = Vector3.zero;*/
		if (type == "ankylosaurus") {
				animation ["swing2"].wrapMode = WrapMode.Loop;
				animation.CrossFade ("swing2");
		} else if (type == "stegosaurus") {
				animation ["swing"].wrapMode = WrapMode.Loop;
				animation.CrossFade ("swing");
		} else {
			animation["bite"].wrapMode = WrapMode.Loop;
			animation.CrossFade ("bite");
		}
	}
	
	public void idle(){
		animation["idle"].wrapMode = WrapMode.Loop;
		animation.CrossFade ("idle");
		state = (int)States.idle;
	}
	
	
	public void eating(){
		if (type == "ankylosaurus" || type == "stegosaurus") {
			animation["idle"].wrapMode = WrapMode.Loop;
			animation.CrossFade ("idle");
		}else {
			animation["bite"].wrapMode = WrapMode.Loop;
			animation.CrossFade ("bite");
		}
	}
}
