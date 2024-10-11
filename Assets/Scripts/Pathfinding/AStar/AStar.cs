using System;
using System.Collections.Generic;
using UnityEngine;

namespace Muks.PathFinding.AStar
{
    public enum MapType
    {
        TopDown,
        Platformer,
    }


    /// <summary>AStar ��ã�� �Ŵ���</summary>
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
                }

                return _instance;
            }
        }

        private static AStar _instance;
        [SerializeField] private MapType _mapType;
        [SerializeField] private LayerMask _wallLayer;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private bool _drawGizmos;
        [SerializeField] private bool _enableDiagonalMovement;
        [SerializeField] private float _nodeSize;
        public float NodeSize => _nodeSize;
        [SerializeField] private Vector2Int _mapSize;
        [SerializeField] private Vector2 _mapBottomLeft;
        public Vector2 MapBottomLeft => _mapBottomLeft;


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
            Init();
        }


        private void OnDestroy()
        {
            _instance = null;
        }

        /// <summary> ��Ƽ �����带 �̿��� ��ã�⸦ ��� �� �ݹ� �Լ��� �����ϴ� �Լ�</summary>
        public void RequestPath(Vector2 startPos, Vector2 targetPos, Action<List<Vector2>> callback)
        {
            if (_nodes == null || _nodes.Length <= 0)
                return;

            PathfindingQueue.Enqueue(() =>
            {
                List<Vector2> pathResult = PathFinding(startPos, targetPos);
                MainThreadDispatcher.Instance.Enqueue(() => callback(pathResult));
            });
        }


        /// <summary> ���� �ٽ� Ž���ϴ� �Լ� </summary>
        public void ResearchNodes()
        {
            _nodes = new Node[_sizeX, _sizeY];

            for (int i = 0; i < _sizeX; ++i)
            {
                float posX = _mapBottomLeft.x + (i + 0.5f) * _nodeSize;

                for (int j = 0; j < _sizeY; ++j)
                {
                    float posY = _mapBottomLeft.y + (j + 0.5f) * _nodeSize;
                    bool isWall = false;
                    bool isGround = false;

                    //Node��ġ�� Wall���̾ ��ġ�ϰų� ��Ÿ���� �÷������̸鼭 Ground���̾ �ִٸ� ������
                    foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(posX, posY), _nodeSize * 0.4f, _wallLayer | _groundLayer))
                    {
                        if (((1 << col.gameObject.layer) & _wallLayer) != 0 || (_mapType == MapType.Platformer && ((1 << col.gameObject.layer) & _groundLayer) != 0))
                            isWall = true;
                    }

                    //���� �� Ÿ���� �÷������̸鼭 j�� 0�� �ƴҰ��
                    if (_mapType == MapType.Platformer && j != 0)
                    {
                        //�ش� ����� �� ��带 Ȯ���Ѵ�.
                        float groundPosY = _mapBottomLeft.y + (j - 1 + 0.5f) * _nodeSize;
                        foreach (Collider2D col in Physics2D.OverlapCircleAll(new Vector2(posX, groundPosY), _nodeSize * 0.4f, _groundLayer))
                        {
                            //�� ��尡 �̹� �� �����̶�� �ǳʶڴ�.
                            if (isWall)
                                break;

                            //�� ��忡 Ground���̾ ������ �ٴ� ����
                            if (((1 << col.gameObject.layer) & _groundLayer) != 0)
                                isGround = true;

                        }
                    }

                    //�� Ÿ���� ž�ٿ� �ϰ�� ���� �ƴ� ���� ��� Ground�������� �Ѵ�.
                    else if (_mapType == MapType.TopDown)
                    {
                        if (!isWall)
                            isGround = true;
                    }
                    Node node = new Node(isWall, isGround, i, j);
                    _nodes[i, j] = node;

                }
            }
        }


        /// <summary> �ʱ� ���� </summary>
        private void Init()
        {
            _sizeX = Mathf.CeilToInt(_mapSize.x / _nodeSize);
            _sizeY = Mathf.CeilToInt(_mapSize.y / _nodeSize);

            ResearchNodes();
        }


        /// <summary>AStar �˰����� �̿�, ��������� ���������� �ִܰŸ��� List<Node> ���·� ��ȯ</summary>
        private List<Vector2> PathFinding(Vector2 startPos, Vector2 targetPos)
        {
            //�� ������ ����� ��� �����޼��� ���
            if (startPos.x < _mapBottomLeft.x || _mapBottomLeft.x + _mapSize.x < startPos.x || startPos.y < _mapBottomLeft.y || _mapBottomLeft.y + _mapSize.y < startPos.y)
            {
                Debug.LogError("���� ��ġ�� �� ������ ������ϴ�: " + startPos);
                return new List<Vector2>();
            }

            if (targetPos.x < _mapBottomLeft.x || _mapBottomLeft.x + _mapSize.x < targetPos.x || targetPos.y < _mapBottomLeft.y || _mapBottomLeft.y + _mapSize.y < targetPos.y)
            {
                Debug.LogError("��ǥ ��ġ�� �� ������ ������ϴ�: " + targetPos);
                return new List<Vector2>();
            }

            //����, ��ǥ ��ġ�� Node Index�� ���� �� �����Ѵ�.
            Vector2Int sPos = WorldToNodePosition(startPos);
            Vector2Int tPos = WorldToNodePosition(targetPos);

            for (int i = 0; i < _sizeX; ++i)
            {
                for (int j = 0; j < _sizeY; ++j)
                {
                    _nodes[i, j].H = 0;
                    _nodes[i, j].G = int.MaxValue;
                    _nodes[i, j].ParentNode = null;
                }
            }

            Node startNode = _nodes[sPos.x, sPos.y];
            Node targetNode = _nodes[tPos.x, tPos.y];

            List<Node> openList = new List<Node>() { startNode };
            List<Node> closedList = new List<Node>();
            List<Vector2> tmpList = new List<Vector2>();

            //openList�� 0���Ϸ� ������������ �ݺ��Ѵ�.
            while (0 < openList.Count)
            {
                Node currentNode = openList[0];
                for (int i = 1, cnt = openList.Count; i < cnt; ++i)
                {
                    if (openList[i].F <= currentNode.F && openList[i].H < currentNode.H)
                        currentNode = openList[i];
                }

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode.X == targetNode.X && currentNode.Y == targetNode.Y)
                {
                    Node node = targetNode;
                    while (node != startNode)
                    {
                        tmpList.Add(NodetoWorldPosition(node));
                        node = node.ParentNode;
                    }

                    tmpList.Add(NodetoWorldPosition(startNode));
                    tmpList.Reverse();
                    return tmpList;
                }
  

                int dirCnt = _enableDiagonalMovement ? 8 : 4;
                for (int i = 0; i < dirCnt; ++i)
                {

                    int nextX = currentNode.X + _dirX[i];
                    int nextY = currentNode.Y + _dirY[i];

                    if (0 <= nextX && nextX < _sizeX && 0 <= nextY && nextY < _sizeY)
                    {
                        Node nextNode = _nodes[nextX, nextY];

                        //�밢�� �̵��� ���ǰ� ž �ٿ� �� �� ���
                        if(_enableDiagonalMovement && _mapType == MapType.TopDown)
                        {
                            //�밢�� �̵� �� ���� �հ� �������ٸ� �ǳʶڴ�.
                            if (_nodes[currentNode.X, nextY].IsWall || _nodes[nextX, currentNode.Y].IsWall)
                                continue;

                            if (_nodes[nextX, nextY].IsWall)
                                continue;
                        }

                        //�ش� ��ġ�� ���̰ų� ���� �ƴ϶�� �ǳʶڴ�.
                        if (nextNode.IsWall || !nextNode.IsGround)
                            continue;

                        //���� ����Ʈ�� ������ �ǳʶڴ�.
                        if (closedList.Contains(nextNode))
                            continue;

                        int moveCost = currentNode.G + _cost[i];

                        //�̵� �ڽ�Ʈ�� ���� ����� �ڽ�Ʈ���� ����, ���� ����Ʈ�� ����� ������ �ʴٸ�
                        //���� ����Ʈ�� �߰��Ѵ�.
                        if (moveCost < nextNode.G || !openList.Contains(nextNode))
                        {
                            nextNode.G = moveCost;
                            nextNode.H = (Math.Abs(nextNode.X - targetNode.X) + Math.Abs(nextNode.Y - targetNode.Y)) * 10;
                            nextNode.ParentNode = currentNode;

                            openList.Add(nextNode);
                        }
                    }
                }
            }

            return new List<Vector2>();
        }

        /// <summary> ���� ��ǥ�� ��� ��ǥ�� ��ȯ�ϴ� �Լ� </summary>
        private Vector2Int WorldToNodePosition(Vector2 pos)
        {
            int posX = Mathf.FloorToInt((pos.x - _mapBottomLeft.x) / _nodeSize);
            int posY = Mathf.FloorToInt((pos.y - _mapBottomLeft.y) / _nodeSize);

            posX = Mathf.Clamp(posX, 0, _sizeX - 1);
            posY = Mathf.Clamp(posY, 0, _sizeY - 1);

            return new Vector2Int(posX, posY);
        }

        /// <summary> ��� ��ǥ�� ���� ��ǥ�� ��ȯ�� ��ȯ</summary>
        internal Vector2 NodetoWorldPosition(Node node)
        {
            float posX = _mapBottomLeft.x + (node.X + 0.5f) * _nodeSize;
            float posY = _mapBottomLeft.y + (node.Y + 0.5f) * _nodeSize;
            return new Vector2(posX, posY);
        }


        private void OnDrawGizmos()
        {
            if (!_drawGizmos)
                return;


            // ��ü ���� �׸��� �ڵ�
            Vector2 mapCenter = new Vector2(_mapBottomLeft.x + _mapSize.x * 0.5f, _mapBottomLeft.y + _mapSize.y * 0.5f);
            Vector2 mapSize = new Vector2(_mapSize.x, _mapSize.y);
            Gizmos.DrawWireCube(mapCenter, mapSize);

            if (_nodeSize <= 0)
                return;

            int sizeX = Mathf.CeilToInt(_mapSize.x / _nodeSize);
            int sizeY = Mathf.CeilToInt(_mapSize.y / _nodeSize);

            // �� ��带 �׸��� �ڵ�
            for (int i = 0; i < sizeX; i++)
            {
                float posX = _mapBottomLeft.x + (i + 0.5f) * _nodeSize;
                for (int j = 0; j < sizeY; j++)
                {
                    float posY = _mapBottomLeft.y + (j + 0.5f) * _nodeSize;

                    if (_nodes != null)
                    {
                        if (_nodes[i, j].IsWall)
                        {
                            Gizmos.color = Color.red;
                            Gizmos.DrawCube(new Vector2(posX, posY), Vector2.one * _nodeSize);
                        }

                        else if (_nodes[i, j].IsGround)
                        {
                            Gizmos.color = Color.green;
                            Gizmos.DrawCube(new Vector2(posX, posY), Vector2.one * _nodeSize);
                        }

                        else
                        {
                            Gizmos.color = Color.white;
                            Gizmos.DrawWireCube(new Vector2(posX, posY), Vector2.one * _nodeSize);
                        }
                    }
                    else
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(new Vector2(posX, posY), Vector2.one * _nodeSize);
                    }

                }
            }
        }
    }
}
