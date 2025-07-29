using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace DragonBones
{
    ///<inheritDoc/>
    [ExecuteInEditMode, DisallowMultipleComponent]
    [RequireComponent(typeof(SortingGroup))]
    public class ArmatureUnityInstance : DragonBoneEventDispatcher, IArmatureProxy
    {
        /// <private/>
        public UnityDragonBonesData unityData = null;

        public string armatureName = null;

        public bool isUGUI = false;

        public bool debugDraw = false;
        internal readonly ColorTransform _colorTransform = new ColorTransform();

        public string animationName = null;

        private bool _disposeProxy = true;

        internal Armature _armature = null;

        [Tooltip("0 : Loop")] [Range(0, 100)] [SerializeField]
        protected int _playTimes = 0;

        [Range(-2f, 2f)] [SerializeField] protected float _timeScale = 1.0f;

        [SerializeField] protected bool _flipX = false;

        [SerializeField] protected bool _flipY = false;

        //default open combineMeshs
        [SerializeField] protected bool _closeCombineMeshs;

        private bool _hasSortingGroup = false;
        private Material _debugDrawer;

        internal int _armatureZ;

        public void DBClear()
        {
            if (this._armature != null)
            {
                this._armature = null;
                if (this._disposeProxy)
                {
                    UnityFactoryHelper.DestroyUnityObject(gameObject);
                }
            }

            this.unityData = null;
            this.armatureName = null;
            this.animationName = null;
            this.isUGUI = false;
            this.debugDraw = false;

            this._disposeProxy = true;
            this._armature = null;
            this._colorTransform.Identity();
            this._playTimes = 0;
            this._timeScale = 1.0f;
            this._flipX = false;
            this._flipY = false;

            this._hasSortingGroup = false;

            this._debugDrawer = null;

            this._armatureZ = 0;
            this._closeCombineMeshs = false;
        }

        ///
        public void DBInit(Armature armature)
        {
            this._armature = armature;
        }

        public void DBUpdate()
        {

        }

        void CreateLineMaterial()
        {
            if (!_debugDrawer)
            {
                // Unity has a built-in shader that is useful for drawing
                // simple colored things.
                Shader shader = Shader.Find("Hidden/Internal-Colored");
                _debugDrawer = new Material(shader);
                _debugDrawer.hideFlags = HideFlags.HideAndDontSave;
                // Turn on alpha blending
                _debugDrawer.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                _debugDrawer.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                // Turn backface culling off
                _debugDrawer.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                // Turn off depth writes
                _debugDrawer.SetInt("_ZWrite", 0);
            }
        }

        void OnRenderObject()
        {
            var isDrew = DragonBones.debugDraw || this.debugDraw;
            if (isDrew)
            {
                Color boneLineColor = new Color(0.0f, 1.0f, 1.0f, 0.7f);
                Color boundingBoxLineColor = new Color(1.0f, 0.0f, 1.0f, 1.0f);
                CreateLineMaterial();
                // Apply the line material
                _debugDrawer.SetPass(0);

                GL.PushMatrix();
                // Set transformation matrix for drawing to
                // match our transform
                GL.MultMatrix(transform.localToWorldMatrix);
                //
                var bones = this._armature.GetBones();
                var offset = 0.02f;
                // draw bone line
                for (int i = 0; i < bones.Count; i++)
                {
                    Bone bone = bones[i];
                    float boneLength = System.Math.Max(bone.boneData.length, offset);

                    Vector3 startPos = new Vector3(bone.globalTransformMatrix.tx, bone.globalTransformMatrix.ty, 0.0f);
                    Vector3 endPos = new Vector3(bone.globalTransformMatrix.a * boneLength,
                        bone.globalTransformMatrix.b * boneLength, 0.0f) + startPos;

                    Vector3 torwardDir = (startPos - endPos).normalized;
                    Vector3 leftStartPos = Quaternion.AngleAxis(90, Vector3.forward) * torwardDir * offset + startPos;
                    Vector3 rightStartPos = Quaternion.AngleAxis(-90, Vector3.forward) * torwardDir * offset + startPos;
                    Vector3 newStartPos = startPos + torwardDir * offset;
                    //
                    GL.Begin(GL.LINES);
                    GL.Color(boneLineColor);
                    GL.Vertex(leftStartPos);
                    GL.Vertex(rightStartPos);
                    GL.End();
                    GL.Begin(GL.LINES);
                    GL.Color(boneLineColor);
                    GL.Vertex(newStartPos);
                    GL.Vertex(endPos);
                    GL.End();
                }

                // draw boundingBox
                Point result = new Point();
                var slots = this._armature.GetSlots();
                for (int i = 0; i < slots.Count; i++)
                {
                    var slot = slots[i] as UnitySlot;
                    var boundingBoxData = slot.boundingBoxData;

                    if (boundingBoxData == null)
                    {
                        continue;
                    }

                    var bone = slot.parent;

                    slot.UpdateTransformAndMatrix();
                    slot.UpdateGlobalTransform();

                    var tx = slot.globalTransformMatrix.tx;
                    var ty = slot.globalTransformMatrix.ty;
                    var boundingBoxWidth = boundingBoxData.width;
                    var boundingBoxHeight = boundingBoxData.height;
                    //
                    switch (boundingBoxData.type)
                    {
                        case BoundingBoxType.Rectangle:
                        {
                            //
#if UNITY_5_6_OR_NEWER
                            GL.Begin(GL.LINE_STRIP);
#else
                                GL.Begin(GL.LINES);
#endif
                            GL.Color(boundingBoxLineColor);

                            var leftTopPos = new Vector3(tx - boundingBoxWidth * 0.5f, ty + boundingBoxHeight * 0.5f,
                                0.0f);
                            var leftBottomPos = new Vector3(tx - boundingBoxWidth * 0.5f, ty - boundingBoxHeight * 0.5f,
                                0.0f);
                            var rightTopPos = new Vector3(tx + boundingBoxWidth * 0.5f, ty + boundingBoxHeight * 0.5f,
                                0.0f);
                            var rightBottomPos = new Vector3(tx + boundingBoxWidth * 0.5f,
                                ty - boundingBoxHeight * 0.5f, 0.0f);

                            GL.Vertex(leftTopPos);
                            GL.Vertex(rightTopPos);
                            GL.Vertex(rightBottomPos);
                            GL.Vertex(leftBottomPos);
                            GL.Vertex(leftTopPos);

                            GL.End();
                        }
                            break;
                        case BoundingBoxType.Ellipse:
                        {

                        }
                            break;
                        case BoundingBoxType.Polygon:
                        {
                            var vertices = (boundingBoxData as PolygonBoundingBoxData).vertices;
#if UNITY_5_6_OR_NEWER
                            GL.Begin(GL.LINE_STRIP);
#else
                                GL.Begin(GL.LINES);
#endif
                            GL.Color(boundingBoxLineColor);
                            for (var j = 0; j < vertices.Count; j += 2)
                            {
                                slot.globalTransformMatrix.TransformPoint(vertices[j], vertices[j + 1], result);
                                GL.Vertex3(result.x, result.y, 0.0f);
                            }

                            slot.globalTransformMatrix.TransformPoint(vertices[0], vertices[1], result);
                            GL.Vertex3(result.x, result.y, 0.0f);
                            GL.End();
                        }
                            break;
                        default:
                            break;
                    }
                }

                GL.PopMatrix();
            }

        }

        /// <inheritDoc/>
        public void Dispose(bool disposeProxy = true)
        {
            _disposeProxy = disposeProxy;

            if (_armature != null)
            {
                _armature.Dispose();
            }
        }

        /// <summary>
        /// Get the Armature.
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// 获取骨架。
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public Armature armature
        {
            get { return _armature; }
        }

        /// <summary>
        /// Get the animation player
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>

        /// <summary>
        /// 获取动画播放器。
        /// </summary>
        /// <readOnly/>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public new Animation animation
        {
            get { return _armature != null ? _armature.animation : null; }
        }

        /// <summary>
        /// - The armature color.
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>en_US</language>
        /// 
        /// <summary>
        /// - 骨架的颜色。
        /// </summary>
        /// <version>DragonBones 4.5</version>
        /// <language>zh_CN</language>
        public ColorTransform color
        {
            get { return this._colorTransform; }
            set
            {
                this._colorTransform.CopyFrom(value);

                foreach (var slot in this._armature.GetSlots())
                {
                    slot._colorDirty = true;
                }
            }
        }

#if UNITY_EDITOR
        private bool IsPrefab()
        {
            return PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null
                   && PrefabUtility.GetPrefabInstanceHandle(gameObject) != null;
        }
#endif

        /// <private/>
        void Awake()
        {
#if UNITY_EDITOR
            if (IsPrefab())
            {
                return;
            }
#endif
            if (IsDataSetupCorrectly())
            {
                var dragonBonesData = DBUnityFactory.factory.LoadData(unityData, isUGUI);

                if (dragonBonesData != null && !string.IsNullOrEmpty(armatureName))
                {
                    DBUnityFactory.factory.BuildArmatureComponent(armatureName, unityData.dataName, null, null,
                        gameObject, isUGUI);
                }
            }

            if (_armature != null)
            {
                _armature.flipX = _flipX;
                _armature.flipY = _flipY;

                _armature.animation.timeScale = _timeScale;

                if (!string.IsNullOrEmpty(animationName))
                {
                    _armature.animation.Play(animationName, _playTimes);
                }
            }
        }

        private bool IsDataSetupCorrectly()
        {
            return unityData != null && unityData.dragonBonesJSON != null && unityData.textureAtlas != null;
        }

        void Start()
        {
            // this._closeCombineMeshs = true;
            //默认开启合并
            if (this._closeCombineMeshs)
            {
                this.CloseCombineMeshs();
            }
            else
            {
                this.OpenCombineMeshs();
            }
        }

        void LateUpdate()
        {
            if (_armature == null)
            {
                return;
            }

            _flipX = _armature.flipX;
            _flipY = _armature.flipY;

#if UNITY_5_6_OR_NEWER
            var hasSortingGroup = GetComponent<UnityEngine.Rendering.SortingGroup>() != null;
            if (hasSortingGroup != _hasSortingGroup)
            {
                _hasSortingGroup = hasSortingGroup;

            }
#endif
        }

        /// <private/>
        void OnDestroy()
        {
            if (_armature != null)
            {
                var armature = _armature;
                _armature = null;

                armature.Dispose();

                if (!Application.isPlaying)
                {
                    DBUnityFactory.factory._dragonBones.AdvanceTime(0.0f);
                }
            }

            _disposeProxy = true;
            _armature = null;
        }

        private void OpenCombineMeshs()
        {
            if (this.isUGUI)
            {
                return;
            }

            //
            var cm = gameObject.GetComponent<UnityCombineMeshes>();
            if (cm == null)
            {
                cm = gameObject.AddComponent<UnityCombineMeshes>();
            }
            //

            if (this._armature == null)
            {
                return;
            }

            var slots = this._armature.GetSlots();
            foreach (var slot in slots)
            {
                if (slot.childArmature != null)
                {
                    (slot.childArmature.proxy as ArmatureUnityInstance).OpenCombineMeshs();
                }
            }
        }

        public void CloseCombineMeshs()
        {
            this._closeCombineMeshs = true;
            //
            var cm = gameObject.GetComponent<UnityCombineMeshes>();
            if (cm != null)
            {
                DestroyImmediate(cm);
            }

            if (this._armature == null)
            {
                return;
            }

            //
            var slots = this._armature.GetSlots();
            foreach (var slot in slots)
            {
                if (slot.childArmature != null)
                {
                    (slot.childArmature.proxy as ArmatureUnityInstance).CloseCombineMeshs();
                }
            }
        }
    }
}