using UnityEngine;
using Photon.Pun;
using TMPro;

[RequireComponent(typeof(PhotonView))]
public class PlayerSync : MonoBehaviourPun, IPunObservable
{
    [Header("Scene References")]
    [SerializeField] private Transform mapSpace;
    [SerializeField] private Transform arCamera;

    [Header("Visuals")]
    [SerializeField] private GameObject localOnlyRoot;
    [SerializeField] private GameObject remoteOnlyRoot;
    [SerializeField] private TMP_Text nameLabel;

    [Header("Smoothing")]
    [SerializeField] private float lerpSpeed = 6f;

    private Vector3 targetLocalPos;
    private Quaternion targetLocalRot;

    void Start()
    {
        if (mapSpace == null)
        {
            GameObject mapSpaceGo = GameObject.Find("Map Space");
            mapSpace = mapSpaceGo != null ? mapSpaceGo.transform : null;
        }

        if (arCamera == null && Camera.main != null)
        {
            arCamera = Camera.main.transform;
        }

        if (nameLabel != null)
        {
            nameLabel.text = photonView.Owner != null ? photonView.Owner.NickName : "Player";
        }

        if (photonView.IsMine)
        {
            if (localOnlyRoot != null) localOnlyRoot.SetActive(true);
            if (remoteOnlyRoot != null) remoteOnlyRoot.SetActive(false);
        }
        else
        {
            if (localOnlyRoot != null) localOnlyRoot.SetActive(false);
            if (remoteOnlyRoot != null) remoteOnlyRoot.SetActive(true);
        }

        targetLocalPos = transform.localPosition;
        targetLocalRot = transform.localRotation;
    }

    void Update()
    {
        if (!photonView.IsMine)
        {
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetLocalPos, Time.deltaTime * lerpSpeed);
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetLocalRot, Time.deltaTime * lerpSpeed);
            return;
        }

        if (mapSpace == null || arCamera == null)
        {
            return;
        }

        Vector3 localPos = mapSpace.InverseTransformPoint(arCamera.position);
        Quaternion localRot = Quaternion.Inverse(mapSpace.rotation) * arCamera.rotation;
        transform.localPosition = localPos;
        transform.localRotation = localRot;
    }

    public void SetReferences(Transform mapSpaceRef, Transform arCameraRef)
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

            Vector3 localPos = mapSpace.InverseTransformPoint(arCamera.position);
            Quaternion localRot = Quaternion.Inverse(mapSpace.rotation) * arCamera.rotation;
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
