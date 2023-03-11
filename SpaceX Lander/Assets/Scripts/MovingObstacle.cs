using UnityEngine;

//[DisallowMultipleComponent] // to avoid mutiple same classes to single gameobject
public class MovingObstacle : MonoBehaviour
{
    [SerializeField] Vector3 movementFactor;
    [SerializeField] float moveSpeed;
    [SerializeField] Space handleMovement = Space.World; // set to rotate in local/world space
    [Tooltip("Ocilate (movement is always set to ocilation) -> Spin Clockwisse/Anticlockwise ")]
    [SerializeField] bool OcilateSpin = true;
    [SerializeField] Vector3 spinFactor;
    [Tooltip("Spin Speed in Deg per Sec")]
    [SerializeField] float spinSpeed;
    [SerializeField] Space handleRotation = Space.Self; // set to rotate in local/world space
    Vector3 MovVec; Vector3 MovStep; Vector3 RotStep;
    int m; int s;

    void SetMovementVector()
    {
        if (handleMovement == Space.World)
            MovVec = movementFactor;
        else if (handleMovement == Space.Self)
            MovVec = movementFactor.magnitude * transform.TransformPoint(movementFactor).normalized;
    }

    // Start is called before the first frame update
    void Start()
    {
        m = 1; s = 1;
        moveSpeed = Mathf.Abs(moveSpeed);
        spinSpeed = Mathf.Abs(spinSpeed);
        MovStep = Vector3.zero; RotStep = Vector3.zero;
        SetMovementVector();
    }

    // Update is called once per frame
    void Update()
    {
        if (moveSpeed != 0)
        {
            // movement occilation
            if (MovVec.magnitude - Mathf.Sign(Mathf.Sin(Vector3.Angle(MovVec, MovStep))) * MovStep.magnitude <= Mathf.Epsilon)
            {
                m *= -1;
                MovStep = -MovStep;
                //SetMovementVector(); // to try change in runtime
            }
            else
            {
                var updateDelta = moveSpeed * Time.deltaTime * MovVec;
                MovStep += updateDelta;
                transform.position += m * updateDelta;
            }
        }
        if (spinFactor.magnitude != 0)
        {
            var spin_speed = spinSpeed / spinFactor.magnitude;
            // spin ocilation
            if (OcilateSpin)
            {
                if (spinFactor.magnitude - Mathf.Sign(Mathf.Sin(Vector3.Angle(spinFactor, RotStep))) * RotStep.magnitude <= Mathf.Epsilon) // detects negativw side also
                { 
                    // Mathf.Epsilon is the smallest float value,
                    // we are using this as comparing float values by == is depreciated;
                    // As epsilon is the smallest float value, dont forget to include <'=' in comparison
                    s *= -1;
                    RotStep = -RotStep;
                }
                else
                {
                    var updateDelta = spin_speed * Time.deltaTime * spinFactor;
                    RotStep += updateDelta;
                    transform.Rotate(s * updateDelta, handleRotation);
                }
            }
            else
            {
                transform.Rotate(spin_speed * Time.deltaTime * spinFactor, handleRotation);
            }
        }
    }
}
