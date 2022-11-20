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
        public Dictionary<Node, Edge> Outgoing = new Dictionary<Node, Edge>();
        public Dictionary<Node, Edge> Ingoing = new Dictionary<Node, Edge>();
        public void AddNext(Node next, int capacity)
        {
            var edge = new Edge(capacity);
            edge.Start = this;
            edge.End = next;
            Outgoing.Add(next, edge);
            next.Ingoing.Add(this, edge);
        }

        public void AddPrev(Node next, int capacity)
        {
            var edge = new Edge(capacity);
            edge.End = this;
            edge.Start = next;
            Ingoing.Add(next, edge);
            next.Outgoing.Add(this, edge);
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
                var outg = current.Outgoing.Where(x => !visited.Contains(x.Value));

                foreach (var positive in outg)
                {
                    visited.Add(positive.Value);
                    positive.Value.End.Parent = positive.Value;
                    queue.Enqueue(positive.Value.End);
                    if (positive.Value.End == transportNetwork.Sink)
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
            while (temp.Parent != null)
            {
                if (temp.Parent.Start == transportNetwork.Source)
                {
                    temp.Ingoing.Remove(transportNetwork.Source);
                    transportNetwork.Source.Outgoing.Remove(temp);
                }
                else if (temp == transportNetwork.Sink)
                {
                    temp.Ingoing.Remove(temp.Parent.Start);
                    temp.Parent.Start.Outgoing.Remove(temp);
                }
                else
                {
                    var toInvert = temp.Parent;

                    temp.Parent.Start.Outgoing.Remove(temp);
                    temp.Ingoing.Remove(temp.Parent.Start);
                    temp.Parent.Start.AddNext(temp.Parent.Start, 1);
                }

                candidates.Add(temp.Parent);
                var prev = temp;
                temp = temp.Parent.Start;
                prev.Parent = null;
            }

            return ((IEnumerable<Edge>)candidates).Reverse().ToList();
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