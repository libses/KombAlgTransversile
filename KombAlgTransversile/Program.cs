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
        public void AddNext(Node next, int flow)
        {
            var edge = new Edge(flow);
            edge.Start = this;
            edge.End = next;
            Outgoing.Add(edge);
            next.Ingoing.Add(edge);
        }

        public void AddPrev(Node next, int flow)
        {
            var edge = new Edge(flow);
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
    }

    internal class Program
    {
        static void Main(string[] args)
        {
            var tn = new TransportNetwork();
            var a = new Node();
            a.Name = "a";
            var b = new Node();
            b.Name = "b";
            var c = new Node();
            c.Name = "c";
            var d = new Node();
            d.Name = "d";
            var e = new Node();
            e.Name = "e";
            var f = new Node();
            f.Name = "f";
            var g = new Node();
            g.Name = "g";
            a.AddNext(d, 3);
            a.AddNext(b, 3);
            a.AddPrev(c, 3);
            b.AddPrev(e, 1);
            b.AddNext(c, 4);
            c.AddNext(d, 1);
            c.AddNext(e, 2);
            d.AddNext(f, 6);
            d.AddNext(e, 2);
            e.AddNext(g, 1);
            f.AddNext(g, 9);
            tn.Source = a;
            tn.Sink = g;
            Algorithm.EdmondsCarp(tn);
        }
    }
}