using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Reflection;
using UnityEditor.SceneManagement;

namespace DragonBones
{
    [CustomEditor(typeof(ArmatureUnityInstance))]
    public class UnityArmatureEditor : UnityEditor.Editor
    {
        private long _nowTime = 0;
        private float _frameRate = 1.0f / 24.0f;

        private int _armatureIndex = -1;
        private int _animationIndex = -1;
        private int _sortingModeIndex = -1;
        private int _sortingLayerIndex = -1;

        private List<string> _armatureNames = null;
        private List<string> _animationNames = null;
        private List<string> _sortingLayerNames = null;

        private ArmatureUnityInstance armatureUnityInstance = null;

        private SerializedProperty _playTimesPro;
        private SerializedProperty _timeScalePro;
        private SerializedProperty _flipXPro;
        private SerializedProperty _flipYPro;
        private SerializedProperty _closeCombineMeshsPro;

        void ClearUp()
        {
            this._armatureIndex = -1;
            this._animationIndex = -1;
            // this._sortingModeIndex = -1;
            // this._sortingLayerIndex = -1;

            this._armatureNames = null;
            this._animationNames = null;
            // this._sortingLayerNames = null;
        }

        void OnDisable()
        {
        }

        void OnEnable()
        {
            this.armatureUnityInstance = target as ArmatureUnityInstance;
            if (_IsPrefab())
            {
                return;
            }

            // 
            this._nowTime = System.DateTime.Now.Ticks;


            this._playTimesPro = serializedObject.FindProperty("_playTimes");
            this._timeScalePro = serializedObject.FindProperty("_timeScale");
            this._flipXPro = serializedObject.FindProperty("_flipX");
            this._flipYPro = serializedObject.FindProperty("_flipY");
            this._closeCombineMeshsPro = serializedObject.FindProperty("_closeCombineMeshs");

            // Update armature.
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                armatureUnityInstance.armature == null &&
                armatureUnityInstance.unityData != null &&
                !string.IsNullOrEmpty(armatureUnityInstance.armatureName))
            {
                // Clear cache
                DBUnityFactory.factory.Clear(true);

                // Unload
                EditorUtility.UnloadUnusedAssetsImmediate();
                System.GC.Collect();

                // Load data.
                var dragonBonesData = DBUnityFactory.factory.LoadData(armatureUnityInstance.unityData);

                // Refresh texture atlas.
                DBUnityFactory.factory.RefreshAllTextureAtlas(armatureUnityInstance);

                // Refresh armature.
                DBUnityEditor.ChangeArmatureData(armatureUnityInstance, armatureUnityInstance.armatureName, dragonBonesData.name);

                // Refresh texture.
                armatureUnityInstance.armature.InvalidUpdate(null, true);

                // Play animation.
                if (!string.IsNullOrEmpty(armatureUnityInstance.animationName))
                {
                    armatureUnityInstance.animation.Play(armatureUnityInstance.animationName, _playTimesPro.intValue);
                }
            }

            // Update hideFlags.
            if (!EditorApplication.isPlayingOrWillChangePlaymode &&
                armatureUnityInstance.armature != null &&
                armatureUnityInstance.armature.parent != null)
            {
                armatureUnityInstance.gameObject.hideFlags = HideFlags.NotEditable;
            }
            else
            {
                armatureUnityInstance.gameObject.hideFlags = HideFlags.None;
            }

            _UpdateParameters();
        }

