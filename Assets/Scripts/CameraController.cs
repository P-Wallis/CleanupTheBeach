using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraID
{
    Main,
    Notice,
    Scanning,
    Digging,
    Examine,
    Trigger_Memory,
    Remembering
}

public class CameraController : MonoBehaviour
{
    public Transform playerPos;
    //[Range(0.01f, 25f)]public float speed = 1;
    [Range(0.01f, 25f)]public float tweenTime = 1;
    public CameraTweenData[] cameraPositions = { };
    private Dictionary<CameraID, CameraTweenData> cameraDict = new Dictionary<CameraID, CameraTweenData>();

    private Vector3 offset;
    private Camera theCamera;

    private void Awake()
    {
        offset = transform.parent.position - playerPos.position;
        theCamera = GetComponent<Camera>();

        for (int i = 0; i < cameraPositions.Length; i++)
        {
            CameraID id = cameraPositions[i].id;
            if (cameraDict.ContainsKey(id))
            {
                Debug.LogError("Two cameras in the Camera Controller can't use the same ID!");
            }
            else
            {
                cameraDict.Add(id, cameraPositions[i]);
            }

            cameraPositions[i].CopyDataAndDisableCamera();
        }
    }

    private void Update()
    {
        transform.parent.position = playerPos.position + offset;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            TweenTo(CameraID.Main);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            TweenTo(CameraID.Notice);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            TweenTo(CameraID.Scanning);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            TweenTo(CameraID.Digging);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            TweenTo(CameraID.Examine);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            TweenTo(CameraID.Trigger_Memory);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            TweenTo(CameraID.Remembering);

        if (Input.GetKeyDown(KeyCode.Alpha0))
            TweenSequence(CameraID.Main, CameraID.Notice, CameraID.Trigger_Memory);
    }

    private Coroutine tween;
    private bool isTweening;
    private AnimationCurve smoothCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

    public void TweenSequence(params CameraID[] cameras)
    {
        if (isTweening)
        {
            StopCoroutine(tween);
            isTweening = false;
        }
        tween = StartCoroutine(CoTweenSeq(cameras));
    }

    public void TweenTo(CameraID camera)
    {
        TweenSequence(camera);
    }


    private IEnumerator CoTweenSeq(CameraID[] seq)
    {
        if (seq.Length > 0)
        {
            isTweening = true;
            float segmentLength = 1f / seq.Length;
            float segmentStart, segmentEnd, smoothStart, smoothEnd;
            CameraTweenData data;
            float fov;
            Vector3 pos;
            Quaternion rot;
            float percent;
            float dt = 1 / tweenTime;
            float t;

            for (int i = 0; i < seq.Length; i++)
            {
                segmentStart = i * segmentLength;
                smoothStart = smoothCurve.Evaluate(segmentStart);
                segmentEnd = segmentStart + segmentLength;
                smoothEnd = smoothCurve.Evaluate(segmentEnd);
                data = cameraDict[seq[i]];
                fov = theCamera.fieldOfView;
                pos = transform.localPosition;
                rot = transform.localRotation;
                percent = 0;


                while (percent < 1)
                {

                    t = Mathf.InverseLerp(smoothStart, smoothEnd, smoothCurve.Evaluate(Mathf.Lerp(segmentStart, segmentEnd, percent)));
                    theCamera.fieldOfView = Mathf.Lerp(fov, data.fov, t);
                    transform.localPosition = Vector3.Lerp(pos, data.position, t);
                    transform.localRotation = Quaternion.Lerp(rot, data.rotation, t);

                    yield return null;
                    percent += dt * Time.deltaTime;
                }

                theCamera.fieldOfView = data.fov;
                transform.localPosition = data.position;
                transform.localRotation = data.rotation;
            }
        }
        isTweening = false;
    }


    [System.Serializable]
    public class CameraTweenData
    {
        [HideInInspector]public string name;
        public CameraID id;
        public Camera camera;
        [HideInInspector]public Vector3 position;
        [HideInInspector]public Quaternion rotation;
        [HideInInspector]public float fov;

        public void CopyDataAndDisableCamera()
        {
            if (camera == null)
                return;

            position = camera.transform.localPosition;
            rotation = camera.transform.localRotation;
            fov = camera.fieldOfView;
            camera.gameObject.SetActive(false);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        for (int i = 0; i < cameraPositions.Length; i++)
        {
            cameraPositions[i].name = cameraPositions[i].id.ToString();
        }
    }
#endif
}
