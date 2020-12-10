using Assets.Authory.Scripts;
using UnityEngine;

/// <summary>
/// Handles the movement of the player. 
/// If the player position changes it will send a movement packet to the server.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    const float RANGE_ERROR_POINT = 2f;

    [SerializeField] PlayerEntity Player;

    UIController UIController;

    bool stop = false;
    float time = 0;

    float Range;
    Entity Target;

    private void Awake()
    {
        UIController = FindObjectOfType<UIController>();
        Player = GetComponent<PlayerEntity>();
    }

    void Update()
    {
        time += Time.deltaTime;

        float inX = Input.GetAxisRaw("Horizontal");
        float inZ = Input.GetAxisRaw("Vertical");

        float movementVectorMagnitude = Time.deltaTime * Player.MovementSpeed;
        Vector3 movementVector = (this.transform.forward * inZ + this.transform.right * inX).normalized * movementVectorMagnitude;

        if (Input.GetButtonUp("Vertical") || Input.GetButtonUp("Horizontal"))
            stop = false;


        if (!UIController.IsActive && !stop && Player.Alive)
        {
            if (inX != 0 || inZ != 0)
            {
                this.transform.position += movementVector;
                Target = null;
            }
        }

        if (Target != null)
        {
            this.transform.LookAt(Target.transform.position);
            this.transform.eulerAngles = new Vector3(0, this.transform.eulerAngles.y, 0);
            this.transform.position += this.transform.forward * movementVectorMagnitude;

            if (Vector2.Distance(this.transform.position.XZ(), Target.transform.position.XZ()) < (Range - RANGE_ERROR_POINT))
            {
                Target = null;
            }
        }

        if (time > 0.1f && this.transform.hasChanged)
        {
            transform.hasChanged = false;
            AuthorySender.Movement();
            time = 0;
        }
    }

    public void EnableMovement(bool value = true)
    {
        stop = value;
    }

    public bool IsStopping()
    {
        return stop;
    }

    public void MoveTowards(Entity entity, float range)
    {
        Target = entity;
        Range = range;
    }
}
