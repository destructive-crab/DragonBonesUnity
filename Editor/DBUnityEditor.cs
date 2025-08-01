/**
 * The MIT License (MIT)
 *
 * Copyright (c) 2012-2017 DragonBones team and other contributors
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy of
 * this software and associated documentation files (the "Software"), to deal in
 * the Software without restriction, including without limitation the rights to
 * use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
 * the Software, and to permit persons to whom the Software is furnished to do so,
 * subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
 * FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
 * COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
 * IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
 * CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DragonBones
{
    public class DBUnityEditor
    {
        [MenuItem("GameObject/DragonBones/Armature Object", false, 10)]
        private static void _CreateArmatureObjectMenuItem()
        {
            _CreateEmptyObject(GetSelectionParentTransform());
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object", true)]
        private static bool _CreateArmatureObjectFromSkeValidateMenuItem()
        {
            return _GetDragonBonesSkePaths().Count > 0;
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object", false, 10)]
        private static void _CreateArmatureObjectFromSkeMenuItem()
        {
            var parentTransform = GetSelectionParentTransform();
            foreach (var dragonBonesJSONPath in _GetDragonBonesSkePaths())
            {
                var armatureComponent = _CreateEmptyObject(parentTransform);
                var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;

                ChangeDragonBonesData(armatureComponent, dragonBonesJSON);
            }
        }

        [MenuItem("GameObject/DragonBones/Armature Object(UGUI)", false, 11)]
        private static void _CreateUGUIArmatureObjectMenuItem()
        {
            var armatureComponent = _CreateEmptyObject(GetSelectionParentTransform());
            armatureComponent.isUGUI = true;
            if (armatureComponent.GetComponentInParent<Canvas>() == null)
            {
                var canvas = GameObject.Find("/Canvas");
                if (canvas)
                {
                    armatureComponent.transform.SetParent(canvas.transform);
                }
            }

            armatureComponent.transform.localScale = Vector2.one * 100.0f;
            armatureComponent.transform.localPosition = Vector3.zero;
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object(UGUI)", true)]
        private static bool _CreateUGUIArmatureObjectFromJSONValidateMenuItem()
        {
            return _GetDragonBonesSkePaths().Count > 0;
        }

        [MenuItem("Assets/Create/DragonBones/Armature Object(UGUI)", false, 11)]
        private static void _CreateUGUIArmatureObjectFromJSOIMenuItem()
        {
            var parentTransform = GetSelectionParentTransform();
            foreach (var dragonBonesJSONPath in _GetDragonBonesSkePaths())
            {
                var armatureComponent = _CreateEmptyObject(parentTransform);
                armatureComponent.isUGUI = true;
                if (armatureComponent.GetComponentInParent<Canvas>() == null)
                {
                    var canvas = GameObject.Find("/Canvas");
                    if (canvas)
                    {
                        armatureComponent.transform.SetParent(canvas.transform);
                    }
                }
                armatureComponent.transform.localScale = Vector2.one * 100.0f;
                armatureComponent.transform.localPosition = Vector3.zero;
                var dragonBonesJSON = AssetDatabase.LoadMainAssetAtPath(dragonBonesJSONPath) as TextAsset;

                ChangeDragonBonesData(armatureComponent, dragonBonesJSON);
            }
        }


        [MenuItem("Assets/Create/DragonBones/Create Unity Data", true)]
        private static bool _CreateUnityDataValidateMenuItem()
        {
            return _GetDragonBonesSkePaths(true).Count > 0;
        }

        [MenuItem("Assets/Create/DragonBones/Create Unity Data", false, 32)]
        private static void _CreateUnityDataMenuItem()
        {
            foreach (var dragonBonesSkePath in _GetDragonBonesSkePaths(true))
            {
                var dragonBonesSke = AssetDatabase.LoadMainAssetAtPath(dragonBonesSkePath) as TextAsset;
                var textureAtlasJSONs = new List<string>();
                GetTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(dragonBonesSke.GetInstanceID()));
                Debug.Log(AssetDatabase.GetAssetPath(dragonBonesSke.GetInstanceID()));
                UnityDragonBonesData.TextureAtlas[] textureAtlas = new UnityDragonBonesData.TextureAtlas[textureAtlasJSONs.Count];

                for (int i = 0; i < textureAtlasJSONs.Count; ++i)
                {
                    string path = textureAtlasJSONs[i];
                    //load textureAtlas data
                    UnityDragonBonesData.TextureAtlas ta = new UnityDragonBonesData.TextureAtlas();
                    ta.textureAtlasJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    //load texture
                    path = path.Substring(0, path.LastIndexOf(".json"));
                    ta.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path + ".png");
                    //load material
                    ta.material = AssetDatabase.LoadAssetAtPath<Material>(path + "_Mat.mat");
                    ta.uiMaterial = AssetDatabase.LoadAssetAtPath<Material>(path + "_UI_Mat.mat");
                    textureAtlas[i] = ta;
                }

                //
                CreateUnityDragonBonesData(dragonBonesSke, textureAtlas);
            }
        }

        public static UnityDragonBonesData.TextureAtlas[] GetTextureAtlasByJSONs(List<string> textureAtlasJSONs)
        {
            UnityDragonBonesData.TextureAtlas[] textureAtlas = new UnityDragonBonesData.TextureAtlas[textureAtlasJSONs.Count];

            for (int i = 0; i < textureAtlasJSONs.Count; ++i)
            {
                string path = textureAtlasJSONs[i];
                //load textureAtlas data
                UnityDragonBonesData.TextureAtlas ta = new UnityDragonBonesData.TextureAtlas();
                ta.textureAtlasJSON = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                //load texture
                path = path.Substring(0, path.LastIndexOf(".json"));
                ta.texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path + ".png");
                //load material
                ta.material = AssetDatabase.LoadAssetAtPath<Material>(path + "_Mat.mat");
                ta.uiMaterial = AssetDatabase.LoadAssetAtPath<Material>(path + "_UI_Mat.mat");
                textureAtlas[i] = ta;
            }

            return textureAtlas;
        }


        public static bool ChangeDragonBonesData(ArmatureUnityInstance armatureUnityInstance, TextAsset dragonBoneJSON)
        {
            if (dragonBoneJSON != null)
            {
                var textureAtlasJSONs = new List<string>();
                DBUnityEditor.GetTextureAtlasConfigs(textureAtlasJSONs, AssetDatabase.GetAssetPath(dragonBoneJSON.GetInstanceID()));

                UnityDragonBonesData.TextureAtlas[] textureAtlas = DBUnityEditor.GetTextureAtlasByJSONs(textureAtlasJSONs);

                UnityDragonBonesData data = DBUnityEditor.CreateUnityDragonBonesData(dragonBoneJSON, textureAtlas);
                armatureUnityInstance.unityData = data;

                var dragonBonesData = DBUnityFactory.factory.LoadData(data, armatureUnityInstance.isUGUI);
                if (dragonBonesData != null)
                {
                    Undo.RecordObject(armatureUnityInstance, "Set DragonBones");

                    armatureUnityInstance.unityData = data;

                    var armatureName = dragonBonesData.armatureNames[0];
                    ChangeArmatureData(armatureUnityInstance, armatureName, armatureUnityInstance.unityData.dataName);

                    armatureUnityInstance.gameObject.name = armatureName;

                    EditorUtility.SetDirty(armatureUnityInstance);

                    return true;
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Could not load Dragon Bones data.", "OK", null);

                    return false;
                }
            }
            else if (armatureUnityInstance.unityData != null)
            {
                Undo.RecordObject(armatureUnityInstance, "Set Dragon Bones");

                armatureUnityInstance.unityData = null;

                if (armatureUnityInstance.armature != null)
                {
                    armatureUnityInstance.Dispose(false);
                }

                EditorUtility.SetDirty(armatureUnityInstance);

                return true;
            }

            return false;
        }

        public static void ChangeArmatureData(ArmatureUnityInstance armatureUnityInstance, string armatureName, string dragonBonesName)
        {
            bool isUGUI = armatureUnityInstance.isUGUI;
            UnityDragonBonesData unityData = null;
            Slot slot = null;
            if (armatureUnityInstance.armature != null)
            {
                unityData = armatureUnityInstance.unityData;
                slot = armatureUnityInstance.armature.parent;
                armatureUnityInstance.Dispose(false);

                DBUnityFactory.factory._dragonBones.AdvanceTime(0.0f);

                armatureUnityInstance.unityData = unityData;
            }

            armatureUnityInstance.armatureName = armatureName;
            armatureUnityInstance.isUGUI = isUGUI;

            armatureUnityInstance = DBUnityFactory.factory.BuildArmatureComponent(armatureUnityInstance.armatureName, dragonBonesName, null, armatureUnityInstance.unityData.dataName, armatureUnityInstance.gameObject, armatureUnityInstance.isUGUI);
            if (slot != null)
            {
                slot.childArmature = armatureUnityInstance.armature;
            }
        }

        public static UnityEngine.Transform GetSelectionParentTransform()
        {
            var parent = Selection.activeObject as GameObject;
            return parent != null ? parent.transform : null;
        }

        public static void GetTextureAtlasConfigs(List<string> textureAtlasFiles, string filePathRelatedToProjectFolder, string rawName = null, string suffix = "tex")
        {
            var folder = Directory.GetParent(filePathRelatedToProjectFolder).ToString();
            string folderRelatedToProjectFolder = Path.GetDirectoryName(filePathRelatedToProjectFolder);

            var name = rawName != null ? rawName : filePathRelatedToProjectFolder.Substring(0, filePathRelatedToProjectFolder.LastIndexOf(".")).Substring(filePathRelatedToProjectFolder.LastIndexOf("/") + 1);
            if (name.LastIndexOf("_ske") == name.Length - 4)
            {
                name = name.Substring(0, name.LastIndexOf("_ske"));
            }
            int index = 0;
            var textureAtlasName = "";
            var textureAtlasConfigFile = "";

            textureAtlasName = !string.IsNullOrEmpty(name) ? name + (!string.IsNullOrEmpty(suffix) ? "_" + suffix : suffix) : suffix;
            textureAtlasConfigFile = folder + "/" + textureAtlasName + ".json";

            if (File.Exists(textureAtlasConfigFile))
            {
                AddToTextureAtlasFiles(textureAtlasConfigFile);
                return;
            }

            while (true)
            {
                textureAtlasName = (!string.IsNullOrEmpty(name) ? name + (!string.IsNullOrEmpty(suffix) ? "_" + suffix : suffix) : suffix) + "_" + (index++);
                textureAtlasConfigFile = folder + "/" + textureAtlasName + ".json";
                if (File.Exists(textureAtlasConfigFile))
                {
                    AddToTextureAtlasFiles(textureAtlasConfigFile);
                }
                else if (index > 1)
                {
                    break;
                }
            }

            if (textureAtlasFiles.Count > 0 || rawName != null)
            {
                return;
            }

            GetTextureAtlasConfigs(textureAtlasFiles, filePathRelatedToProjectFolder, "", suffix);
            if (textureAtlasFiles.Count > 0)
            {
                return;
            }

            index = name.LastIndexOf("_");
            if (index >= 0)
            {
                name = name.Substring(0, index);

                GetTextureAtlasConfigs(textureAtlasFiles, filePathRelatedToProjectFolder, name, suffix);
                if (textureAtlasFiles.Count > 0)
                {
                    return;
                }

                GetTextureAtlasConfigs(textureAtlasFiles, filePathRelatedToProjectFolder, name, "");
                if (textureAtlasFiles.Count > 0)
                {
                    return;
                }
            }

            if (suffix != "texture")
            {
                GetTextureAtlasConfigs(textureAtlasFiles, filePathRelatedToProjectFolder, null, "texture");
            }

            void AddToTextureAtlasFiles(string s)
            {
                textureAtlasFiles.Add(Path.Combine(folderRelatedToProjectFolder, Path.GetFileName(s)));
            }
        }

        public static UnityDragonBonesData CreateUnityDragonBonesData(TextAsset dragonBonesAsset, UnityDragonBonesData.TextureAtlas[] textureAtlas)
        {
            if (dragonBonesAsset != null)
            {
                bool isDirty = false;
                string path = AssetDatabase.GetAssetPath(dragonBonesAsset);
                path = path.Substring(0, path.Length - 5);
                int index = path.LastIndexOf("_ske");
                if (index > 0)
                {
                    path = path.Substring(0, index);
                }
                //
                string dataPath = path + "_Data.asset";

                var jsonObject = (Dictionary<string, object>)MiniJSON.Json.Deserialize(dragonBonesAsset.text);
                if (dragonBonesAsset.text == "DBDT")
                {
                    int headerLength  = 0;
                    jsonObject = BinaryDataParser.DeserializeBinaryJsonData(dragonBonesAsset.bytes, out headerLength);
                }
                else
                {
                    jsonObject = MiniJSON.Json.Deserialize(dragonBonesAsset.text) as Dictionary<string, object>;
                }

                var dataName = jsonObject.ContainsKey("name") ? jsonObject["name"] as string : "";

                //先从缓存里面取
                UnityDragonBonesData data = DBUnityFactory.factory.GetCacheUnityDragonBonesData(dataName);

                //缓存中没有，从资源里面取
                if (data == null)
                {
                    data = AssetDatabase.LoadAssetAtPath<UnityDragonBonesData>(dataPath);
                }

                //资源里面也没有，那么重新创建
                if (data == null)
                {
                    data = UnityDragonBonesData.CreateInstance<UnityDragonBonesData>();
                    data.dataName = dataName;
                    AssetDatabase.CreateAsset(data, dataPath);
                    isDirty = true;
                }

                //
                if (string.IsNullOrEmpty(data.dataName) || !data.dataName.Equals(dataName))
                {
                    //走到这里，说明原先已经创建了，之后手动改了名字,既然又走了创建流程，那么名字也重置下
                    data.dataName = dataName;
                    isDirty = true;
                }

                if (data.dragonBonesJSON != dragonBonesAsset)
                {
                    data.dragonBonesJSON = dragonBonesAsset;
                    isDirty = true;
                }

                if (textureAtlas != null && textureAtlas.Length > 0 && textureAtlas[0] != null && textureAtlas[0].texture != null)
                {
                    if (data.textureAtlas == null || data.textureAtlas.Length != textureAtlas.Length)
                    {
                        isDirty = true;
                    }
                    else
                    {
                        for (int i = 0; i < textureAtlas.Length; ++i)
                        {
                            if (textureAtlas[i].material != data.textureAtlas[i].material ||
                                textureAtlas[i].uiMaterial != data.textureAtlas[i].uiMaterial ||
                                textureAtlas[i].texture != data.textureAtlas[i].texture ||
                                textureAtlas[i].textureAtlasJSON != data.textureAtlas[i].textureAtlasJSON
                            )
                            {
                                isDirty = true;
                                break;
                            }
                        }
                    }
                    data.textureAtlas = textureAtlas;
                }

                if (isDirty)
                {
                    AssetDatabase.Refresh();
                    EditorUtility.SetDirty(data);
                }

                //
                DBUnityFactory.factory.AddCacheUnityDragonBonesData(data);

                AssetDatabase.SaveAssets();
                return data;
            }
            return null;
        }


        private static List<string> _GetDragonBonesSkePaths(bool isCreateUnityData = false)
        {
            var dragonBonesSkePaths = new List<string>();
            foreach (var guid in Selection.assetGUIDs)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (assetPath.EndsWith(".json"))
                {
                    var jsonCode = File.ReadAllText(assetPath);
                    if (jsonCode.IndexOf("\"armature\":") > 0)
                    {
                        dragonBonesSkePaths.Add(assetPath);
                    }
                }
                if (assetPath.EndsWith(".bytes"))
                {
                    TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
                    if (asset && asset.text == "DBDT")
                    {
                        dragonBonesSkePaths.Add(assetPath);
                    }
                }
                else if (!isCreateUnityData && assetPath.EndsWith("_Data.asset"))
                {
                    UnityDragonBonesData data = AssetDatabase.LoadAssetAtPath<UnityDragonBonesData>(assetPath);

                    dragonBonesSkePaths.Add(AssetDatabase.GetAssetPath(data.dragonBonesJSON));
                }
            }

            return dragonBonesSkePaths;
        }


        private static ArmatureUnityInstance _CreateEmptyObject(UnityEngine.Transform parentTransform)
        {
            var gameObject = new GameObject("New Armature Object", typeof(ArmatureUnityInstance));
            var armatureComponent = gameObject.GetComponent<ArmatureUnityInstance>();
            gameObject.transform.SetParent(parentTransform, false);

            //
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = gameObject;
            EditorGUIUtility.PingObject(Selection.activeObject);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Armature Object");
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());

            return armatureComponent;
        }

    }
}