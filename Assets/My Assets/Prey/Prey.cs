using UnityEngine;
using System.Collections;

public class Prey : MonoBehaviour
{
    //public Transform m_Prey;
    public float hp;			//Salud de la entidad
    public int speed; 			//Velocidad de la entidad
    public int comRange;			//Rango de comunicacion
    public double stamina;			//Resistencia (nesesaria para correr etc....)
    public float lifetime;		//Tiempo de vida en segundos 
    public float attack;			//Daño que realiza la entidad
    public float flesh;             //Nutricion aportada a quien se alimente de la entidad 
    public float defense;              //Defensa de la entidad.
    public int state;

    private bool isNeededRun = false;
    private NavMeshAgent nav;
    private GameObject leader;
    public GameObject actualFood;
    private float stoppingDistance;

    //Enum Para los estados del seguidor
    enum States { ChoosingLeader, Searching, Following, Moving, Hunting, Eating, Reproduce, Hiding, Waiting, Reagruping, Die };

    private void initValue()
    {
        //Propiedades fijas
        hp = 100f;
        stamina = 100f;
        comRange = 10;
        isNeededRun = false;

        //Propiedades variables
        flesh = Random.Range(300, 700);
        speed = Random.Range(6, 10);
        lifetime = Random.Range(540, 720); //De 9 a 12 minutos
        attack = Random.Range(6, 12);
        defense = Random.Range(0, 5);

        state = (int)States.ChoosingLeader;
    }

    // Use this for initialization
    void Start()
    {
        //Inicializar rangos
        initValue();

        //Fija los parametros iniciales en torno a la escala
        comRange = (int)(comRange * ((float)transform.localScale.x / 0.3));
        this.stoppingDistance = travelStopDistance();

        //Inicializa el NavMeshAgent
        nav = GetComponent<NavMeshAgent>();
        nav.speed = (float)((speed / 3.0) * (stamina / 100.0));
        if (isNeededRun)
            nav.speed = (float)(speed * (stamina / 100.0));

        //Si no cuenta con eleccion de lider, el es el lider
        if (GetComponent<PreyLeaderChoosing>() == null)
            setLeader(gameObject);
        else
        {
            GetComponent<PreyLeaderChoosing>().choose();
        }
    }


    // Update is called once per frame	
    void Update()
    {
        if (!metabolism())
            return;

        if (isNeededRun)
        {
            nav.speed = (float)(speed * (stamina / 100.0));
            if (state != (int)States.Hiding)
                isNeededRun = false;
        }
        else
            nav.speed = (float)((speed / 3.0) * (stamina / 100.0));

        if (state == (int)States.Hiding)
        {
            if (!isNeededRun)
            {
                PreyNeuronalChoose.NeuralReturn r = GetComponent<PreyNeuronalChoose>().migrate();
                nav.destination = r.node.transform.position;
                isNeededRun = true;
            }
            else
            {
                if (isOnRangeToStop())
                {
                    stop();
                    state = (int)States.ChoosingLeader;
                }
            }

        }
        else if (leader == null && state != (int)States.ChoosingLeader)
        {

            if (GetComponent<PreyLeaderChoosing>() == null)
                setLeader(gameObject);
            else
            {
                GetComponent<PreyLeaderChoosing>().choose();
            }

        }
        else if (state != (int)States.ChoosingLeader)
        {



            //LEADER BEHAVIOR 
            if (isMyLeader(gameObject))
            {

                //senseForSomething();
                if (state == (int)States.Searching)
                {			//Entra en estado para buscar comida
                    ////Debug.Log("Buscando por lugar con comida");
                    behavior_leader_searching();
                    //Debug.Log("LEader searching");

                }
                else if (state == (int)States.Following)
                {	//Entra en estado de viaje en grupo
                    ////Debug.Log("Viajando lugar con comida");
                    behavior_leader_following();
                    //Debug.Log("LEader Follow");

                }
                else if (state == (int)States.Hunting)
                {
                    ////Debug.Log("Cazando comida");
                    behavior_leader_Hunting();
                    //Debug.Log("LEader Hunting");

                }
                else if (state == (int)States.Eating)
                {
                    ////Debug.Log("Comiendo...");
                    behavior_leader_Eating();
                    //Debug.Log("LEader eating");
                }



                //FOLLOWER BEHAVIOR 
            }
            else
            {
                if (state == (int)States.Following)
                {			//Seguir al lider
                    behavior_follower_following();

                }
                else if (state == (int)States.Waiting)
                {		//Esperar a que el lider tome una decicion
                    behavior_follower_waiting();

                }
                else if (state == (int)States.Reagruping)
                {

                }
                else if (state == (int)States.Hunting)
                {
                    ////Debug.Log("Cazando comida");
                    behavior_follower_Hunting();

                }
                else if (state == (int)States.Eating)
                {
                    ////Debug.Log("Comiendo...");
                    behavior_follower_Eating();
                }
            }
        }
    }


    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////// Comportamiento del lider ///////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void behavior_leader_searching()
    {
        //Calcula nueva posicion de la comida
        Vector3 foodPosition = searchForFood();
        if (foodPosition != Vector3.zero)
        {
            state = (int)States.Following;
            order_followMe(gameObject);
            nav.destination = foodPosition;
        }
    }

