using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using Random = UnityEngine.Random;

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
        }

        /// <summary>
        /// Comida actual
        /// </summary>
        public GameObject actualPredator;

        public float LastMating = 0;

        #region Estimulos

        private static List<double[]> lstEstimulusPrey = new List<double[]>();
        private static List<double[]> lstEstimulusPredator = new List<double[]>(); 
        public StimulusEnum SelectStimulu(NeuralNetwork nn)
        {
            //if (this.identifier != "Pedro") return StimulusEnum.Rest;
            var a = GetStimulus();
            double[] result = null;
            
            


            //TODO: Debug Pedro
            if (this is Prey)
            {
                Agent.lstEstimulusPrey.Add(a);
            }
            else
            {
                Agent.lstEstimulusPredator.Add(a);
            }
            if (state == States.Reagruping && identifier == "Pedro")
            {
                StreamWriter source = File.AppendText("test_prey.txt");
                foreach (var s in lstEstimulusPrey)
                {
                    string str = s.Aggregate("", (current, t) => current + (Math.Round(t,4) + ","));
                    source.WriteLine(str);
                }
                source.WriteLine();
                source.Close();


                source = File.AppendText("test_predator.txt");
                foreach (var s in lstEstimulusPredator)
                {
                    string str = s.Aggregate("", (current, t) => current + (Math.Round(t, 4) + ","));
                    source.WriteLine(str);
                }
                source.WriteLine();
                source.Close();
                state = States.Searching;
            }







            //result = nn.IsNeedTraining() ? nn.Training(a) : nn.Ejecution(a);
            /*var sb2 = new StringBuilder();
            sb2.Append("(");
            if (result != null)
            {
                var count = this is Prey?0:1;
                foreach (var d in result)
                {
                    sb2.Append(d + " ");
                    if (Math.Abs(1-d)<0.04)
                    {
                        sb2.Append(Enum.GetName(typeof(StimulusEnum), count));
                    }
                    sb2.Append(",");
                    count++;
                }
            }
            sb2.Append(")\n");
            Debug.Log("Output (" + identifier + "):" + sb2);*/
            /*var sb = new StringBuilder();
            sb.Append("(");
            foreach (var d in a)
            {
                sb.Append(d + ",");
            }
            sb.Append(")");
            Debug.Log("Output (" + identifier + "):" + sb);*/
            if (this is Prey)
            {
                if (a[1] > 0)
                {
                    return StimulusEnum.Fear;
                }
                else if(a[2] > 0)
                {
                    return StimulusEnum.LeaderShip;
                }
                /*else if (a[4] > 0)
                {
                    return StimulusEnum.Mating;
                }*/
            }
            if (this is Predator)
            {
                if (a[1] > 0)
                {
                    return StimulusEnum.LeaderShip;
                }
                else if (a[3] > 0)
                {
                    return StimulusEnum.Mating;
                }
            }
            return StimulusEnum.Hungry;
            if (result == null) return StimulusEnum.Hungry;
            if(this is Prey)
            {
                if (Math.Abs(1 - result[0]) < 0.05)
                {
                    return StimulusEnum.Fear;
                }
                if (Math.Abs(1 - result[1]) < 0.05)
                {
                    return StimulusEnum.LeaderShip;
                }
                if (Math.Abs(1 - result[2]) < 0.05)
                {
                    return StimulusEnum.Hungry;
                }
                return Math.Abs(1 - result[0]) < 0.04 ? StimulusEnum.Mating : StimulusEnum.Hungry;
            }
            if (Math.Abs(1 - result[0]) < 0.05)
            {
                return StimulusEnum.LeaderShip;
            }
            if (Math.Abs(1 - result[1]) < 0.05)
            {
                return StimulusEnum.Hungry;
            }
            return Math.Abs(1 - result[2]) < 0.05 ? StimulusEnum.Mating : StimulusEnum.Hungry;
        }

        /// <summary>
        /// Obtiene los estimulos del agente en el instante actual
        /// </summary>
        /// <returns>Retorna un arreglo con los estimulos del agente. Miedo, Liderazgo, Hambre y Apareamiento.</returns>
        protected double[] GetStimulus()
        {
            double[] lstStimulus;

			if (this is Prey) {
                lstStimulus = new double[5];
				lstStimulus [0] = 1;
				lstStimulus [1] = GetFearStimulus ();
				lstStimulus [2] = GetLeaderShipStimulus ();
				lstStimulus [3] = isLeader ? GetHungryStimulus () : 0;
				lstStimulus [4] = GetMatingStimulus ();
			} else {
                lstStimulus = new double[4];
				lstStimulus [0] = 1;
				lstStimulus [1] = GetLeaderShipStimulus ();
				lstStimulus [2] = isLeader ? GetHungryStimulus () : 0;
				lstStimulus [3] = GetMatingStimulus ();
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
            List<Agent> lstCharm = GetHerd<Agent>();

            //Cuenta los lideres en la manada
            var leaderCount = 0;
            foreach (var x in lstCharm)
            {
                if (x.isLeader && x.state != States.Die && Vector3.Distance(transform.position, x.transform.position) <= 50f)
                {
                    leaderCount++;
                }
            }
            return leaderCount == 1 ? 0 : 1; 
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
                List<Agent> lstPredator = GetColliders<Predator>(2.0f);
                if (lstPredator.Count > 0)
                    fearIndicator = 1;
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
            List<Agent> lstCharm = GetHerd<Agent>();

            //Se obtiene el promedio de stanmina
            var hungryIndicator = lstCharm.Average(x => x.stamina);

            return (100 - hungryIndicator) / 100;
        }

        /// <summary>
        /// Obtiene el estimulo de apareamiento de 0 - 1
        /// </summary>
        /// <returns>Retorna nivel de apareamiento de la manada</returns>
        private double GetMatingStimulus()
        {
            //Se obtiene la manada
            List<Agent> lstCharm = GetHerd<Agent>();

            //Se obtiene la razon de los agentes que estan en edad de procrear entre el total de la manada
            int count = 0;
            foreach (var x in lstCharm)
            {
                if (x.LifeState == LifeEnum.Adulto && (Time.time - x.LastMating > 40 || x.LastMating <=0))
                {
                    count++;
                }
            }
            double matingIndicator = (double) count/lstCharm.Count;
			return matingIndicator;
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

            if (actualFood != null)
            {
                if (Vector3.Distance(actualFood.transform.position, transform.position) > 2 * comRange)
                {
                    actualFood = null;
                }
            }

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
        protected List<T> GetHerd<T>() where T : Agent
        {
            var lstHerd = new List<T>();
            /*if (!lstHerd.Contains((T) this))
            {
                lstHerd.Add((T) this);
            }*/

            foreach (var e in herd)
            {
                var oAgent = e.GetComponent<T>();
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
                if (GetColliders<Predator>(2.5f).Count > 0)
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
                if (GetColliders<Prey>(2.5f).Count > 0)
                {
                    Gizmos.color = new Color(255, 0, 0, .2f);
                }
                else
                {
                    Gizmos.color = new Color(0, 255, 255, .2f);
                }
            }
            Gizmos.DrawSphere(transform.position, 2.5f * comRange);
        }
    }
}