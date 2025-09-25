using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyPatrol2D))]
public class EnemyPatrol2DEditor : Editor
{
    SerializedProperty mode;

    SerializedProperty speed;
    SerializedProperty startFacingRight;
    SerializedProperty waitAtTurn;

    // BetweenPoints
    SerializedProperty pointA;
    SerializedProperty pointB;
    SerializedProperty arriveThreshold;

    // Range
    SerializedProperty center;
    SerializedProperty leftOffset;
    SerializedProperty rightOffset;
    SerializedProperty useOwnCenter;
    SerializedProperty useColliderWidth;

    // GroundWalker
    SerializedProperty groundCheck;
    SerializedProperty wallCheck;
    SerializedProperty groundMask;
    SerializedProperty groundCheckDistance;
    SerializedProperty wallCheckDistance;

    // Animator
    SerializedProperty animatorProp;

    // Gizmos
    SerializedProperty gizmoPatrolColor;
    SerializedProperty gizmoLimitColor;
    SerializedProperty gizmoRayColor;

    void OnEnable()
    {
        mode = serializedObject.FindProperty("mode");

        speed = serializedObject.FindProperty("speed");
        startFacingRight = serializedObject.FindProperty("startFacingRight");
        waitAtTurn = serializedObject.FindProperty("waitAtTurn");

        pointA = serializedObject.FindProperty("pointA");
        pointB = serializedObject.FindProperty("pointB");
        arriveThreshold = serializedObject.FindProperty("arriveThreshold");

        center = serializedObject.FindProperty("center");
        leftOffset = serializedObject.FindProperty("leftOffset");
        rightOffset = serializedObject.FindProperty("rightOffset");
        useOwnCenter = serializedObject.FindProperty("useOwnCenter");
        useColliderWidth = serializedObject.FindProperty("useColliderWidth");

        groundCheck = serializedObject.FindProperty("groundCheck");
        wallCheck = serializedObject.FindProperty("wallCheck");
        groundMask = serializedObject.FindProperty("groundMask");
        groundCheckDistance = serializedObject.FindProperty("groundCheckDistance");
        wallCheckDistance = serializedObject.FindProperty("wallCheckDistance");

        animatorProp = serializedObject.FindProperty("animator");

        gizmoPatrolColor = serializedObject.FindProperty("gizmoPatrolColor");
        gizmoLimitColor = serializedObject.FindProperty("gizmoLimitColor");
        gizmoRayColor = serializedObject.FindProperty("gizmoRayColor");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(mode);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Movimiento básico", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(speed);
        EditorGUILayout.PropertyField(startFacingRight);
        EditorGUILayout.PropertyField(waitAtTurn);

        EditorGUILayout.Space();

        // Mostrar campos según el modo
        var m = (EnemyPatrol2D.PatrolMode)mode.enumValueIndex;
        switch (m)
        {
            case EnemyPatrol2D.PatrolMode.BetweenPoints:
                EditorGUILayout.LabelField("Entre puntos", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(pointA);
                EditorGUILayout.PropertyField(pointB);
                EditorGUILayout.PropertyField(arriveThreshold);
                DrawUtilityButtons_CreatePoints();
                break;

            case EnemyPatrol2D.PatrolMode.Range:
                EditorGUILayout.LabelField("Por rango", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(useOwnCenter, new GUIContent("Usar centro propio", "Ignora 'center' y usa la X inicial del enemigo"));
                EditorGUILayout.PropertyField(center);
                EditorGUILayout.PropertyField(leftOffset);
                EditorGUILayout.PropertyField(rightOffset);
                EditorGUILayout.PropertyField(useColliderWidth, new GUIContent("Usar ancho collider", "Ajusta los límites considerando el ancho del collider"));
                if (GUILayout.Button("Set center = current X"))
                {
                    var t = ((EnemyPatrol2D)target).transform;
                    if (center.objectReferenceValue == null)
                    {
                        // crear dummy center
                        var go = new GameObject("PatrolCenter");
                        Undo.RegisterCreatedObjectUndo(go, "Create Patrol Center");
                        go.transform.SetParent(t, false);
                        go.transform.localPosition = Vector3.zero;
                        center.objectReferenceValue = go.transform;
                    }
                    else
                    {
                        var c = (Transform)center.objectReferenceValue;
                        c.position = new Vector3(t.position.x, c.position.y, c.position.z);
                    }
                }
                if (useOwnCenter.boolValue)
                {
                    EditorGUILayout.HelpBox("Con 'Usar centro propio' activo, el script ignora el objeto 'center' y usa la X actual del enemigo al iniciar.", MessageType.Info);
                }
                break;

            case EnemyPatrol2D.PatrolMode.GroundWalker:
                EditorGUILayout.LabelField("Caminante de borde", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(groundCheck);
                EditorGUILayout.PropertyField(wallCheck);
                EditorGUILayout.PropertyField(groundMask);
                EditorGUILayout.PropertyField(groundCheckDistance);
                EditorGUILayout.PropertyField(wallCheckDistance);
                DrawUtilityButtons_CreateChecks();
                break;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Animator (opcional)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(animatorProp);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug / Gizmos", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(gizmoPatrolColor);
        EditorGUILayout.PropertyField(gizmoLimitColor);
        EditorGUILayout.PropertyField(gizmoRayColor);

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawUtilityButtons_CreatePoints()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Crear pointA"))
        {
            var t = ((EnemyPatrol2D)target).transform;
            var go = new GameObject("PointA");
            Undo.RegisterCreatedObjectUndo(go, "Create PointA");
            go.transform.SetParent(t, false);
            go.transform.localPosition = Vector3.left;
            pointA.objectReferenceValue = go.transform;
        }
        if (GUILayout.Button("Crear pointB"))
        {
            var t = ((EnemyPatrol2D)target).transform;
            var go = new GameObject("PointB");
            Undo.RegisterCreatedObjectUndo(go, "Create PointB");
            go.transform.SetParent(t, false);
            go.transform.localPosition = Vector3.right;
            pointB.objectReferenceValue = go.transform;
        }
        EditorGUILayout.EndHorizontal();
    }

    private void DrawUtilityButtons_CreateChecks()
    {
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Crear groundCheck"))
        {
            var t = ((EnemyPatrol2D)target).transform;
            var go = new GameObject("GroundCheck");
            Undo.RegisterCreatedObjectUndo(go, "Create GroundCheck");
            go.transform.SetParent(t, false);
            go.transform.localPosition = new Vector3(0.2f, -0.1f, 0f);
            groundCheck.objectReferenceValue = go.transform;
        }
        if (GUILayout.Button("Crear wallCheck"))
        {
            var t = ((EnemyPatrol2D)target).transform;
            var go = new GameObject("WallCheck");
            Undo.RegisterCreatedObjectUndo(go, "Create WallCheck");
            go.transform.SetParent(t, false);
            go.transform.localPosition = new Vector3(0.25f, 0f, 0f);
            wallCheck.objectReferenceValue = go.transform;
        }
        EditorGUILayout.EndHorizontal();
    }
}
