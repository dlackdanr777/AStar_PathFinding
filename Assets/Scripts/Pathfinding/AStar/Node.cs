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

        internal Node(bool isWall, bool isGround, int x, int y)
        {
            IsWall = isWall;
            IsGround = isGround;
            X = x;
            Y = y;
        }
    }
}
