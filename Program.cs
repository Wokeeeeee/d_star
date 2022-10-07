enum Tag { NEW, CLOSED, OPEN }
enum State { CLEAR, OBSTACLE, PATH, START, END }

class Node
{
    public int x, y;
    public Node? parent = null;
    public double h = 0, k = 0;
    public Tag tag = Tag.NEW;
    public State state = State.CLEAR;
    public static int MAX_VALUE = 1000;


    public Node(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public double cost(Node a)//a多半是this的父节点
    {
        if (a.state == State.OBSTACLE || this.state == State.OBSTACLE)
        {
            return MAX_VALUE;
        }
        else return Math.Sqrt(Math.Pow(a.x - this.x, 2) + Math.Pow(a.y - this.y, 2));
    }


    public void set_state(State s)
    {
        this.state = s;
    }


    public override string ToString()
    {
        return this.x + " " + this.y;
    }



}

class Map
{
    public int row, col;

    public List<List<Node>> map;

    public Map(int row, int col)
    {
        this.row = row;
        this.col = col;
        this.map = new List<List<Node>>();
        for (int i = 0; i < this.row; i++)
        {
            List<Node> rows = new List<Node>();
            for (int j = 0; j < this.col; j++)
            {
                rows.Add(new Node(i, j));
            }
            this.map.Add(rows);
        }


    }

    public List<Node> getNeighbors(Node node)
    {
        List<Node> nodes = new List<Node>();
        foreach (int i in new List<int> { -1, 0, 1 })
        {
            foreach (int j in new List<int> { -1, 0, 1 })
            {
                if (i == 0 && j == 0) continue;
                if (node.x + i < 0 || node.x + i >= this.row) continue;
                if (node.y + j < 0 || node.y + j >= this.col) continue;

                nodes.Add(this.map[node.x + i][node.y + j]);
            }
        }
        return nodes;
    }

    public void set_obstacles(List<Node> obstacle_list)
    {
        foreach (Node n in obstacle_list)
        {
            this.map[n.x][n.y].set_state(State.OBSTACLE);
        }
    }

    public void set_obstacles(int[,] obstacle_list)
    {
        for (int i = 0; i < obstacle_list.GetLength(0); i++)
        {
            this.map[obstacle_list[i, 0]][obstacle_list[i, 1]].set_state(State.OBSTACLE);
        }
    }


    public string transSign(State s)
    {
        switch (s)
        {
            case State.CLEAR: return ".";
            case State.OBSTACLE: return "#";
            case State.PATH: return "@";
            case State.START: return "S";
            case State.END: return "E";
        }
        return "error";
    }

    public void print_map()
    {
        for (int i = 0; i < this.row; i++)
        {
            string s = i+" ";
            for (int j = 0; j < this.col; j++)
            {
                s += transSign(map[i][j].state) + " ";
            }
            Console.WriteLine(s);
        }
    }

    public void clear_map()
    {
        for (int i = 0; i < this.row; i++)
        {
            for (int j = 0; j < this.col; j++)
            {
                if (map[i][j].state!=State.OBSTACLE) map[i][j].set_state(State.CLEAR);
            }
        }
    }

}


class DStar
{
    public Map map;
    public List<Node> open_list;
    public Node end;
    public Node start;

    public DStar(Map map, Node end, Node start)
    {
        this.map = map;
        this.open_list = new List<Node>();
        this.end = end;
        this.start = start;

    }

    public Node min_state()
    {
        if (!this.open_list.Any()) return null;
        else
        {
            open_list.Sort(
            delegate (Node n1, Node n2)
            {
                return n1.k.CompareTo(n2.k);
            }
            );
            return open_list[0];
        }
    }

    public void remove(Node node)
    {
        if (node.tag == Tag.OPEN) node.tag = Tag.CLOSED;
        this.open_list.Remove(node);
    }

