using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Assets.My_Assets;
using Assets.My_Assets.scripts;
using UnityEngine;
using Random = UnityEngine.Random;


public class Prey : Agent
{
    private TextMesh textMesh;



	public static string[] names =
	{
		"Gibran", "Pedro", "Celeste", "Lea", "Ivan", "Victor", "Alberto", "Hector", "Mayra",
		"Orlando", "Mario", "Ruben", "Armando", "Edith", "Arturo", "Jairo"
	};
	public int herdid ;
	public static int indice = 0;
	private NeuralNetwork nn;

    void Awake()
    {
        InitValue();

        state = States.ChoosingLeader;

        //Si no cuenta con eleccion de lider, el es el lider
        /*if (GetComponent<LeaderSelectorPrey>() == null)
            setLeader(gameObject);
        else
        {
            GetComponent<LeaderSelectorPrey>().;
        }*/
		//setLeader(gameObject);
        name = Prey.names[indice];
		//setLeader (gameObject);

        indice++;

        textMesh = (TextMesh) gameObject.AddComponent("TextMesh");
        var f = (Font) Resources.LoadAssetAtPath("Assets/My Assets/Fonts/coolvetica.ttf", typeof (Font));
        textMesh.font = f;
        textMesh.renderer.sharedMaterial = f.material;
        textMesh.text = name;
		//getNewLeader();
        if (name == "Pedro")
        {
            nn = new NeuralNetwork();
        }

    }

    private void Update()
    {
        if (!Metabolism())
            return;
        nav.speed = Velocidad(isNeededRun);

        StimulusEnum stimulus = SelectStimulu(nn);
        switch (stimulus)
        {
            case StimulusEnum.LeaderShip: //Se elige el lider
                behavior_select_leader();
                break;

            case StimulusEnum.Fear:
                if (isLeader == true)
                {
                    behavior_fear();
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
        }

    }

    #region Hungry Stimulus
    private void behavior_hungry()
    {
        if (isLeader == true)
        {
            if (state == States.Hiding)
            {
                if (actualPredator != null)
                {
                    if (Vector3.Distance(actualPredator.transform.position, transform.position) > 3 * comRange)
                    {
                        actualPredator = null;
                    }
                }
                else
                {
                    state = States.Searching;
                }
            }
            else if (state == States.Searching)
            {
                Debug.Log(name + ": Search");
                isNeededRun = false;
                behavior_searching();
            }
            else if (state == States.Moving)
            {
                behavior_moving();
            }
            else if (state == States.Hunting)
            {
                behavior_hunting();
            }
            else if (state == States.Eating)
            {
                behavior_eating();
            }
        }
        else
        {
            if (state == States.Following)
            {
                behavior_following();
            }
            else if (state == States.Waiting)
            {
                behavior_waiting();
            }
            else if (state == States.Hunting)
            {
                behavior_hunting();
            }
            else if (state == States.Eating)
            {
                behavior_eating();
            }
        }
    }

    private void behavior_searching()
    {
        //Calcula nueva posicion de la comida
        Vector3 foodPosition = SearchForFood();
        if (foodPosition != Vector3.zero)
        {
            nav.destination = foodPosition;
            state = States.Moving;
            order_followMe(gameObject);
        }
    }

    private void behavior_moving()
    {
        //Entra en estado de viaje en grupo
        if (IsOnRangeToStop(1f))
        {
            Stop();
            state = States.Hunting;
            order_hunt(gameObject);
        }
    }

    private void behavior_hunting()
    {
        //Busca una nueva planta en el area en el que esta
        if (actualFood == null)
        {
            actualFood = GetBestFood();
            if (actualFood == null)
            {
                if (isLeader == true)
                {
                    state = States.Searching;
                }
                else
                {
                    state = States.Following;
                }
            }
        }
        else
        {
            nav.destination = actualFood.transform.position;
            if (DistanceFromDestination() <= DistanceToBite(true))
            {
                Stop();
                //Volteo a ver a la planta
                transform.LookAt(actualFood.transform);

                GetComponent<DinasorsAnimationCorrector>().eating();
                if (actualFood.GetComponent<Plant>().hp < 0)
                {
                    state = States.Eating;
                }
                else
                {
                    BiteEnemy(true);
                }
            }
            else
            {
                GetComponent<DinasorsAnimationCorrector>().idle();
            }
        }
    }

    private void behavior_eating()
    {
        if (actualFood == null)
        {
            GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Searching;
        }
        else
        {
            EatEnemy(true);
            if (actualFood.GetComponent<Plant>().flesh < 0)
            {
                GetComponent<DinasorsAnimationCorrector>().idle();
                state = States.Hunting;
            }
        }
    }

    private void behavior_following()
    {
        nav.destination = leader.transform.position;
    }

    private void behavior_waiting()
    {
        //Esperar a que el lider tome una decicion
        if (nav.velocity != Vector3.zero)
        {
            Stop();
        }
    }

    /// <summary>
    /// Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
    /// </summary>
    /// <returns>Regresa ruta de la comida</returns>
    private Vector3 SearchForFood()
    {
        return GetComponent<PreySearchFood>().SearchForFood(transform.position);
    }

    /// <summary>
    /// Retorna la mejor planta disponible
    /// </summary>
    /// <returns>Retorna la mejor planta disponible</returns>
    private GameObject GetBestFood()
    {
        List<GameObject> lstFood = GetNearbyFood();
        if (lstFood.Count == 0)
            return null;
        return lstFood[Random.Range(0, lstFood.Count - 1)];
    }

    /// <summary>
    /// Obtiene los objetos "COMIDA", cercanos a la posicion del objeto
    /// </summary>
    /// <returns>Retorna la lista de comida</returns>
    protected List<GameObject> GetNearbyFood()
    {
        List<GameObject> lstFood = new List<GameObject>();

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange * 2.5f);
        foreach (Collider e in hitColliders)
        {
            Plant oPlant = e.GetComponent<Plant>();
            if (oPlant != null)
            {
                lstFood.Add(e.gameObject);
            }
        }
        return lstFood;
    }
    #endregion

