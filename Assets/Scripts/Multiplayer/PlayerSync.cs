using UnityEngine;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(PhotonView))]
public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    [Header("Scene References")]
    [Tooltip("Drag the Map Space transform here (shared coordinate root)")]
    [SerializeField] private Transform mapSpace;
    [Tooltip("AR Camera (from XR Origin). If null, will use Camera.main")]
    [SerializeField] private Camera arCamera;

    [Header("Visuals")]
    [Tooltip("Capsule marker shown for remote players")]
    [SerializeField] private GameObject capsuleBody;
    [Tooltip("Full humanoid model — keep inactive for now")]
    [SerializeField] private GameObject humanoidBody;
    [Tooltip("Root GameObject of the name tag canvas (enable/disable)")]
    [SerializeField] private GameObject nameTagRoot;
    [SerializeField] private TMP_Text nameTagText;

    [Header("Smoothing")]
    [SerializeField] private float lerpSpeed = 12f;

    private Vector3 targetLocalPos;
    private Quaternion targetLocalRot;

    void Start()
    {
        if (mapSpace == null)
        {
            GameObject mapSpaceGo = GameObject.Find("Map Space");
            mapSpace = mapSpaceGo != null ? mapSpaceGo.transform : null;
        }

        if (arCamera == null)
        {
            if (Camera.main != null) arCamera = Camera.main;
        }

        // Set name label text from owner
        if (nameTagText != null)
        {
            nameTagText.text = photonView.Owner != null ? photonView.Owner.NickName : "Player";
        }

        ApplyOwnershipVisuals();

        targetLocalPos = transform.localPosition;
        targetLocalRot = transform.localRotation;
    }

    void ApplyOwnershipVisuals()
    {
        if (photonView.IsMine)
        {
            if (capsuleBody != null) capsuleBody.SetActive(false);
            if (humanoidBody != null) humanoidBody.SetActive(false);
            if (nameTagRoot != null) nameTagRoot.SetActive(false);
        }
        else
        {
            if (capsuleBody != null) capsuleBody.SetActive(true);
            if (humanoidBody != null) humanoidBody.SetActive(false);
            if (nameTagRoot != null) nameTagRoot.SetActive(true);
        }
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * lerpSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRot, Time.deltaTime * lerpSpeed);

            // Billboard name tag to face local AR camera
            if (nameTagRoot != null && arCamera != null)
            {
                Vector3 lookPos = arCamera.transform.position;
                Vector3 dir = lookPos - nameTagRoot.transform.position;
                dir.y = 0; // keep upright
                if (dir.sqrMagnitude > 0.001f)
                {
                    nameTagRoot.transform.rotation = Quaternion.LookRotation(dir);
                }
            }

            return;
        }

        // Local player: update transform to follow AR camera in map-space
        if (mapSpace == null || arCamera == null)
        {
            return;
        }

        Vector3 localPos = mapSpace.InverseTransformPoint(arCamera.transform.position);
        Quaternion localRot = Quaternion.Inverse(mapSpace.rotation) * arCamera.transform.rotation;

        // Apply directly for immediate local feedback
        transform.localPosition = localPos;
        transform.localRotation = localRot;
    }

    public void SetReferences(Transform mapSpaceRef, Camera arCameraRef)
    {
        mapSpace = mapSpaceRef;
        arCamera = arCameraRef;
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            if (mapSpace == null || arCamera == null)
            {
                stream.SendNext(transform.localPosition);
                stream.SendNext(transform.localRotation);
                return;
            }

            Vector3 localPos = mapSpace.InverseTransformPoint(arCamera.transform.position);
            Quaternion localRot = Quaternion.Inverse(mapSpace.rotation) * arCamera.transform.rotation;
            stream.SendNext(localPos);
            stream.SendNext(localRot);
        }
        else
        {
            targetLocalPos = (Vector3)stream.ReceiveNext();
            targetLocalRot = (Quaternion)stream.ReceiveNext();
        }
    }
}
