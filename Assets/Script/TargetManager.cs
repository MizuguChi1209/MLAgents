using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject Agent;
    [SerializeField] private GameObject[] Targets;
    [SerializeField] private float distanceThreshold = 5.0f; // ‹——£‚Ìè‡’l
    private int currentTargetIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        if (Targets.Length > 0)
        {
            MoveToNextTarget();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Targets.Length == 0) return;

        float distance = Vector3.Distance(Agent.transform.position, transform.position);
        if (distance <= distanceThreshold)
        {
            MoveToNextTarget();
        }
    }

    private void MoveToNextTarget()
    {
        currentTargetIndex = (currentTargetIndex + 1) % Targets.Length;
        transform.DOMove(Targets[currentTargetIndex].transform.position, 1.0f);
    }
}
