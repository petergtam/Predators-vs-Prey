using System.Collections.Generic;
using System.Linq;
using LambdaMessage = System.Collections.Generic.SortedDictionary<string, System.Collections.Generic.SortedDictionary<string, double>>;
using PiMessages = System.Collections.Generic.SortedDictionary<string, System.Collections.Generic.SortedDictionary<string, double>>;


namespace Assets.My_Assets.scripts
{
    public class BayesianNetwork //: MonoBehaviour
    {
        public Probability P;
        public List<Vertex> V;

        private const int Alpha = 2;
        private Vertex _root;
        private Messages _m;
        private List<Vertex> A;
        private List<Value> a;

        public BayesianNetwork()
        {
            P = new Probability();
            V = new List<Vertex>();
            _m = new Messages
            {
                LambdaMessage = new LambdaMessage(),
                PiMessage = new LambdaMessage()
            };
            InitValue();
        }

        private void InitValue()
        {
            P["h1"] = .2;
            P["b1|h1"] = .25;
            P["b1|h2"] = .05;
            P["l1|h1"] = .003;
            P["l1|h2"] = .00005;
            P["c1|l1"] = .6;
            P["c1|l2"] = .02;

            A = new List<Vertex>();
            a = new List<Value>();

            //construccion del grafo
            Vertex v = new Vertex("H");
            v.AddValues("h1", "h2");

            Vertex v1 = new Vertex("B");
            v1.AddValues("b1", "b2");
            Vertex.AddEdge(ref v, ref v1);

            Vertex v2 = new Vertex("L");
            v2.AddValues("l1", "l2");
            Vertex.AddEdge(ref v, ref v2);

            Vertex v3 = new Vertex("C");
            v3.AddValues("c1", "c2");
            Vertex.AddEdge(ref v2, ref v3);

            _root = v;
            V.Add(v);
            V.Add(v1);
            V.Add(v2);
            V.Add(v3);

            //Metodos
            initial_tree();
            update_tree(v2, v2.Val[0]);
            //update_tree(v3, v3.val[0]);
        }
        private string getA()
        {
            if (!a.Any())
                return "\\emptyset";
            string s = "{";
            foreach (var e in a)
            {
                s += e.Name + ",";
            }
            s += "}";
            return s;
        }
        private void send_pi_message(Vertex Z, Vertex X)
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
                        multi *= _m.LambdaMessage[child.Name][zVal.Name];
                    }
                }
                double newValue = zVal.Pi * multi;
                //_m.PiMessage.SetPiMessages(X.Name, zVal.Name, newValue);
            }

            double denomitator = 0;
            List<string> lstNormalize = new List<string>();

            //Calculate pi for x in X and P(x|a)
            for (int index = 0; index < X.Val.Count; index++)
            {
                var x = X.Val[index];
                double sum = 0;

                //Sumatoria de P(x|z) px de x con z
                foreach (var z in Z.Val)
                {
                    string s = x.Name + "|" + z.Name;
                    if (P[s] <= 0) //TODO: Verificar si funciona
                    {
                        string s2 = X.Val[index - 1].Name + "|" + z.Name;
                        P[s] = 1 - P[s2];
                    }
                    sum += P[s] * _m.PiMessage[X.Name][z.Name];
                }

                x.Pi = sum;
                string name = x.Name + "|" + getA();
                P[name] = x.LambdaValue * x.Pi * Alpha;

                denomitator += P[name];
                lstNormalize.Add(name);
            }

            //Normalizar P(x|a)
            foreach (var e in lstNormalize)
            {
                P[e] = P[e] / denomitator;
            }

            //for cada Y en los hijos de X tal que Y no esta en A
            foreach (var Y in X.Childs)
            {
                if (!A.Contains(Y))
                {
                    send_pi_message(X, Y);
                }
            }
        }
        private void send_lambda_message(Vertex Y, Vertex X)
        {
            double denomitator = 0;
            List<string> lstNormalize = new List<string>();

            //Calculate lambday for each x in X
            foreach (var x in X.Val)
            {
                double sum = 0;

                //Sumatoria de P(y|x) por lamba y
                for (int index = 0; index < Y.Val.Count; index++)
                {
                    var y = Y.Val[index];
                    string s = y.Name + "|" + x.Name;
                    if (P[s] <= 0) //TODO: Cambiar
                    {
                        string s2 = Y.Val[index - 1].Name + "|" + x.Name;
                        P[s] = 1 - P[s2];
                    }
                    sum += P[s] * y.LambdaValue;
                }
                //_m.LambdaMessage.SetLambdaMessage(Y.Name, x.Name, sum);



                double multi = 1;
                //Productorias de lambda message de U con valor x
                foreach (var U in X.Childs)
                {
                    multi *= _m.LambdaMessage[U.Name][x.Name];
                }
                x.LambdaValue = multi;

                //Calcular P(x|a)
                string name = x.Name + "|" + getA();
                P[name] = x.LambdaValue * x.Pi * Alpha;

                //Para normalizacion
                denomitator += P[name];
                lstNormalize.Add(name);
            }

            //Normalizar P(x|a)
            foreach (var e in lstNormalize)
            {
                P[e] = P[e] / denomitator;
            }

            //Si X no es root y sus padres no pertenecen a A
            if (X != _root)
            {
                foreach (var Z in X.Parents)
                {
                    if (!A.Contains(Z))
                    {
                        send_lambda_message(X, Z);
                    }
                }
            }

            //for cada W hijo de X que no pertenecen a A y sea diferente de Y
            foreach (var W in X.Childs)
            {
                if (Y.Name != W.Name && !A.Contains(W))
                {
                    send_pi_message(X, W);
                }
            }
        }
        public void initial_tree()
        {
            //Inicializo en vacio
            A = new List<Vertex>();
            a = new List<Value>();

            //for each X in V
            foreach (var X in V)
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
                        //_m.LambdaMessage.SetLambdaMessage(X.Name, z.Name, 1);
                    }
                }
            }

            //for each value r of the root
            for (int i = 0; i < _root.Val.Count; i++)
            {
                var r = _root.Val[i];
                string s = r.Name + "|emptyset"; //P(r|a)
                if (P[r.Name] > 0) //TODO: Verificar si funciona
                {
                    P[s] = P[r.Name];
                    r.Pi = P[r.Name];
                }
                else
                {
                    P[s] = 1 - P[_root.Val[i - 1].Name];
                    r.Pi = 1 - P[_root.Val[i - 1].Name];
                }
            }

            //for each child X of the root
            foreach (var X in _root.Childs)
            {
                send_pi_message(_root, X);
            }
        }
        public void update_tree(Vertex V1, Value v)
        {
            //Agregar valores nuevos
            A.Add(V1);
            a.Add(v);

            //Inicializar valores
            v.LambdaValue = 1;
            v.Pi = 1;
            string s = v.Name + "|" + getA();
            P[s] = 1; //P(v|a1,a2,a3,an)

            //for cada u en val != v
            foreach (var u in V1.Val)
            {
                if (v != u)
                {
                    //Como ya sabes que ocurrio el valor v de V los demas valores no pueden ocurrir
                    u.LambdaValue = 0;
                    u.Pi = 0;
                    string name = u.Name + "|" + getA(); //P(u|a1,a2,a3,an)
                    P[name] = 0;
                }
            }

            //Si V1 no es root y sus padres no pertenecen a A
            if (V1.Name != _root.Name)
            {
                foreach (var Z in V1.Parents)
                {
                    if (!A.Contains(Z))
                    {
                        send_lambda_message(V1, Z);
                    }
                }
            }

            //for cada X hijo de V1 que no pertenecen a A
            foreach (var X in V1.Childs)
            {
                if (!A.Contains(X))
                {
                    send_pi_message(V1, X);
                }
            }
        }

        public class Value
        {
            public string Name { get; set; }
            public double LambdaValue { get; set; }
            public double Pi { get; set; }

            public Value(string prName)
            {
                Name = prName;
                LambdaValue = 0;
                Pi = 0;
            }
        }
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
        }
        private class Messages
        {
            public LambdaMessage LambdaMessage { get; set; }
            public PiMessages PiMessage { get; set; }
        }
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
                    if (!_probability.ContainsKey(index))
                    {
                        _probability.Add(index, 0);
                    }
                    return _probability[index];
                }

                set
                {
                    if (_probability.ContainsKey(index))
                    {
                        _probability[index] = value;
                    }
                    else
                    {
                        _probability.Add(index, value);
                    }
                }
            }
        }
    }
}
