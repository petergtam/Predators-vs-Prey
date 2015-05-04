using System;
using System.Collections.Generic;
using Assets.My_Assets;
using UnityEngine;
using Random = UnityEngine.Random;


public class MatingPredator : MonoBehaviour
{
    public Predator Hijo;
    private List<Predator> herdList;

    //regresa la lista con 2 candidatos a reproducirse
    //valida que la lista de candidatos sea mayor que 1
    //valida que los candidatos tengan estamina mayor al 50%
    public void Procreate(List<GameObject> lstHerd)
    {
        // solo si la lista es mayor a uno realiza el procedimiento.
        if (lstHerd.Count > 1)
        {
            herdList = new List<Predator>();

            foreach (var p in lstHerd)
            {
                herdList.Add(p.GetComponent<Predator>());
            }

            int count = 0;

            //valida la cantidad de estamina y la edad necesarias para reproducirse.
            for (int index = 0; index < herdList.Count; index++)
            {
                var x = herdList[index];
                var stamina = herdList[count].stamina;
                if (stamina < 50f || x.LifeState != DinoObject.LifeEnum.Adulto || (Time.time - x.LastMating <= 40 && x.LastMating > 0))
                {
                    //elimina los elementos que no cumplen condición.
                    herdList.Remove(herdList[count]);
                }
                count++;
            }
            if (herdList.Count > 2)
            {
                //elimina aleatoriamente elementos de la lista hasta que solo queden dos.
                while (herdList.Count > 2)
                {
                    int random = Random.Range(0, herdList.Count);
                    herdList.Remove(herdList[random]);
                }
                Debug.Log("Entro");
                ObtenerCarac();
                Instantiate(Hijo, herdList[0].transform.position, Quaternion.identity);
                herdList[0].LastMating = Time.time;
                herdList[1].LastMating = Time.time;
            }
        }
    }

    // crea el objeto y asigna valores según las características obtenidas.
    private void ObtenerCarac()
    {
        int hp, stamina, speed, flesh, attack, comRange, maxLifeTime = 0;
        List<int> caracateristicas = new List<int>();
        //se obtiene la lista enviada por el metodo cruce
        caracateristicas = Cruce();
        if (caracateristicas[0] > 80)
        {
            hp = 100;
            Hijo.hp = hp;
        }
        else
        {
            hp = Random.Range(60, 80);
            Hijo.hp = hp;
        }
        if (caracateristicas[1] > 80)
        {
            stamina = 100;
            Hijo.stamina = stamina;
        }
        else
        {
            stamina = Random.Range(60, 80);
            Hijo.stamina = stamina;
        }
        if (caracateristicas[2] > 8)
        {
            speed = Random.Range(8, 10);
            Hijo.speed = speed;
        }
        else
        {
            speed = Random.Range(6, 7);
            Hijo.speed = speed;
        }
        if (caracateristicas[3] > 500)
        {
            flesh = Random.Range(501, 800);
            Hijo.flesh = flesh;
        }
        else
        {
            flesh = Random.Range(400, 500);
            Hijo.flesh = flesh;
        }
        if (caracateristicas[4] > 10)
        {
            attack = Random.Range(10, 16);
            Hijo.attack = attack;
        }
        else
        {
            attack = Random.Range(6, 9);
            Hijo.attack = attack;
        }
        if (caracateristicas[5] > 10)
        {
            comRange = Random.Range(10, 12);
            Hijo.comRange = comRange;
        }
        else
        {
            comRange = Random.Range(8, 9);
            Hijo.comRange = comRange;
        }
        if (caracateristicas[6] > 650)
        {
            maxLifeTime = Random.Range(540, 720);
            Hijo.maxLifeTime = maxLifeTime;
        }
        else
        {
            maxLifeTime = Random.Range(540, 720);
            Hijo.maxLifeTime = maxLifeTime;
        }
        Hijo.defense = (Random.Range(0, 5));
        Hijo.lifetime = 0;
        Hijo.isLeader = false;
        Hijo.state = DinoObject.States.Following;
        //Fija los parametros iniciales en torno a la escala
        Hijo.comRange = (int)(comRange * ((float)transform.localScale.x / 0.3));
        Hijo.LastMating = 0;
    }

    // obtiene el cromosoma segun las caracteristicas, si tiene un buen porcentaje-->1 sino ---->0.
    private List<int> ObtineCromosomaPadre()
    {
        List<int> padre = new List<int>();
        padre.Add(herdList[0].hp > 80f ? 1 : 0); //100
        padre.Add(herdList[0].stamina > 80f ? 1 : 0); //100
        padre.Add(herdList[0].speed > 8 ? 1 : 0); //6-10
        padre.Add(herdList[0].flesh > 500 ? 1 : 0); //300-800
        padre.Add(herdList[0].attack > 10 ? 1 : 0); //6-16
        padre.Add(herdList[0].comRange > 10 ? 1 : 0); //8-12
        padre.Add(herdList[0].maxLifeTime > 650 ? 1 : 0); //540-720

        return padre;
    }

    // obtiene el cromosoma segun las caracteristicas, si tiene un buen porcentaje-->1 sino ---->0.
    private List<int> ObtineCromosomaMadre()
    {
        List<int> madre = new List<int>();
        madre.Add(herdList[1].hp > 80f ? 1 : 0);
        madre.Add(herdList[1].stamina > 80f ? 1 : 0);
        madre.Add(herdList[1].speed > 8 ? 1 : 0);
        madre.Add(herdList[1].flesh > 500 ? 1 : 0);
        madre.Add(herdList[1].attack > 10 ? 1 : 0);
        madre.Add(herdList[1].comRange > 10 ? 1 : 0);
        madre.Add(herdList[1].maxLifeTime > 650 ? 1 : 0);

        return madre;
    }



    // realiza un cruce entre un rango
    //obtiene el rango de corte para las listas, antes del punto de corte permanece igual, despues
    //del punto de corte se realiza el cruce.
    private List<int> Cruce()
    {

        int random = Random.Range(2, 6);
        int conC = 0;
        int conD = 0;
        List<int> a = new List<int>();
        List<int> b = new List<int>();
        List<int> c = new List<int>();
        List<int> d = new List<int>();
        a = ObtineCromosomaMadre();
        b = ObtineCromosomaPadre();
        c = a;
        d = b;

        for (int i = a.Count - random; i == a.Count; i++)
        {
            if (a[i] == 1 && b[i] == 0)
            {
                c[i] = 1;
            }
            else
            {
                c[i] = 0;
            }
            if (a[i] == 0 && b[i] == 1)
            {
                d[i] = 1;
            }
            else
            {
                d[i] = 0;
            }

        }

        foreach (int element in c)
        {
            if (element == 1)
            {
                conC++;
            }
        }
        foreach (int element in d)
        {
            if (element == 1)
            {
                conD++;
            }
        }

        if (conC >= conD)
        {
            return (Mutacion(c));
        }
        else
        {
            return (Mutacion(d));
        }

    }

    //realiza la mutació con una probabilidad de .3
    //la mutación se lleva a cabo en la primer parte de la lista que no ha sido cambiada en el cruce.
    private List<int> Mutacion(List<int> mutacion)
    {
        int random = Random.Range(0, 10);
        int random2 = Random.Range(0, 1);
        if (random == 1 || random == 2 || random == 3)
        {
            mutacion[random2] = 1;

        }
        return mutacion;

    }
}