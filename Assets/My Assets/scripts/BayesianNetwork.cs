using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using LambdaMessage = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, double>>;
using PiMessages = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, double>>;

namespace Assets.My_Assets.scripts
{
    public class BayesianNetworkPolyTree
    {
        #region Propiedades
        public Probability P;
        public Dictionary<string,Vertex> V;

        private const int Alpha = 1;
        public readonly List<Vertex> _root;
        private readonly Messages _messages;
        private List<Vertex> A;
        private List<Value> a;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructor de la clase
        /// </summary>
        public BayesianNetworkPolyTree()
        {
            P = new Probability();
            V = new Dictionary<string, Vertex>();
            A = new List<Vertex>();
            a = new List<Value>();
            _root = new List<Vertex>();

            _messages = new Messages
            {
                LambdaMessage = new LambdaMessage(),
                PiMessage = new LambdaMessage()
            };
            InitValue();
        }

        /// <summary>
        /// Inicializa la red bayesiana usada en el proyecto
        /// </summary>
        private void InitValue()
        {
            /*
            //Plantas
            P["t1"] = .85;
            P["t2"] = .15;

            //Presas
            P["y1"] = .3;
            P["y2"] = .7;

            //Predadores
            P["x1"] = .1;
            P["x2"] = .9;

            //Mover dado planta, predador y presa
            P["m1|t1,x1,y1"] = .005;
            P["m2|t1,x1,y1"] = .995;

            P["m1|t1,x1,y2"] = .001;
            P["m2|t1,x1,y2"] = .999;

            P["m1|t1,x2,y1"] = .95;
            P["m2|t1,x2,y1"] = .05;

            P["m1|t1,x2,y2"] = .85;
            P["m2|t1,x2,y2"] = .15;

            P["m1|t2,x1,y1"] = .002;
            P["m2|t2,x1,y1"] = .998;

            P["m1|t2,x1,y2"] = .0005;
            P["m2|t2,x1,y2"] = .9995;

            P["m1|t2,x2,y1"] = .4;
            P["m2|t2,x2,y1"] = .6;

            P["m1|t2,x2,y2"] = .008;
            P["m2|t2,x2,y2"] = .992;

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


            _root.Add(vertexT);
            _root.Add(vertexX);
            _root.Add(vertexY);
            V.Add(vertexT.Name,vertexT);
            V.Add(vertexX.Name,vertexX);
            V.Add(vertexY.Name,vertexY);
            V.Add(vertexM.Name,vertexM);

            //Metodos
            InitialTree();
            UpdateTree(vertexT, vertexT.Val[0]);
            UpdateTree(vertexX, vertexX.Val[0]);
            UpdateTree(vertexY, vertexY.Val[0]);
            */
        }
        #endregion

        #region Metodos de la red bayesiana
        /// <summary>
        /// Inicializa la red bayesiana
        /// </summary>
        public void InitialTree()
        {
            //Inicializo en vacio
            A = new List<Vertex>();
            a = new List<Value>();

            //for each X in V
            foreach (var X in V.Values)
            {
                //for each value on x of X
                foreach (var x in X.Val)
                {
                    x.LambdaValue = 1;
                }

                //for each parent Z of X
                foreach (var Z in X.Parents)
                {
                    foreach (var z in Z.Val)
                    {
                        SetLambdaMessage(X.Name, z.Name, 1);
                    }
                }

                //for each child Y of X
                foreach (var Y in X.Childs)
                {
                    foreach (var x in X.Val)
                    {
                        SetPiMessages(Y.Name, x.Name, 1);
                    }
                }
            }

            //for each value r of the root
            foreach (var R in _root)
            {
                foreach (var r in R.Val)
                {
                    string s = r.Name + "|emptyset"; //P(r|a)
                    P[s] = P[r.Name];
                    r.PiValue = P[r.Name];
                }

                //for each child X of the root
                foreach (var X in R.Childs)
                {
                    SendPiMessage(R, X);
                }
            }
        }

