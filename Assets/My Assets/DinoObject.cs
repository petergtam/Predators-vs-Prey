using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using System.Collections.Generic;

namespace Assets.My_Assets
{
    public class DinoObject : MonoBehaviour
    {
        #region Propiedades
        /// <summary>
        /// Salud de la entidad
        /// </summary>
        public float hp;

        /// <summary>
        /// Velocidad de la entidad
        /// </summary>
        public int speed;

        /// <summary>
        /// Rango de comunicacion
        /// </summary>
        public int comRange;

        /// <summary>
        /// Resistencia (nesesaria para correr etc....)
        /// </summary>
        public double stamina;

        /// <summary>
        /// Tiempo de vida en segundos
        /// </summary>
        public float maxLifeTime;

        /// <summary>
        /// Daño que realiza la entidad
        /// </summary>
        public float attack;

        /// <summary>
        /// Nutricion aportada a quien se alimente de la entidad 
        /// </summary>
        public float flesh;

        /// <summary>
        /// Defensa de la entidad.
        /// </summary>
        public float defense;

        /// <summary>
        /// Estado actual de la entidad
        /// </summary>
        public States state;

        /// <summary>
        /// Estado del ciclo de vida de la unidad
        /// </summary>
        public LifeEnum LifeState;

        /// <summary>
        /// Tiempo de vida transcurridos en segundos 
        /// </summary>
        public float lifetime;

        /// <summary>
        /// Indica si este agente es lider
        /// </summary>
        public bool isLeader;

        /// <summary>
        /// Inidica si necesita correr
        /// </summary>
        public bool isNeededRun;

        /// <summary>
        /// Comida actual
        /// </summary>
        public GameObject actualFood;

        /// <summary>
        /// Sistema de navegacion
        /// </summary>
        protected NavMeshAgent nav;

        /// <summary>
        /// Inidca quien es el lider
        /// </summary>
        public GameObject leader;

        /// <summary>
        // sexo del dino (true si es muchachita)
        /// </summary>
        public bool female;

        /// <summary>
        // lista que contiene la manada a la que se es parte
        /// </summary>
        public List<GameObject> herd = new List<GameObject>();

        public float mutation;

        public float crossover;

        public enum States
        {
            Initial,
            ChoosingLeader,
            Searching,
            Moving,
            Hunting,
            Eating,
            Following,
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
        #endregion

        #region Funciones de metabolismo
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
                flesh -= Time.deltaTime * (1 / 5f); //Putrefaccion
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
                Die();
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

        /// <summary>
        /// Muere el agente
        /// </summary>
        protected void Die()
        {
            state = States.Die;
            GetComponent<DinasorsAnimationCorrector>().die();
            defense = 0;
            if (IsMyLeader(gameObject) || isLeader == true)
            {
                Destroy(gameObject.transform.Find("leaderLigth").gameObject);
            }
        }
        #endregion

        #region Funciones de movimiento
        /// <summary>
        /// Indica a que distancia debe de deternerse el agente
        /// </summary>
        protected float StoppingDistance()
        {
            return comRange * ((float)Random.Range(30, 50) / 100);
        }

