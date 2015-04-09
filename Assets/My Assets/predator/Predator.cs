﻿using System;
using UnityEngine;
using Assets.My_Assets;
using Random = UnityEngine.Random;

public class Predator : Agent
{
    public GameObject actualFood;
    protected override void InitValue()
    {
        //Propiedades fijas
        hp = 100f;
        stamina = 100f;
        comRange = 10;
        lifetime = 0;
        isNeededRun = false;

        //Propiedades variables
        flesh = Random.Range(100, 300);
        speed = Random.Range(6, 10);
        maxLifeTime = Random.Range(540, 720);
        attack = Random.Range(6, 16);

        state = States.ChoosingLeader;
    }

    // Use this for initialization
    void Start()
    {
        InitValue(); 

        //Fija los parametros iniciales en torno a la escala
        comRange = (int)(comRange * ((float)transform.localScale.x / 0.3));

        //Inicializa el NavMeshAgent
        nav = GetComponent<NavMeshAgent>();

        nav.speed = (float)((speed / 3.0) * ((stamina < 50 ? 50 : stamina) / 100.0));
        if (isNeededRun)
            nav.speed = (float)(speed * ((stamina < 50 ? 50 : stamina) / 100.0));

        //Si no cuenta con eleccion de lider, el es el lider
        if (GetComponent<PredatorLeaderChoosing>() == null)
            setLeader(gameObject);
        else
        {
            GetComponent<PredatorLeaderChoosing>().choose();
        }
    }


    // Update is called once per frame	
    void Update()
    {
        if (!Metabolism())
            return;

        if (isNeededRun)
        {//TODO: Nunca cambia entre correr y caminar las presas
            nav.speed = (float)(speed * ( (stamina < 50 ? 50: stamina)  / 100.0));
        }
        else
            nav.speed = (float)((speed / 3.0) * ((stamina < 50 ? 50 : stamina) / 100.0));

        if (leader == null && state != States.ChoosingLeader)
        {

            if (GetComponent<PreyLeaderChoosing>() == null)
                setLeader(gameObject);
            else
            {
                GetComponent<PreyLeaderChoosing>().choose();
            }

        }
        else if (state != States.ChoosingLeader)
        {
            //LEADER BEHAVIOR 
            if (isMyLeader(gameObject))
            {

                //senseForSomething();
                if (state == States.Searching)
                {			//Entra en estado para buscar comida
                    ////Debug.Log("Buscando por lugar con comida");
                    behavior_leader_searching();
                    //Debug.Log("LEader searching");

                }
                else if (state == States.Following)
                {	//Entra en estado de viaje en grupo
                    ////Debug.Log("Viajando lugar con comida");
                    behavior_leader_following();
                    //Debug.Log("LEader Follow");

                }
                else if (state == States.Hunting)
                {
                    ////Debug.Log("Cazando comida");
                    behavior_leader_Hunting();
                    //Debug.Log("LEader Hunting");

                }
                else if (state == States.Eating)
                {
                    ////Debug.Log("Comiendo...");
                    behavior_leader_Eating();
                    //Debug.Log("LEader eating");
                }
                //FOLLOWER BEHAVIOR 
            }
            else
            {
                if (state == States.Following)
                {			//Seguir al lider
                    behavior_follower_following();

                }
                else if (state == States.Waiting)
                {		//Esperar a que el lider tome una decicion
                    behavior_follower_waiting();

                }
                else if (state == States.Reagruping)
                {

                }
                else if (state == States.Hunting)
                {
                    ////Debug.Log("Cazando comida");
                    behavior_follower_Hunting();

                }
                else if (state == States.Eating)
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
            state = States.Following;
            nav.destination = foodPosition;
            order_followMe(gameObject);
        }
    }

    void behavior_leader_following()
    {
        if (isOnRangeToStop())
        {
            if (hungry())
            {
                state = States.Hunting;
                order_hunt(gameObject);
                Stop();
                actualFood = getBestFood();
                if (actualFood == null)
                {
                    state = States.Searching;
                    return;
                }
                nav.destination = actualFood.transform.position;
            }
            else
            {
                //Debug.Log("Descanzar");
                state = States.Searching;
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
                state = States.Searching;
                //order_stop(gameObject);
            }
        }

        nav.destination = actualFood.transform.position;
        if (DistanceFromDestination() <= distanceToBite())
        {
            nav.destination = transform.position;
            transform.LookAt(actualFood.transform);
            if (actualFood.GetComponent<Prey>().hp < 0)
            {
                state = States.Eating;
                this.GetComponent<DinasorsAnimationCorrector>().eating();
            }
            else
            {
                BiteEnemy();
            }
        }
    }

    void behavior_leader_Eating()
    {
        if (actualFood == null)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Searching;
            return;
        }

        EatEnemy();
        if (actualFood.GetComponent<Prey>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Searching;
        }

        if (satisfied())
        {
            state = States.Searching;
            this.GetComponent<DinasorsAnimationCorrector>().idle();
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    ///////////////// Comportamiento del Seguidor ////////////////////////////////////////////////////////////////////////////////
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

    void behavior_follower_following()
    {
        nav.destination = leader.transform.position;
        /*if( leader.GetComponent<Predator>().state != States.Following ){
            if( isOnRangeToStop(1.5f) ){
                stop();
                state = (int) States.Waiting;
            }
        }*/
    }

    void behavior_follower_waiting()
    {
        if (nav.velocity != Vector3.zero)
        {
            Stop();
        }
    }

    void behavior_follower_reagruping()
    {
        if (IsOnRangeToStop(3f))
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
                state = States.Following;
                ////Debug.Log ("No Food, nearby");
                return;
            }
        }

        nav.stoppingDistance = 0;
        //nav.stoppingDistance = distanceToBite();
        nav.destination = actualFood.transform.position;
        if (DistanceFromDestination() <= distanceToBite())
        {

            nav.destination = transform.position;
            if (actualFood.GetComponent<Prey>().hp < 0)
            {
                state = States.Eating;
                this.GetComponent<DinasorsAnimationCorrector>().eating();
            }
            else
            {
                BiteEnemy();
            }
        }
    }

