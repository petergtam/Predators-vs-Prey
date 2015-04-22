using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreySearchFood : MonoBehaviour
{
    private NodesController nodes;
    private FuzzyLogic fLogic;
    private Dictionary<string, double[]> storage;

    private string GetEdgeName(string vertex1, string vertex2)
    {
        string result;
        if (String.CompareOrdinal(vertex1, vertex2) >= 0)
        {
            result = vertex1 + vertex2;
        }
        else
        {
            result = vertex2 + vertex1;
        }
        return result;
    }

    public Vector3 searchForFood(Vector3 actualPosition)
    {
        if (nodes == null)
            setNodesController();
        if (fLogic == null)
            setFuzzyLogic();
        if (storage == null)
            storage = new Dictionary<string, double[]>();



        GameObject actualNode = nodes.getNeartestNode(actualPosition);	//Obtiene el nodo actual
        GameObject[] lstNeighbors = nodes.getNeighbors(actualNode);					//obtiene los nodos vecinos
        foreach (var a in lstNeighbors)
        {
            var name = GetEdgeName(actualNode.GetComponent<PathNode>().name, a.GetComponent<PathNode>().name);
            
            if (storage.ContainsKey(name))
            {
                storage[name][0] = actualNode.GetComponent<PathNode>().getPlants() + a.GetComponent<PathNode>().getPlants() / 2;
                storage[name][1] = actualNode.GetComponent<PathNode>().getPrays() + a.GetComponent<PathNode>().getPrays();
                storage[name][2] = actualNode.GetComponent<PathNode>().getPredators() + a.GetComponent<PathNode>().getPredators();
                storage[name][3] = Time.time;
            }
            else
            {
                double[] nodesData = new double[5];
                nodesData[0] = actualNode.GetComponent<PathNode>().getPlants() + a.GetComponent<PathNode>().getPlants() / 2;
                nodesData[1] = actualNode.GetComponent<PathNode>().getPrays() + a.GetComponent<PathNode>().getPrays();
                nodesData[2] = actualNode.GetComponent<PathNode>().getPredators() + a.GetComponent<PathNode>().getPredators();
                nodesData[3] = Time.time;
                nodesData[4] = Mathf.Abs(actualPosition.magnitude - actualNode.transform.position.magnitude);

                storage.Add(name, nodesData);    
            }
        }

        return searchAStar(actualPosition);
        
        
        
        //double[,] nodesData = formatData(n, neighbors);				//Fomatea la data para enviarlo al fuzzy Logic
        //double[] ret = fLogic.calculate(nodesData);				//Calcula el fuzzy value de los nodos

        //Selecionar el que tiene un mayor fuzzy value
        //int max = 0;
        //for (int i = 0; i < ret.Length; i++)
        //    if (ret[max] <= ret[i])
        //        max = i;

        ////Si fue el ultimo, entonses la pocicion actual es la mejor
        //if (max == ret.Length - 1)
        //    return actualPosition;
        //return neighbors[max].transform.position;
    }

    private void setNodesController()
    {
        nodes = GameObject.Find("Global").GetComponent<NodesController>();
    }

    private void setFuzzyLogic()
    {
        fLogic = GameObject.Find("Global").GetComponent<FuzzyLogic>();
    }
    
    private Vector3 searchAStar(Vector3 actualPosition)
    {
        GameObject n = nodes.getNeartestNode(actualPosition);	//Obtiene el nodo actual
        Stack<GameObject> closedStack = new Stack<GameObject>(); 
        Stack<GameObject> openStack = new Stack<GameObject>(nodes.getNeighbors(n)); //obtiene los nodos vecinos

        float score = heuristicCost(n);
        closedStack.Push(n);
        while (openStack.Count != 0)
        {
            GameObject current = openStack.Pop();
            float currentScore = (Mathf.Abs(actualPosition.magnitude - current.transform.position.magnitude)) + heuristicCost(current);

            if (currentScore < score)
            {
                score = currentScore;
                n = current;
            }

            if (closedStack.Contains(current) == false)
            {
                closedStack.Push(current);
            }
        }
        return n.transform.position;
    }

    private int heuristicCost(GameObject node)
    {
        int numPlants = node.GetComponent<PathNode>().getPlants();
        int numPrays = node.GetComponent<PathNode>().getPrays();
        int numPredator = node.GetComponent<PathNode>().getPredators();

        return (numPlants + numPrays * 10) / numPredator;
    }

    /*
     * FormatData
     * Le da formato a la informacion de los nodos para procesarla
     */
    private double[,] formatData(GameObject actualNode, GameObject[] neighbors)
    {
        double[,] nodesData = new double[3, neighbors.Length + 1];

        //Agrega los vecinos para ser procesados
        for (int i = 0; i < neighbors.Length; i++)
        {
            nodesData[0, i] = neighbors[i].GetComponent<PathNode>().getPlants();
            nodesData[1, i] = actualNode.GetComponent<PathNode>().getPrays() + actualNode.GetComponent<PathNode>().getPredators();
            nodesData[2, i] = 1;
        }
        //Agrega el nodo actual para ser procesado tambien
        nodesData[0, neighbors.Length] = actualNode.GetComponent<PathNode>().getPlants();
        nodesData[1, neighbors.Length] = 1;
        nodesData[2, neighbors.Length] = 1;

        return nodesData;
    }

}