    #region Leader Stimulus
    private void behavior_select_leader()
    {
        if (state != States.ChoosingLeader)
        {
            //state = States.ChoosingLeader;
			//getNewLeader();
        }
    }

	/// <summary>
	/// Retorna la capacidad de liderazgo de la unidad
	/// </summary>
	/// <returns>Valor de liderazgo</returns>
	public float getLeadershipStat()
	{
		return
			(this.hp / 100) +
				(this.speed / 3.0f) +
				((float)this.stamina / 100) +
				((this.lifetime * 2) / 10000);
	}
	
	/// <summary>
	/// Fijar el objeto lider
	/// </summary>
	/// <param name="l">Lider del objeto</param>
	public void setLeader(GameObject l)
	{
		if( nav != null )
			nav.avoidancePriority = 1;
		if (this.isLeader) {
			isLeader = true;
			leader = gameObject;
			state = States.Searching;
		} else {
			leader = l;
			state = States.Waiting;
		}
		/*if (IsMyLeader(gameObject))
        {
            isLeader = true;
            state = States.Searching;
        }
        else
        {
            isLeader = false;
            state = States.Waiting;
        }*/
	}
	#endregion

    #region Fear Stimulus
    private void behavior_fear()
    {
        if (state != States.Hiding)
        {
            Agent oPredator = GetColliders<Predator>(2.5f).First();
            var scalar = 4 * (transform.position - oPredator.transform.position);
            nav.destination = (transform.position + scalar);
            actualPredator = oPredator.gameObject;

            state = States.Hiding;
            isNeededRun = true;
        }
        else if (Vector3.Distance(transform.position, actualPredator.transform.position) < 50f)
        {
            Agent oPredator = GetColliders<Predator>(2.5f).First();
            var scalar = 4 * (transform.position - oPredator.transform.position);
            nav.destination = (transform.position + scalar);
            actualPredator = oPredator.gameObject;
        }
    }
    #endregion

