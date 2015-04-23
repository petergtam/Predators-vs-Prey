using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        var t = Time.time;
        GameObject actualNode = _nodes.getNeartestNode(actualPosition);	//Obtiene el nodo actual
        GameObject[] lstNeighbors = _nodes.getNeighbors(actualNode);
        FormatData(actualPosition, actualNode, lstNeighbors);

        GameObject result = actualNode;
        double score = HeuristicCost(actualNode,actualNode); //TODO: Cambiar la heuristica 
        foreach (var current in lstNeighbors)
        {
            double currentScore = HeuristicCost(current,actualNode);

            if (currentScore > score)
            {
                score = currentScore;
                result = current;
            }
            else if (currentScore == score)
            {
                if (actualNode.name != current.name)
                {
                    score = currentScore;
                    result = current;
                }
                else if (Mathf.Abs(actualPosition.sqrMagnitude - result.transform.position.sqrMagnitude) > Mathf.Abs(actualPosition.sqrMagnitude - current.transform.position.sqrMagnitude))
                {
                    score = currentScore;
                    result = current;
                }
            }
        }
        var tResult = Time.time - t;
        //Debug.Log(tResult);
        return result.transform.position;
    }

    private double HeuristicCost(GameObject node, GameObject actualNode)
    {
        //return 10;
        //Datos del vecino
        const double maxTime = 20;
        List<Edge> lstDatosVecinos = _edges[node.GetComponent<PathNode>().name];
        List<Edge> lstDatosActuales = lstDatosVecinos.Where(x => Time.time - x.Storage[3] < maxTime).ToList();

        //Nodo a evualuar
        int numPlants = node.GetComponent<PathNode>().getPlants();
        int numPrays = node.name == actualNode.name? 0: node.GetComponent<PathNode>().getPrays();
        int numPredator = node.GetComponent<PathNode>().getPredators();

        //Valores usados
        List<Value> lstValues = new List<Value>();
        Vertex vertex;
        //Inicializar red bayesiana
        _bayesianNetwork.InitialTree();
        
        //Inidicar si hay plantas
        if (numPlants > 150)
        {
            vertex = _bayesianNetwork.V["A"];
            lstValues.Add(vertex.Val[0]);
            _bayesianNetwork.UpdateTree(vertex,vertex.Val[0]);
        }
        else
        {
            vertex = _bayesianNetwork.V["A"];
            lstValues.Add(vertex.Val[1]);
            _bayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
        }

        //Indicar si hay predadores
        if (numPredator > 0)
        {
            vertex = _bayesianNetwork.V["D"];
            lstValues.Add(vertex.Val[0]);
            _bayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
        }
        else
        {
            vertex = _bayesianNetwork.V["D"];
            lstValues.Add(vertex.Val[1]);
            _bayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
        }

        //Indicar si hay presas 
        if (numPrays > 0)
        {
            vertex = _bayesianNetwork.V["P"];
            lstValues.Add(vertex.Val[0]);
            _bayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
        }
        else
        {
            vertex = _bayesianNetwork.V["P"];
            lstValues.Add(vertex.Val[1]);
            _bayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
        }

        
        if (lstDatosVecinos != null && lstDatosActuales.Count / lstDatosVecinos.Count > .4)
        {
            if (lstDatosActuales.Sum(x => x.Storage[1]) > 0)
            {
                vertex = _bayesianNetwork.V["PA"];
                lstValues.Add(vertex.Val[0]);
                _bayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
            }
            else
            {
                vertex = _bayesianNetwork.V["PA"];
                lstValues.Add(vertex.Val[1]);
                _bayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
            }

            if (lstDatosActuales.Sum(x => x.Storage[2]) > 0)
            {
                vertex = _bayesianNetwork.V["DA"];
                lstValues.Add(vertex.Val[0]);
                _bayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
            }
            else
            {
                vertex = _bayesianNetwork.V["DA"];
                lstValues.Add(vertex.Val[1]);
                _bayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
            }
        }
        
        string s = "m1|" + lstValues.Aggregate("@", (result, next) => result + "," + next).Replace("@,", "");
        return _bayesianNetwork.P[s];
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
            nodesData[1] = n.GetComponent<PathNode>().getPrays();
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
        _bayesianNetwork.P["a1"] = .85;
        _bayesianNetwork.P["a2"] = .15;

        //Presas
        _bayesianNetwork.P["p1"] = .3;
        _bayesianNetwork.P["p2"] = .7;

        //Presas en nodos adyasentes
        _bayesianNetwork.P["pa1"] = .6;
        _bayesianNetwork.P["pa2"] = .4;

        //Predadores
        _bayesianNetwork.P["d1"] = .35;
        _bayesianNetwork.P["d2"] = .65;

        //Predadores en nodos adyasentes
        _bayesianNetwork.P["da1"] = .1;
        _bayesianNetwork.P["da2"] = .9;

        //Mover dado planta, predador y presa
        _bayesianNetwork.P["m1|a1,p1,pa1,d1,da1"] = .0003;
        _bayesianNetwork.P["m2|a1,p1,pa1,d1,da1"] = .9997;

        _bayesianNetwork.P["m1|a1,p1,pa1,d1,da2"] = .005;
        _bayesianNetwork.P["m2|a1,p1,pa1,d1,da2"] = .995;

        _bayesianNetwork.P["m1|a1,p1,pa1,d2,da1"] = .3;
        _bayesianNetwork.P["m2|a1,p1,pa1,d2,da1"] = .7;

        _bayesianNetwork.P["m1|a1,p1,pa1,d2,da2"] = .99;
        _bayesianNetwork.P["m2|a1,p1,pa1,d2,da2"] = .01;

        _bayesianNetwork.P["m1|a1,p1,pa2,d1,da1"] = .00005;
        _bayesianNetwork.P["m2|a1,p1,pa2,d1,da1"] = .99995;

        _bayesianNetwork.P["m1|a1,p1,pa2,d1,da2"] = .0008;
        _bayesianNetwork.P["m2|a1,p1,pa2,d1,da2"] = .9992;

        _bayesianNetwork.P["m1|a1,p1,pa2,d2,da1"] = .25;
        _bayesianNetwork.P["m2|a1,p1,pa2,d2,da1"] = .75;

        _bayesianNetwork.P["m1|a1,p1,pa2,d2,da2"] = .95;
        _bayesianNetwork.P["m2|a1,p1,pa2,d2,da2"] = .05;

        _bayesianNetwork.P["m1|a1,p2,pa1,d1,da1"] = .00001;
        _bayesianNetwork.P["m2|a1,p2,pa1,d1,da1"] = .99999;

        _bayesianNetwork.P["m1|a1,p2,pa1,d1,da2"] = .0002;
        _bayesianNetwork.P["m2|a1,p2,pa1,d1,da2"] = .9998;

        _bayesianNetwork.P["m1|a1,p2,pa1,d2,da1"] = .2;
        _bayesianNetwork.P["m2|a1,p2,pa1,d2,da1"] = .8;

        _bayesianNetwork.P["m1|a1,p2,pa1,d2,da2"] = .9;
        _bayesianNetwork.P["m2|a1,p2,pa1,d2,da2"] = .1;

        _bayesianNetwork.P["m1|a1,p2,pa2,d1,da1"] = .000005;
        _bayesianNetwork.P["m2|a1,p2,pa2,d1,da1"] = .999995;

        _bayesianNetwork.P["m1|a1,p2,pa2,d1,da2"] = .0001;
        _bayesianNetwork.P["m2|a1,p2,pa2,d1,da2"] = .9999;

        _bayesianNetwork.P["m1|a1,p2,pa2,d2,da1"] = .15;
        _bayesianNetwork.P["m2|a1,p2,pa2,d2,da1"] = .85;

        _bayesianNetwork.P["m1|a1,p2,pa2,d2,da2"] = .85;
        _bayesianNetwork.P["m2|a1,p2,pa2,d2,da2"] = .15;

        _bayesianNetwork.P["m1|a2,p1,pa1,d1,da1"] = .00003;
        _bayesianNetwork.P["m2|a2,p1,pa1,d1,da1"] = .99997;

        _bayesianNetwork.P["m1|a2,p1,pa1,d1,da2"] = .0005;
        _bayesianNetwork.P["m2|a2,p1,pa1,d1,da2"] = .9995;

        _bayesianNetwork.P["m1|a2,p1,pa1,d2,da1"] = .03;
        _bayesianNetwork.P["m2|a2,p1,pa1,d2,da1"] = .97;

        _bayesianNetwork.P["m1|a2,p1,pa1,d2,da2"] = .099;
        _bayesianNetwork.P["m2|a2,p1,pa1,d2,da2"] = .901;

        _bayesianNetwork.P["m1|a2,p1,pa2,d1,da1"] = .000005;
        _bayesianNetwork.P["m2|a2,p1,pa2,d1,da1"] = .999995;

        _bayesianNetwork.P["m1|a2,p1,pa2,d1,da2"] = .00008;
        _bayesianNetwork.P["m2|a2,p1,pa2,d1,da2"] = .99992;

        _bayesianNetwork.P["m1|a2,p1,pa2,d2,da1"] = .025;
        _bayesianNetwork.P["m2|a2,p1,pa2,d2,da1"] = .975;

        _bayesianNetwork.P["m1|a2,p1,pa2,d2,da2"] = .095;
        _bayesianNetwork.P["m2|a2,p1,pa2,d2,da2"] = .905;

        _bayesianNetwork.P["m1|a2,p2,pa1,d1,da1"] = .000001;
        _bayesianNetwork.P["m2|a2,p2,pa1,d1,da1"] = .999999;

        _bayesianNetwork.P["m1|a2,p2,pa1,d1,da2"] = .00002;
        _bayesianNetwork.P["m2|a2,p2,pa1,d1,da2"] = .99998;

        _bayesianNetwork.P["m1|a2,p2,pa1,d2,da1"] = .02;
        _bayesianNetwork.P["m2|a2,p2,pa1,d2,da1"] = .98;

        _bayesianNetwork.P["m1|a2,p2,pa1,d2,da2"] = .09;
        _bayesianNetwork.P["m2|a2,p2,pa1,d2,da2"] = .91;

        _bayesianNetwork.P["m1|a2,p2,pa2,d1,da1"] = .0000005;
        _bayesianNetwork.P["m2|a2,p2,pa2,d1,da1"] = .9999995;

        _bayesianNetwork.P["m1|a2,p2,pa2,d1,da2"] = .00001;
        _bayesianNetwork.P["m2|a2,p2,pa2,d1,da2"] = .99999;

        _bayesianNetwork.P["m1|a2,p2,pa2,d2,da1"] = .015;
        _bayesianNetwork.P["m2|a2,p2,pa2,d2,da1"] = .985;

        _bayesianNetwork.P["m1|a2,p2,pa2,d2,da2"] = .085;
        _bayesianNetwork.P["m2|a2,p2,pa2,d2,da2"] = .915;
        
        //construccion del grafo
        Vertex vertexA = new Vertex("A");
        vertexA.AddValues("a1", "a2");

        Vertex vertexD = new Vertex("D");
        vertexD.AddValues("d1", "d2");

        Vertex vertexDA = new Vertex("DA");
        vertexDA.AddValues("da1", "da2");

        Vertex vertexP = new Vertex("P");
        vertexP.AddValues("p1", "p2");

        Vertex vertexPA = new Vertex("PA");
        vertexPA.AddValues("pa1", "pa2");

        Vertex vertexM = new Vertex("M");
        vertexM.AddValues("m1", "m2");
        Vertex.AddEdge(ref vertexA, ref vertexM);
        Vertex.AddEdge(ref vertexD, ref vertexM);
        Vertex.AddEdge(ref vertexDA, ref vertexM);
        Vertex.AddEdge(ref vertexP, ref vertexM);
        Vertex.AddEdge(ref vertexPA, ref vertexM);


        _bayesianNetwork._root.Add(vertexA);
        _bayesianNetwork._root.Add(vertexD);
        _bayesianNetwork._root.Add(vertexDA);
        _bayesianNetwork._root.Add(vertexP);
        _bayesianNetwork._root.Add(vertexPA);

        _bayesianNetwork.V.Add(vertexA.Name, vertexA);
        _bayesianNetwork.V.Add(vertexD.Name, vertexD);
        _bayesianNetwork.V.Add(vertexDA.Name, vertexDA);
        _bayesianNetwork.V.Add(vertexP.Name, vertexP);
        _bayesianNetwork.V.Add(vertexPA.Name, vertexPA);
        _bayesianNetwork.V.Add(vertexM.Name, vertexM);

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
        public List<Edge> this[string index, string actual]
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
