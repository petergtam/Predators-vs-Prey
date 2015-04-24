using System.Collections.Generic;
using System.Linq;
using Assets.My_Assets.scripts;
using Assets.My_Assets.scripts.my_scripts;
using UnityEngine;

public class PredatorSearchFood : Search
{
    /// <summary>
    /// Inicializa la red bayesiana
    /// </summary>
    protected override void InitBayesianNetwork()
    {
        BayesianNetwork = new BayesianNetworkPolyTree();

        //Presas
        BayesianNetwork.P["p1"] = .3;
        BayesianNetwork.P["p2"] = .7;

        //Presas en nodos adyasentes
        BayesianNetwork.P["pa1"] = .6;
        BayesianNetwork.P["pa2"] = .4;

        //Predadores
        BayesianNetwork.P["d1"] = .35;
        BayesianNetwork.P["d2"] = .65;

        //Predadores en nodos adyasentes
        BayesianNetwork.P["da1"] = .1;
        BayesianNetwork.P["da2"] = .9;

        //Mover dado planta, predador y presa
        BayesianNetwork.P["m1|p1,pa1,d1,da1"] = .75;
        BayesianNetwork.P["m2|p1,pa1,d1,da1"] = .25;

        BayesianNetwork.P["m1|p1,pa1,d1,da2"] = .85;
        BayesianNetwork.P["m2|p1,pa1,d1,da2"] = .15;

        BayesianNetwork.P["m1|p1,pa1,d2,da1"] = .92;
        BayesianNetwork.P["m2|p1,pa1,d2,da1"] = .07;

        BayesianNetwork.P["m1|p1,pa1,d2,da2"] = .99;
        BayesianNetwork.P["m2|p1,pa1,d2,da2"] = .01;

        BayesianNetwork.P["m1|p1,pa2,d1,da1"] = .70;
        BayesianNetwork.P["m2|p1,pa2,d1,da1"] = .30;

        BayesianNetwork.P["m1|p1,pa2,d1,da2"] = .80;
        BayesianNetwork.P["m2|p1,pa2,d1,da2"] = .20;

        BayesianNetwork.P["m1|p1,pa2,d2,da1"] = .90;
        BayesianNetwork.P["m2|p1,pa2,d2,da1"] = .10;

        BayesianNetwork.P["m1|p1,pa2,d2,da2"] = .95;
        BayesianNetwork.P["m2|p1,pa2,d2,da2"] = .05;

        BayesianNetwork.P["m1|p2,pa1,d1,da1"] = .005;
        BayesianNetwork.P["m2|p2,pa1,d1,da1"] = .995;

        BayesianNetwork.P["m1|p2,pa1,d1,da2"] = .01;
        BayesianNetwork.P["m2|p2,pa1,d1,da2"] = .99;

        BayesianNetwork.P["m1|p2,pa1,d2,da1"] = .10;
        BayesianNetwork.P["m2|p2,pa1,d2,da1"] = .90;

        BayesianNetwork.P["m1|p2,pa1,d2,da2"] = .15;
        BayesianNetwork.P["m2|p2,pa1,d2,da2"] = .85;

        BayesianNetwork.P["m1|p2,pa2,d1,da1"] = .0001;
        BayesianNetwork.P["m2|p2,pa2,d1,da1"] = .9999;

        BayesianNetwork.P["m1|p2,pa2,d1,da2"] = .0005;
        BayesianNetwork.P["m2|p2,pa2,d1,da2"] = .9995;

        BayesianNetwork.P["m1|p2,pa2,d2,da1"] = .005;
        BayesianNetwork.P["m2|p2,pa2,d2,da1"] = .995;

        BayesianNetwork.P["m1|p2,pa2,d2,da2"] = .001;
        BayesianNetwork.P["m2|p2,pa2,d2,da2"] = .999;

        //construccion del grafo
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
        Vertex.AddEdge(ref vertexD, ref vertexM);
        Vertex.AddEdge(ref vertexDA, ref vertexM);
        Vertex.AddEdge(ref vertexP, ref vertexM);
        Vertex.AddEdge(ref vertexPA, ref vertexM);

        BayesianNetwork._root.Add(vertexD);
        BayesianNetwork._root.Add(vertexDA);
        BayesianNetwork._root.Add(vertexP);
        BayesianNetwork._root.Add(vertexPA);

        BayesianNetwork.V.Add(vertexD.Name, vertexD);
        BayesianNetwork.V.Add(vertexDA.Name, vertexDA);
        BayesianNetwork.V.Add(vertexP.Name, vertexP);
        BayesianNetwork.V.Add(vertexPA.Name, vertexPA);
        BayesianNetwork.V.Add(vertexM.Name, vertexM);

        //Metodos
        BayesianNetwork.InitialTree();
    }

