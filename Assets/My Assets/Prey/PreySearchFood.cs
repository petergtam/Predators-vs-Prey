using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Assets.My_Assets.scripts;

public class PreySearchFood : MonoBehaviour
{
    private NodesController _nodes;
    private GraphEdges _edges;
    private BayesianNetworkPolyTree _bayesianNetwork;

    public Vector3 searchForFood(Vector3 actualPosition)
    {
        if (_nodes == null)
            _nodes = GameObject.Find("Global").GetComponent<NodesController>();
        if (_edges == null)
            _edges = new GraphEdges();
        if (_bayesianNetwork == null)
            InitBayesianNetwork();

        return SearchAStar(actualPosition);
    }


    private Vector3 SearchAStar(Vector3 actualPosition)
    {
        GameObject actualNode = _nodes.getNeartestNode(actualPosition);	//Obtiene el nodo actual
        GameObject[] lstNeighbors = _nodes.getNeighbors(actualNode);
        FormatData(actualPosition, actualNode, lstNeighbors);


        Stack<GameObject> closedStack = new Stack<GameObject>(); 
        Stack<GameObject> openStack = new Stack<GameObject>(_nodes.getNeighbors(actualNode)); //obtiene los nodos vecinos

        float score = HeuristicCost(actualNode);
        closedStack.Push(actualNode);
        while (openStack.Count != 0)
        {
            GameObject current = openStack.Pop();
            float currentScore = (Mathf.Abs(actualPosition.magnitude - current.transform.position.magnitude)) + HeuristicCost(current);

            if (currentScore < score)
            {
                score = currentScore;
                actualNode = current;
            }

            if (closedStack.Contains(current) == false)
            {
                closedStack.Push(current);
            }
        }
        return actualNode.transform.position;
    }

    private int HeuristicCost(GameObject node)
    {
        var datos = _edges[node.GetComponent<PathNode>().name];


        int numPlants = node.GetComponent<PathNode>().getPlants();
        int numPrays = node.GetComponent<PathNode>().getPrays();
        int numPredator = node.GetComponent<PathNode>().getPredators();

        return (numPlants + numPrays * 10) / numPredator;
    }

    /*
     * FormatData
     * Le da formato a la informacion de los nodos para procesarla
     */
    private void FormatData(Vector3 actualPosition, GameObject actualNode, GameObject[] lstNeighbors)
    {
        foreach (var n in lstNeighbors)
        {
            double[] nodesData = new double[5];
            nodesData[0] = actualNode.GetComponent<PathNode>().getPlants() + n.GetComponent<PathNode>().getPlants() / 2;
            nodesData[1] = actualNode.GetComponent<PathNode>().getPrays() + n.GetComponent<PathNode>().getPrays();
            nodesData[2] = actualNode.GetComponent<PathNode>().getPredators() + n.GetComponent<PathNode>().getPredators();
            nodesData[3] = Time.time;
            nodesData[4] = Mathf.Abs(actualPosition.magnitude - actualNode.transform.position.magnitude);

            _edges.Add(actualNode.GetComponent<PathNode>().name, n.GetComponent<PathNode>().name, nodesData);
        }
    }
    private void InitBayesianNetwork()
    {
        _bayesianNetwork = new BayesianNetworkPolyTree();

        //Plantas
        _bayesianNetwork.P["t1"] = .85;
        _bayesianNetwork.P["t2"] = .15;

        //Presas
        _bayesianNetwork.P["y1"] = .3;
        _bayesianNetwork.P["y2"] = .7;

        //Predadores
        _bayesianNetwork.P["x1"] = .1;
        _bayesianNetwork.P["x2"] = .9;

        //Mover dado planta, predador y presa
        _bayesianNetwork.P["m1|t1,x1,y1"] = .005;
        _bayesianNetwork.P["m2|t1,x1,y1"] = .995;

        _bayesianNetwork.P["m1|t1,x1,y2"] = .001;
        _bayesianNetwork.P["m2|t1,x1,y2"] = .999;

        _bayesianNetwork.P["m1|t1,x2,y1"] = .95;
        _bayesianNetwork.P["m2|t1,x2,y1"] = .05;

        _bayesianNetwork.P["m1|t1,x2,y2"] = .85;
        _bayesianNetwork.P["m2|t1,x2,y2"] = .15;

        _bayesianNetwork.P["m1|t2,x1,y1"] = .002;
        _bayesianNetwork.P["m2|t2,x1,y1"] = .998;

        _bayesianNetwork.P["m1|t2,x1,y2"] = .0005;
        _bayesianNetwork.P["m2|t2,x1,y2"] = .9995;

        _bayesianNetwork.P["m1|t2,x2,y1"] = .4;
        _bayesianNetwork.P["m2|t2,x2,y1"] = .6;

        _bayesianNetwork.P["m1|t2,x2,y2"] = .008;
        _bayesianNetwork.P["m2|t2,x2,y2"] = .992;

        //construccion del grafo
        Vertex vertexT = new Vertex("T");
        vertexT.AddValues("t1", "t2");

        Vertex vertexX = new Vertex("X");
        vertexX.AddValues("x1", "x2");

        Vertex vertexY = new Vertex("Y");
        vertexY.AddValues("y1", "y2");

        Vertex vertexM = new Vertex("M");
        vertexM.AddValues("m1", "m2");
        Vertex.AddEdge(ref vertexT, ref vertexM);
        Vertex.AddEdge(ref vertexX, ref vertexM);
        Vertex.AddEdge(ref vertexY, ref vertexM);


        _bayesianNetwork._root.Add(vertexT);
        _bayesianNetwork._root.Add(vertexX);
        _bayesianNetwork._root.Add(vertexY);
        _bayesianNetwork.V.Add(vertexT);
        _bayesianNetwork.V.Add(vertexX);
        _bayesianNetwork.V.Add(vertexY);
        _bayesianNetwork.V.Add(vertexM);

        //Metodos
        _bayesianNetwork.InitialTree();
    }
    private class GraphEdges
    {
        public readonly List<Edge> Edges;

        public GraphEdges()
        {
            Edges = new List<Edge>();
        }

        public List<Edge> this[string index]
        {
            get
            {
                List<Edge> lstEdges = new List<Edge>();
                foreach (var e in Edges)
                {
                    if (e.NodeA == index || e.NodeB == index)
                    {
                        lstEdges.Add(e);
                    }
                }
                return lstEdges;
            }
        }

        public void Add(string nodeA, string nodeB, double[] storage)
        {
            foreach (var e in Edges)
            {
                if ((e.NodeA == nodeA && e.NodeB == nodeB) || (e.NodeA == nodeB && e.NodeB == nodeA))
                {
                    e.Storage = storage;
                    return;
                }
            }

            Edge oEdge = new Edge(nodeA,nodeB,storage);
            Edges.Add(oEdge);
        }
    }
    private class Edge
    {
        public string NodeA { get; set; }
        public string NodeB { get; set; }
        public double[] Storage { get; set; }

        public Edge(string nodeA, string nodeB, double[] storage)
        {
            NodeB = nodeB;
            NodeA = nodeA;
            Storage = storage;
        }

        public override string ToString()
        {
            return NodeA + "-" + NodeB + " , " + Storage;
        }
    }
}
