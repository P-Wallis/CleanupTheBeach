using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public enum SoundID
{
    Main_Theme,
    Ambient_Beach,
    Digging,
    Pop,
    Scanner_Found,
    Scanner,
    Walking_On_Sand,
    Memory_Trigger_Theme,
    Grab_Object,
    Special_Object
}

public enum MixerID
{
    Master = 0,
    Music,
    SFX,
    Ambient
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager _;

    [System.Serializable]
    public class AudioClipData
    {
        [HideInInspector]public string name;
        public SoundID id;
        public AudioClip clip;
    }

    public AudioMixerGroup[] mixers;
    public AudioClipData[] clips = { };
    public int oneShotPoolSize = 5;

    public Dictionary<SoundID, AudioClipData> clipDict = new Dictionary<SoundID, AudioClipData>();
    private Queue<AudioSource> oneShotPool = new Queue<AudioSource>();

    private void Awake()
    {
        if (_ == null)
        {
            _ = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }


        for (int i = 0; i < clips.Length; i++)
        {
            SoundID id = clips[i].id;
            if (clipDict.ContainsKey(id))
            {
                Debug.LogError("Two clips in the Audio Manager can't use the same ID!");
            }
            else
            {
                clipDict.Add(id, clips[i]);
            }
        }
    }

    private AudioSource mainTheme;
    private AudioSource memoryTheme;
    private AudioSource ambience;

    private void Start()
    {
        mainTheme = PlayLoopedAudio(SoundID.Main_Theme, MixerID.Music);
        memoryTheme = PlayLoopedAudio(SoundID.Memory_Trigger_Theme, MixerID.Music);
        memoryTheme.Stop();

        ambience = PlayLoopedAudio(SoundID.Ambient_Beach, MixerID.Ambient);
    }

    Coroutine crossfade;
    public void CrossfadeMusic(bool toMain = false)
    {
        if (crossfade != null)
            StopCoroutine(crossfade);
        crossfade = StartCoroutine(CoCrossfadeMusic(toMain));
    }

    IEnumerator CoCrossfadeMusic(bool toMain)
    {
        float percent = 0;

        if (toMain)
        {
            mainTheme.Play();
            mainTheme.volume = 0;
        }
        else
        {
            memoryTheme.Play();
            memoryTheme.volume = 0;
        }

        while (percent < 1)
        {
            mainTheme.volume = Mathf.Lerp(0, 1, toMain ? percent : 1 - percent);
            memoryTheme.volume = Mathf.Lerp(0, 1, toMain ? 1 - percent : percent);

            percent += Time.deltaTime;
            yield return null;
        }

        if (toMain)
        {
            memoryTheme.Stop();
            mainTheme.volume = 1;
        }
        else
        {
            mainTheme.Stop();
            memoryTheme.volume = 1;
        }
    }

    public AudioSource PlayLoopedAudio(SoundID sound, MixerID mixer)
    {
        AudioSource source = MakeNewSource(Vector3.zero);
        source.spatialBlend = 0;
        source.dopplerLevel = 0;
        source.loop = true;

        SetUpSound(source, sound, mixer);
        source.Play();

        return source;
    }

    public void PlayOneShotSFX(SoundID sound, Vector3 position, bool randomizePitch = false)
    {
        AudioSource source;
        if (oneShotPool.Count < oneShotPoolSize)
        {
            source = MakeNewSource(position);
            source.spatialBlend = 0;
            source.dopplerLevel = 0;
            source.loop = false;
        }
        else
        {
            source = oneShotPool.Dequeue();
            source.Stop();
        }

        if (randomizePitch)
        {
            source.pitch = Random.Range(0.9f, 1.15f);
        }
        source.gameObject.name = "(OneShot) ";
        SetUpSound(source, sound, MixerID.SFX);
        source.Play();
        oneShotPool.Enqueue(source);
    }

    private void SetUpSound(AudioSource source, SoundID sound, MixerID mixer)
    {
        AudioClipData data = clipDict[sound];
        if (data == null)
            return;

        source.gameObject.name += data.name;
        source.outputAudioMixerGroup = mixers[(int)mixer];
        source.clip = data.clip;
    }

    private AudioSource MakeNewSource(Vector3 position)
    {
        GameObject go = new GameObject();
        go.name = "";
        go.transform.parent = transform;
        go.transform.position = position;
        return go.AddComponent<AudioSource>();
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < clips.Length; i++)
        {
            clips[i].name = clips[i].id.ToString().Replace('_', ' ');
        }
    }
#endif
}