        /// <summary>
        /// Actualiza los valores de la red bayesiana dado V1 con el valor v
        /// </summary>
        /// <param name="V1">Vertice del evento que se cumple</param>
        /// <param name="v">Valor a cumplir</param>
        public void UpdateTree(Vertex V1, Value v)
        {
            //Agregar valores nuevos
            A.Add(V1);
            a.Add(v);

            //Inicializar valores
            v.LambdaValue = 1;
            v.PiValue = 1;
            string s = v.Name + "|" + Get_a();
            P[s] = 1; //P(v|a1,a2,a3,an)

            //for cada u en val != v
            foreach (var u in V1.Val)
            {
                if (v != u)
                {
                    //Como ya sabes que ocurrio el valor v de V los demas valores no pueden ocurrir
                    u.LambdaValue = 0;
                    u.PiValue = 0;
                    string name = u.Name + "|" + Get_a(); //P(u|a1,a2,a3,an)
                    P[name] = 0;
                }
            }

            //Si V1 no es root y sus padres no pertenecen a A
            foreach (var R in _root)
            {
                if (V1.Name != R.Name)
                {
                    foreach (var Z in V1.Parents)
                    {
                        if (!A.Contains(Z))
                        {
                            SendLambdaMessage(V1, Z);
                        }
                    }
                }
            }

            //for cada X hijo de V1
            foreach (var X in V1.Childs)
            {
                SendPiMessage(V1, X);
            }
        }

        /// <summary>
        /// Paso de mensajes hacia arriba de la red para hacer backtraking de las probabilidades
        /// </summary>
        /// <param name="Z">Vertice</param>
        /// <param name="X">Vertice</param>
        private void SendPiMessage(Vertex Z, Vertex X)
        {
            //Calculate pix for valor val in Z
            foreach (var zVal in Z.Val)
            {
                //Se optiene el pi message de x con el valor val con los valores de los hijos de Z
                double multi = 1;
                foreach (var child in Z.Childs)
                {
                    if (X != child)
                    {
                        multi *= _messages.LambdaMessage[child.Name][zVal.Name];
                    }
                }
                double newValue = zVal.PiValue * multi;
                SetPiMessages(X.Name, zVal.Name, newValue);
            }

            //Si no contiene A
            if (!A.Contains(X))
            {
                double denomitator = 0;
                List<string> lstNormalize = new List<string>();

                //Para cada valor de X
                foreach (var xVal in X.Val)
                {
                    //Para todos los valores de las probabilidades posibles de los padres de X
                    double sum = 0;
                    foreach (var permutation in Vertex.GetValuePermutations(X.Parents))
                    {
                        string s = xVal.Name + "|";
                        double productioria = 1;
                        for (int i = 0; i < permutation.Count; i++)
                        {
                            var val = permutation[i];
                            productioria *= _messages.PiMessage[X.Name][val.Name];
                            s += i == permutation.Count - 1 ? val.Name : val.Name + ",";
                        }
                        sum += P[s] * productioria;
                    }
                    xVal.PiValue = sum;

                    string name = xVal.Name + "|" + Get_a();
                    P[name] = xVal.LambdaValue * xVal.PiValue * Alpha;

                    //Valores para la normalizacion
                    denomitator += P[name];
                    lstNormalize.Add(name);
                }

                //Normalizar P(x|a)
                foreach (var e in lstNormalize)
                {
                    P[e] = P[e] / denomitator;
                }

                //for cada Y en los hijos de X
                foreach (var Y in X.Childs)
                {
                    SendPiMessage(X, Y);
                }
            }

            //Si no todos los valores lambda de x son igual a 1
            bool condition = false;
            foreach (var xVal in X.Val)
            {
                if (xVal.LambdaValue >= 1)
                {
                    condition = true;
                }
            }

            //Enviar mensajes a todos los W que son padres de X y no son Z ni estan A
            if (condition)
            {
                foreach (var W in X.Parents)
                {
                    if (W.Name != Z.Name && A.Contains(W) == false)
                    {
                        SendLambdaMessage(X, W);
                    }
                }
            }
        }

