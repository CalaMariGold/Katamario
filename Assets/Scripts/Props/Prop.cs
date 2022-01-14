using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Prop : MonoBehaviour
{
    private Action<Prop> _releaseAction;

    public void Init(Action<Prop> releaseAction)
    {
        _releaseAction = releaseAction;
    }

    void Update()
    {
        // Aborb this prop into the parent if it's been collected
        if(this.gameObject.tag == "Collected")
            StartCoroutine(AbsorbPropOverTime(this.transform, this.GetComponentInParent<SphereCollider>().transform));
    }

    public IEnumerator AbsorbPropOverTime(Transform child, Transform absorber)
    {
        // Seconds to wait before absorption starts
        yield return new WaitForSeconds(3);
        Vector3 destinationScale = new Vector3(0, 0, 0);

        if (child != null)
        {
            // Move the child's transform towards the center of the absorber
            // Decrease the child's scale to 0
            // 0.05 is an arbituary number to slow down the process
            child.transform.position = Vector3.MoveTowards(child.transform.position, absorber.position, Time.deltaTime * 0.05f);
            child.transform.localScale = Vector3.Lerp(child.transform.localScale, destinationScale, Time.deltaTime * 0.05f);
        }

        // Release prop and exit when child object reaches the center
        if (child.transform.position == absorber.transform.position)
        {
            _releaseAction(this);
            yield return null;
        }
    }

}
