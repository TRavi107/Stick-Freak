using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickManController : MonoBehaviour
{
    public Transform stickTransform;
    public bool walk;

    [SerializeField] float lineIncreaseRate;
    [SerializeField] float movementSpeed;

    private bool lineDrawing;
    private float lineLength;
    private bool moving;

    private Vector3 position;
    private Vector3 scale;
    private Quaternion rotation;
    // Start is called before the first frame update
    void Start()
    {
        position = stickTransform.localPosition;
        rotation = stickTransform.localRotation;
        scale = stickTransform.localScale;

    }

    // Update is called once per frame
    [System.Obsolete]
    void Update()
    {
        if (moving)
            return;
        if (Input.GetMouseButtonDown(0))
        {
            lineDrawing = true;
        }
        else if(Input.GetMouseButtonUp(0))
        {
            lineDrawing = false;
            StopCoroutine(nameof(MoveCharacterCour));
            StartCoroutine(nameof(MoveCharacterCour));
        }
        if (lineDrawing)
        {
            lineLength += lineIncreaseRate * Time.deltaTime;
            stickTransform.localScale = new Vector3(lineLength, stickTransform.localScale.y, stickTransform.localScale.z);
        }
    }

    IEnumerator MoveCharacterCour()
    {
        moving = true;
        stickTransform.SetParent(null);
        stickTransform.rotation = Quaternion.Euler(0, 0, 0);
        yield return new WaitForSeconds(0.2f);
        while (lineLength >0)
        {
            transform.position = new(transform.position.x + movementSpeed * Time.deltaTime, transform.position.y, transform.position.z);
            lineLength -= movementSpeed * Time.deltaTime*2;
            yield return null;
        }
        //Check if edge
        //Add score 
        //Can Draw
        moving = false;
        ResetStickPosition();
    }

    private void ResetStickPosition()
    {
        stickTransform.SetParent(this.transform);
        stickTransform.localPosition = position;
        stickTransform.localScale = scale;
        stickTransform.localRotation = rotation;
    }
}
