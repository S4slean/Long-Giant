using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class WigglyLine : MonoBehaviour
{
    public bool debug;

    public Vector3 start;
    public Vector3 end;

    [Range(0f, 1f)]
    public float tightness;
    [Range(0f, 1f)]
    public float damping;

    [Range(0f, 1f)]
    public float maxTightness;
    [Range(0f, 1f)]
    public float maxDamping;

    public AnimationCurve influenceCurve;

    [Range(2,500)]
    public int subdiv;

    public Vector3[] desiredPositions;
    public Vector3[] positions;
    public Vector3[] velocities;

    // Start is called before the first frame update
    void Start()
    {
        desiredPositions = new Vector3[subdiv];
        positions = new Vector3[subdiv];
        velocities = new Vector3[subdiv];

        InitPositions();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateDesiredPositions();

        UpdatePositions();
    }

    void UpdateDesiredPositions()
    {
        for (int i = 0; i < subdiv; i++)
        {
            desiredPositions[i] = Vector3.Lerp(start, end, Mathf.InverseLerp(0, subdiv - 1, i));
        }
    }

    void InitPositions()
    {
        for (int i = 0; i < subdiv; i++)
        {
            positions[i] = desiredPositions[i];
        }
    }


    void UpdatePositions()
    {
        for (int i = 0; i < subdiv; i++)
        {
            velocities[i] += (-(GetTightness(i) * (positions[i] - desiredPositions[i]))) - (GetDamping(i) * velocities[i]);
            positions[i] += velocities[i];
        }
    }

    float GetTightness(int i)
    {
        return Mathf.Lerp(maxTightness, tightness, influenceCurve.Evaluate(Mathf.InverseLerp(0, subdiv - 1, i)));
    }

    float GetDamping(int i)
    {
        return Mathf.Lerp(maxDamping, damping, influenceCurve.Evaluate(Mathf.InverseLerp(0, subdiv - 1, i)));
    }

    private void OnDrawGizmos()
    {
        if (!debug) return;

        if (!Application.isPlaying) return;

        Gizmos.color = new Color(.1f, .1f, .1f, .1f);

        for (int i = 0; i < subdiv; i++)
        {
            Gizmos.DrawSphere(desiredPositions[i], .04f);
        }

        Gizmos.color = Color.white;

        for (int i = 0; i < subdiv; i++)
        {
            Gizmos.DrawSphere(positions[i], .04f);
        }
    }
}
