using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.Text;

namespace Assets.My_Assets
{
    public class Agent : DinoObject
    {
        public string identifier;
        public enum StimulusEnum
        {
            LeaderShip,
            Fear,
            Hungry,
            Mating,
			Rest
        }

        /// <summary>
        /// Comida actual
        /// </summary>
        public GameObject actualPredator;

        #region Estimulos

        public StimulusEnum SelectStimulu(NeuralNetwork nn)
        {
            double[] lstEstimulus = GetStimulus();
            if (this is Prey && lstEstimulus[1] > 0)
            {
                return StimulusEnum.Fear;
            }
            return StimulusEnum.Hungry;

            if (this.identifier == "Pedro")
            {
                var a = GetStimulus();
                double[] result = null;

				if (nn.IsNeedTraining()){ 
					result = nn.Training(a);
					return StimulusEnum.Rest;
				}
                else Debug.Log("Terminado el training");

				result = nn.Ejecution(a);
                var sb = new StringBuilder();
                var sb2 = new StringBuilder();
                sb.Append("(");
                foreach (double t in a)
                {
                    sb.Append(t + ",");
                }
                sb.Append(")\n");
                sb2.Append("(");
                if (result != null)
                {
                    for (int i = 0; i < a.Length-1; i++)
                    {
                        sb2.Append(result[i] + ",");
                    }
                }
                sb2.Append(")\n");
                Debug.Log("Stimulus:" + sb);
                Debug.Log("Output" + sb2);
                if (result != null)
                {
					if(this is Prey){
	                    if (result[0] >= 1)
	                    {
	                        return StimulusEnum.Fear;
	                    }
	                    else if (result[1] >= 1)
	                    {
	                        return StimulusEnum.LeaderShip;
	                    }
	                    else if (result[2] >= 1)
	                    {
	                        return StimulusEnum.Hungry;
	                    }
	                    else if (result[3] >= 1)
	                    {
	                        return StimulusEnum.Mating;
	                    }
	                    else return StimulusEnum.Rest;
					}else{
						if (result[0] >= 1)
						{
							return StimulusEnum.LeaderShip;
						}
						else if (result[1] >= 1)
						{
							return StimulusEnum.Hungry;
						}
						else if (result[2] >= 1)
						{
							return StimulusEnum.Mating;
						}
						else return StimulusEnum.Rest;
					}
                }
            }
            return StimulusEnum.Rest;
        }

        /// <summary>
        /// Obtiene los estimulos del agente en el instante actual
        /// </summary>
        /// <returns>Retorna un arreglo con los estimulos del agente. Miedo, Liderazgo, Hambre y Apareamiento.</returns>
        protected double[] GetStimulus()
        {
            double[] lstStimulus = new double[5];

			if (this is Prey) {
				lstStimulus [0] = 1;
				lstStimulus [1] = GetFearStimulus ();
				lstStimulus [2] = GetLeaderShipStimulus ();
				lstStimulus [3] = isLeader ? GetHungryStimulus () : 0;
				lstStimulus [4] = isLeader ? GetMatingStimulus () : 0;
			} else {
				lstStimulus [0] = 1;
				lstStimulus [1] = GetLeaderShipStimulus ();
				lstStimulus [2] = isLeader ? GetHungryStimulus () : 0;
				lstStimulus [3] = isLeader ? GetMatingStimulus () : 0;
			}

            return lstStimulus;
        }

