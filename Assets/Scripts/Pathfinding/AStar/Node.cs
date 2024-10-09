using UnityEngine;

namespace Muks.PathFinding.AStar
{
    /// <summary>AStar에서 사용하는 Node Class</summary>
    internal class Node
    {
        internal int F => G + H;
        internal int H;
        internal int G;
        internal int Y;
        internal int X;
        internal bool IsWall;
        internal bool IsGround;
        internal Node ParentNode;

        private float _nodeSize => AStar.Instance.NodeSize;
        private Vector2 _mapBottomLeft => AStar.Instance.MapBottomLeft;


        internal Node(bool isWall, bool isGround, int x, int y)
        {
            IsWall = isWall;
            IsGround = isGround;
            X = x;
            Y = y;
        }


        /// <summary> 노드 좌표를 Vector2 형식으로 반환</summary>
        internal Vector2 toVector2()
        {
            return new Vector2(X, Y);
        }


        /// <summary> 노드 좌표를 월드 좌표로 변환해 반환</summary>
        internal Vector2 toWorldPosition()
        {
            float posX = _mapBottomLeft.x + (X + 0.5f) * _nodeSize;
            float posY = _mapBottomLeft.y + (Y + 0.5f) * _nodeSize;
            return new Vector2(posX, posY);
        }
    }
}
