using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClutterTempFix
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class ClutterTempFixPlugin : BaseUnityPlugin
    {
        internal const string ModName = "ClutterTempFix";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "Azumatt";
        private const string ModGUID = Author + "." + ModName;
        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource ClutterTempFixLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);
        public void Awake()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
        }
    }
    
    [HarmonyPatch(typeof(Sign), nameof(Sign.Awake))]
    static class SignAwakePatch
    {
        static void Prefix(Sign __instance)
        {
            var oldTextGameObject = Utils.FindChild(__instance.transform, "Text");
            if (oldTextGameObject != null)
            {
                var oldTextField = oldTextGameObject.GetComponent<Text>();
                if (oldTextField != null)
                {
                    // Create a new GameObject to hold the TextMeshProUGUI component
                    var newTextMeshProGameObject = new GameObject("Text");
                    newTextMeshProGameObject.transform.SetParent(oldTextGameObject.transform.parent);
                
                    // Just in case she has a different scale and shit
                    var transform = oldTextGameObject.transform;
                    newTextMeshProGameObject.transform.localPosition = transform.localPosition;
                    newTextMeshProGameObject.transform.localRotation = transform.localRotation;
                    newTextMeshProGameObject.transform.localScale = transform.localScale;

                    var textMeshPro = newTextMeshProGameObject.AddComponent<TextMeshProUGUI>();
                    textMeshPro.text = oldTextField.text;
                    textMeshPro.font = TMP_FontAsset.CreateFontAsset(oldTextField.font);
                    textMeshPro.fontSize = oldTextField.fontSize;
                    textMeshPro.color = oldTextField.color;
                    textMeshPro.alignment = TextAlignmentOptions.Center;
                    // Hoping this is enough...

                    // Replace the field value
                    __instance.m_textWidget = textMeshPro;

                    // Remove the old Text component
                    UnityEngine.Object.Destroy(oldTextField.gameObject);
                }
            }
        }

        static void Postfix(Sign __instance)
        {
            // Ensure the Sign script remains enabled, when testing, it was disabled for some reason
            if (!__instance.enabled)
            {
                __instance.enabled = true;
            }
        }
    }
}