        /// <summary>
        /// Obtiene el estimulo de liderazgo de 0 - 1
        /// </summary>
        /// <returns>Retorna 0 si no se necesita cambiar lider, 1 si se necesita cambiar</returns>
        private double GetLeaderShipStimulus()
        {
            //Se obtiene la manada
            List<Agent> lstCharm = GetHerd();

            //Cuenta los lideres en la manada
            int leaderCount = lstCharm.Count(x => x.isLeader && x.state != States.Die);
                ///TODO:Poner rango de vision de la manada

            double leaderShipIndicator;
            if (leaderCount != 1)
            {
                leaderShipIndicator = 1;
            }
            else
            {
                leaderShipIndicator = 0;
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
            var classType = GetType();

            //Solo las presas tienen miedo.
            if (classType == typeof (Prey))
            {
                List<Agent> lstCharm = GetHerd();
                List<Agent> lstPredator = GetColliders<Predator>(2.0f);

                fearIndicator = (double) lstPredator.Count/lstCharm.Count;
            }

            return fearIndicator;
        }

        /// <summary>
        /// Obtiene el estimulo de hambre de la manada de 0 - 1
        /// </summary>
        /// <returns>Retorna el nivel de hambre</returns>
        private double GetHungryStimulus()
        {
            //Se obtiene la manada
            List<Agent> lstCharm = GetHerd();

            //Se obtiene el promedio de stanmina
            var hungryIndicator = lstCharm.Average(x => x.stamina);
            if (hungryIndicator < 80)//satisfecho!
				return 1;//(100 - hungryIndicator)/100;
            return 0;
        }

        /// <summary>
        /// Obtiene el estimulo de apareamiento de 0 - 1
        /// </summary>
        /// <returns>Retorna nivel de apareamiento de la manada</returns>
        private double GetMatingStimulus()
        {
            //Se obtiene la manada
            List<Agent> lstCharm = GetHerd();

            //Se obtiene la razon de los agentes que estan en edad de procrear entre el total de la manada
            double matingIndicator = (double) lstCharm.Count(x => x.LifeState == LifeEnum.Adulto)/lstCharm.Count;
			//Debug.Log (matingIndicator);
			if (matingIndicator > .5)
				return 1;
            return 0;
        }

        #endregion

        /// <summary>
        /// Obtiene los objetos en el rango del agente
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a buscar</typeparam>
        /// <returns>Retorna la lista de objetos que colicionan</returns>
        public List<Agent> GetColliders<T>(float factor) where T : Agent
        {
            List<Agent> lstColliders = new List<Agent>();

            //Si el agente actual pertenece a la manada lo agrega
            var classType = GetType();
            if (classType == typeof (T))
            {
                lstColliders.Add(this);
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange * factor);
            foreach (Collider e in hitColliders)
            {
                T oAgent = e.GetComponent<T>();
                if (oAgent != null)
                {
                    lstColliders.Add(oAgent);
                }
            }

            return lstColliders;
        }

        /// <summary>
        /// Realiza las funciones biologicas de consumir energia del individuo
        /// </summary>
        /// <returns>Retorna si el individuo esta vivo</returns>
        protected new bool Metabolism()
        {
            bool metabolism = base.Metabolism();

            /*
            if (actualFood != null)
            {
                if (Vector3.Distance(actualFood.transform.position, transform.position) > 2.5 * comRange)
                {
                    actualFood = null;
                }
            }
             * */

            //Cambiar tamaño
            const float scale = 0.5f;
            if (metabolism)
            {
                float newScale = scale;
                if (LifeState == LifeEnum.Joven)
                {
                    newScale = scale*.7f;
                }
                else if (LifeState == LifeEnum.Vejez)
                {
                    newScale = scale*1.5f;
                }
                gameObject.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
            return metabolism;
        }

        /// <summary>
        /// Muere el agente
        /// </summary>
        protected override void Die()
        {
            state = States.Die;
            GetComponent<DinasorsAnimationCorrector>().die();
            defense = 0;
            if (gameObject.transform.Find("leaderLigth") != null)
            {
                Destroy(gameObject.transform.Find("leaderLigth").gameObject);
            }

            if (herd.Contains(gameObject))
            {
                herd.Remove(gameObject);
            }
            foreach (GameObject go in herd)
            {
                go.GetComponent<DinoObject>().herd.Remove(gameObject);
            }
            if (isLeader == true)
            {
                Prey p = gameObject.GetComponent<Prey>();
                Predator pr = gameObject.GetComponent<Predator>();
                if (p != null)
                {
                    List<Prey> listHerd = new List<Prey>();
                    foreach (GameObject go in p.herd)
                    {
                        listHerd.Add(go.GetComponent<Prey>());
                    }
                    if (listHerd.Count > 0)
                    {
                        p.getNewLeader(listHerd);
                    }
                    else
                    {
                        p.isLeader = false;
                    }
                }
                if (pr != null)
                {
                    List<Predator> listHerd = new List<Predator>();
                    foreach (GameObject go in pr.herd)
                    {
                        listHerd.Add(go.GetComponent<Predator>());
                    }
                    if (listHerd.Count > 0)
                    {
                        pr.getNewLeader(listHerd);
                    }
                    else
                    {
                        pr.isLeader = false;
                    }
                }
            }
        }

        /// <summary>
        /// Obtiene la manada
        /// </summary>
        /// <returns>Retorna la lista de integrantes de la manada</returns>
        public List<Agent> GetHerd()
        {
            List<Agent> lstHerd = new List<Agent>();
            if (!lstHerd.Contains(this))
            {
                lstHerd.Add(this);
            }

            foreach (var e in herd)
            {
                Agent oAgent = e.GetComponent<Agent>();
                if (oAgent != null)
                {
                    lstHerd.Add(oAgent);
                }
            }
            return lstHerd;
        }

        public void OnDrawGizmos()
        {
            var classType = GetType();
            if (classType == typeof(Prey))
            {
                if (GetColliders<Predator>(1).Count > 0)
                {
                    Gizmos.color = new Color(255, 0, 0);
                }
                else
                {
                    Gizmos.color = new Color(255, 255, 0);
                }
            }
            else
            {
                if (GetColliders<Prey>(1).Count > 0)
                {
                    Gizmos.color = new Color(255, 0, 0);
                }
                else
                {
                    Gizmos.color = new Color(0, 255, 255);
                }
            }
            //Gizmos.DrawWireSphere(transform.position, comRange);
            if (nav != null)
            {
                Gizmos.DrawLine(transform.position, nav.destination);
            }
        }

        public void OnDrawGizmosSelected()
        {
            var classType = GetType();
            if (classType == typeof(Prey))
            {
                if (GetColliders<Predator>(1).Count > 0)
                {
                    Gizmos.color = new Color(255, 0, 0, .2f);
                }
                else
                {
                    Gizmos.color = new Color(255, 255, 0, .2f);
                }
            }
            else
            {
                if (GetColliders<Prey>(1).Count > 0)
                {
                    Gizmos.color = new Color(255, 0, 0, .2f);
                }
                else
                {
                    Gizmos.color = new Color(0, 255, 255, .2f);
                }
            }
            Gizmos.DrawSphere(transform.position, comRange);
        }
    }
}