        public override void OnInspectorGUI()
        {
            if (_IsPrefab())
            {
                return;
            }

            serializedObject.Update();

            if (_armatureIndex == -1)
            {
                _UpdateParameters();
            }

            // DragonBones Data
            EditorGUILayout.BeginHorizontal();

            armatureUnityInstance.unityData = EditorGUILayout.ObjectField("DragonBones Data", armatureUnityInstance.unityData, typeof(UnityDragonBonesData), false) as UnityDragonBonesData;

            var created = false;
            if (armatureUnityInstance.unityData != null)
            {
                if (armatureUnityInstance.armature == null)
                {
                    if (GUILayout.Button("Create"))
                    {
                        created = true;
                    }
                }
                else
                {
                    if (GUILayout.Button("Reload"))
                    {
                        if (EditorUtility.DisplayDialog("DragonBones Alert", "Are you sure you want to reload data", "Yes", "No"))
                        {
                            created = true;
                        }
                    }
                }
            }
            else
            {
                //create UnityDragonBonesData by a json data
                if (GUILayout.Button("JSON"))
                {
                    PickJsonDataWindow.OpenWindow(armatureUnityInstance);
                }
            }

            if (created)
            {
                //clear cache
                DBUnityFactory.factory.Clear(true);
                ClearUp();
                armatureUnityInstance.animationName = null;

                if (DBUnityEditor.ChangeDragonBonesData(armatureUnityInstance, armatureUnityInstance.unityData.dragonBonesJSON))
                {
                    _UpdateParameters();
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();

            if (armatureUnityInstance.armature != null)
            {
                var dragonBonesData = armatureUnityInstance.armature.armatureData.parent;

                // Armature
                if (DBUnityFactory.factory.GetAllDragonBonesData().ContainsValue(dragonBonesData) && _armatureNames != null)
                {
                    var armatureIndex = EditorGUILayout.Popup("Armature", _armatureIndex, _armatureNames.ToArray());
                    if (_armatureIndex != armatureIndex)
                    {
                        _armatureIndex = armatureIndex;

                        var armatureName = _armatureNames[_armatureIndex];
                        DBUnityEditor.ChangeArmatureData(armatureUnityInstance, armatureName, dragonBonesData.name);
                        _UpdateParameters();

                        armatureUnityInstance.gameObject.name = armatureName;

                        MarkSceneDirty();
                    }
                }

                // Animation
                if (_animationNames != null && _animationNames.Count > 0)
                {
                    EditorGUILayout.BeginHorizontal();
                    List<string> anims = new List<string>(_animationNames);
                    anims.Insert(0, "<None>");
                    var animationIndex = EditorGUILayout.Popup("Animation", _animationIndex + 1, anims.ToArray()) - 1;
                    if (animationIndex != _animationIndex)
                    {
                        _animationIndex = animationIndex;
                        if (animationIndex >= 0)
                        {
                            armatureUnityInstance.animationName = _animationNames[animationIndex];
                            var animationData = armatureUnityInstance.animation.animations[armatureUnityInstance.animationName];
                            armatureUnityInstance.animation.Play(armatureUnityInstance.animationName, _playTimesPro.intValue);
                            _UpdateParameters();
                        }
                        else
                        {
                            armatureUnityInstance.animationName = null;
                            _playTimesPro.intValue = 0;
                            armatureUnityInstance.animation.Stop();
                        }

                        MarkSceneDirty();
                    }

                    if (_animationIndex >= 0)
                    {
                        if (armatureUnityInstance.animation.isPlaying)
                        {
                            if (GUILayout.Button("Stop"))
                            {
                                armatureUnityInstance.animation.Stop();
                            }
                        }
                        else
                        {
                            if (GUILayout.Button("Play"))
                            {
                                armatureUnityInstance.animation.Play(null, _playTimesPro.intValue);
                            }
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    //playTimes
                    EditorGUILayout.BeginHorizontal();
                    var playTimes = _playTimesPro.intValue;
                    EditorGUILayout.PropertyField(_playTimesPro, false);
                    if (playTimes != _playTimesPro.intValue)
                    {
                        if (!string.IsNullOrEmpty(armatureUnityInstance.animationName))
                        {
                            armatureUnityInstance.animation.Reset();
                            armatureUnityInstance.animation.Play(armatureUnityInstance.animationName, _playTimesPro.intValue);
                        }
                    }
                    EditorGUILayout.EndHorizontal();

                    // TimeScale
                    var timeScale = _timeScalePro.floatValue;
                    EditorGUILayout.PropertyField(_timeScalePro, false);
                    if (timeScale != _timeScalePro.floatValue)
                    {
                        armatureUnityInstance.animation.timeScale = _timeScalePro.floatValue;
                    }
                }

                //
                EditorGUILayout.Space();

                if (!armatureUnityInstance.isUGUI)
                {
                   
                }

                EditorGUILayout.Space();

                // Flip
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PrefixLabel("Flip");
                var flipX = _flipXPro.boolValue;
                var flipY = _flipYPro.boolValue;
                _flipXPro.boolValue = GUILayout.Toggle(_flipXPro.boolValue, "X", GUILayout.Width(30));
                _flipYPro.boolValue = GUILayout.Toggle(_flipYPro.boolValue, "Y", GUILayout.Width(30));
                if (flipX != _flipXPro.boolValue || flipY != _flipYPro.boolValue)
                {
                    armatureUnityInstance.armature.flipX = _flipXPro.boolValue;
                    armatureUnityInstance.armature.flipY = _flipYPro.boolValue;

                    MarkSceneDirty();
                }

                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            if (armatureUnityInstance.armature != null && armatureUnityInstance.armature.parent == null)
            {
                if (!Application.isPlaying && !this.armatureUnityInstance.isUGUI)
                {
                    //
                    var oldValue = this._closeCombineMeshsPro.boolValue;
                    if (!this._closeCombineMeshsPro.boolValue)
                    {
                        this._closeCombineMeshsPro.boolValue = EditorGUILayout.Toggle("CloseCombineMeshs", this._closeCombineMeshsPro.boolValue);

                        if (GUILayout.Button("Show Slots"))
                        {
                            ShowSlotsWindow.OpenWindow(this.armatureUnityInstance);
                        }
                    }

                    if(oldValue != this._closeCombineMeshsPro.boolValue)
                    {
                        if(this._closeCombineMeshsPro.boolValue)
                        {
                            this.armatureUnityInstance.CloseCombineMeshs();
                        }
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();

            if (!EditorApplication.isPlayingOrWillChangePlaymode && GUI.changed && Selection.activeGameObject == armatureUnityInstance.gameObject)
            {
                EditorUtility.SetDirty(armatureUnityInstance);
                HandleUtility.Repaint();
            }
        }

        void OnSceneGUI()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode && armatureUnityInstance.armature != null)
            {
                var dt = (System.DateTime.Now.Ticks - _nowTime) * 0.0000001f;
                if (dt >= _frameRate)
                {
                    armatureUnityInstance.armature.AdvanceTime(dt);

                    foreach (var slot in armatureUnityInstance.armature.GetSlots())
                    {
                        if (slot.childArmature != null)
                        {
                            slot.childArmature.AdvanceTime(dt);
                        }
                    }

                    //
                    _nowTime = System.DateTime.Now.Ticks;
                }
            }
        }

        private void _UpdateParameters()
        {
            if (armatureUnityInstance.armature != null)
            {
                _frameRate = 1.0f / (float)armatureUnityInstance.armature.armatureData.frameRate;

                if (armatureUnityInstance.armature.armatureData.parent != null)
                {
                    _armatureNames = armatureUnityInstance.armature.armatureData.parent.armatureNames;
                    _animationNames = armatureUnityInstance.animation.animationNames;
                    _armatureIndex = _armatureNames.IndexOf(armatureUnityInstance.armature.name);
                    //
                    if (!string.IsNullOrEmpty(armatureUnityInstance.animationName))
                    {
                        _animationIndex = _animationNames.IndexOf(armatureUnityInstance.animationName);
                    }
                }
                else
                {
                    _armatureNames = null;
                    _animationNames = null;
                    _armatureIndex = -1;
                    _animationIndex = -1;
                }
            }
            else
            {
                _armatureNames = null;
                _animationNames = null;
                _armatureIndex = -1;
                _animationIndex = -1;
            }
        }

        private bool _IsPrefab()
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(armatureUnityInstance.gameObject) == null
                && PrefabUtility.GetPrefabInstanceHandle(armatureUnityInstance.gameObject) != null;
        }

        private List<string> _GetSortingLayerNames()
        {
            var internalEditorUtilityType = typeof(InternalEditorUtility);
            var sortingLayersProperty = internalEditorUtilityType.GetProperty("sortingLayerNames", BindingFlags.Static | BindingFlags.NonPublic);

            return new List<string>(sortingLayersProperty.GetValue(null, new object[0]) as string[]);
        }

        private void MarkSceneDirty()
        {
            EditorUtility.SetDirty(armatureUnityInstance);
            //
            if (!Application.isPlaying && !_IsPrefab())
            {
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
        }
    }
}