    void behavior_follower_Eating()
    {
        if (actualFood == null)
        {
            state = States.Following;
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            return;
        }

        EatEnemy();
        if (actualFood.GetComponent<Prey>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Hunting;
        }

        if (satisfied())
        {
            state = States.Following;
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

    ///////////////////////////////////////////////////////////////
    ///////////////// Reacciones a ordenes del lider //////////////
    ///////////////////////////////////////////////////////////////
    void LeaderSaysFollowMe(GameObject l)
    {
        if (state != States.Following && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Following;
                    order_followMe(l);	//Reply the message to others
                }
            }
        }
    }

    void LeaderSaysStop(GameObject l)
    {
        if (state != States.Waiting && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Waiting;
                    order_stop(l);	//Reply the message to others
                }
            }
        }
    }

    void LeaderSaysReagrupate(GameObject l)
    {
        if (state != States.Reagruping && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Reagruping;
                    nav.destination = Dispersal(l.transform.position);
                    order_reagrupate(l);	//Reply the message to others
                }
            }
        }
    }

    void LeaderSaysHunt(GameObject l)
    {
        if (state != States.Hunting && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Hunting;
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
    void BroadCast(string message, object obj)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (!isMe(hitColliders[i].gameObject))
            { //No me lo envio a mi
                if (hitColliders[i].GetComponent<Predator>() != null)
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
                (this.speed / 3) +
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
        state = States.Searching;
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

    /*
     *	Regresa una pocicion aleatoria alrededor de la pocicion dada
     */
    Vector3 Dispersal(Vector3 pos)
    {
        pos.x = pos.x + (((float)Random.Range(-50, 50) / 100) * this.comRange);
        pos.z = pos.z + (((float)Random.Range(-50, 50) / 100) * this.comRange);
        return pos;
    }


    bool isOnRangeToStop()
    {
        return IsOnRangeToStop(1f);
    }

    /// <summary>
    /// Funcion que inflige daño al enemigo
    /// </summary>
    protected override void BiteEnemy()
    {
        actualFood.GetComponent<Prey>().hp -= (this.attack / (1f / Time.deltaTime));
    }

    /// <summary>
    /// Funcion de comer al enemigo
    /// </summary>
    protected override void EatEnemy()
    {
        actualFood.GetComponent<Prey>().flesh -= (this.attack - actualFood.GetComponent<Prey>().defense) * Time.deltaTime;
        if (this.stamina < 100f)
            this.stamina += (this.attack * Time.deltaTime) / 10;
        else
            this.hp += (this.attack * Time.deltaTime) / 10;
    }

    

    protected override void Die()
    {
        state = States.Die;
        this.GetComponent<DinasorsAnimationCorrector>().die();
        gameObject.GetComponent<PredatorLeaderChoosing>().enabled = false;
        if (isMyLeader(gameObject))
        {
            //LeaderSaysUnsetLeader (gameObject);
            Destroy(gameObject.transform.Find("leaderLigth").gameObject);
        }
    }




    /**
     * Distancia Optima para atacar al enemigo actual
     */
    float distanceToBite()
    {
        return ((nav.radius) * transform.localScale.x * 1.3f) + ((actualFood.GetComponent<NavMeshAgent>().radius) * actualFood.transform.localScale.x * 1.3f);
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
                if (hitColliders[i].GetComponent<Prey>() != null)
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
                if (hitColliders[i].GetComponent<Prey>() != null)
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
        for (int i = 0; i < g.Length; i++)
        {
            if (g[i].GetComponent<Prey>().hp <= 0)
                return g[i];
        }
        //return g [Random.Range (0, g.Length - 1)];
        return getNeardest(g);
    }


    /*
    *	Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
    */
    private Vector3 searchForFood()
    {
        return GetComponent<PredatorSearchFood>().searchForFood(transform.position);
    }

    private bool hungry()//TODO: Cambiar estos valores.
    {
        if (stamina < 120f || hp < 100)
            return true;
        return false;
    }

    private bool satisfied()
    {
        if (stamina < 150 || hp < 100)
            return false;
        return true;
    }
}