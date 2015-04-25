using System.Collections.Generic;
using UnityEngine;

namespace Assets.My_Assets.scripts.my_scripts
{
    public abstract class Search : MonoBehaviour
    {
        protected NodesController Nodes;
        protected GraphEdges Edges;
        protected BayesianNetworkPolyTree BayesianNetwork;

        /// <summary>
        /// Ejecuta la rutina de buscar comida
        /// </summary>
        /// <param name="actualPosition">Posicion actual del elemento</param>
        /// <returns>Regresa el nuevo vector de destino</returns>
        public Vector3 SearchForFood(Vector3 actualPosition)
        {
            if (Nodes == null)
                Nodes = GameObject.Find("Global").GetComponent<NodesController>();
            if (Edges == null)
                Edges = new GraphEdges();
            if (BayesianNetwork == null)
                InitBayesianNetwork();

            return SearchAStar(actualPosition);
        }

        /// <summary>
        /// Funcion de costos de la heuristica sobre el nodo
        /// </summary>
        /// <param name="node">Nodo a obtener valor de heuristica</param>
        /// <param name="actualNode">Nodo actual del agente</param>
        /// <returns>Retorna el valor de la heuristica</returns>
        protected abstract double HeuristicCost(GameObject node, GameObject actualNode);

        /// <summary>
        /// Inicializa la red bayesiana
        /// </summary>
        protected abstract void InitBayesianNetwork();

        /// <summary>
        /// Algoritmo de busqueda A star
        /// </summary>
        /// <param name="actualPosition">Posicion actual</param>
        /// <returns>Retorna el vector del resultado de la busqueda</returns>
        private Vector3 SearchAStar(Vector3 actualPosition)
        {
            GameObject actualNode = Nodes.getNeartestNode(actualPosition);	//Obtiene el nodo actual
            GameObject[] lstNeighbors = Nodes.getNeighbors(actualNode);
            FormatData(actualNode, lstNeighbors);

            GameObject result = actualNode;
            double score = HeuristicCost(actualNode, actualNode); //TODO: Cambiar la heuristica 
            foreach (var current in lstNeighbors)
            {
                double currentScore = HeuristicCost(current, actualNode);

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
                    else if (Vector3.Distance(actualPosition, result.transform.position) > Vector3.Distance(actualPosition, current.transform.position))
                    {
                        score = currentScore;
                        result = current;
                    }
                }
            }
            return result.transform.position;
        }

        /// <summary>
        /// Da formato a la informacion de los nodos para procesarla y la guarda para futuro procesamiento
        /// </summary>
        /// <param name="actualNode">Nodo actual del agente</param>
        /// <param name="lstNeighbors">Lista de los los vecinos del nodo actual</param>
        private void FormatData(GameObject actualNode, GameObject[] lstNeighbors)
        {
            foreach (var n in lstNeighbors)
            {
                double[] nodesData = new double[5];
                nodesData[0] = actualNode.GetComponent<PathNode>().getPlants() + n.GetComponent<PathNode>().getPlants() / 2;
                nodesData[1] = n.GetComponent<PathNode>().getPrays();
                nodesData[2] = actualNode.GetComponent<PathNode>().getPredators() + n.GetComponent<PathNode>().getPredators();
                nodesData[3] = Time.time;
                nodesData[4] = Vector3.Distance(actualNode.transform.position, n.transform.position);

                Edges.Add(actualNode.GetComponent<PathNode>().name, n.GetComponent<PathNode>().name, nodesData);
            }
        }

        /// <summary>
        /// Estado del grafo que el agente conoce
        /// </summary>
        protected class GraphEdges
        {
            public readonly List<Edge> Edges;

            public GraphEdges()
            {
                Edges = new List<Edge>();
            }

            /// <summary>
            /// Obtiene la lista de aristas en las que incidie nodo
            /// </summary>
            /// <param name="nodo">Nodo a buscar</param>
            /// <returns>Retorna la lista de aristas que conoce el agente que incidie nodo</returns>
            public List<Edge> this[string nodo]
            {
                get
                {
                    List<Edge> lstEdges = new List<Edge>();
                    foreach (var e in Edges)
                    {
                        if (e.NodeA == nodo || e.NodeB == nodo)
                        {
                            lstEdges.Add(e);
                        }
                    }
                    return lstEdges;
                }
            }

            /// <summary>
            /// Obtiene la lista de aristas en las que incidie nodo y que no incide actual
            /// </summary>
            /// <param name="nodo">Nodo a buscar</param>
            /// <param name="actual">Nodo actual</param>
            /// <returns>Retorna la lista de aristas que conoce el agente que incidie nodo y que no incide actual</returns>
            public List<Edge> this[string nodo, string actual]
            {
                get
                {
                    List<Edge> lstEdges = new List<Edge>();
                    foreach (var e in Edges)
                    {
                        if (e.NodeA != actual && e.NodeB != actual)
                        {
                            if (e.NodeA == nodo || e.NodeB == nodo)
                            {
                                lstEdges.Add(e);
                            }
                        }
                    }
                    return lstEdges;
                }
            }

            /// <summary>
            /// Agrega un nuevo elemento al grafo
            /// </summary>
            /// <param name="nodeA">Nodo A a agregar</param>
            /// <param name="nodeB">Nodo B a agregar</param>
            /// <param name="storage">Informacion sobre la arista a agregar</param>
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

                Edge oEdge = new Edge(nodeA, nodeB, storage);
                Edges.Add(oEdge);
            }
        }

        /// <summary>
        /// Arista del grafo Graph Edges
        /// </summary>
        protected class Edge
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
}