        /// <summary>
        /// Indica si esta en rango para poder comer
        /// </summary>
        /// <param name="isPrey">Indica si el agente es presa</param>
        /// <returns>Retorna el valor minimo para poder comer</returns>
        protected float DistanceToBite(bool isPrey)
        {
            if (isPrey)
            {
                return ((nav.radius) * transform.localScale.x * 1.3f) + ((actualFood.GetComponent<MeshRenderer>().bounds.size.x) * 1.3f);
            }
            return ((nav.radius) * transform.localScale.x * 1.3f) + ((actualFood.GetComponent<NavMeshAgent>().radius) * actualFood.transform.localScale.x * 1.3f);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pos"></param>
        /// <returns></returns>
        protected Vector3 Dispersal(Vector3 pos)
        {
            pos.x = pos.x + (((float)Random.Range(-50, 50) / 100) * comRange);
            pos.z = pos.z + (((float)Random.Range(-50, 50) / 100) * comRange);
            return pos;
        }

        /// <summary>
        /// Funcion de velocidad del agente
        /// </summary>
        /// <param name="isRun">Indica si el agente esta corriendo</param>
        /// <returns>Regresa la velocidad del agente</returns>
        protected float Velocidad(bool isRun)
        {
            if (isRun)
            {
                return (float)(speed * ((stamina < 50 ? 50 : stamina) / 100.0));
            }
            return (float)((speed / 3.0) * ((stamina < 50 ? 50 : stamina) / 100.0));
        }

        /// <summary>
        /// Funcion que detiene al nav Agent
        /// </summary>
        protected void Stop()
        {
            nav.destination = transform.position;
        }

        /// <summary>
        /// Regresa la distancia desde la pocion actual a el destino seleccionado
        /// </summary>
        /// <returns>Retorna distancia</returns>
        protected float DistanceFromDestination()
        {
            return Vector3.Distance(transform.position, nav.destination);
        }

        /// <summary>
        /// Indica si el agente se debe detener
        /// </summary>
        /// <param name="factor">Factor de paro</param>
        /// <returns>Retorna un valor booleano si el agente de debe de detener</returns>
        protected bool IsOnRangeToStop(float factor)
        {
            return (DistanceFromDestination() < StoppingDistance() * factor);
        }

        /// <summary>
        /// Indica si el agente se debe detener
        /// </summary>
        /// <returns>Retorna un valor booleano si el agente de debe de detener</returns>
        protected bool IsOnRangeToStop()
        {
            return IsOnRangeToStop(1f);
        }
        #endregion

        #region Funciones complementarias
        /// <summary>
        /// Inicializa las variables a valores por default
        /// </summary>
        protected void InitValue()
        {

            //Propiedades fijas
            hp = 100f;
            stamina = 100f;
            lifetime = Random.Range(0, 500);
            isNeededRun = false;
            isLeader = false;
            mutation = 0.3f;
            crossover = 0.5f;

            //Propiedades variables
            comRange = Random.Range(8, 12);
            flesh = Random.Range(300, 700);
            speed = Random.Range(6, 10);
            maxLifeTime = Random.Range(540, 720);
            attack = Random.Range(6, 16);

            if (Random.Range(0, 100) < 50)
            {
                female = true;
            }
            else
            {
                female = false;
            }

            //Fija los parametros iniciales en torno a la escala
            comRange = (int)(comRange * ((float)transform.localScale.x / 0.3));

            //Inicializa el NavMeshAgent
            nav = GetComponent<NavMeshAgent>();
            nav.speed = Velocidad(isNeededRun);
            actualFood = null;
        }

        /// <summary>
        /// Indica si el objeto dado es mi lider
        /// </summary>
        /// <param name="l">Objeto a comparar</param>
        /// <returns>Retorna si el objeto es mi lider</returns>
        protected bool IsMyLeader(GameObject l)
        {
            if (l.GetInstanceID() == leader.GetInstanceID())
                return true;
            return false;
        }

        /// <summary>
        /// Retorna si el gameobject enviado es igual a la entidad actual
        /// </summary>
        /// <param name="g"></param>
        /// <returns>Retorna si el gameobject enviado es igual a la entidad actual</returns>
        protected bool IsMe(GameObject g)
        {
            if (g.GetInstanceID() == gameObject.GetInstanceID())
                return true;
            return false;
        }
        #endregion

        #region Funciones de ataque
        /// <summary>
        /// Funcion que inflige daño al enemigo
        /// </summary>
        protected void BiteEnemy(bool isPrey)
        {
            if (isPrey)
            {
                actualFood.GetComponent<Plant>().hp -= attack / (1f / Time.deltaTime);
            }
            else
            {
                actualFood.GetComponent<Prey>().hp -= (attack / (1f / Time.deltaTime));
            }
        }

        /// <summary>
        /// Funcion de comer al enemigo
        /// </summary>
        protected void EatEnemy(bool isPrey)
        {
            if (isPrey)
            {
                actualFood.GetComponent<Plant>().flesh -= attack * Time.deltaTime;
                if (stamina < 100f)
                {
                    stamina += (attack * Time.deltaTime) / 10;
                }
                else
                {
                    hp += (attack / Time.deltaTime) / 10;
                }
            }
            else
            {
                actualFood.GetComponent<Prey>().flesh -= (attack - actualFood.GetComponent<Prey>().defense) * Time.deltaTime;
                if (stamina < 100f)
                {
                    stamina += (attack * Time.deltaTime) / 10;
                }
                else
                {
                    hp += (attack * Time.deltaTime) / 10;
                }
            }
        }
        #endregion

        #region Comunicacion
        /// <summary>
        /// Funcion para enviar a todos los objetos cercanos
        /// </summary>
        /// <param name="message">Funcion que sera ejecutada en los objetos encontrados</param>
        /// <param name="obj">Parametros para enviar a esa funcion</param>
        public void BroadCast(string message, object obj)
        {
            herd.RemoveAll(item => item == null);
            if (herd.Count > 0)
            {
                foreach (GameObject dino in herd)
                {
                    //Si el dino es de la manada y esta en el rango, se le envia el mensaje
                    if (dino != null && dino.GetComponent<DinoObject>().state != States.Die && Vector3.Distance(dino.transform.position, gameObject.transform.position) <= comRange)
                    {
                        dino.SendMessage(message, (GameObject)obj);
                    }

                }
            }
        }
        #endregion
    }
}
