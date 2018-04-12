using System;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;
using UnityStandardAssets.Utility;
using UnityEngine.Networking;
using Manager;
using Random = UnityEngine.Random;

[RequireComponent(typeof(CharacterController))]
public class PlayerControllerPong : NetworkBehaviour
{
    [SerializeField] private bool m_IsWalking;
    [SerializeField] private float m_WalkSpeed;
    [SerializeField] private float m_RunSpeed;
    [SerializeField] [Range(0f, 1f)] private float m_RunstepLenghten;
    [SerializeField] private float m_JumpSpeed;
    [SerializeField] private float m_StickToGroundForce;
    [SerializeField] private float m_GravityMultiplier;
    [SerializeField] private UnityStandardAssets.Characters.FirstPerson.MouseLook m_MouseLook;
    [SerializeField] private bool m_UseFovKick;
    [SerializeField] private FOVKick m_FovKick = new FOVKick();
    [SerializeField] private bool m_UseHeadBob;
    [SerializeField] private CurveControlledBob m_HeadBob = new CurveControlledBob();
    [SerializeField] private LerpControlledBob m_JumpBob = new LerpControlledBob();
    [SerializeField] private float m_StepInterval;
    [SerializeField] private GameObject m_poutre;
    [SerializeField] private GameObject m_spawn;
    [SerializeField] private GameObject m_Bubble;
    [SerializeField] private GameObject m_BubbleSpawn;
    [SerializeField] private GameObject m_Manager;


    private Camera m_Camera;
    private bool m_Jump;
    private float m_YRotation;
    private Vector2 m_Input;
    private Vector3 m_MoveDir = Vector3.zero;
    private CharacterController m_CharacterController;
    private CollisionFlags m_CollisionFlags;
    private bool m_PreviouslyGrounded;
    private Vector3 m_OriginalCameraPosition;
    private float m_StepCycle;
    private float m_NextStep;
    private bool m_Jumping;
    private float m_poutreLength = 1;
    private Transform m_spawnPoint;
    private int m_canFire = 0;

