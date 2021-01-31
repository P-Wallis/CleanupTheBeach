using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InstructionID
{
    NONE,
    Movement,
    Exit_Memory
}

public class InstructionManager : MonoBehaviour
{
    public GameObject movement, memoryExit;

    public void ShowInstructions(InstructionID instruction)
    {
        movement.SetActive(instruction == InstructionID.Movement);
        memoryExit.SetActive(instruction == InstructionID.Exit_Memory);
    }
}
