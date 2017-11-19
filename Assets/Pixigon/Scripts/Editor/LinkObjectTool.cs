using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Pixigon.Editor.Tools
{
    [InitializeOnLoad]
    public class LinkObjectTool
    {
        public static System.Type filterDestinationType = null;
        public static System.Type filterSourceType = null;

        public delegate void LinkObjectsDelegate(GameObject[] source, GameObject destination);
        public static LinkObjectsDelegate onLinkObjects = null;
        public delegate void UnlinkObjectDelegate(GameObject[] gameObjects);
        public static UnlinkObjectDelegate onUnlinkObjects = null;

        private static GameObject destGameObject = null;
        private static bool isActive = true;

        private const int MAX_DEBUG_NAMES_LENGTH = 10;

        static LinkObjectTool()
        {
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
            SceneView.onSceneGUIDelegate += OnSceneGUI;
        }

        public static void SetActive(bool value)
        {
            isActive = value;

            if (!isActive)
                SceneView.onSceneGUIDelegate -= OnSceneGUI;
            else
                SceneView.onSceneGUIDelegate += OnSceneGUI;
        }


        static void OnSceneGUI(SceneView sceneView)
        {
            if (!isActive)
                return;

            if (!Selection.activeTransform)
                return;

            if (!IsCommandKey())
                return;

            if (!IsSourceFilterValid())
                return;

            Event e = Event.current;
            int controlID = GUIUtility.GetControlID(FocusType.Passive);
            switch (e.GetTypeForControl(controlID))
            {
                case EventType.MouseDown:
                    GUIUtility.hotControl = controlID;
                    e.Use();

                    if (Event.current.button == 0)
                    {
                        if (destGameObject && IsDestinationPresent())
                            InvokePickedObjectDelegate();
                        else
                        {
                            InvokeUnlinkObjectDelegate();
                            Selection.activeGameObject = null;
                        }
                    }

                    break;

                case EventType.MouseUp:
                    GUIUtility.hotControl = 0;
                    e.Use();
                    break;

                case EventType.MouseMove:
                    destGameObject = PickObject();

                    if (destGameObject && !IsValidObjectToBePicked())
                        destGameObject = null;
                    break;
            }

            bool isDestinationPresent = IsDestinationPresent();
            Vector3 worldMousePos = destGameObject && isDestinationPresent ? destGameObject.transform.position : GetMousePosition();

            Color lastColor = Handles.color;
            Handles.color = Color.green;

            Handles.DrawLine(GetSelectionPivot(), worldMousePos);

            if (destGameObject && isDestinationPresent)
                Handles.DrawWireCube(destGameObject.transform.position, Vector3.one * 0.5f);

            Handles.color = lastColor;
            HandleUtility.Repaint();
        }

        private static bool IsValidObjectToBePicked()
        {
            for (int i = 0; i < Selection.gameObjects.Length; i++)
            {
                // Do not pick itself
                if (Selection.gameObjects[i].GetHashCode() == destGameObject.GetHashCode())
                    return false;
            }

            return true;
        }

        private static bool IsSourceFilterValid()
        {
            if (filterSourceType == null)
                return true;

            for (int i = 0; i < Selection.transforms.Length; i++)
            {
                if (!Selection.transforms[i].GetComponent(filterSourceType))
                    return false;
            }

            return true;
        }

        static Vector3 GetSelectionPivot()
        {
            Transform[] trans = Selection.transforms;

            if (trans == null || trans.Length == 0)
                return Vector3.zero;

            if (trans.Length == 1)
                return trans[0].position;

            float minX = Mathf.Infinity;
            float minY = Mathf.Infinity;
            float minZ = Mathf.Infinity;

            float maxX = -Mathf.Infinity;
            float maxY = -Mathf.Infinity;
            float maxZ = -Mathf.Infinity;

            foreach (Transform tr in trans)
            {
                if (tr.position.x < minX)
                    minX = tr.position.x;
                if (tr.position.y < minY)
                    minY = tr.position.y;
                if (tr.position.z < minZ)
                    minZ = tr.position.z;

                if (tr.position.x > maxX)
                    maxX = tr.position.x;
                if (tr.position.y > maxY)
                    maxY = tr.position.y;
                if (tr.position.z > maxZ)
                    maxZ = tr.position.z;
            }

            return new Vector3((minX + maxX) / 2.0f, (minY + maxY) / 2.0f, (minZ + maxZ) / 2.0f);
        }

        static bool IsDestinationPresent()
        {
            return filterDestinationType != null ? destGameObject && destGameObject.GetComponent(filterDestinationType) != null : true;
        }

        static void InvokePickedObjectDelegate()
        {
            if (onLinkObjects == null)
            {
                string names = string.Join(", ", Selection.gameObjects.Take(MAX_DEBUG_NAMES_LENGTH)
                    .Select(x => x.name).ToArray());

                Debug.LogFormat("{0} -> {1}", names, destGameObject.name);
                return;
            }

            onLinkObjects.Invoke(Selection.gameObjects, destGameObject);
        }

        private static void InvokeUnlinkObjectDelegate()
        {
            if (onUnlinkObjects == null)
                return;

            onUnlinkObjects.Invoke(Selection.gameObjects);
        }

        static bool IsCommandKey()
        {
            return Event.current.control && Event.current.shift;
        }

        static GameObject PickObject()
        {
            return HandleUtility.PickGameObject(Event.current.mousePosition, false);
        }

        public static Vector3 GetMousePosition()
        {
            return HandleUtility.GUIPointToWorldRay(Event.current.mousePosition).origin;
        }

    }
}