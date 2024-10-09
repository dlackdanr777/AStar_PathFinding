using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Muks.PathFinding.AStar;
using Muks.PathFinding;

public class Unit : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private List<Vector2> _path;
    private Coroutine _coroutine;

    private void Start()
    {
        AStar.Instance.RequestPath(transform.position, _target.position, GetPath);
    }


    private void GetPath(List<Vector2> pathList)
    {
        _path = pathList;

        if (_coroutine != null)
            StopCoroutine(_coroutine);
        _coroutine = StartCoroutine(FollowPath());
    }


    IEnumerator FollowPath()
    {
        while (_path == null)
            yield return null;

        foreach (Vector2 vec in _path)
        {
            while (Vector3.Distance(transform.position, vec) > 0.05f)
            {
                Vector2 dir = (vec - (Vector2)transform.position).normalized;
                transform.Translate(dir * Time.deltaTime * 5, Space.World);

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
                Gizmos.DrawLine(_path[i], _path[i + 1]);
            }
        }
    }
}
