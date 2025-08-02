using UnityEngine;

public class MyManagerDoor : MonoBehaviour
{
    public bool[] managerDoor = new bool[2];
    public Animator doorAnimator;
    private bool hasOpened;

    void Start()
    {
        if (doorAnimator == null)
            doorAnimator = GetComponent<Animator>();
    }

    void Update()
    {
        if (hasOpened) return;

        bool allTrue = true;
        foreach (bool flag in managerDoor)
        {
            if (!flag)
            {
                allTrue = false;
                break;
            }
        }

        if (allTrue)
        {
            doorAnimator.SetTrigger("Open");
            hasOpened = true;
        }
    }

    public void ActivateManagerDoor(int index)
    {
        if (index < 0 || index >= managerDoor.Length) return;
        managerDoor[index] = true;
    }

    public void ActivateAllManagerDoors()
    {
        for (int i = 0; i < managerDoor.Length; i++)
            managerDoor[i] = true;
    }
}