using System;
using Assets.My_Assets;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Predator : Agent
{
    private TextMesh textMesh;
    public static string[] names = {"Dr Mario", "Dr Andres", "Dr Mellado", "Dr Felix", "Dr Raul", "Ing. Elvia", "Dr "};
    public static int indice = 0;

	private NeuralNetwork nn;

	public int herdid;
    void Awake()
    {
        InitValue();

        state = States.ChoosingLeader;

        identifier = Predator.names[indice];
        indice++;

        textMesh = (TextMesh) gameObject.AddComponent("TextMesh");
        var f = (Font) Resources.LoadAssetAtPath("Assets/My Assets/Fonts/coolvetica.ttf", typeof (Font));
        textMesh.font = f;
        textMesh.renderer.sharedMaterial = f.material;
        textMesh.text = identifier;

		nn = new NeuralNetwork (this);

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
        if (IsMyLeader(gameObject))//TODO: isLeader == true
        {
            if (state == States.Searching)
            {
                behavior_leader_searching();
            }
            else if (state == States.Moving)
            {
                behavior_leader_moving();
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
                //Seguir al lider
                behavior_follower_following();
            }
            else if (state == States.Waiting)
            {
                //Esperar a que el lider tome una decicion
                behavior_follower_waiting();
            }
            else if (state == States.Reagruping)//TODO: Borrar
            {

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

    private void behavior_leader_searching()
    {
        //Calcula nueva posicion de la comida
        isNeededRun = false;
        Vector3 foodPosition = SearchForFood();
        if (foodPosition != Vector3.zero)
        {
            nav.destination = foodPosition;
            state = States.Moving;
            order_followMe(gameObject);
        }
    }

    private void behavior_leader_moving()
    {
        if (IsOnRangeToStop(1f))
        {
            Stop();
            state = States.Hunting;
            order_hunt(gameObject);
        }
        else if (IsThereFood(2.5f))
        {
            state = States.Hunting;
            order_hunt(gameObject);
        }
    }

    private void behavior_hunting()
    {
        if (actualFood == null)
        {
            actualFood = GetBestFood();
            if (actualFood == null)
            {
                state = isLeader ? States.Searching : States.Following;
            }
        }
        else
        {
            isNeededRun = true;
            nav.destination = actualFood.transform.position;
            if (DistanceFromDestination() <= DistanceToBite(false))
            {
                //Volteo a ver a la presa
                GetComponent<DinasorsAnimationCorrector>().eating();
                if (actualFood.GetComponent<Prey>().hp < 0)
                {
                    state = States.Eating;
                }
                else
                {
                    BiteEnemy(false);
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
        isNeededRun = false;
        if (actualFood == null)
        {
            GetComponent<DinasorsAnimationCorrector>().idle();
            state = States.Hunting;
        }
        else
        {
            EatEnemy(false);
            if (actualFood.GetComponent<Prey>().flesh < 0)
            {
                GetComponent<DinasorsAnimationCorrector>().idle();
                state = States.Hunting;
            }
        }
    }

    private void behavior_follower_following()
    {
        nav.destination = leader.transform.position;
        if (IsThereFood(2.5f))
        {
            state = States.Hunting;
            order_hunt(gameObject);
        }
    }

    private void behavior_follower_waiting()
    {
        if (nav.velocity != Vector3.zero)
        {
            Stop();
        }
    }

    private void behavior_follower_reagruping()
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
    
    /// <summary>
    /// Llama al modulo de logica difusa para encontrar el area mas conveniente para encontrr comida
    /// </summary>
    /// <returns>Regresa ruta de la comida</returns>
    private Vector3 SearchForFood()
    {
        return GetComponent<PredatorSearchFood>().SearchForFood(transform.position);
    }

    /// <summary>
    /// Retorna la mejor presa posible
    /// </summary>
    /// <returns>Retorna la mejor presa posible</returns>
    private GameObject GetBestFood()
    {
        List<Agent> lstFood = GetNearbyFood();
        if (lstFood.Count == 0)
        {
            return null;
        }
        else
        {
            Agent ret = lstFood[0];
            double staminaMin = ret.stamina; 
            for (int i = 1; i < lstFood.Count; i++)
            {
                Agent oFood = lstFood[i];
                //Si esta muerto
                if (oFood.hp <= 0)
                {
                    return oFood.gameObject;
                }
                //Heuristica de seleccion
                if (oFood.stamina < staminaMin)
                {
                    staminaMin = oFood.stamina;
                    ret = oFood;
                }
            }
            return ret.gameObject;
        }
    }

    /// <summary>
    /// Obtiene los objetos "COMIDA", cercanos a la posicion del objeto
    /// </summary>
    /// <returns>Retorna la lista de comida</returns>
    private List<Agent> GetNearbyFood()
    {
        List<Agent> lstFood = GetColliders<Prey>(2.5f);
        return lstFood;
    }

    private bool IsThereFood(float factor)
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange * factor);
        return hitColliders.Select(e => e.GetComponent<Prey>()).Any(oAgent => oAgent != null);
    }
    #endregion

    #region Leader Stimulus
    private void behavior_select_leader()
    {
		List<Predator> herdList = new List<Predator>();
		for( int j=0;j<herd.Count;j++){
			herdList.Add(herd[j].GetComponent<Predator>());
		}

		this.getNewLeader(herdList);
            /*if (GetComponent<PreyLeaderChoosing>() == null)
                setLeader(gameObject);
            else
            {
                GetComponent<PreyLeaderChoosing>().choose();
            }*/
   }

    /// <summary>
    /// Fijar el objeto lider
    /// </summary>
    /// <param name="l">Lider del objeto</param>
    public float getLeadershipStat()
    {
        return
            (this.hp / 100) +
            (this.speed / 3) +
            ((float)this.stamina / 100) +
            ((this.lifetime * 2) / 10000);
    }

    /// <summary>
    /// Retorna la capacidad de liderazgo de la unidad
    /// </summary>
    /// <returns>Valor de liderazgo</returns>
    public void setLeader(GameObject l)
    {
        leader = l;
        nav.avoidancePriority = 1;
        state = States.Searching;

        if (IsMyLeader(gameObject))
        {
            isLeader = true;
            state = States.Searching;
        }
        else
        {
            isLeader = false;
            state = States.Waiting;
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
    #endregion

	public void getNewLeader(List<Predator> herd){
		
		Predator newLeader = GetComponent<LeaderSelectorPredator>().getLeader(herd);
		foreach( Predator p in herd ){
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
		brigth.light.color = Color.blue;
		brigth.light.intensity = 2;
		brigth.light.range = 50F;
		brigth.light.spotAngle = 180f;
		
	}
}