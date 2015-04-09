using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.My_Assets;
using Random = UnityEngine.Random;

public class Prey : Agent
{
    public Plant actualFood;
    protected override void InitValue()
    {
        //Propiedades fijas
        hp = 100f;
        stamina = 100f;
        comRange = 10;
        lifetime = 0;	
        isNeededRun = false;

        //Propiedades variables
        flesh = Random.Range(300, 700);
        speed = Random.Range(6, 10);
        maxLifeTime = Random.Range(540, 720); //De 9 a 12 minutos
        attack = Random.Range(6, 12);
        defense = Random.Range(0, 5);

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
        if (GetComponent<PreyLeaderChoosing>() == null)
            setLeader(gameObject);
        else
        {
            GetComponent<PreyLeaderChoosing>().choose();
        }
    }

    private void Update()
    {
        if (!Metabolism())
            return;

        if (isNeededRun)
        {//TODO: Nunca cambia entre correr y caminar las presas
            nav.speed = (float)(speed * ((stamina < 50 ? 50 : stamina) / 100.0));
        }
        else
            nav.speed = (float)((speed / 3.0) * ((stamina < 50 ? 50 : stamina) / 100.0));


        StimulusEnum stimulus = SelectStimulu();

        switch (stimulus)
        {
            case StimulusEnum.LeaderShip: //Se elige el lider
                behavior_select_leader();
                break;

            case StimulusEnum.Fear:
                if (isLeader == true)
                {
                    behavior_run();
                }
                else
                {
                    //Comunicar al lider
                    BroadCast("Peligro manada", null);
                }
                break;

            case StimulusEnum.Hungry:
                behavior_hungry();
                break;

            case StimulusEnum.Mating:
                if (isLeader == true)
                {
                    behavior_mating();
                }
                break;

            default:
                //TODO: Definir cual es el dafault
                throw new NotImplementedException();
                break;
        }

    }

    private void behavior_hungry()
    {
        if (isLeader == true || isMyLeader(gameObject))
        {
            if (state == States.Searching)
            {
                //Calcula nueva posicion de la comida
                Vector3 foodPosition = searchForFood();
                if (foodPosition != Vector3.zero)
                {
                    nav.destination = foodPosition;
                    state = States.Moving;
                    order_followMe(gameObject);
                }
            }
            else if (state == States.Moving)
            {
                //Entra en estado de viaje en grupo
                if (IsOnRangeToStop(1f))
                {
                    Stop();
                    state = States.Hunting;
                    order_hunt(gameObject);
                }
            }
            else if (state == States.Hunting)
            {
                //Busca una nueva planta en el area en el que esta
                if (actualFood == null)
                {
                    actualFood = getBestFood();
                    if (actualFood == null)
                    {
                        state = States.Searching;
                    }
                    else
                    {
                        nav.destination = actualFood.transform.position;
                    }
                }
                else
                {
                    if (DistanceFromDestination() <= distanceToBite())
                    {
                        Stop();
                        //Volteo a ver a la planta
                        transform.LookAt(actualFood.transform);
                        state = States.Eating;
                        GetComponent<DinasorsAnimationCorrector>().eating();
                    }
                }
            }
            else if (state == States.Eating)
            {
                if (actualFood == null)
                {
                    GetComponent<DinasorsAnimationCorrector>().idle();
                    state = States.Searching;
                }
                else
                {
                    BiteEnemy();
                    EatEnemy();

                    if (actualFood.GetComponent<Plant>().flesh < 0)
                    {
                        GetComponent<DinasorsAnimationCorrector>().idle();
                        state = States.Searching;
                    }
                }
            }
        }
        else
        {
            if (state == States.Following)
            {
                //Seguir al lider
                behavior_follower_following();
            }
            else if (state == States.Waiting)
            {
                //Esperar a que el lider tome una decicion
                behavior_follower_waiting();
            }
            else if (state == States.Hunting)
            {
                behavior_follower_Hunting();
            }
            else if (state == States.Eating)
            {
                behavior_follower_Eating();
            }
        }
    }

