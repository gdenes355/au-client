using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class AU_PlayerController : MonoBehaviour
{
    public int id = -1;

    [SerializeField] public bool m_hasControl;

    Rigidbody m_rigidBody;
    Transform m_avatar;
    Animator m_animator;

    [SerializeField] InputAction WASD;
    [SerializeField] InputAction KILL;
    [SerializeField] InputAction REPORT;
    [SerializeField] InputAction USE;
    [SerializeField] InputAction VENT; 

    public Vector2 m_movementInput;

    [SerializeField] float m_movementSpeed;

    [SerializeField] public Color m_color;

    SpriteRenderer m_spriteRenderer;

    [SerializeField] public bool m_isImpostor;
    public bool m_hasCalledVote;
   

    List<AU_PlayerController> m_targets;
    [SerializeField] Collider m_collider;

    public string m_name;

    public Text name1Text;
    public Text name2Text;

    // to replay other player's motion
    public int seq;
    public List<float> xs;
    public List<float> ys;
    public float finalvx;
    public float finalvy;
    public float currvx;
    public float currvy;

    public void setName(string name)
    {
        m_name = name;
        name1Text.text = name;
        name2Text.text = name;
    }

    public bool m_isDead;

    [SerializeField] GameObject m_bodyPrefab;

    List<AU_Body> m_bodiesFound;

    public AU_Task activeTask;
    private bool m_onTask = false;

    public AU_Vent activeVent;
    private bool isVenting;

    private float m_ventedAgo = 0.0f;

    public float killCooldown;
    

    private HashSet<AU_PlayerController> nearbyPlayers = new HashSet<AU_PlayerController>();

    private void Awake()
    {
        KILL.performed += KillTarget;
        REPORT.performed += ReportBody;
        USE.performed += StartTask;
        VENT.performed += VentToggle;
    }

    private void OnEnable()
    {
        WASD.Enable();
        KILL.Enable();
        REPORT.Enable();
        USE.Enable();
        VENT.Enable();
    }

    private void OnDisable()
    {
        WASD.Disable();
        KILL.Disable();
        REPORT.Disable();
        USE.Disable();
        VENT.Disable();
    }

    // Start is called before the first frame update
    void Start()
    {
        if (m_hasControl)
        {
            if (GameController.myColor != null)
            {
                // take previous colour
                m_color = GameController.myColor;
            }
            GameController.Instance.LocalPlayer = this;
        }
        m_targets = new List<AU_PlayerController>();
        m_rigidBody = GetComponent<Rigidbody>();
        m_avatar = transform.GetChild(0);
        m_animator = GetComponent<Animator>();
        m_spriteRenderer = m_avatar.GetComponent<SpriteRenderer>();
        if (m_color == Color.clear)
        {
            m_color = Color.blue;
        }
        m_spriteRenderer.color = m_color;

        m_bodiesFound = new List<AU_Body>();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.state == "Voting" || m_onTask)
        {
            return;
        }


        if (m_hasControl)
        {
            m_movementInput = WASD.ReadValue<Vector2>();
            if (m_movementInput.x != 0)
            {
                m_avatar.localScale = new Vector2(Mathf.Sign(m_movementInput.x), 1);
            }
            if (isVenting && activeVent != null)
            {
                if (m_ventedAgo <= 0.5f)
                {
                    m_movementInput = Vector2.zero;
                    return;
                }
                m_animator.SetFloat("Speed", 0);
                if (m_movementInput.x > 0 && activeVent.D != null)
                {
                    transform.localPosition = new Vector3(activeVent.D.transform.localPosition.x, activeVent.D.transform.localPosition.y, transform.localPosition.z);
                    m_ventedAgo = 0.0f;
                }
                else if (m_movementInput.x < 0 && activeVent.A != null)
                {
                    transform.localPosition = new Vector3(activeVent.A.transform.localPosition.x, activeVent.A.transform.localPosition.y, transform.localPosition.z);
                    m_ventedAgo = 0.0f;
                }
                else if (m_movementInput.y > 0 && activeVent.W != null)
                {
                    transform.localPosition = new Vector3(activeVent.W.transform.localPosition.x, activeVent.W.transform.localPosition.y, transform.localPosition.z);
                    m_ventedAgo = 0.0f;
                }
                else if (m_movementInput.y < 0 && activeVent.S != null)
                {
                    transform.localPosition = new Vector3(activeVent.S.transform.localPosition.x, activeVent.S.transform.localPosition.y, transform.localPosition.z);
                    m_ventedAgo = 0.0f;
                }
                m_movementInput = Vector2.zero;
            }
            else
            {
                m_animator.SetFloat("Speed", m_movementInput.magnitude);
            }
            
        } else
        {
            if (currvx != 0)
            {
                m_avatar.localScale = new Vector2(Mathf.Sign(currvx), 1);
            }
            m_animator.SetFloat("Speed", currvx * currvx + currvy * currvy);
        }
    }

    private void FixedUpdate()
    {
        if (GameController.state == "Voting")
        {
            m_rigidBody.velocity = new Vector3(0, 0, 0);
            return;
        }

        m_ventedAgo += Time.fixedDeltaTime;

        killCooldown -= Time.fixedDeltaTime;
        killCooldown = Mathf.Max(0.0f, killCooldown);

        if (m_onTask)
        {
            m_rigidBody.velocity = new Vector3(0, 0, 0);
            return;
        }

        if (!m_hasControl) { 
            // other player
            if (xs.Count > 0 && ys.Count > 0)
            {
                currvx = (xs[0] - transform.position.x) / Time.fixedDeltaTime;
                currvy = (ys[0] - transform.position.y) / Time.fixedDeltaTime;
                transform.position = new Vector3(xs[0], ys[0], 0);
                xs.RemoveAt(0);
                ys.RemoveAt(0);
            } else
            {
                currvx = finalvx;
                currvy = finalvy;
                m_rigidBody.velocity = new Vector3(finalvx, finalvy, 0);
            }
            return;
        }


        // this player
        m_rigidBody.velocity = m_movementInput * m_movementSpeed;
        xs.Add(transform.position.x);
        ys.Add(transform.position.y);
        seq++;
        foreach (var elem in GameController.Instance.OtherPlayers)
        {
            bool close = (elem.Value.transform.position - transform.position).sqrMagnitude < 4;
            if (close && !nearbyPlayers.Contains(elem.Value))
            {
                OnTriggerEnter(elem.Value.GetComponent<CapsuleCollider>());
                nearbyPlayers.Add(elem.Value);
            }
            else if (!close && nearbyPlayers.Contains(elem.Value))
            {
                OnTriggerExit(elem.Value.GetComponent<CapsuleCollider>());
                nearbyPlayers.Remove(elem.Value);
            }
        }

        var bodies = FindObjectsOfType<AU_Body>(); 
        foreach (var body in bodies)
        {
            bool close = (body.transform.position - transform.position).sqrMagnitude < 4;
            if (close && !m_bodiesFound.Contains(body))
            {
                OnTriggerEnter(body.GetComponent<SphereCollider>());
                m_bodiesFound.Add(body);
                Debug.Log("Near a body");
            }
            else if (!close && m_bodiesFound.Contains(body))
            {
                OnTriggerExit(body.GetComponent<SphereCollider>());
                m_bodiesFound.Remove(body);
                Debug.Log("Left a body");
            }
        }
    }

    public void SetColor(Color newColor)
    {
        m_color = newColor;
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.color = m_color;
        }
    }

    public void SetIsImpostor(bool isImpostor)
    {
        m_isImpostor = isImpostor;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!m_hasControl) {
            return;
        }

        if (other.tag == "Player")
        {
            AU_PlayerController tempTarget = other.GetComponent<AU_PlayerController>();
            if (m_isImpostor)
            {
                if (tempTarget.m_isImpostor)
                {
                    return;
                }
                else
                {
                    m_targets.Add(tempTarget);
                }
            }
        } else if (other.tag == "Body")
        {
            AU_Body tempTarget = other.GetComponent<AU_Body>();
            if (m_bodiesFound.Contains(tempTarget))
            {
                return;
            }
            m_bodiesFound.Add(tempTarget);
        }
        else if (other.tag == "Task")
        {
            var candidate = other.GetComponent<AU_Task>();
            if (!candidate.solved)
            {
                activeTask = candidate;
                activeTask.setActive(true);
            }
        }
        else if (other.tag == "Vent")
        {
            var candidate = other.GetComponent<AU_Vent>();
            activeVent = candidate;
            Debug.Log("On vent " + activeVent.name);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!m_hasControl)
        {
            return;
        }

        if (other.tag == "Player")
        {
            AU_PlayerController tempTarget = other.GetComponent<AU_PlayerController>();
            if (m_targets.Contains(tempTarget))
            {
                m_targets.Remove(tempTarget);
            }
        }
        else if (other.tag == "Body")
        {
            AU_Body tempTarget = other.GetComponent<AU_Body>();
            if (m_bodiesFound.Contains(tempTarget))
            {
                m_bodiesFound.Remove(tempTarget);
            }
        }
        else if (other.tag == "Task")
        {
            if (activeTask != null)
            {
                activeTask.setActive(false);
            }
            activeTask = null;
        }
        else if (other.tag == "Vent")
        {
            if (activeVent == other.GetComponent<AU_Vent>())
            {
                activeVent = null;
            }
            Debug.Log("Left Vent ");
        }
    }

    private void KillTarget(InputAction.CallbackContext context)
    {
        if (GameController.state != "Game" || m_onTask || killCooldown > 0.0f)
        {
            return;
        }

        if (context.phase == InputActionPhase.Performed)
        {
            if (m_isDead)
            {
                return;
            }

            List<AU_PlayerController> targetAlive = m_targets.FindAll(t => !t.m_isDead);
            if (targetAlive.Count == 0)
            {
                return;
            }
    
            AU_PlayerController lastTarget = targetAlive[targetAlive.Count - 1];
            transform.position = lastTarget.transform.position;
            lastTarget.Die();
            m_targets.Remove(lastTarget);
            INetworkManager.sActiveInstace.KillOtherPlayer(lastTarget.id);
            killCooldown = 20.0f;
        }
    }

    private void StartTask(InputAction.CallbackContext context)
    {
        if (GameController.state != "Game" || m_onTask || activeTask == null)
        {
            return;
        }

        m_onTask = true;
        activeTask.Open();
    }

    private void VentToggle(InputAction.CallbackContext context)
    {
        if (!m_isImpostor || GameController.state != "Game" || activeVent == null)
        {
            return;
        }

        if (activeVent != null)
        {
            isVenting = !isVenting;
            m_spriteRenderer.enabled = !isVenting;
            Debug.Log("Venting " + isVenting);
        }
    }

    private void ReportBody(InputAction.CallbackContext context)
    {
        if (GameController.state != "Game" || m_onTask || m_isDead)
        {
            return;
        }

        if (context.phase == InputActionPhase.Performed)
        {
            List<AU_Body> unreportedBodies = m_bodiesFound.FindAll(b => !b.m_reported);
            if (unreportedBodies.Count == 0)
            {
                return;
            }

            AU_Body lastTarget = m_bodiesFound[m_bodiesFound.Count - 1];
            lastTarget.m_reported = true;
            INetworkManager.sActiveInstace.ReportBody(lastTarget.Player.id);

            m_bodiesFound.Remove(lastTarget);
            Debug.Log("REPORT");
        }
    }

    public void Die()
    {
        m_isDead = true;
        m_animator.SetBool("IsDead", m_isDead);
        m_collider.enabled = false;

        AU_Body tempBody = Instantiate(m_bodyPrefab, transform.position, transform.rotation).GetComponent<AU_Body>();
        tempBody.setColor(m_color);
        tempBody.Player = this;
        gameObject.layer = 9; // set to ghost
        foreach (Transform trans in gameObject.GetComponentsInChildren<Transform>(true))
        {
            trans.gameObject.layer = 9;
        }
        LightCaster lightCaster = GetComponent<LightCaster>();
        if (lightCaster.enabled)
        {
            lightCaster.enabled = false;
            GameObject.Find("LightMasking").SetActive(false);
            GameObject.Find("MapShadow").SetActive(false);
            Camera camera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
            camera.cullingMask = camera.cullingMask | (1 << LayerMask.NameToLayer("Otherside")) | (1 << LayerMask.NameToLayer("Ghost"));
        }
    }

    public void solvePuzzle()
    {
        INetworkManager.sActiveInstace.SolvedPuzzle();
        m_onTask = false;
        if (activeTask != null)
        {
            activeTask.Done();
        }
        activeTask = null;
    }
}