    // Use this for initialization
    private void Start()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Camera = Camera.main;
        m_OriginalCameraPosition = m_Camera.transform.localPosition;
        m_FovKick.Setup(m_Camera);
        m_HeadBob.Setup(m_Camera, m_StepInterval);
        m_StepCycle = 0f;
        m_NextStep = m_StepCycle / 2f;
        m_Jumping = false;
        m_MouseLook.Init(transform, m_Camera.transform);
        m_Manager = GameObject.Find("GameManager");
        //m_spawnPoint = m_spawn.transform;
    }

    public override void OnStartLocalPlayer()
    {
        gameObject.GetComponentInChildren<Camera>().enabled = true;
        gameObject.GetComponentInChildren<UnityEngine.UI.RawImage>().enabled = true;
        gameObject.GetComponentInChildren<UnityEngine.UI.Text>().enabled = true;

    }


    // Update is called once per frame
    private void Update()
    {
        if (!isLocalPlayer)
            return;

        RotateView();
        // the jump state needs to read here to make sure it is not missed
        if (!m_Jump)
        {
            m_Jump = CrossPlatformInputManager.GetButtonDown("Jump");
        }

        if (!m_PreviouslyGrounded && m_CharacterController.isGrounded)
        {
            StartCoroutine(m_JumpBob.DoBobCycle());
            m_MoveDir.y = 0f;
            m_Jumping = false;
        }
        if (!m_CharacterController.isGrounded && !m_Jumping && m_PreviouslyGrounded)
        {
            m_MoveDir.y = 0f;
        }
        if (Input.mouseScrollDelta.magnitude != 0)
        {
            m_poutreLength += Input.mouseScrollDelta.y;
            if (m_poutreLength < 1)
                m_poutreLength = 1;
            if (m_poutreLength > 5)
                m_poutreLength = 5;
        }

        if (CrossPlatformInputManager.GetButtonDown("Fire1") && m_canFire == 0)
        {
            m_canFire = 20;
            CmdShootBubble();
        }

        m_PreviouslyGrounded = m_CharacterController.isGrounded;
        if (Input.GetKey("r") && !isServer)
        {
            foreach (GameObject message in GameObject.FindGameObjectsWithTag("Message"))
            {
                message.GetComponent<UnityEngine.UI.Text>().text = "";
            }
            CmdAskReset();
        }
            
    }

    [Command]
    private void CmdAskReset()
    {
        m_Manager.GetComponent<Manager.Manager>().AskReset();
    }

    private void FixedUpdate()
    {
        float speed;
        GetInput(out speed);
        if (!isLocalPlayer)
            return;
        // always move along the camera forward as it is the direction that it being aimed at
        Vector3 desiredMove = transform.forward * m_Input.y + transform.right * m_Input.x;

        // get a normal for the surface that is being touched to move along it
        RaycastHit hitInfo;
        Physics.SphereCast(transform.position, m_CharacterController.radius, Vector3.down, out hitInfo,
                           m_CharacterController.height / 2f, Physics.AllLayers, QueryTriggerInteraction.Ignore);
        desiredMove = Vector3.ProjectOnPlane(desiredMove, hitInfo.normal).normalized;

        m_MoveDir.x = desiredMove.x * speed;
        m_MoveDir.z = desiredMove.z * speed;


        if (m_CharacterController.isGrounded)
        {
            m_MoveDir.y = -m_StickToGroundForce;

            if (m_Jump)
            {
                m_MoveDir.y = m_JumpSpeed;
                m_Jump = false;
                m_Jumping = true;
            }
        }
        else
        {
            m_MoveDir += Physics.gravity * m_GravityMultiplier * Time.fixedDeltaTime;
        }
        m_CollisionFlags = m_CharacterController.Move(m_MoveDir * Time.fixedDeltaTime);

        ProgressStepCycle(speed);
        UpdateCameraPosition(speed);

        m_MouseLook.UpdateCursorLock();

        if (m_canFire > 0)
            m_canFire--;
    }


    private void ProgressStepCycle(float speed)
    {
        if (m_CharacterController.velocity.sqrMagnitude > 0 && (m_Input.x != 0 || m_Input.y != 0))
        {
            m_StepCycle += (m_CharacterController.velocity.magnitude + (speed * (m_IsWalking ? 1f : m_RunstepLenghten))) *
                         Time.fixedDeltaTime;
        }

        if (!(m_StepCycle > m_NextStep))
        {
            return;
        }

        m_NextStep = m_StepCycle + m_StepInterval;

    }


    private void UpdateCameraPosition(float speed)
    {
        Vector3 newCameraPosition;
        if (!m_UseHeadBob)
        {
            return;
        }
        if (m_CharacterController.velocity.magnitude > 0 && m_CharacterController.isGrounded)
        {
            m_Camera.transform.localPosition =
                m_HeadBob.DoHeadBob(m_CharacterController.velocity.magnitude +
                                  (speed * (m_IsWalking ? 1f : m_RunstepLenghten)));
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_Camera.transform.localPosition.y - m_JumpBob.Offset();
        }
        else
        {
            newCameraPosition = m_Camera.transform.localPosition;
            newCameraPosition.y = m_OriginalCameraPosition.y - m_JumpBob.Offset();
        }
        m_Camera.transform.localPosition = newCameraPosition;
    }


    private void GetInput(out float speed)
    {
        // Read input
        float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
        float vertical = CrossPlatformInputManager.GetAxis("Vertical");

        bool waswalking = m_IsWalking;

#if !MOBILE_INPUT
        // On standalone builds, walk/run speed is modified by a key press.
        // keep track of whether or not the character is walking or running
        m_IsWalking = !Input.GetKey(KeyCode.LeftShift);
#endif
        // set the desired speed to be walking or running
        speed = m_IsWalking ? m_WalkSpeed : m_RunSpeed;
        m_Input = new Vector2(horizontal, vertical);

        // normalize input if it exceeds 1 in combined length:
        if (m_Input.sqrMagnitude > 1)
        {
            m_Input.Normalize();
        }

        // handle speed change to give an fov kick
        // only if the player is going to a run, is running and the fovkick is to be used
        if (m_IsWalking != waswalking && m_UseFovKick && m_CharacterController.velocity.sqrMagnitude > 0)
        {
            StopAllCoroutines();
            StartCoroutine(!m_IsWalking ? m_FovKick.FOVKickUp() : m_FovKick.FOVKickDown());
        }
    }


    private void RotateView()
    {
        m_MouseLook.LookRotation(transform, m_Camera.transform);
    }


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        Rigidbody body = hit.collider.attachedRigidbody;
        //dont move the rigidbody if the character is on top of it
        if (m_CollisionFlags == CollisionFlags.Below)
        {
            return;
        }

        if (body == null || body.isKinematic)
        {
            return;
        }
        body.AddForceAtPosition(m_CharacterController.velocity * 0.1f, hit.point, ForceMode.Impulse);
    }

    [Command]
    private void CmdShootPoutre()
    {
        GameObject tmp = Instantiate(m_poutre, m_spawnPoint.position, m_spawnPoint.rotation);
        Vector3 scale = tmp.transform.localScale;
        scale.z *= m_poutreLength;
        tmp.transform.localScale = scale;
        Rigidbody body = tmp.GetComponent<Rigidbody>();
        body.velocity = tmp.transform.forward * 40;
        m_canFire = 20;

        NetworkServer.Spawn(tmp);

    }

    [Command]
    private void CmdShootBubble()
    {
        Debug.Log("shoot");
        GameObject tmp = Instantiate(m_Bubble, m_BubbleSpawn.transform.position, m_BubbleSpawn.transform.rotation);
        Rigidbody body = tmp.GetComponent<Rigidbody>();
        body.velocity = tmp.transform.forward * 200;

        NetworkServer.Spawn(tmp);

    }
}