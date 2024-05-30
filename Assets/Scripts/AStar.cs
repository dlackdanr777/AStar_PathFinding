using System;
using System.Collections;
using System.Collections.Generic;
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
    public Node(bool isWall, int x, int y) { IsWall = isWall; X = x; Y = y; }

    public Vector2 toVector2()
    {
        return new Vector2(X, Y);
    }
}

public class AStar : MonoBehaviour
{
    private static AStar _instance;
    public static AStar Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject obj = new GameObject("AStarManager");
                _instance = obj.AddComponent<AStar>();
            }

            return _instance;
        }
    }

    [SerializeField] private Vector2 _mapBottomLeft;
    [SerializeField] private Vector2Int _mapSize;

    private int[] _dirX = new int[8] { 0, 0, 1, -1, 1, 1, -1, -1 };
    private int[] _dirY = new int[8] { 1, -1, 0, 0, 1, -1, -1, 1 };
    private int[] _cost = new int[8] { 10, 10, 10, 10, 14, 14, 14, 14 };

    private Node[,] _nodes;
    private List<Node> tmpList = new List<Node>();
    private int _sizeX;
    private int _sizeY;




    private void Awake()
    {
        _instance = this;
        Init();
    }


    private void Init()
    {
        _sizeX = _mapSize.x;
        _sizeY = _mapSize.y;

        _nodes = new Node[_sizeX, _sizeY];

        for(int i = 0; i < _sizeX; ++i)
        {
            float posX = _mapBottomLeft.x + i + 0.5f;

            for (int j = 0; j < _sizeY; ++j)
            {
                float posY = _mapBottomLeft.y + j + 0.5f;
                bool isWall = false;
                foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(posX, posY), 0.45f))
                    if (col.gameObject.layer == LayerMask.NameToLayer("Wall")) 
                        isWall = true;

                Node node = new Node(isWall, i, j);
                _nodes[i,j] = node;
            }
        }
    }


    public Vector2 PathFinding(Vector2 startPos, Vector2 targetPos)
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
        tmpList.Clear();

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

                if (tmpList.Count == 0)
                    return targetPos;

                else if(tmpList.Count == 1)
                    return NodeToWorldPos(new Vector2Int(tmpList[0].X, tmpList[0].Y));
                
                else if(2 <= tmpList.Count)
                    return NodeToWorldPos(new Vector2Int(tmpList[1].X, tmpList[1].Y));
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

        return startPos;
    }


    private Vector2Int WorldToNodePos(Vector2 pos)
    {
        int posX = Mathf.FloorToInt(pos.x - _mapBottomLeft.x);
        int posY = Mathf.FloorToInt(pos.y - _mapBottomLeft.y);

        posX = Mathf.Clamp(posX, 0, _sizeX - 1);
        posY = Mathf.Clamp(posY, 0, _sizeY - 1);

        return new Vector2Int(posX, posY);
    }


    private Vector2 NodeToWorldPos(Vector2Int pos)
    {
        float posX = _mapBottomLeft.x + pos.x + 0.5f;
        float posY = _mapBottomLeft.y + pos.y + 0.5f;
        return new Vector2(posX, posY);
    }


    private void OnDrawGizmos()
    {
        Vector2 mapCenter = new Vector2(_mapBottomLeft.x + _mapSize.x * 0.5f, _mapBottomLeft.y + _mapSize.y * 0.5f);
        Vector2 mapSize = _mapSize;
        Gizmos.DrawWireCube(mapCenter, mapSize);

        for(int i = 0, cntI = _mapSize.x; i < cntI; ++i)
        {
            float posX = _mapBottomLeft.x + i + 0.5f;
            for (int j = 0, cntJ = _mapSize.y; j < cntJ; ++j)
            {
                float posY = _mapBottomLeft.y + j + 0.5f;

                if (_nodes != null && 0 < _nodes.Length)
                {

                    if (_nodes[i, j].IsWall)
                    {
                        Gizmos.color = Color.red;
                        Gizmos.DrawCube(new Vector2(posX, posY), new Vector2(1, 1));
                    }

                    else
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(new Vector2(posX, posY), new Vector2(1, 1));
                    }

                    continue;
                }

                Gizmos.color = Color.white;
                Gizmos.DrawWireCube(new Vector2(posX, posY), new Vector2(1, 1));
            }

        }

        if(tmpList != null && 0 < tmpList.Count)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < tmpList.Count - 1;  ++i)
            {
                Gizmos.DrawLine(tmpList[i].toVector2(), tmpList[i + 1].toVector2());
            }
        }

    }
}