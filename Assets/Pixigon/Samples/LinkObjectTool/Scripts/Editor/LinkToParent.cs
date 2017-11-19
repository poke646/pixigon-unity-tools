using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pixigon.Editor.Tools.Samples
{
    [InitializeOnLoad]
    public static class LinkToParent
    {
        static LinkToParent()
        {
            LinkObjectTool.onLinkObjects -= onLinkObjects;
            LinkObjectTool.onLinkObjects += onLinkObjects;

            LinkObjectTool.onUnlinkObjects -= onUnlinkObjects;
            LinkObjectTool.onUnlinkObjects += onUnlinkObjects;
        }

        private static void onLinkObjects(GameObject[] source, GameObject destination)
        {
            if (source.Length == 0)
                return;

            for (int i = 0; i < source.Length; i++)
            {
                source[i].transform.SetParent(destination.transform);
            }
            
        }

        private static void onUnlinkObjects(GameObject[] gameObjects)
        {
            if (gameObjects.Length == 0)
                return;

            for (int i = 0; i < gameObjects.Length; i++)
            {
                gameObjects[i].transform.SetParent(null);
            }
        }
    }
}