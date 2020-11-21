using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectController : MonoBehaviour
{
    [SerializeField]
    private UnityEvent interactEvent;
    [SerializeField]
    private UnityEvent startEvent;
    [SerializeField]
    private float intreractDistance = 3.0f;
    [SerializeField]
    private GameObject interactImage;
    [SerializeField]
    private bool canInteract = true;

    private System.Action interactAction;
    private System.Action startAction;

    private Transform player;
    private Transform centerTarget;
    private bool palyerInRange;

    public bool CanInteract
    {
        set { canInteract = value; }
    }

    void Start()
    {
        player = PlayerController.Instance.transform;

        centerTarget = transform.Find("CenterTarget");

        interactAction += interactEvent.Invoke;
        startAction += startEvent.Invoke;

        palyerInRange = false;
    }

    void Update()
    {
        if (!canInteract || interactImage == null) return;

        if (palyerInRange && centerTarget != null)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(centerTarget.position);

            interactImage.transform.position = screen;

            if (PlayerInput.Instance.getKeyDown(KeyCode.F))
            {
                playAction();
            }

            float sqrDistance = (player.position - transform.position).sqrMagnitude;
            if (sqrDistance > intreractDistance * intreractDistance)
            {
                hideInteractImage();
            }
        }
    }

    public void setCanInteract(bool val)
    {
        canInteract = val;
    }

    public void playAction()
    {
        interactAction();

        if (interactImage != null)
        {
            interactImage.SetActive(false);
        }

        gameObject.tag = "Untagged";
        this.enabled = false;
    }

    public void showInteractImage()
    {
        if (!canInteract) return;

        if (interactImage != null)
        {
            Vector3 screen = Camera.main.WorldToScreenPoint(centerTarget.position);

            interactImage.transform.position = screen;
            interactImage.SetActive(true);
        }

        startAction();

        palyerInRange = true;
    }

    public void moveToTarget(Transform target)
    {
        StartCoroutine(move(target));
    }

    private IEnumerator move(Transform target)
    {
        float distance = Mathf.Infinity;
        while (!Mathf.Approximately(distance, 0))
        {
            transform.position = Vector3.MoveTowards(transform.position, target.position, Time.deltaTime);

            distance = (transform.position - target.position).sqrMagnitude;

            yield return null;
        }
    }

    private void hideInteractImage()
    {
        if (interactImage != null)
        {
            interactImage.SetActive(false);
        }

        palyerInRange = false;
    }

    public void spawnEnemy(EnemyController target)
    {
        target.spawn();
    }

    public void openChestBox(Transform target)
    {
        target.eulerAngles = new Vector3(-90, target.eulerAngles.y, target.eulerAngles.z);
    }

    public void openDoor(Transform target)
    {
        //target.eulerAngles = new Vector3(target.eulerAngles.x, 0, target.eulerAngles.z);
        StartCoroutine(openDoorCoroutine(target));
    }

    private IEnumerator openDoorCoroutine(Transform target)
    {
        Vector3 angle = new Vector3(target.eulerAngles.x, 0, target.eulerAngles.z);

        while (!Mathf.Approximately(target.eulerAngles.y, 0))
        {
            if (target.eulerAngles.y < 180)
                target.eulerAngles = Vector3.MoveTowards(target.eulerAngles, angle, 50 * Time.deltaTime);
            else
                target.eulerAngles = Vector3.MoveTowards(target.eulerAngles, angle, -50 * Time.deltaTime);

            yield return null;
        }
    }
}