    /// <summary>
    /// Funcion de costos de la heuristica sobre el nodo
    /// </summary>
    /// <param name="node">Nodo a obtener valor de heuristica</param>
    /// <param name="actualNode">Nodo actual del agente</param>
    /// <returns>Retorna el valor de la heuristica</returns>
    protected override double HeuristicCost(GameObject node, GameObject actualNode)
    {
        //return 10;
        //Datos del vecino
        const double maxTime = 30;
        List<Edge> lstDatosVecinos = Edges[node.GetComponent<PathNode>().name, actualNode.GetComponent<PathNode>().name];
        List<Edge> lstDatosActuales = lstDatosVecinos.Where(x => Time.time - x.Storage[3] < maxTime).ToList();

        //Nodo a evualuar
        int numPlants = node.GetComponent<PathNode>().getPlants();
        int numPrays = node.name == actualNode.name ? 0 : node.GetComponent<PathNode>().getPrays();
        int numPredator = node.GetComponent<PathNode>().getPredators();

        //Valores usados
        List<Value> lstValues = new List<Value>();
        Vertex vertex;
        //Inicializar red bayesiana
        BayesianNetwork.InitialTree();

        //Indicar si hay predadores
        if (numPredator > 0)
        {
            vertex = BayesianNetwork.V["D"];
            lstValues.Add(vertex.Val[0]);
            BayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
        }
        else
        {
            vertex = BayesianNetwork.V["D"];
            lstValues.Add(vertex.Val[1]);
            BayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
        }

        //Indicar si hay presas 
        if (numPrays > 0)
        {
            vertex = BayesianNetwork.V["P"];
            lstValues.Add(vertex.Val[0]);
            BayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
        }
        else
        {
            vertex = BayesianNetwork.V["P"];
            lstValues.Add(vertex.Val[1]);
            BayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
        }

        if (lstDatosVecinos.Count > 0)
        {
            if (lstDatosVecinos != null && lstDatosActuales.Count/lstDatosVecinos.Count > .4)
            {
                if (lstDatosActuales.Sum(x => x.Storage[1]) > 0)
                {
                    vertex = BayesianNetwork.V["PA"];
                    lstValues.Add(vertex.Val[0]);
                    BayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
                }
                else
                {
                    vertex = BayesianNetwork.V["PA"];
                    lstValues.Add(vertex.Val[1]);
                    BayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
                }

                if (lstDatosActuales.Sum(x => x.Storage[2]) > 0)
                {
                    vertex = BayesianNetwork.V["DA"];
                    lstValues.Add(vertex.Val[0]);
                    BayesianNetwork.UpdateTree(vertex, vertex.Val[0]);
                }
                else
                {
                    vertex = BayesianNetwork.V["DA"];
                    lstValues.Add(vertex.Val[1]);
                    BayesianNetwork.UpdateTree(vertex, vertex.Val[1]);
                }
            }
        }

        string s = "m1|" + lstValues.Aggregate("@", (result, next) => result + "," + next).Replace("@,", "");
        return BayesianNetwork.P[s];
    }
}