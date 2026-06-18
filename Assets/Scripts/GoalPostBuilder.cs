using UnityEngine;

/// <summary>
/// Builds a regulation-size football goal from Unity primitives at runtime.
/// Attach to an empty GameObject named "Goal" positioned at the far end of the pitch.
/// </summary>
public class GoalPostBuilder : MonoBehaviour
{
    [Header("Goal Dimensions (metres)")]
    public float goalWidth  = 7.32f;
    public float goalHeight = 2.44f;
    public float postRadius = 0.06f;
    public float depth      = 1.5f;

    [Header("Materials")]
    public Material postMaterial;
    public Material netMaterial;

    void Awake()
    {
        BuildGoal();
    }

    void BuildGoal()
    {
        // --- Posts & Crossbar ---
        CreatePost("LeftPost",
            new Vector3(-goalWidth / 2f, goalHeight / 2f, 0f),
            new Vector3(postRadius * 2f, goalHeight, postRadius * 2f));

        CreatePost("RightPost",
            new Vector3(goalWidth / 2f, goalHeight / 2f, 0f),
            new Vector3(postRadius * 2f, goalHeight, postRadius * 2f));

        CreatePost("Crossbar",
            new Vector3(0f, goalHeight, 0f),
            new Vector3(goalWidth, postRadius * 2f, postRadius * 2f));

        // --- Back top bar ---
        CreatePost("BackTopBar",
            new Vector3(0f, goalHeight, -depth),
            new Vector3(goalWidth, postRadius * 2f, postRadius * 2f));

        // --- Side top bars ---
        CreatePost("LeftTopBar",
            new Vector3(-goalWidth / 2f, goalHeight, -depth / 2f),
            new Vector3(postRadius * 2f, postRadius * 2f, depth));

        CreatePost("RightTopBar",
            new Vector3(goalWidth / 2f, goalHeight, -depth / 2f),
            new Vector3(postRadius * 2f, postRadius * 2f, depth));

        // --- Back bottom bar ---
        CreatePost("BackBottomBar",
            new Vector3(0f, postRadius, -depth),
            new Vector3(goalWidth, postRadius * 2f, postRadius * 2f));

        // --- Side bottom bars ---
        CreatePost("LeftBottomBar",
            new Vector3(-goalWidth / 2f, postRadius, -depth / 2f),
            new Vector3(postRadius * 2f, postRadius * 2f, depth));

        CreatePost("RightBottomBar",
            new Vector3(goalWidth / 2f, postRadius, -depth / 2f),
            new Vector3(postRadius * 2f, postRadius * 2f, depth));

        // --- Net (back plane, flat quad) ---
        CreateNet();

        // --- Goal trigger volume ---
        CreateGoalTrigger();
    }

    void CreatePost(string postName, Vector3 localPos, Vector3 scale)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        // Use a cube instead for bars — cylinders are round, cubes are fine for posts too
        DestroyImmediate(go);

        go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = postName;
        go.transform.SetParent(transform);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;

        if (postMaterial != null)
            go.GetComponent<Renderer>().material = postMaterial;

        // Posts don't need colliders for ball interaction (trigger handles scoring)
        // but we keep them so the ball can visually hit the post
    }

    void CreateNet()
    {
        var net = GameObject.CreatePrimitive(PrimitiveType.Quad);
        net.name = "Net_Back";
        net.transform.SetParent(transform);
        net.transform.localPosition = new Vector3(0f, goalHeight / 2f, -depth);
        net.transform.localScale    = new Vector3(goalWidth, goalHeight, 1f);
        // Face toward the camera (toward the ball)
        net.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);

        if (netMaterial != null)
            net.GetComponent<Renderer>().material = netMaterial;
        else
        {
            // Default semi-transparent white net look
            var mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(1f, 1f, 1f, 0.25f);
            mat.SetFloat("_Mode", 3f); // Transparent
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
            net.GetComponent<Renderer>().material = mat;
        }

        // Remove collider from net — we want the ball to pass through
        DestroyImmediate(net.GetComponent<MeshCollider>());
    }

    void CreateGoalTrigger()
    {
        var trigger = new GameObject("GoalTrigger");
        trigger.transform.SetParent(transform);
        trigger.transform.localPosition = new Vector3(0f, goalHeight / 2f, -depth / 2f);
        trigger.layer = LayerMask.NameToLayer("Default");

        var box = trigger.AddComponent<BoxCollider>();
        box.isTrigger = true;
        box.size = new Vector3(goalWidth - postRadius * 2f, goalHeight - postRadius, depth - postRadius);

        trigger.AddComponent<GoalTrigger>();
    }
}
