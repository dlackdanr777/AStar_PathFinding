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
        StartCoroutine(PathFinding());

        _coroutine = StartCoroutine(FollowPath());

    }

    private IEnumerator PathFinding()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);
           _path = AStar.Instance.PathFinding(transform.position, _target.position);


        }
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
}
