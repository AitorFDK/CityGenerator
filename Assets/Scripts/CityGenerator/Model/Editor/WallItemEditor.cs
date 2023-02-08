using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace CityGen.MenuItem
{

    [CustomEditor(typeof(WallItem), true)]
    public class WallItemEditor : Editor
    {
        public override bool HasPreviewGUI() { return true; }
        Editor gameObjectEditor;

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            GameObject obj = (target as WallItem).prefab != null ? (target as WallItem).prefab.gameObject : null;
            if (obj != null)
            {
                if (gameObjectEditor == null)
                {
                    gameObjectEditor = Editor.CreateEditor(obj);

                    //https://answers.unity.com/questions/133718/leaking-textures-in-custom-editor.html
                    //https://answers.unity.com/questions/643942/how-does-setting-the-hideflags-resolves-leaking-is.html?_ga=2.220097178.280693444.1610301585-205903802.1595764574
                    gameObjectEditor.hideFlags = HideFlags.DontSave;
                }
                gameObjectEditor.OnPreviewGUI(r, background);
            }

        }

        private void OnDisable()
        {

        }
    }
}