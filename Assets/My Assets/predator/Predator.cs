using UnityEngine;	
using System.Collections;

public class Predator : MonoBehaviour {
	//public Transform m_Prey;
	public float hp 		= 100f;			//Salud de la entidad
	public int np 			= 10;			//Nutricion aportada a quien se alimente de la entidad
	public int speed 		= 8;			//Velocidad de la entidad
	public int comRange		= 10;			//Rango de comunicacion
	public double stamina	= 100f;			//Resistencia (nesesaria para correr etc....)
	public float lifetime	= 10000f;		//Tiempo de vida
	public float attack		= 10f;			//Daño que realiza la entidad
	public float flesh 		= 200f;
	public int state;
	
	private bool isNeededRun = false;
	private NavMeshAgent nav;
	private GameObject leader;
	public GameObject actualFood;
	private float stoppingDistance;
	
	//Enum Para los estados del seguidor
	enum States { ChoosingLeader, Searching, Following , Moving, Hunting, Eating, Reproduce, Waiting, Reagruping, Die };
	
	
	
	// Use this for initialization
	void Start () {
		state = (int)States.ChoosingLeader;
		
		//Fija los parametros iniciales en torno a la escala
		comRange = (int) ( comRange * ((float)transform.localScale.x/0.3));
		this.stoppingDistance = travelStopDistance ();
		
		//Inicializa el NavMeshAgent
		nav = GetComponent<NavMeshAgent> ();
		
		nav.speed = (float)speed/3;
		if(isNeededRun)
			nav.speed = (float)speed*3;
		
		//Si no cuenta con eleccion de lider, el es el lider
		if (GetComponent<PredatorLeaderChoosing> () == null)
			setLeader (gameObject);
		else {
			GetComponent<PredatorLeaderChoosing> ().choose ();
		}
	}
	
	
	// Update is called once per frame	
	void Update () {
		if (!metabolism ()) 
			return;


		if (leader == null && state != (int)States.ChoosingLeader) {
	
			if (GetComponent<PreyLeaderChoosing> () == null)
					setLeader (gameObject);
			else {
					GetComponent<PreyLeaderChoosing> ().choose ();
			}
	
		} else if (state != (int)States.ChoosingLeader) {



			//LEADER BEHAVIOR 
			if ( isMyLeader(gameObject) ) {
				
				//senseForSomething();
				if (state == (int)States.Searching) {			//Entra en estado para buscar comida
					////Debug.Log("Buscando por lugar con comida");
					behavior_leader_searching();
					//Debug.Log("LEader searching");
					
				} else if ( state == (int)States.Following) {	//Entra en estado de viaje en grupo
					////Debug.Log("Viajando lugar con comida");
					behavior_leader_following();
					//Debug.Log("LEader Follow");
					
				} else if ( state == (int) States.Hunting ) {
					////Debug.Log("Cazando comida");
					behavior_leader_Hunting();
					//Debug.Log("LEader Hunting");
					
				} else if ( state == (int) States.Eating ) {
					////Debug.Log("Comiendo...");
					behavior_leader_Eating();
					//Debug.Log("LEader eating");
				}
				
				
				
				//FOLLOWER BEHAVIOR 
			}else{
				if ( state == (int) States.Following ){			//Seguir al lider
					behavior_follower_following();
					
				}else if ( state == (int) States.Waiting ){		//Esperar a que el lider tome una decicion
					behavior_follower_waiting();
					
				}else if ( state == (int) States.Reagruping ){	
					
				}else if ( state == (int) States.Hunting ) {
					////Debug.Log("Cazando comida");
					behavior_follower_Hunting();
					
				} else if ( state == (int) States.Eating ) {
					////Debug.Log("Comiendo...");
					behavior_follower_Eating();
				}
			}
		}
	}
	
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////// Comportamiento del lider ///////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void behavior_leader_searching(){
		//Calcula nueva posicion de la comida
		Vector3 foodPosition = 	searchForFood();
		if (foodPosition != Vector3.zero) {
			state = (int)States.Following;
			nav.destination = foodPosition;
			order_followMe (gameObject);
		}
	}
	
	void behavior_leader_following(){
		if( isOnRangeToStop() ){
			if ( hungry() ){
				state = (int) States.Hunting;
				order_hunt(gameObject);
				stop();
				actualFood = getBestFood();
				if ( actualFood == null ){
					state = (int)States.Searching;
					return;
				}
				nav.destination = actualFood.transform.position;
			}else{
				//Debug.Log("Descanzar");
				state = (int)States.Searching;
			}
		}
	}
	
	
	void behavior_leader_Hunting(){
		if ( actualFood == null ){
			actualFood = getBestFood();
			if ( actualFood == null ){
				state = (int)States.Searching;
				//order_stop(gameObject);
			}
		}
		
		nav.destination = actualFood.transform.position;
		if( distanceFromDestination() <= distanceToBite() ){
			nav.destination = transform.position;
			transform.LookAt (actualFood.transform);
			if (actualFood.GetComponent<Prey> ().hp < 0) {
				state = (int)States.Eating;
				this.GetComponent<DinasorsAnimationCorrector> ().eating();
			}else {
				biteEnemy();
			}
		}
	}
	
	
	void behavior_leader_Eating(){
		if ( actualFood == null ){
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			state = (int)States.Searching;
			return;
		}
		
		eatEnemy();
		if ( actualFood.GetComponent<Prey>().flesh < 0){
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			state = (int) States.Searching;
		}
		
		if ( satisfied() ) {
			state = (int)States.Searching;
			this.GetComponent<DinasorsAnimationCorrector>().idle();
		}
	}	
	
	
	
	
	
	
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	///////////////// Comportamiento del Seguidor ////////////////////////////////////////////////////////////////////////////////
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void behavior_follower_following(){
		nav.stoppingDistance = travelStopDistance ();
		nav.destination = leader.transform.position;
		/*if( leader.GetComponent<Predator>().state != (int)States.Following ){
			if( isOnRangeToStop(1.5f) ){
				stop();
				state = (int) States.Waiting;
			}
		}*/
	}
	
	void behavior_follower_waiting(){
		if (nav.velocity != Vector3.zero ){
			stop();
		}
	}
	
	void behavior_follower_reagruping(){
		if( isOnRangeToStop(3f) ){
			/*stop();
			state = (int) States.Hunting;
			GameObject[] food = getNearbyFood();
			actualFood = getNeardest(food);
			nav.destination = actualFood.transform.position;*/
		}
	}
	
	
	void behavior_follower_Hunting(){
		if (actualFood == null) {
			actualFood = getBestFood ();
			if (actualFood == null) {
				state = (int)States.Following;
				nav.stoppingDistance = travelStopDistance();
				////Debug.Log ("No Food, nearby");
				return;
			}
		}
		
		nav.stoppingDistance = 0;
		//nav.stoppingDistance = distanceToBite();
		nav.destination = actualFood.transform.position;
		if (distanceFromDestination () <= distanceToBite ()) {
			
			nav.destination = transform.position;
			if (actualFood.GetComponent<Prey> ().hp < 0) {
				state = (int)States.Eating;
				this.GetComponent<DinasorsAnimationCorrector> ().eating ();
			} else {
				biteEnemy ();
			}
		}
	}
	
	void behavior_follower_Eating(){
		if ( actualFood == null ){
			state = (int)States.Following;
			nav.stoppingDistance = travelStopDistance();
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			return;
		}
		
		eatEnemy();
		if ( actualFood.GetComponent<Prey>().flesh < 0){
			this.GetComponent<DinasorsAnimationCorrector>().idle();
			state = (int) States.Hunting;
		}
		
		if ( satisfied() ) {
			state = (int)States.Following;
			nav.stoppingDistance = travelStopDistance();
			this.GetComponent<DinasorsAnimationCorrector>().idle();
		}
	}
	
	
	
	
	
	
	
	///////////////////////////////////////////////////////////////
	///////////////// Ordenes del lider ///////////////////////////
	///////////////////////////////////////////////////////////////
	void order_followMe( GameObject l ){
		BroadCast ("LeaderSaysFollowMe", l);
	}
	
	void order_stop( GameObject l ){
		BroadCast ("LeaderSaysStop", l);
	}
	
	void order_reagrupate( GameObject l ){
		BroadCast ("LeaderSaysReagrupate", l);
	}
	
	
	void order_hunt( GameObject l ){
		BroadCast ("LeaderSaysHunt", l);
	}
	
	
	
	
	///////////////////////////////////////////////////////////////
	///////////////// Reacciones a ordenes del lider //////////////
	///////////////////////////////////////////////////////////////
	void LeaderSaysFollowMe( GameObject l ){
		if (state != (int)States.Following && 0 < hp ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = (int)States.Following;
					order_followMe(l);	//Reply the message to others
				}
			}
		}
	}
	
	
	void LeaderSaysStop( GameObject l ){
		if (state != (int)States.Waiting  && 0 < hp  ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = (int)States.Waiting;
					order_stop(l);	//Reply the message to others
				}
			}
		}
	}
	
	void LeaderSaysReagrupate( GameObject l ){
		if (state != (int)States.Reagruping && 0 < hp ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = (int)States.Reagruping;
					nav.destination = Dispersal( l.transform.position );
					order_reagrupate(l);	//Reply the message to others
				}
			}
		}
	}
	
	void LeaderSaysHunt( GameObject l ){
		if (state != (int)States.Hunting && 0 < hp ) {
			if ( isMyLeader(l) ) {
				if( !isMe(leader) ){
					state = (int)States.Hunting;
					nav.destination = l.GetComponent<NavMeshAgent>().destination;
					order_hunt(l);	//Reply the message to others
				}
			}
		}
	}
	
	/*
	 * Funcion para enviar a todos los objetos cercanos
	 * string Messaage: Funcion que sera ejecutada en los objetos encontrados
	 * object obj: Parametros para enviar a esa funcion
	 */
	void BroadCast(string message, object obj){
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
		for (int i = 0; i < hitColliders.Length; i++) {
			if( !isMe( hitColliders[i].gameObject ) ){ //No me lo envio a mi
				if (hitColliders [i].GetComponent<Predator> () != null) {
					hitColliders[i].SendMessage(message, (GameObject) obj);	
				}
			}
		}
	}
	
	
	
	/*
	 * getLeadershipStat
	 * Retorna la capacidad de liderazgo de la unidad
	 */
	public float getLeadershipStat(){
		return 
			(this.hp / 100) + 
				(this.speed / 3) + 
				((float)this.stamina / 100) + 
				((this.lifetime * 2) / 10000);
	}
	
	/**
	 *	Fijar el objeto lider
	 */
	public void setLeader(GameObject l ){
		leader = l;
		nav.avoidancePriority = 1;
		state = (int) States.Searching;
	}
	
	
	//Retorna si el gameobject enviado es igual a la entidad actual
	bool isMe(GameObject g){
		if ( g.GetInstanceID() == gameObject.GetInstanceID() )
			return true;
		return false;
	}
	
	//Retorna si el gameobject enviado es igual al lider de la unidad actual
	bool isMyLeader(GameObject l){
		if ( l.GetInstanceID () == leader.GetInstanceID () )
			return true;
		return false;
	}
	
	
	
	
	/**
	 *	Regresa la distancia desde la pocion actual a el destino deseado
	 */
	float distanceFromDestination(){
		return Vector3.Distance(transform.position, nav.destination);
	}
	
	
	/*
	 *	Regresa una pocicion aleatoria alrededor de la pocicion dada
	 */
	Vector3 Dispersal(Vector3 pos){
		pos.x = pos.x + (((float)Random.Range (-50, 50) / 100) * this.comRange);
		pos.z = pos.z + (((float)Random.Range (-50, 50) / 100) * this.comRange);
		return pos;
	}
	
	
	float travelStopDistance(){
		return comRange * ((float)Random.Range (30, 50) / 100);
	}
	
	
	bool isOnRangeToStop(){
		return isOnRangeToStop (1f);
	}
	
	bool isOnRangeToStop(float factor){
		return ( distanceFromDestination() < this.stoppingDistance*factor );
	}
	
	/*
	 *	Funcion que detiene al nav Agent
	 */
	private void stop(){
		nav.destination = transform.position;
	}
	
	
	/**
	 *	Funciones Biologicas de consumir energia
	 */
	private bool metabolism(){
		if ( state == (int)States.Die ){
			if ( this.flesh <= 0 )
				Destroy( gameObject );
			return false;
		}
		if (0 < this.stamina) {	
			this.stamina -= 0.000001;			
		}
		if (stamina <= 0) {
			if ( 0 < this.hp ) {	
				this.hp -= 0.001f;
			}
		}
		if( this.hp <= 0){
			die ();
			return false;
		}
		return true;
	}
	
	
	//Mueve las estadisticas del enemigo y del agente
	void eatEnemy(){
		actualFood.GetComponent<Prey> ().flesh -= ((float)this.attack / (1f / Time.deltaTime))*0.6f;
		if ( this.stamina < 100f )
			this.stamina += ((float)this.attack / (1f / Time.deltaTime));
		else 
			this.hp += ((float)this.attack / (1f / Time.deltaTime));
	}
	
	private void die(){
		state = (int)States.Die;
		this.GetComponent<DinasorsAnimationCorrector>().die();
		gameObject.GetComponent<PredatorLeaderChoosing> ().enabled = false;
		if (isMyLeader (gameObject)) {
			//LeaderSaysUnsetLeader (gameObject);
			Destroy( gameObject.transform.Find("leaderLigth").gameObject );
		}
	}
	
	
	
	
	/**
	 * Distancia Optima para atacar al enemigo actual
	 */
	float distanceToBite(){
		return ((nav.radius) * transform.localScale.x * 1.3f) +
			((actualFood.GetComponent<NavMeshAgent>().radius) * actualFood.transform.localScale.x * 1.3f);
	}
	
	
	/**
	 * Funcion que inflige daño al enemigo
	 */
	void biteEnemy(){
		actualFood.GetComponent<Prey> ().hp -= (this.attack / (1f / Time.deltaTime));
	}
	
	
	
	
	/**
	 **Recive un arreglo de GameObject y regresa el mas cercano a la posicion actual
	 */
	GameObject getNeardest( GameObject[] objects ) {
		if (objects == null)
			return null;
		if (objects.Length == 0) {
			////Debug.Log("GetNeardes: Lista vacia");
			return null;
		}
		GameObject ret = objects [0];
		float distMin, distTemp;
		distMin = Vector3.Distance (transform.position, ret.transform.position );
		for (int i = 1; i < objects.Length; i++) {
			distTemp = Vector3.Distance (transform.position, objects[i].transform.position);
			if ( distTemp  < distMin ){
				distMin = distTemp;
				ret = objects[i];
			}
		}
		return ret;
	}
	
	
	/**
	 *	Obtiene los objetos "COMIDA", cercanos a la posicion del objeto
	 */
	GameObject[] getNearbyFood(){
		int foodCounter = 0;
		Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange*2.5f);
		for (int i = 0; i < hitColliders.Length; i++) {
			if( !isMe( hitColliders[i].gameObject ) ){ //No me lo envio a mi
				if (hitColliders [i].GetComponent<Prey> () != null ){
					foodCounter++;
				}
			}
		}
		GameObject[] ret = new GameObject[foodCounter];
		for (int i = 0; i < hitColliders.Length; i++) {
			if( !isMe( hitColliders[i].gameObject ) ){ //No me lo envio a mi
				if (hitColliders [i].GetComponent<Prey> () != null ){
					ret[--foodCounter] = hitColliders[i].gameObject;
				}
			}
		}
		return ret;
	}
	
	
	/*
	 * Retorna la mejor presa posible
	 */
	GameObject getBestFood(){
		GameObject[] g = getNearbyFood();
		if (g.Length == 0)
			return null;
		for (int i = 0; i < g.Length; i++) {
			if( g[i].GetComponent<Prey>().hp <= 0 )
				return g[i];
		}
		//return g [Random.Range (0, g.Length - 1)];
		return getNeardest (g);
	}
	
	
	/*
	*	Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
	*/
	private Vector3 searchForFood(){
		return GetComponent<PredatorSearchFood> ().searchForFood (transform.position);
	}
	
	private bool hungry(){
		if (stamina < 120f || hp < 100)
			return true;
		return false;
	}
	
	private bool satisfied(){
		if (stamina < 150 || hp < 100)
			return false;
		return true;
	}
}