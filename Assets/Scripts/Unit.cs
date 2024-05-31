using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Transform _target;


    private Vector2 _targetPos;
    private List<Node> _path;
    private Coroutine _coroutine;

    private void Start()
    {
        AStar.Instance.RequestPath(transform.position, _target.position, GetPath);
    }


    private void GetPath(List<Node> nodeList)
    {
        _path = nodeList;
        _coroutine = StartCoroutine(FollowPath());
    }


    IEnumerator FollowPath()
    {
        while (_path == null)
            yield return null;

        foreach (Node node in _path)
        {
            while (Vector3.Distance(transform.position, node.toWorldPosition()) > 0.05f)
            {
                transform.position = Vector3.MoveTowards(transform.position, node.toWorldPosition(), 1 * Time.deltaTime);
                yield return null;
            }
        }
    }


    private void OnDrawGizmos()
    {
             // 경로를 그리는 코드
        if (_path != null && _path.Count > 0)
        {
            Gizmos.color = Color.green;

            for (int i = 0; i < _path.Count - 1; i++)
            {
                Gizmos.DrawLine(_path[i].toWorldPosition(), _path[i + 1].toWorldPosition());
            }
        }
    }
}
