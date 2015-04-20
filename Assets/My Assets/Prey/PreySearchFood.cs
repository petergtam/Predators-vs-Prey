﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreySearchFood : MonoBehaviour
{
    private NodesController nodes;
    private FuzzyLogic fLogic;

    public Vector3 searchForFood(Vector3 actualPosition)
    {
        if (nodes == null)
            setNodesController();
        if (fLogic == null)
            setFuzzyLogic();

        return searchAStar(actualPosition);

//        GameObject n = nodes.getNeartestNode(actualPosition);	//Obtiene el nodo actual
//        GameObject[] neighbors = nodes.getNeighbors(n);					//obtiene los nodos vecinos
//        double[,] nodesData = formatData(n, neighbors);				//Fomatea la data para enviarlo al fuzzy Logic
//        double[] ret = fLogic.calculate(nodesData);				//Calcula el fuzzy value de los nodos
//
//        //Selecionar el que tiene un mayor fuzzy value
//        int max = 0;
//        for (int i = 0; i < ret.Length; i++)
//            if (ret[max] <= ret[i])
//                max = i;
//
//        //Si fue el ultimo, entonses la pocicion actual es la mejor
//        if (max == ret.Length - 1)
//            return actualPosition;
//        return neighbors[max].transform.position;
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