    #region Mating Stimulus
    private void behavior_mating()
    {
        //Find couple

        //Procreate

        //Born a new child
        throw new NotImplementedException();
    }
    #endregion

    #region Ordenes del lider
    private void order_followMe(GameObject l)
    {
        BroadCast("LeaderSaysFollowMe", l);
    }

    private void order_stop(GameObject l)
    {
        BroadCast("LeaderSaysStop", l);
    }

    private void order_reagrupate(GameObject l)
    {
        BroadCast("LeaderSaysReagrupate", l);
    }

    private void order_hunt(GameObject l)
    {
        BroadCast("LeaderSaysHunt", l);
    }

    private void order_unsetLeader(GameObject l)
    {
        BroadCast("LeaderSaysUnsetLeader", l);
    }
    #endregion

    #region Reacciones a ordenes del lider
    private void LeaderSaysFollowMe(GameObject l)
    {
        if (state != States.Following && 0 < hp)
        {
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
                {
                    state = States.Following;
                    order_followMe(l); //Reply the message to others
                }
            }
        }
    }

    private void LeaderSaysStop(GameObject l)
    {
        if (state != States.Waiting && 0 < hp)
        {
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
                {
                    state = States.Waiting;
                    order_stop(l); //Reply the message to others
                }
            }
        }
    }

    private void LeaderSaysReagrupate(GameObject l)
    {
        if (state != States.Reagruping && 0 < hp)
        {
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
                {
                    state = States.Reagruping;
                    nav.destination = Dispersal(l.transform.position);
                    order_reagrupate(l); //Reply the message to others
                }
            }
        }
    }

    private void LeaderSaysHunt(GameObject l)
    {
        if (state != States.Hunting && 0 < hp)
        {
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
                {
                    state = States.Hunting;
                    nav.destination = l.GetComponent<NavMeshAgent>().destination;
                    order_hunt(l); //Reply the message to others
                }
            }
        }
    }

    private void LeaderSaysUnsetLeader(GameObject l)
    {
        if (leader != null && 0 < hp)
        {
            if (IsMyLeader(l))
            {
                if (!IsMe(leader))
                {
                    state = States.Hiding;
                    leader = null;
                    order_unsetLeader(l); //Reply the message to others
                }
            }
        }
    }

    private void SaysPanic(GameObject l)
    {
        if (0 < hp)
            state = States.Hiding;
    }
    #endregion

    #region Liderazgo

	public void getNewLeader(List<Prey> herd){

		Prey newLeader = GetComponent<LeaderSelectorPrey>().getLeader(herd);
		foreach( Prey p in herd ){
				p.setLeader(newLeader.gameObject);
				p.leader = this.gameObject;
				if(p.gameObject.transform.Find("leaderLigth") != null){
					Destroy(p.gameObject.transform.Find("leaderLigth").gameObject);
				}
				p.isLeader=false;
		}
		newLeader.isLeader = true;
		newLeader.setLeader (newLeader.gameObject);
		GameObject brigth = new GameObject("leaderLigth");
		brigth.AddComponent(typeof(Light));							//se le agrega la luz
		
		brigth.transform.parent = newLeader.gameObject.transform;							//Se fija a la entidad
		
		
		brigth.light.type = LightType.Spot;								//Se elije el tipo de luz SPOT
		
		//Se pone la mira hacia abajo
		brigth.transform.position = brigth.transform.parent.position + new Vector3(0, 1, 0);
		brigth.transform.rotation = Quaternion.Euler(new Vector3(90, 0, 0));
		
		//Color, Alcance, Dispercion
		brigth.light.color = Color.white;
		brigth.light.intensity = 2;
		brigth.light.range = 50F;
		brigth.light.spotAngle = 180f;
		
	}
	#endregion
}
