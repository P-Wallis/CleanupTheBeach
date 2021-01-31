using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

    private Image baseImage;
    private Image consoleImage;
    private Image melodicaImage;
    private Image phoneImage;
    private Image toyImage;
    private Image fullImage;

    private Color whiteClear = new Color(1, 1, 1, 0);

    private void Awake()
    {
        baseImage = memoriesBase.GetComponent<Image>();
        consoleImage = console.GetComponent<Image>();
        melodicaImage = melodica.GetComponent<Image>();
        phoneImage = phone.GetComponent<Image>();
        toyImage = toy.GetComponent<Image>();
        fullImage = memoriesFull.GetComponent<Image>();

    }

    private uint memoryFlags = 0;
    public bool AllMemoriesShown { get { return IsFlagUp(memoryFlags, MemoryFlag.ALL); } }
    MemoryFlag newMemory = MemoryFlag.NONE;

    public void ShowNewMemory(MemoryFlag memory)
    {
        memoryFlags |= (uint)memory;
        if (newMemory == MemoryFlag.NONE)
            StartCoroutine(CoFadeIn(baseImage));
        newMemory = memory;
        ShowMemories(memoryFlags);
    }

    public void ShowMemories() { ShowMemories(memoryFlags); }
    public void ShowMemories(uint flags)
    {
        if (IsFlagUp(flags, MemoryFlag.ALL))
        {
            StartCoroutine(CoFadeIn(fullImage));
            memoriesFull.SetActive(true);
            return;
        }

        HideMemories();
        memoriesBase.SetActive(true); // If we're here, we don't have the full one shown, so we need the base

        if (IsFlagUp(flags, MemoryFlag.Console))
            console.SetActive(true);
        if (IsFlagUp(flags, MemoryFlag.Melodica))
            melodica.SetActive(true);
        if (IsFlagUp(flags, MemoryFlag.Phone))
            phone.SetActive(true);
        if (IsFlagUp(flags, MemoryFlag.Toy))
            toy.SetActive(true);

        switch (newMemory)
        {
            case MemoryFlag.Console:
            StartCoroutine(CoFadeIn(consoleImage));
            break;
            case MemoryFlag.Melodica:
            StartCoroutine(CoFadeIn(melodicaImage));
            break;
            case MemoryFlag.Phone:
            StartCoroutine(CoFadeIn(phoneImage));
            break;
            case MemoryFlag.Toy:
            StartCoroutine(CoFadeIn(toyImage));
            break;
        }
    }

    IEnumerator CoFadeIn(Image image)
    {
        float percent = 0;
        image.color = whiteClear;

        yield return new WaitForSeconds(0.75f);

        while (percent < 1)
        {
            image.color = Color.Lerp(whiteClear, Color.white, percent);
            percent += Time.deltaTime;
            yield return null;
        }

        image.color = Color.white;
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