        /// <summary>
        /// Paso de mensajes hacia abajo de la red para hacer backtraking de las probabilidades
        /// </summary>
        /// <param name="Y">Vertice</param>
        /// <param name="X">Vertice</param>
        private void SendLambdaMessage(Vertex Y, Vertex X)
        {
            double denomitator = 0;
            List<string> lstNormalize = new List<string>();

            //Para cada valor de X
            foreach (var xVal in X.Val)
            {
                //Sumatora para cada valor de Y
                double sumYVals = 0;
                foreach (var yVal in Y.Val)
                {
                    //Para todos los valores de las probabilidades posibles de los padres de Y diferentes a X
                    double sum = 0;
                    List<Vertex> lstParent = Y.Parents.Where(parent => parent.Name != X.Name).ToList();
                    foreach (var permutation in Vertex.GetValuePermutations(lstParent))
                    {
                        string s = yVal.Name + "|" + xVal.Name + ",";
                        double productioria = 1;
                        for (int i = 0; i < permutation.Count; i++)
                        {
                            var val = permutation[i];
                            productioria *= _messages.PiMessage[Y.Name][val.Name];
                            s += i == permutation.Count - 1 ? val.Name : val.Name + ",";
                        }
                        sum += P[s] * productioria;
                    }
                    sumYVals += sum * yVal.LambdaValue;
                }
                SetLambdaMessage(Y.Name, xVal.Name, sumYVals);

                //Para cada hijo U de X multiplicar los mensajes lambd de U con el valor xVal
                double productoria = 1;
                foreach (var U in X.Childs)
                {
                    productoria *= _messages.LambdaMessage[U.Name][xVal.Name];
                }
                xVal.LambdaValue = productoria;

                //P(x|{a})
                string name = xVal.Name + "|" + Get_a();
                P[name] = xVal.LambdaValue * xVal.PiValue * Alpha;

                //Valores para la normalizacion
                denomitator += P[name];
                lstNormalize.Add(name);
            }

            //Normalizar P(x|a)
            foreach (var e in lstNormalize)
            {
                P[e] = P[e] / denomitator;
            }

            //Para cada padre Z de X que no esta en A
            foreach (var Z in X.Parents)
            {
                if (!A.Contains(Z))
                {
                    SendLambdaMessage(X, Z);
                }
            }

            //for cada W hijo de X que sea diferente de Y
            foreach (var W in X.Childs)
            {
                if (Y.Name != W.Name)
                {
                    SendPiMessage(X, W);
                }
            }
        }
        #endregion

        #region Metodos auxiliares
        /// <summary>
        /// Actualiza los lambda mensajes de la red
        /// </summary>
        /// <param name="childName">Nombre del nodo hijo</param>
        /// <param name="parentVal">Nombre del valor del padre</param>
        /// <param name="val">Valor a actualizar</param>
        private void SetLambdaMessage(string childName, string parentVal, double val)
        {
            if (_messages.LambdaMessage.ContainsKey(childName))
            {
                if (_messages.LambdaMessage[childName].ContainsKey(parentVal))
                {
                    _messages.LambdaMessage[childName][parentVal] = val;
                }
                else
                {
                    _messages.LambdaMessage[childName].Add(parentVal, val);
                }
            }
            else
            {
                Dictionary<string, double> dir = new Dictionary<string, double>
                {
                    {parentVal, val}
                };

                _messages.LambdaMessage.Add(childName, dir);
            }
        }

        /// <summary>
        /// Actualiza los pi mensajes de la red
        /// </summary>
        /// <param name="parentName">Nombre del nodo padre</param>
        /// <param name="childVal">Nombre del valor del hijo</param>
        /// <param name="val">Valor a actualizar</param>
        private void SetPiMessages(string parentName, string childVal, double val)
        {
            if (_messages.PiMessage.ContainsKey(parentName))
            {
                if (_messages.PiMessage[parentName].ContainsKey(childVal))
                {
                    _messages.PiMessage[parentName][childVal] = val;
                }
                else
                {
                    _messages.PiMessage[parentName].Add(childVal, val);
                }
            }
            else
            {
                Dictionary<string, double> dir = new Dictionary<string, double>
                {
                    {childVal, val}
                };

                _messages.PiMessage.Add(parentName, dir);
            }
        }

        /// <summary>
        /// Obtiene el contenido del conjunto A en formato string
        /// </summary>
        /// <returns>Retorna los valores del conjunto A</returns>
        private string Get_a()
        {
            if (!a.Any())
                return "emptyset";
            return a.Aggregate("@", (x, y) => x + "," + y).Replace("@,", "");
        }
        #endregion
    }
    #region Clases auxiliares
    /// <summary>
    /// Estructura de dato del valor de las variables aleatorias
    /// </summary>
    public class Value
    {
        public string Name { get; set; }
        public double LambdaValue { get; set; }
        public double PiValue { get; set; }

