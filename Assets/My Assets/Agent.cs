using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.My_Assets
{
    public class Agent : DinoObject
    {
        public enum StimulusEnum
        {
            LeaderShip,
            Fear,
            Hungry,
            Mating
        }

        #region Estimulos

        public StimulusEnum SelectStimulu(NeuralNetwork nn)
        {
            if (this.name == "Pedro")
            {
                /*
                nn.training(GetStimulus());
                foreach (var weight in nn.weights)
                {
                    Console.WriteLine(weight);
                }*/
            }
            //return StimulusEnum.Hungry;
            double[] lstEstimulus = GetStimulus();
            if (lstEstimulus[0] > 0)
            {
                return StimulusEnum.Fear;
            }
            else
            {
                return StimulusEnum.Hungry;
            }

            //TODO: Implementar red bayesiana
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
            lstStimulus[2] = isLeader ? GetHungryStimulus() : 0;
            lstStimulus[3] = isLeader ? GetMatingStimulus() : 0;

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
                List<Agent> lstPredator = GetColliders<Predator>();

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
            if (hungryIndicator < 100)//satisfecho!
                return (100 - hungryIndicator)/100;
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

            return matingIndicator;
        }

        #endregion

        /// <summary>
        /// Obtiene los objetos en el rango del agente
        /// </summary>
        /// <typeparam name="T">Tipo del objeto a buscar</typeparam>
        /// <returns>Retorna la lista de objetos que colicionan</returns>
        public List<Agent> GetColliders<T>() where T : Agent
        {
            List<Agent> lstColliders = new List<Agent>();

            //Si el agente actual pertenece a la manada lo agrega
            var classType = GetType();
            if (classType == typeof (T))
            {
                lstColliders.Add(this);
            }

            Collider[] hitColliders = Physics.OverlapSphere(transform.position, comRange);
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
    }
}