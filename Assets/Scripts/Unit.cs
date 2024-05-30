using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    [SerializeField] private Transform _target;


    private Vector2 _targetPos;

    private void Start()
    {
        StartCoroutine(PathFinding());
    }

    private IEnumerator PathFinding()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.1f);
            _targetPos = AStar.Instance.PathFinding(transform.position, _target.position);
        }
    }


    private void Update()
    {
        if (_targetPos == Vector2.zero)
            return;

        Vector2 currentPos = transform.position;

        Vector2 dir = (_targetPos - currentPos).normalized;
        Debug.Log(_targetPos);
        transform.Translate(dir * 1 * Time.deltaTime);
    }
}