    void behavior_leader_following()
    {
        if (isOnRangeToStop())
        {
            if (hungry())
            { //TODO: Solo importa si el lider tiene hambre no los demas (cambiar)
                state = (int)States.Hunting;
                order_hunt(gameObject);
                stop();
                actualFood = getBestFood();
                if (actualFood == null)
                {
                    state = (int)States.Searching;
                    return;
                }
                nav.destination = actualFood.transform.position;
            }
            else
            {
                state = (int)States.Searching;
                //order_stop(gameObject);
            }
        }
    }


    void behavior_leader_Hunting()
    {
        if (actualFood == null)
        {
            actualFood = getBestFood();
            if (actualFood == null)
            {
                state = (int)States.Searching;
                //order_stop(gameObject);
            }
        }

        nav.destination = actualFood.transform.position;
        if (distanceFromDestination() <= distanceToBite())
        {
            nav.destination = transform.position;
            transform.LookAt(actualFood.transform);
            if (actualFood.GetComponent<Plant>().hp < 0)
            {
                state = (int)States.Eating;
                this.GetComponent<DinasorsAnimationCorrector>().eating();
            }
            else
            {
                biteEnemy();
            }
        }
    }


    void behavior_leader_Eating()
    {
        if (actualFood == null)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = (int)States.Searching;
            return;
        }

        eatEnemy();
        if (actualFood.GetComponent<Plant>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = (int)States.Searching;
        }