    public void insert_node(Node node, double h_new)
    {
        if (node.tag == Tag.NEW) node.k = h_new;
        else if (node.tag == Tag.OPEN) node.k = Math.Min(h_new, node.k);
        else if (node.tag == Tag.CLOSED) node.k = Math.Min(h_new, node.k);

        node.h = h_new;
        node.tag = Tag.OPEN;
        this.open_list.Add(node);
    }
    public void modify_cost(Node node)
    {
        if (node.tag == Tag.CLOSED) insert_node(node, node.parent.h + node.cost(node.parent));
    }

    public void modify(Node node)
    {
        this.modify_cost(node);
        while (true)
        {
            double k_min = process_state();
            if (node.h <= k_min) break;
        }
    }


    public double process_state()
    {
        Node node_x = min_state();
        if (node_x == null) return -1;

        double old_k = node_x.k;
        remove(node_x);

        if (old_k < node_x.h)
        {
            foreach (Node node_y in this.map.getNeighbors(node_x))
            {
                if (old_k >= node_y.h && node_x.h > node_y.h + node_x.cost(node_y))
                {
                    node_x.parent = node_y;
                    node_x.h = node_y.h + node_x.cost(node_y);
                }
            }
        }
        else if (old_k == node_x.h)
        {
            foreach (Node node_y in this.map.getNeighbors(node_x))
            {
                if ((node_y.tag == Tag.NEW || node_y.parent == node_x && node_y.h != node_x.h + node_x.cost(node_y) || node_y.parent != node_x && node_y.h > node_x.h + node_x.cost(node_y))
                    && node_y != end)
                {
                    node_y.parent = node_x;
                    insert_node(node_y, node_x.h + node_x.cost(node_y));
                }
            }
        }
        else
        {
            foreach (Node node_y in this.map.getNeighbors(node_x))
            {
                if (node_y.tag == Tag.NEW || node_y.parent == node_x && node_y.h != node_x.h + node_x.cost(node_y))
                {
                    node_y.parent = node_x;
                    insert_node(node_y, node_x.h + node_x.cost(node_y));
                }
                else
                {
                    if (node_y.parent != node_x && node_y.h > node_x.h + node_x.cost(node_y))
                    {
                        insert_node(node_x, node_x.h);
                    }
                    else
                    {
                        if (node_y.parent != node_x && node_x.h > node_y.h + node_x.cost(node_y) && node_y.tag == Tag.CLOSED && node_y.h > old_k)
                        {
                            insert_node(node_y, node_y.h);
                        }
                    }
                }
            }


        }
        return min_state().k;
    }


    public void run()
    {
        insert_node(this.end, 0);

        while (true)
        {
            process_state();
            if (this.start.tag == Tag.CLOSED) break;
        }
        start.set_state(State.START);
        Node s = start;
        while (s != end)
        {
            s = s.parent;
            s.set_state(State.PATH);

        }
        s.set_state(State.END);


        map.print_map();

        Console.WriteLine("\n\n\n obstacle change\n\n\n");
        map.clear_map();

        Node tmp_s = start;
        int[,] obs_new = new int[8, 2]
        {
            {12, 4},
            { 12, 5},
            {12, 6},
            { 12, 7},
            { 12, 9},
            { 13,10},
            { 13,9},
            { 13,8}
        };
        map.set_obstacles(obs_new);

        while (tmp_s != end)
        {
            tmp_s.set_state(State.PATH);
            if (tmp_s.parent.state == State.OBSTACLE)
            {
                modify(tmp_s);
                continue;
            }
            tmp_s = tmp_s.parent;
        }
        tmp_s.set_state(State.END);
        start.set_state(State.START);

        this.map.print_map();


    }


    public static void Main(string[] args)
    {
        Map mh = new Map(25, 25);

        int[,] obs = new int[8, 2]
        {
            {6,3 },
            { 6,4},
            {6,5 },
            { 6,6},
            { 6,7},
            { 7,4},
            { 7,5},
            { 7,6}
        };



        mh.set_obstacles(obs);

        DStar dStar = new DStar(mh, mh.map[3][2], mh.map[23][16]);
        dStar.run();

    }

}