    private void behavior_select_leader()
    {
        if (GetComponent<PreyLeaderChoosing>() == null)
            setLeader(gameObject);
        else
        {
            GetComponent<PreyLeaderChoosing>().choose();
        }
    }

    private void behavior_run()
    {
        //Find a safe place
        throw new NotImplementedException();
    }

    private void behavior_mating()
    {
        //Find couple

        //Procreate

        //Born a new child
        throw new NotImplementedException();
    }



    #region Comportamiento del Seguidor
    void behavior_follower_following()
    {
        nav.destination = leader.transform.position;

        /*if( leader.GetComponent<Prey>().state != States.Following && leader.GetComponent<Prey>().state != States.Searching){
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

                //Debug.Log ("No Food, nearby");
                return;
            }
        }

        nav.stoppingDistance = 0;
        //nav.stoppingDistance = distanceToBite();
        nav.destination = actualFood.transform.position;
        if (DistanceFromDestination() <= distanceToBite())
        {

            nav.destination = transform.position;
            if (actualFood.GetComponent<Plant>().hp < 0)
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
        if (actualFood.GetComponent<Plant>().flesh < 0)
        {
            this.GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Hunting;
        }
    }
    #endregion

    #region Ordenes del lider
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
    #endregion

    #region Reacciones a ordenes del lider
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

    void LeaderSaysUnsetLeader(GameObject l)
    {
        if (leader != null && 0 < hp)
        {
            if (isMyLeader(l))
            {
                if (!isMe(leader))
                {
                    state = States.Hiding;
                    leader = null;
                    order_unsetLeader(l);	//Reply the message to others
                }
            }
        }
    }

    void SaysPanic(GameObject l)
    {
        if (0 < hp)
            state = States.Hiding;

    }
    #endregion
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
        state = States.Moving;
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

    /// <summary>
    /// Funcion de comer al enemigo
    /// </summary>
    protected override void EatEnemy()
    {
        actualFood.GetComponent<Plant>().flesh -= this.attack * Time.deltaTime;
        if (this.stamina < 100f)
            this.stamina += (this.attack * Time.deltaTime) / 10;
        else
            this.hp += (this.attack / Time.deltaTime) / 10; //Time.deltaTime Es el tiempo desde el ultimo frame
    }

    /// <summary>
    /// Funcion que inflige daño al enemigo
    /// </summary>
    protected override void BiteEnemy()
    {
        actualFood.GetComponent<Plant>().hp -= this.attack / (1f / Time.deltaTime);
    }


    protected override void Die()
    {
        state = States.Die;
        GetComponent<DinasorsAnimationCorrector>().die();
        gameObject.GetComponent<PreyNeuronalChoose>().enabled = false;
        defense = 0;
        if (isMyLeader(gameObject))
        {
            Destroy(gameObject.transform.Find("leaderLigth").gameObject);
        }
    }




    /**
     * Distancia Optima para atacar al enemigo actual
     */
    float distanceToBite()//TODO:Ver exactamente que hace
    {
        return ((nav.radius) * transform.localScale.x * 1.3f) + ((actualFood.GetComponent<MeshRenderer>().bounds.size.x) * 1.3f);
    }


    
    
    /// <summary>
    /// Retorna la mejor planta disponible
    /// </summary>
    /// <returns>Retorna la mejor planta disponible</returns>
    Plant getBestFood()
    {
        List<Plant> lstFood = getNearbyFood();
        if (lstFood.Count == 0)
            return null;
        return lstFood[Random.Range(0,lstFood.Count-1)];
    }

    /// <summary>
    /// Obtiene los objetos "COMIDA", cercanos a la posicion del objeto
    /// </summary>
    /// <returns>Retorna la lista de comida</returns>
    protected List<Plant> getNearbyFood()
    {
        List<Plant> lstFood = new List<Plant>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange * 2.5f);
        foreach (Collider e in hitColliders)
        {
            Plant oPlant = e.GetComponent<Plant>();
            if (oPlant != null)
            {
                lstFood.Add(oPlant);
            }
        }
        return lstFood;
    }

    /*
    *	Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
    */
    private Vector3 searchForFood()
    {
        return GetComponent<PreySearchFood>().searchForFood(transform.position);
    }
}