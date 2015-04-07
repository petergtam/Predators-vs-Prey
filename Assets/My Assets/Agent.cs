using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.My_Assets
{
    public abstract class Agent : MonoBehaviour
    {
        //public Transform m_Prey;
        public float hp;			//Salud de la entidad
        public int speed; 			//Velocidad de la entidad
        public int comRange;		//Rango de comunicacion
        public double stamina;		//Resistencia (nesesaria para correr etc....)
        public float lifetime;		//Tiempo de vida en segundos 
        public float attack;		//Daño que realiza la entidad
        public float flesh;         //Nutricion aportada a quien se alimente de la entidad 
        public float defense;       //Defensa de la entidad.
        public int state;

        public bool isNeededRun = false;
        public GameObject actualFood;

        protected NavMeshAgent nav;
        protected GameObject leader;
        protected float stoppingDistance;

        protected enum States
        {
            ChoosingLeader, 
            Searching, 
            Following, 
            Moving, 
            Hunting, 
            Eating, 
            Reproduce, 
            Hiding, 
            Waiting, 
            Reagruping, 
            Die
        };

        /// <summary>
        /// Inicializa los valores del agente.
        /// </summary>
        protected abstract void InitValue();

        /// <summary>
        /// Muere el agente
        /// </summary>
        protected abstract void die();

        #region Estimulos
        /// <summary>
        /// Obtiene los lideres que hay en la manada
        /// </summary>
        /// <returns>Retorna la cantidad de lideres en la manada</returns>
        public float GetLeaderShip()
        {
            float leaderShipIndicator = 0;
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);

            //Por cada objeto encontrado
            foreach (Collider e in hitColliders)
            {
                if (e.GetComponent<Predator>() != null)
                {
                    leaderShipIndicator++;///TODO: No cuenta lideres
                }
            }
            return leaderShipIndicator;
        }

        /// <summary>
        /// Obtiene el numero de presas para cazar
        /// </summary>
        /// <returns>Retorna el numero de presas disponible para la caza</returns>
        public float GetHunt()
        {
            float huntIndicator = 0;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
            foreach (Collider e in hitColliders)
            {
                if (e.GetComponent<Prey>() != null)
                {
                    huntIndicator++;
                }
            }
            return huntIndicator;
        }

        /// <summary>
        /// Obtiene el numero de depredadores acechando la manada
        /// </summary>
        /// <returns>Retorna el numero de depresadores</returns>
        public float GetFear()
        {
            float fearIndicator = 0;

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
            foreach (Collider e in hitColliders)
            {
                if (e.GetComponent<Predator>() != null)
                {
                    fearIndicator++;
                }
            }
            return fearIndicator;
        }

        /// <summary>
        /// Obtiene el nivel de hambre de la manada
        /// </summary>
        /// <returns>Retorna el nivel de hambre</returns>
        public double GetHungry()
        {
            double hungryIndicator = 0;
            var classType = this.GetType();

            //Se obtiene la manada
            List<Agent> lstCharm = new List<Agent>();
            if (classType == typeof(Predator))
            {
                lstCharm = GetCharm<Predator>();
            }
            else if (classType == typeof(Prey))
            {
                lstCharm = GetCharm<Prey>();
            }

            //Se obtiene el promedio de stanmina
            hungryIndicator = lstCharm.Average(x => x.stamina);

            return 100 - hungryIndicator;
        }

        /// <summary>
        /// Obtiene el numero de agentes que estan en edad de reporducirse
        /// </summary>
        /// <returns>Retorna el numero de agentes</returns>
        public double GetMating()
        {
            double matingIndicator = 0;
            var classType = this.GetType();

            //Se obtiene la manada
            List<Agent> lstCharm = new List<Agent>();
            if (classType == typeof(Predator))
            {
                lstCharm = GetCharm<Predator>();
            }
            else if (classType == typeof(Prey))
            {
                lstCharm = GetCharm<Prey>();
            }

            //Se obtiene los agentes que estan en edad de procrear
            matingIndicator = lstCharm.Count(x => x.lifetime > 300 && x.lifetime < 540);

            return matingIndicator;
        }
        #endregion
        
        /// <summary>
        /// Obtiene la manada
        /// </summary>
        /// <typeparam name="T">Tipo de la manada</typeparam>
        /// <returns>Retorna la lista de integrantes de la manada</returns>
        public List<Agent> GetCharm<T>() where T : Agent
        {
            List<Agent> lstCharm = new List<Agent>();
            
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
            foreach (Collider e in hitColliders)
            {
                T oAgent = e.GetComponent<T>();    
                if (oAgent != null)
                {
                    lstCharm.Add(oAgent);
                }
            }
            return lstCharm;
        }

        /// <summary>
        /// Realiza las funciones biologicas de consumir energia del individuo
        /// </summary>
        /// <returns>Retorna si el individuo esta vivo</returns>
        protected bool metabolism()
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
                if (flesh <= 0)
                    Destroy(gameObject);
                return false;
            }
            if (0 < this.stamina)
            {
                stamina -= Time.deltaTime * factor * (1 / 10f); //Cada 10 segundo gasta uno de stamina
            }
            if (stamina <= 0)
            {
                if (0 < this.hp)
                {
                    hp -= Time.deltaTime * factor * (1 / 15f); // Cada 15 segundos gasta uno de hp si no tiene stamina
                }
            }
            if (hp <= 0 || lifetime < 0)
            {
                die();
                return false;
            }
            return true;
        }
    }
}
