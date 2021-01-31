using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MemoryFlag
{
    NONE = 0,
    Console = 1,
    Melodica = 2,
    Phone = 4,
    Toy = 8,
    ALL = 15
}

public class MemoriesController : MonoBehaviour
{
    public GameObject memoriesBase;
    public GameObject console;
    public GameObject melodica;
    public GameObject phone;
    public GameObject toy;
    public GameObject memoriesFull;

    private uint memoryFlags = 0;

    public void ShowNewMemory(MemoryFlag memory)
    {
        memoryFlags |= (uint)memory;
        ShowMemories(memoryFlags);
    }

    public void ShowMemories() { ShowMemories(memoryFlags); }
    public void ShowMemories(uint flags)
    {
        HideMemories();
        if (IsFlagUp(flags, MemoryFlag.ALL))
        {
            memoriesFull.SetActive(true);
            return;
        }

        memoriesBase.SetActive(true); // If we're here, we don't have the full one shown, so we need the base

        if (IsFlagUp(flags, MemoryFlag.Console))
            console.SetActive(true);
        if (IsFlagUp(flags, MemoryFlag.Melodica))
            melodica.SetActive(true);
        if (IsFlagUp(flags, MemoryFlag.Phone))
            phone.SetActive(true);
        if (IsFlagUp(flags, MemoryFlag.Toy))
            toy.SetActive(true);
    }

    public void HideMemories()
    {
        memoriesBase.SetActive(false);
        console.SetActive(false);
        melodica.SetActive(false);
        phone.SetActive(false); 
        toy.SetActive(false);
        memoriesFull.SetActive(false);
    }

    bool IsFlagUp(uint flags, MemoryFlag flag)
    {
        return (flags & (uint)flag) == (uint)flag;
    }

}
