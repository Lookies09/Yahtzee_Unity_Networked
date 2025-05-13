using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{
    private int num = 0;

    private bool isRoll = false;

    private bool isTake = false;

    private Animator animator;

    public bool IsTake { get => isTake; set => isTake = value; }
    public int Num { get => num; set => num = value; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void ResetDice()
    {
        animator.SetInteger("Num", num);
    }

    public void StartRollAnimation()
    {
        isRoll = true;
        animator.SetBool("Roll", isRoll);
        animator.SetInteger("Num", num);
    }

    public void StopRoll()
    {
        isRoll = false;
        animator.SetBool("Roll", isRoll);
    }

}
