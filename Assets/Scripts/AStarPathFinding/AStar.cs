using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Node
{
    public int F => G + H;
    public int H;
    public int G;
    public int Y;
    public int X;
    public bool IsWall;
    public Node ParentNode;

    private float _nodeSize => AStar.Instance.NodeSize;
    private Vector2 _mapBottomLeft => AStar.Instance.MapBottomLeft;

    public Node(bool isWall, int x, int y) { IsWall = isWall; X = x; Y = y; }

    public Vector2 toVector2()
    {
        return new Vector2(X, Y);
    }

    public Vector2 toWorldPosition()
    {
        float posX = _mapBottomLeft.x + (X + 0.5f) * _nodeSize;
        float posY = _mapBottomLeft.y + (Y + 0.5f) * _nodeSize;
        return new Vector2(posX, posY);
    }
}

public class AStar : MonoBehaviour
{

    public static AStar Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("AStarManager");
                _instance = obj.AddComponent<AStar>();
                DontDestroyOnLoad(obj);
            }

            return _instance;
        }
    }
    private static AStar _instance;

    public float NodeSize;
    public Vector2 MapBottomLeft;
    [SerializeField] private Vector2Int _mapSize;

    private int[] _dirX = new int[8] { 0, 0, 1, -1, 1, 1, -1, -1 };
    private int[] _dirY = new int[8] { 1, -1, 0, 0, 1, -1, -1, 1 };
    private int[] _cost = new int[8] { 10, 10, 10, 10, 14, 14, 14, 14 };

    private Node[,] _nodes;
    private int _sizeX;
    private int _sizeY;




    private void Awake()
    {
        if (_instance != null)
            return;

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Init();
    }


    private void Init()
    {
        _sizeX = Mathf.CeilToInt(_mapSize.x / NodeSize);
        _sizeY = Mathf.CeilToInt(_mapSize.y / NodeSize);

        _nodes = new Node[_sizeX, _sizeY];

        for(int i = 0; i < _sizeX; ++i)
        {
            float posX = MapBottomLeft.x + (i + 0.5f) * NodeSize;

            for (int j = 0; j < _sizeY; ++j)
            {
                float posY = MapBottomLeft.y + (j + 0.5f) * NodeSize;
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(posX, posY), NodeSize * 0.4f))
                    if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) 
                        isWall = true;

                Node node = new Node(isWall, i, j);
                _nodes[i,j] = node;
            }
        }
    }

    public void RequestPath(Vector2 startPos, Vector2 targetPos, Action<List<Node>> callback)
    {
        PathfindingQueue.Instance.Enqueue(() =>
        {
            List<Node> pathResult = PathFindingTask(startPos, targetPos);
            MainThreadDispatcher.Instance.Enqueue(() => callback(pathResult));
        });
    }


    private List<Node> PathFindingTask(Vector2 startPos, Vector2 targetPos)
    {
        Vector2Int sPos = WorldToNodePos(startPos);
        Vector2Int tPos = WorldToNodePos(targetPos);

        for (int i = 0; i < _sizeX; ++i)
        {
            for (int j = 0; j < _sizeY; ++j)
            {
                _nodes[i, j].H = 0;
                _nodes[i, j].G = int.MaxValue;
                _nodes[i,j].ParentNode = null;
            }
        }

        Node startNode = _nodes[sPos.x, sPos.y];
        Node targetNode = _nodes[tPos.x, tPos.y];

        List<Node> openList = new List<Node>() { startNode };
        List<Node> closedList = new List<Node>();
        List<Node> tmpList = new List<Node>();

        while(0 < openList.Count)
        {
            Node currentNode = openList[0];
            for(int i = 1, cnt = openList.Count; i < cnt; ++i)
            {
                if (openList[i].F <= currentNode.F && openList[i].H < currentNode.H)
                    currentNode = openList[i];
            }

            openList.Remove(currentNode);
            closedList.Add(currentNode);

            if(currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)
            {
                Node node = targetNode;
                while(node != startNode)
                {
                    tmpList.Add(node);
                    node = node.ParentNode;
                }

                tmpList.Add(startNode);
                tmpList.Reverse();
                return tmpList;
            }

            int dirCnt = 4;
            for (int i = 0; i < dirCnt; ++i)
            {

                int nextX = currentNode.X + _dirX[i];
                int nextY = currentNode.Y + _dirY[i];

                if(0 <= nextX && nextX < _sizeX && 0 <= nextY && nextY < _sizeY)
                {
                    Node nextNode = _nodes[nextX, nextY];
                    if (nextNode.IsWall)
                        continue;

                    if (closedList.Contains(nextNode))
                        continue;

                    int moveCost = currentNode.G + _cost[i];

                    if (moveCost < nextNode.G  || !openList.Contains(nextNode))
                    {
                        nextNode.G = moveCost;
                        nextNode.H = (Math.Abs(nextNode.X - targetNode.X) + Math.Abs(nextNode.Y - targetNode.Y)) * 10;
                        nextNode.ParentNode = currentNode;

                        openList.Add(nextNode);
                    }
                }
            }
        }

        return new List<Node>();
    }


    public Vector2 NodeToWorldPos(Node node)
    {
        float posX = MapBottomLeft.x + (node.X + 0.5f) * NodeSize;
        float posY = MapBottomLeft.y + (node.Y + 0.5f) * NodeSize;
        return new Vector2(posX, posY);
    }


    private Vector2Int WorldToNodePos(Vector2 pos)
    {
        int posX = Mathf.FloorToInt((pos.x - MapBottomLeft.x) / NodeSize);
        int posY = Mathf.FloorToInt((pos.y - MapBottomLeft.y) / NodeSize);

        posX = Mathf.Clamp(posX, 0, _sizeX - 1);
        posY = Mathf.Clamp(posY, 0, _sizeY - 1);

        return new Vector2Int(posX, posY);
    }


    private void OnDrawGizmos()
    {
        // 전체 맵을 그리는 코드
        Vector2 mapCenter = new Vector2(MapBottomLeft.x + _mapSize.x * 0.5f, MapBottomLeft.y + _mapSize.y * 0.5f);
        Vector2 mapSize = new Vector2(_mapSize.x, _mapSize.y);
        Gizmos.DrawWireCube(mapCenter, mapSize);


        if (NodeSize <= 0)
            return;

        int sizeX = Mathf.CeilToInt(_mapSize.x / NodeSize);
        int sizeY = Mathf.CeilToInt(_mapSize.y / NodeSize);

        // 각 노드를 그리는 코드
        for (int i = 0; i < sizeX; i++)
        {
            float posX = MapBottomLeft.x + (i + 0.5f) * NodeSize;
            for (int j = 0; j < sizeY; j++)
            {
                float posY = MapBottomLeft.y + (j + 0.5f) * NodeSize;

                if(_nodes != null)
                {
                    if (_nodes[i, j].IsWall)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(new Vector2(posX, posY), Vector2.one * NodeSize);
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(new Vector2(posX, posY), Vector2.one * NodeSize);
                    }
                }
                else
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawWireCube(new Vector2(posX, posY), Vector2.one * NodeSize);
                }

            }
        }
    }
}