using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace KombAlgTransversile
{
    public class TransportNetwork
    {
        public Node Source;
        public Node Sink;
    }

    public class Node
    {
        public string Name;
        public Edge Parent;
        public List<Edge> Outgoing = new List<Edge>();
        public List<Edge> Ingoing = new List<Edge>();
        public void AddNext(Node next, int capacity)
        {
            var edge = new Edge(capacity);
            edge.Start = this;
            edge.End = next;
            Outgoing.Add(edge);
            next.Ingoing.Add(edge);
        }

        public void AddPrev(Node next, int capacity)
        {
            var edge = new Edge(capacity);
            edge.End = this;
            edge.Start = next;
            Ingoing.Add(edge);
            next.Outgoing.Add(edge);
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class Edge
    {
        public int Capacity;
        public int Flow;
        public Node Start;
        public Node End;
        public bool Deleted;
        public bool Inverted;
        public Edge(int capacity)
        {
            Capacity = capacity;
        }

        public override string ToString()
        {
            return $"{Start.Name}{End.Name}, {Flow}/{Capacity}";
        }
    }

    public static class Algorithm
    {
        public static void EdmondsCarp(TransportNetwork transportNetwork)
        {
            var remaining = transportNetwork;
            while (true)
            {
                var way = DepthSearch(remaining);
                if (way.Count == 0)
                {
                    break;
                }

                var min = int.MaxValue;
                var start = transportNetwork.Source;
                for (int i = 0; i < way.Count; i++)
                {
                    var currentEdge = way[i];
                    if (currentEdge.Start == start)
                    {
                        if ((currentEdge.Capacity - currentEdge.Flow) < min)
                        {
                            min = (currentEdge.Capacity - currentEdge.Flow);
                        }

                        start = currentEdge.End;
                    }
                    else
                    {
                        if ((currentEdge.Flow < min))
                        {
                            min = currentEdge.Flow;
                        }

                        start = currentEdge.End;
                    }

                }

                var end = transportNetwork.Sink;
                for (int i = (way.Count - 1); i >= 0; i--)
                {
                    var currentEdge = way[i];
                    if (currentEdge.End == end)
                    {
                        currentEdge.Flow += min;
                        end = currentEdge.Start;
                        if ((currentEdge.Capacity - currentEdge.Flow) == 0)
                        {
                            currentEdge.Deleted = true;
                        }
                    }
                    else
                    {
                        currentEdge.Flow -= min;
                        end = currentEdge.End;
                        if (currentEdge.Flow == 0)
                        {
                            currentEdge.Deleted = true;
                        }
                    }
                }
            }
        }

        public static List<Edge> DepthSearch(TransportNetwork transportNetwork)
        {
            var visited = new HashSet<Edge>();
            var start = transportNetwork.Source;
            var end = transportNetwork.Sink;
            var queue = new Queue<Node>();
            queue.Enqueue(start);
            bool notFind = true;

            List<Edge> candidates = new List<Edge>();
            while (notFind && queue.Count > 0)
            {
                var current = queue.Dequeue();
                var outg = current.Outgoing.Where(x => !x.Deleted && !visited.Contains(x));
                var ing = current.Ingoing.Where(x => !x.Deleted && !visited.Contains(x));

                foreach (var positive in outg)
                {
                    visited.Add(positive);
                    positive.End.Parent = positive;
                    queue.Enqueue(positive.End);
                    if (positive.End == end)
                    {
                        notFind = false;
                    }
                }

                foreach (var negative in ing)
                {
                    visited.Add(negative);
                    negative.Start.Parent = negative;
                    queue.Enqueue(negative.Start);
                    if (negative.Start == end)
                    {
                        notFind = false;
                    }
                }
            }

            if (notFind)
            {
                return new List<Edge>();
            }

            var temp = end;
            while (true)
            {
                if (temp.Parent == null)
                {
                    return ((IEnumerable<Edge>)candidates).Reverse().ToList();
                }

                candidates.Add(temp.Parent);
                if (temp.Parent.Start == temp)
                {
                    temp = temp.Parent.End;
                }
                else
                {
                    temp = temp.Parent.Start;
                }
            }
        }

        public static List<Edge> DepthSearchWithInversion(TransportNetwork transportNetwork)
        {
            var visited = new HashSet<Edge>();
            var queue = new Queue<Node>();
            queue.Enqueue(transportNetwork.Source);
            bool notFind = true;

            List<Edge> candidates = new List<Edge>();
            while (notFind && queue.Count > 0)
            {
                var current = queue.Dequeue();
                var outg = current.Outgoing.Where(x => !x.Deleted && !visited.Contains(x));

                foreach (var positive in outg)
                {
                    visited.Add(positive);
                    positive.End.Parent = positive;
                    queue.Enqueue(positive.End);
                    if (positive.End == transportNetwork.Sink)
                    {
                        notFind = false;
                    }
                }
            }

            if (notFind)
            {
                return new List<Edge>();
            }

            var temp = transportNetwork.Sink;
            while (true)
            {
                if (temp.Parent == null)
                {
                    return ((IEnumerable<Edge>)candidates).Reverse().ToList();
                }

                if (temp.Parent.Start == transportNetwork.Source)
                {
                    temp.Parent.Deleted = true;
                }
                else if (temp.Parent.End == transportNetwork.Sink)
                {
                    temp.Parent.Deleted = true;
                }
                else
                {
                    var toInvert = temp.Parent;

                    temp.Parent.End.AddNext(temp.Parent.Start, 1);
                    temp.Parent.Deleted = true;
                }

                candidates.Add(temp.Parent);
                var prev = temp;
                temp = temp.Parent.Start;
                prev.Parent = null;
            }
        }
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var n = int.Parse(Console.ReadLine());
            var source = new Node();
            var sink = new Node();
            var sets = new HashSet<int>[n];
            var visited = new HashSet<int>();
            var intToNode = new Dictionary<int, Node>();
            var nodeToInt = new Dictionary<Node, int>();
            var tn = new TransportNetwork()
            {
                Sink = sink,
                Source = source
            };

            for (int i = 0; i < n; i++)
            {
                var set = Console.ReadLine().Split().Select(int.Parse).Where(x => x != 0).ToHashSet();
                sets[i] = set;
                var node = new Node();
                node.Name = $"Set {i}";
                source.AddNext(node, 1);
                foreach (var integer in set)
                {
                    if (!intToNode.ContainsKey(integer))
                    {
                        var iN = new Node();
                        iN.Name = integer.ToString();
                        intToNode.Add(integer, iN);
                        nodeToInt.Add(iN, integer);

                    }

                    node.AddNext(intToNode[integer], 1);
                }
            }

            foreach (var node in intToNode.Values)
            {
                node.AddNext(sink, 1);
            }

            var answers = new List<int>();
            while (true)
            {
                var way = Algorithm.DepthSearchWithInversion(tn);
                if (way.Count == 0)
                {
                    break;
                }

                answers.Add(nodeToInt[way[way.Count - 1].Start]);
            }

            if (answers.Count == sets.Length)
            {
                Console.WriteLine("Y");
                Console.WriteLine(string.Join(" ", answers));
            }
            else
            {
                Console.WriteLine("N");
            }
        }
    }
}