        public Value(string prName)
        {
            Name = prName;
            LambdaValue = 0;
            PiValue = 0;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Vertice del DAG
    /// </summary>
    public class Vertex
    {
        public string Name { get; set; }
        public List<Value> Val { get; set; }
        public List<Vertex> Parents { get; set; }
        public List<Vertex> Childs { get; set; }

        public Vertex(string prName)
        {
            Name = prName;
            Val = new List<Value>();
            Parents = new List<Vertex>();
            Childs = new List<Vertex>();
        }

        public void AddValues(params Value[] lstValues)
        {
            foreach (var v in lstValues)
            {
                Val.Add(v);
            }
        }
        public void AddValues(params string[] lstValues)
        {
            foreach (var s in lstValues)
            {
                Val.Add(new Value(s));
            }
        }
        public static void AddEdge(ref Vertex padre, ref Vertex hijo)
        {
            hijo.Parents.Add(padre);
            padre.Childs.Add(hijo);
        }

        private static int[] NextValuePermutation(int[] finalPermutation, int[] currentPermutation, int index)
        {
            int[] permutation = currentPermutation;
            if (index < finalPermutation.Length)
            {
                if (permutation[index] == finalPermutation[index] - 1)
                {
                    permutation = NextValuePermutation(finalPermutation, currentPermutation, index + 1);
                }
                else
                {
                    permutation[index]++;

                    for (int i = 0; i < index; i++)
                    {
                        permutation[i] = 0;
                    }
                }
            }
            return permutation;
        }
        public static List<List<Value>> GetValuePermutations(List<Vertex> lstVertices)
        {
            List<List<Value>> lstPermutations = new List<List<Value>>();

            int[] finalPermutation = lstVertices.Select(x => x.Val.Count).ToArray();
            int[] initialPermutation = lstVertices.Select(x => 0).ToArray();

            int maxPermutation = lstVertices.Aggregate(1, (current, vertex) => current * vertex.Val.Count);
            for (int i = 0; i < maxPermutation; i++)
            {
                List<Value> permutation = new List<Value>();
                for (int j = 0; j < initialPermutation.Length; j++)
                {
                    permutation.Add(lstVertices[j].Val[initialPermutation[j]]);
                }
                lstPermutations.Add(permutation);
                initialPermutation = NextValuePermutation(finalPermutation, initialPermutation, 0);
            }
            return lstPermutations;
        }

        /// <summary>
        /// Metodo auxiliar para impresion
        /// </summary>
        /// <returns>Retorna un string formateado</returns>
        public override string ToString()
        {
            string s = Name + ",{";
            for (int i = 0; i < Val.Count; i++)
            {
                var v = Val[i];
                s += v.Name;
                if (i != Val.Count - 1)
                {
                    s += ",";
                }
            }
            return s + "}";
        }
    }

    /// <summary>
    /// Contenedor de mensajes lambda y pi de la red bayesiana
    /// </summary>
    public class Messages
    {
        public LambdaMessage LambdaMessage { get; set; }
        public PiMessages PiMessage { get; set; }
    }

    /// <summary>
    /// Contenedor de las probabilidades de la red bayesiana
    /// </summary>
    public class Probability
    {
        private readonly Dictionary<string, double> _probability;

        public Probability()
        {
            _probability = new Dictionary<string, double>();
        }

        public double this[string index]// Indexer declaration
        {
            get
            {
                string probabilitiString;

                string[] splitStrings = index.Split('|');
                string[] lstPrecedent = splitStrings[0].Split(',');
                string sPrecedent = lstPrecedent.OrderBy(x => x).Aggregate("@", (a, b) => a + "," + b);

                if (splitStrings.Length > 1)
                {
                    string[] lstConsequent = splitStrings[1].Split(',');
                    var sConsequent = lstConsequent.OrderBy(x => x).Aggregate("@", (a, b) => a + "," + b);

                    probabilitiString = (sPrecedent + "|" + sConsequent).Replace("@,", "");
                }
                else
                {
                    probabilitiString = (sPrecedent).Replace("@,", "");
                }

                if (!_probability.ContainsKey(probabilitiString))
                {
                    _probability.Add(probabilitiString, 0);
                }
                return _probability[probabilitiString];
            }

            set
            {
                string probabilitiString;

                string[] splitStrings = index.Split('|');
                string[] lstPrecedent = splitStrings[0].Split(',');
                string sPrecedent = lstPrecedent.OrderBy(x => x).Aggregate("@", (a, b) => a + "," + b);

                if (splitStrings.Length > 1)
                {
                    string[] lstConsequent = splitStrings[1].Split(',');
                    var sConsequent = lstConsequent.OrderBy(x => x).Aggregate("@", (a, b) => a + "," + b);

                    probabilitiString = (sPrecedent + "|" + sConsequent).Replace("@,", "");
                }
                else
                {
                    probabilitiString = (sPrecedent).Replace("@,", "");
                }

                if (_probability.ContainsKey(probabilitiString))
                {
                    _probability[probabilitiString] = value;
                }
                else
                {
                    _probability.Add(probabilitiString, value);
                }
            }
        }
    }
    #endregion
}