        if (satisfied())
        {
            state = (int)States.Searching;
            this.GetComponent<DinasorsAnimationCorrector>().idle();
        }
    }







    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////// Comportamiento del Seguidor ////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void behavior_follower_following()
    {
        nav.stoppingDistance = travelStopDistance();
        nav.destination = leader.transform.position;

        /*if( leader.GetComponent<Prey>().state != (int)States.Following && leader.GetComponent<Prey>().state != (int)States.Searching){
            if( isOnRangeToStop() ){
                stop();
                state = (int) States.Waiting;
            }
        }*/
    }

    void behavior_follower_waiting()
    {
        if (nav.velocity != Vector3.zero)
        {
            stop();
        }
    }

    void behavior_follower_reagruping()
    {
        if (isOnRangeToStop(3f))
        {
            /*stop();
            state = (int) States.Hunting;
            GameObject[] food = getNearbyFood();
            actualFood = getNeardest(food);
            nav.destination = actualFood.transform.position;*/
        }
    }


    void behavior_follower_Hunting()
    {
        if (actualFood == null)
        {
            actualFood = getBestFood();
            if (actualFood == null)
            {
                state = (int)States.Following;
                nav.stoppingDistance = travelStopDistance();

                //Debug.Log ("No Food, nearby");
                return;
            }
        }

        nav.stoppingDistance = 0;
        //nav.stoppingDistance = distanceToBite();
        nav.destination = actualFood.transform.position;
        if (distanceFromDestination() <= distanceToBite())
        {

            nav.destination = transform.position;
            if (actualFood.GetComponent<Plant>().hp < 0)
            {
                state = (int)States.Eating;
                this.GetComponent<DinasorsAnimationCorrector>().eating();
            }
            else
            {
                biteEnemy();
            }
        }

    }

    void behavior_follower_Eating()
    {
        if (actualFood == null)
        {
            state = (int)States.Following;
            nav.stoppingDistance = travelStopDistance();
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            return;
        }

        eatEnemy();
        if (actualFood.GetComponent<Plant>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = (int)States.Hunting;
        }

        if (satisfied())
        {
            state = (int)States.Following;
            nav.stoppingDistance = travelStopDistance();
            this.GetComponent<DinasorsAnimationCorrector>().idle();
        }
    }







    ///////////////////////////////////////////////////////////////
    ///////////////// Ordenes del lider ///////////////////////////
    ///////////////////////////////////////////////////////////////
    void order_followMe(GameObject l)
    {
        BroadCast("LeaderSaysFollowMe", l);
    }

    void order_stop(GameObject l)
    {
        BroadCast("LeaderSaysStop", l);
    }

    void order_reagrupate(GameObject l)
    {
        BroadCast("LeaderSaysReagrupate", l);
    }


    void order_hunt(GameObject l)
    {
        BroadCast("LeaderSaysHunt", l);
    }

    void order_unsetLeader(GameObject l)
    {
        BroadCast("LeaderSaysUnsetLeader", l);
    }

    void order_panic(GameObject l)
    {
        BroadCast("SaysPanic", l);
    }

    ///////////////////////////////////////////////////////////////
    ///////////////// Reacciones a ordenes del lider //////////////
    ///////////////////////////////////////////////////////////////
    void LeaderSaysFollowMe(GameObject l)
    {
        if (state != (int)States.Following && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = (int)States.Following;
                    order_followMe(l);	//Reply the message to others
                }
            }
        }
    }


    void LeaderSaysStop(GameObject l)
    {
        if (state != (int)States.Waiting && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = (int)States.Waiting;
                    order_stop(l);	//Reply the message to others
                }
            }
        }
    }

    void LeaderSaysReagrupate(GameObject l)
    {
        if (state != (int)States.Reagruping && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = (int)States.Reagruping;
                    nav.destination = Dispersal(l.transform.position);
                    order_reagrupate(l);	//Reply the message to others
                }
            }
        }
    }

    void LeaderSaysHunt(GameObject l)
    {
        if (state != (int)States.Hunting && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = (int)States.Hunting;
                    nav.destination = l.GetComponent<NavMeshAgent>().destination;
                    order_hunt(l);	//Reply the message to others
                }
            }
        }
    }


    void LeaderSaysUnsetLeader(GameObject l)
    {
        if (leader != null && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = (int)States.Hiding;
                    leader = null;
                    order_unsetLeader(l);	//Reply the message to others
                }
            }
        }
    }



    void SaysPanic(GameObject l)
    {
        if (0 < hp)
            state = (int)States.Hiding;

    }


    /*
     * Funcion para enviar a todos los objetos cercanos
     * string Messaage: Funcion que sera ejecutada en los objetos encontrados
     * object obj: Parametros para enviar a esa funcion
     */
    void BroadCast(string message, object obj)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (!isMe(hitColliders[i].gameObject))
            { //No me lo envio a mi
                if (hitColliders[i].GetComponent<Prey>() != null)
                {
                    hitColliders[i].SendMessage(message, (GameObject)obj);
                }
            }
        }
    }



    /*
     * getLeadershipStat
     * Retorna la capacidad de liderazgo de la unidad
     */
    public float getLeadershipStat()
    {
        return
            (this.hp / 100) +
                (this.speed / 3.0f) +
                ((float)this.stamina / 100) +
                ((this.lifetime * 2) / 10000);
    }

    /**
     *	Fijar el objeto lider
     */
    public void setLeader(GameObject l)
    {
        leader = l;
        nav.avoidancePriority = 1;
        state = (int)States.Searching;
    }


    //Retorna si el gameobject enviado es igual a la entidad actual
    bool isMe(GameObject g)
    {
        if (g.GetInstanceID() == gameObject.GetInstanceID())
            return true;
        return false;
    }

    //Retorna si el gameobject enviado es igual al lider de la unidad actual
    bool isMyLeader(GameObject l)
    {
        if (l.GetInstanceID() == leader.GetInstanceID())
            return true;
        return false;
    }




    /**
     *	Regresa la distancia desde la pocion actual a el destino deseado
     */
    float distanceFromDestination()
    {
        return Vector3.Distance(transform.position, nav.destination);
    }


    /*
     *	Regresa una pocicion aleatoria alrededor de la pocicion dada
     */
    Vector3 Dispersal(Vector3 pos)
    {
        pos.x = pos.x + (((float)Random.Range(-50, 50) / 100) * this.comRange);
        pos.z = pos.z + (((float)Random.Range(-50, 50) / 100) * this.comRange);
        return pos;
    }


    float travelStopDistance()
    {
        return comRange * ((float)Random.Range(30, 50) / 100);
    }


    bool isOnRangeToStop()
    {
        return isOnRangeToStop(1f);
    }

    bool isOnRangeToStop(float factor)
    {
        return (distanceFromDestination() < this.stoppingDistance * factor);
    }

    /*
     *	Funcion que detiene al nav Agent
     */
    private void stop()
    {
        nav.destination = transform.position;
    }


    /**
     *	Funciones Biologicas de consumir energia
     */
    private bool metabolism()
    {
        if (lifetime > 0)
        {
            lifetime -= Time.deltaTime;
        }

        float factor = 1f;
        if (isNeededRun)
            factor *= 2f;

        if (state == (int)States.Die)
        {
            if (this.flesh <= 0)
                Destroy(gameObject);
            return false;
        }
        if (0 < this.stamina)
        {
            this.stamina -= Time.deltaTime * factor * (1 / 10f); //Cada 10 segundo gasta uno de stamina
        }
        if (stamina <= 0)
        {
            if (0 < this.hp)
            {
                this.hp -= Time.deltaTime * factor * (1 / 15f); // Cada 15 segundos gasta uno de hp si no tiene stamina
            }
        }
        if (this.hp <= 0 || lifetime < 0)
        {
            die();
            return false;
        }
        return true;
    }


    //Mueve las estadisticas del enemigo y del agente
    void eatEnemy()
    {
        actualFood.GetComponent<Plant>().flesh -= this.attack * Time.deltaTime;
        if (this.stamina < 100f)
            this.stamina += (this.attack * Time.deltaTime) / 10;
        else
            this.hp += (this.attack / Time.deltaTime) / 10; //Time.deltaTime Es el tiempo desde el ultimo frame
    }

    private void die()
    {
        state = (int)States.Die;
        this.GetComponent<DinasorsAnimationCorrector>().die();
        gameObject.GetComponent<PreyNeuronalChoose>().enabled = false;
        if (isMyLeader(gameObject))
        {
            LeaderSaysUnsetLeader(gameObject);
            Destroy(gameObject.transform.Find("leaderLigth").gameObject);
        }

    }




    /**
     * Distancia Optima para atacar al enemigo actual
     */
    float distanceToBite()
    {
        return ((nav.radius) * transform.localScale.x * 1.3f) +
            ((actualFood.GetComponent<MeshRenderer>().bounds.size.x) * 1.3f);
    }


    /**
     * Funcion que inflige daño al enemigo
     */
    void biteEnemy()
    {
        actualFood.GetComponent<Plant>().hp -= this.attack / (1f / Time.deltaTime);
    }




    /**
     **Recive un arreglo de GameObject y regresa el mas cercano a la posicion actual
     */
    GameObject getNeardest(GameObject[] objects)
    {
        if (objects == null)
            return null;
        if (objects.Length == 0)
        {
            ////Debug.Log("GetNeardes: Lista vacia");
            return null;
        }
        GameObject ret = objects[0];
        float distMin, distTemp;
        distMin = Vector3.Distance(transform.position, ret.transform.position);
        for (int i = 1; i < objects.Length; i++)
        {
            distTemp = Vector3.Distance(transform.position, objects[i].transform.position);
            if (distTemp < distMin)
            {
                distMin = distTemp;
                ret = objects[i];
            }
        }
        return ret;
    }


    /**
     *	Obtiene los objetos "COMIDA", cercanos a la posicion del objeto
     */
    GameObject[] getNearbyFood()
    {
        int foodCounter = 0;
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange * 2.5f);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (!isMe(hitColliders[i].gameObject))
            { //No me lo envio a mi
                if (hitColliders[i].tag == "Tree")
                {
                    foodCounter++;
                }
            }
        }
        GameObject[] ret = new GameObject[foodCounter];
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (!isMe(hitColliders[i].gameObject))
            { //No me lo envio a mi
                if (hitColliders[i].tag == "Tree")
                {
                    ret[--foodCounter] = hitColliders[i].gameObject;
                }
            }
        }
        return ret;
    }


    /*
     * Retorna la mejor presa posible
     */
    GameObject getBestFood()
    {
        GameObject[] g = getNearbyFood();
        if (g.Length == 0)
            return null;
        return g[Random.Range(0, g.Length - 1)];
    }


    /*
    *	Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
    */
    private Vector3 searchForFood()
    {
        return GetComponent<PreySearchFood>().searchForFood(transform.position);
    }

    private bool hungry()
    {
        if (stamina < 85f || hp < 100)
            return true;
        return false;
    }

    private bool satisfied()
    {
        if (stamina < 100 || hp < 100)//TODO: Cambiar estos valores.
            return false;
        return true;
    }
}