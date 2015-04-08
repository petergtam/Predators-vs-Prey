using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.My_Assets
{
    public abstract class Agent : MonoBehaviour
    {
        public float hp;			//Salud de la entidad
        public int speed; 			//Velocidad de la entidad
        public int comRange;		//Rango de comunicacion
        public double stamina;		//Resistencia (nesesaria para correr etc....)
        public float maxLifeTime;   //Tiempo de vida en segundos
        public float attack;		//Daño que realiza la entidad
        public float flesh;         //Nutricion aportada a quien se alimente de la entidad 
        public float defense;       //Defensa de la entidad.

        public States state;
        public LifeEnum LifeState;

        public float lifetime;		//Tiempo de vida transcurridos en segundos 
        public bool isLeader = false;  //Indica si este agente es lider
        public bool isNeededRun = false;
        public GameObject actualFood;

        protected NavMeshAgent nav;
        protected GameObject leader;
        protected float stoppingDistance;

        public enum States
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

        public enum LifeEnum
        { 
            Joven,
            Adulto,
            Vejez
        }

        public enum StimulusEnum
        {
            LeaderShip,
            Fear,
            Hungry,
            Mating
        }

        /// <summary>
        /// Inicializa los valores del agente.
        /// </summary>
        protected abstract void InitValue();

        /// <summary>
        /// Muere el agente
        /// </summary>
        protected abstract void die();

        #region Estimulos

        public StimulusEnum SelectStimulu()
        {
            double[] lstEstimulus = GetStimulus();
            //TODO: Implementar red bayesiana

            return StimulusEnum.Hungry;
        }

        /// <summary>
        /// Obtiene los estimulos del agente en el instante actual
        /// </summary>
        /// <returns>Retorna un arreglo con los estimulos del agente. Miedo, Liderazgo, Hambre y Apareamiento.</returns>
        protected double[] GetStimulus()
        {
            double[] lstStimulus = new double[4];

            lstStimulus[0] = GetFearStimulus();
            lstStimulus[1] = GetLeaderShipStimulus();
            lstStimulus[2] = this.isLeader == true ? GetHungryStimulus() : 0;
            lstStimulus[3] = this.isLeader == true ? GetMatingStimulus() : 0;

            return lstStimulus;
        }

        /// <summary>
        /// Obtiene el estimulo de liderazgo de 0 - 1
        /// </summary>
        /// <returns>Retorna 0 si no se necesita cambiar lider, 1 si se necesita cambiar</returns>
        private double GetLeaderShipStimulus()
        {
            double leaderShipIndicator = 0;
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

            //Cuenta los lideres en la manada
            int leaderCount = lstCharm.Count(x => x.isLeader == true);            
            
            if (leaderCount != 1)
            {
                leaderShipIndicator = 1;
            }
            return leaderShipIndicator;
        }

        /// <summary>
        /// Obtiene el estimulo de miedo de 0 - 1
        /// </summary>
        /// <returns>Retorna la razon de peligro en el instante actual</returns>
        private double GetFearStimulus()
        {
            double fearIndicator = 0;
            var classType = this.GetType();

            //Solo las presas tienen miedo.
            if (classType == typeof(Prey))
            {
                List<Agent> lstCharm = GetCharm<Prey>();
                List<Agent> lstPredator = GetCharm<Predator>();

                fearIndicator = lstPredator.Count / lstCharm.Count;
            }
            
            return fearIndicator;
        }

        /// <summary>
        /// Obtiene el estimulo de hambre de la manada de 0 - 1
        /// </summary>
        /// <returns>Retorna el nivel de hambre</returns>
        private double GetHungryStimulus()
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

            return (100 - hungryIndicator) / 100;
        }

        /// <summary>
        /// Obtiene el estimulo de apareamiento de 0 - 1
        /// </summary>
        /// <returns>Retorna nivel de apareamiento de la manada</returns>
        private double GetMatingStimulus()
        {
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

            //Se obtiene la razon de los agentes que estan en edad de procrear entre el total de la manada
            double matingIndicator = (double) lstCharm.Count(x => x.LifeState == LifeEnum.Adulto) / lstCharm.Count;

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

            //Si el agente actual pertenece a la manada lo agrega
            var classType = GetType();
            if (classType == typeof (T))
            {
                lstCharm.Add(this);
            }

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
        protected bool Metabolism()
        {
            UpdateLifeTime();

            float factor = 1f;
            if (isNeededRun)
                factor *= 2f;

            if (state == States.Die)
            {
                if (flesh <= 0)
                    Destroy(gameObject);
                return false;
            }
            if (0 < stamina)
            {
                stamina -= Time.deltaTime * factor * (1 / 10f); //Cada 10 segundo gasta uno de stamina
            }
            if (stamina <= 0)
            {
                if (0 < hp)
                {
                    hp -= Time.deltaTime * factor * (1 / 15f); // Cada 15 segundos gasta uno de hp si no tiene stamina
                }
            }
            if (hp <= 0 || lifetime >= maxLifeTime)
            {
                die();
                return false;
            }
            return true;
        }
        
        /// <summary>
        /// Actualiza el tiempo de vida y el estado del agente
        /// </summary>
        private void UpdateLifeTime()
        {
            lifetime += Time.deltaTime;
    
            if (lifetime < 240)
            {
                LifeState = LifeEnum.Joven;
            }
            else if (lifetime < 480)
            {
                LifeState = LifeEnum.Adulto;
            }
            else
            {
                LifeState = LifeEnum.Vejez;
            }
        }
    }
}
