using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace VfxSandbox
{
    [ExecuteInEditMode]
    public class PlanarReflections : MonoBehaviour
    {
        [Header("Reflection Settings")]
        [Range(128, 2048)]
        public int textureSize = 512;
        public float clipPlaneOffset = 0.02f;
        public LayerMask reflectionMask = -1;

        private Dictionary<Camera, Camera> m_ReflectionCameras = new Dictionary<Camera, Camera>();
        private RenderTexture m_ReflectionTexture;
        private int m_OldTextureSize;
        private static bool s_IsRendering = false;

        private void OnEnable()
        {
            RenderPipelineManager.beginCameraRendering += ExecutePlanarReflections;
        }

        private void OnDisable()
        {
            Cleanup();
        }

        private void OnDestroy()
        {
            Cleanup();
        }

        private void Cleanup()
        {
            RenderPipelineManager.beginCameraRendering -= ExecutePlanarReflections;

            if (m_ReflectionTexture != null)
            {
                DestroyImmediate(m_ReflectionTexture);
                m_ReflectionTexture = null;
            }

            foreach (var kvp in m_ReflectionCameras)
            {
                if (kvp.Value != null)
                {
                    DestroyImmediate(kvp.Value.gameObject);
                }
            }
            m_ReflectionCameras.Clear();
        }

        private void ExecutePlanarReflections(ScriptableRenderContext context, Camera camera)
        {
            if (camera == null) return;
            
            // Bỏ qua nếu camera là camera phản chiếu chính nó để tránh đệ quy vô hạn
            if (camera.cameraType == CameraType.Reflection || camera.name.Contains("PlanarRef")) return;
            if (s_IsRendering) return;

            s_IsRendering = true;

            try
            {
                CreateReflectionResources(camera);

                Camera reflectionCamera = GetReflectionCamera(camera);
                UpdateCameraModes(camera, reflectionCamera);

                // Tính toán mặt phẳng gương thế giới (World Space Plane)
                Vector3 pos = transform.position;
                Vector3 normal = transform.up; // Nước phẳng nằm ngang

                float d = -Vector3.Dot(normal, pos) - clipPlaneOffset;
                Vector4 reflectionPlane = new Vector4(normal.x, normal.y, normal.z, d);

                // Dựng ma trận phản chiếu
                Matrix4x4 reflectionMatrix = Matrix4x4.zero;
                CalculateReflectionMatrix(ref reflectionMatrix, reflectionPlane);

                Vector3 oldPos = camera.transform.position;
                Vector3 newPos = reflectionMatrix.MultiplyPoint(oldPos);
                reflectionCamera.worldToCameraMatrix = camera.worldToCameraMatrix * reflectionMatrix;

                // Tính toán clipping mặt phẳng nghiêng (Oblique Projection Matrix) để cắt phần dưới mặt nước
                Vector4 clipPlane = CameraSpacePlane(reflectionCamera, pos, normal, 1.0f);
                reflectionCamera.projectionMatrix = camera.CalculateObliqueMatrix(clipPlane);

                reflectionCamera.transform.position = newPos;
                Vector3 lookAt = reflectionMatrix.MultiplyVector(camera.transform.forward);
                Vector3 up = reflectionMatrix.MultiplyVector(camera.transform.up);
                reflectionCamera.transform.rotation = Quaternion.LookRotation(lookAt, up);

                // Thực hiện render camera phản chiếu vào texture
                GL.invertCulling = true;
                UniversalRenderPipeline.RenderSingleCamera(context, reflectionCamera);
                GL.invertCulling = false;

                // Truyền kết cấu phản chiếu vào shader nước
                var renderer = GetComponent<Renderer>();
                if (renderer != null && renderer.sharedMaterial != null)
                {
                    renderer.sharedMaterial.SetTexture("_PlanarReflectionTexture", m_ReflectionTexture);
                }
            }
            finally
            {
                s_IsRendering = false;
            }
        }

        private void CreateReflectionResources(Camera currentCamera)
        {
            if (m_ReflectionTexture == null || m_OldTextureSize != textureSize)
            {
                if (m_ReflectionTexture != null)
                {
                    DestroyImmediate(m_ReflectionTexture);
                }

                m_ReflectionTexture = new RenderTexture(textureSize, textureSize, 16, RenderTextureFormat.ARGB32);
                m_ReflectionTexture.name = "_PlanarReflection" + GetHashCode();
                m_ReflectionTexture.isPowerOfTwo = true;
                m_ReflectionTexture.hideFlags = HideFlags.DontSave;
                m_OldTextureSize = textureSize;
            }
        }

        private Camera GetReflectionCamera(Camera currentCamera)
        {
            Camera reflectionCamera;
            if (!m_ReflectionCameras.TryGetValue(currentCamera, out reflectionCamera) || reflectionCamera == null)
            {
                GameObject go = new GameObject("PlanarRefCamera_" + currentCamera.name, typeof(Camera), typeof(UniversalAdditionalCameraData));
                go.hideFlags = HideFlags.HideAndDontSave;
                
                reflectionCamera = go.GetComponent<Camera>();
                reflectionCamera.enabled = false;
                reflectionCamera.transform.position = transform.position;
                reflectionCamera.transform.rotation = transform.rotation;
                
                // Cấu hình URP data cho camera phản chiếu
                var additionalData = go.GetComponent<UniversalAdditionalCameraData>();
                if (additionalData != null)
                {
                    additionalData.renderShadows = false; // Tắt đổ bóng camera phản chiếu để nhẹ máy
                    additionalData.requiresColorOption = CameraOverrideOption.Off;
                    additionalData.requiresDepthOption = CameraOverrideOption.Off;
                }

                m_ReflectionCameras[currentCamera] = reflectionCamera;
            }
            return reflectionCamera;
        }

        private void UpdateCameraModes(Camera src, Camera dest)
        {
            if (dest == null) return;

            dest.clearFlags = src.clearFlags;
            dest.backgroundColor = src.backgroundColor;
            dest.farClipPlane = src.farClipPlane;
            dest.nearClipPlane = src.nearClipPlane;
            dest.orthographic = src.orthographic;
            dest.fieldOfView = src.fieldOfView;
            dest.aspect = src.aspect;
            dest.orthographicSize = src.orthographicSize;
            
            // Chỉ phản chiếu các vật thể nằm trên mặt nước (loại trừ mặt nước chính nó để tránh đệ quy)
            dest.cullingMask = reflectionMask & ~(1 << gameObject.layer);
            dest.targetTexture = m_ReflectionTexture;
        }

        private Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
        {
            Matrix4x4 m = cam.worldToCameraMatrix;
            Vector3 cPos = m.MultiplyPoint(pos);
            Vector3 cNormal = m.MultiplyVector(normal).normalized * sideSign;
            return new Vector4(cNormal.x, cNormal.y, cNormal.z, -Vector3.Dot(cPos, cNormal));
        }

        private void CalculateReflectionMatrix(ref Matrix4x4 reflectionMatrix, Vector4 plane)
        {
            reflectionMatrix.m00 = (1F - 2F * plane[0] * plane[0]);
            reflectionMatrix.m01 = (-2F * plane[0] * plane[1]);
            reflectionMatrix.m02 = (-2F * plane[0] * plane[2]);
            reflectionMatrix.m03 = (-2F * plane[3] * plane[0]);

            reflectionMatrix.m10 = (-2F * plane[1] * plane[0]);
            reflectionMatrix.m11 = (1F - 2F * plane[1] * plane[1]);
            reflectionMatrix.m12 = (-2F * plane[1] * plane[2]);
            reflectionMatrix.m13 = (-2F * plane[3] * plane[1]);

            reflectionMatrix.m20 = (-2F * plane[2] * plane[0]);
            reflectionMatrix.m21 = (-2F * plane[2] * plane[1]);
            reflectionMatrix.m22 = (1F - 2F * plane[2] * plane[2]);
            reflectionMatrix.m23 = (-2F * plane[3] * plane[2]);

            reflectionMatrix.m30 = 0F;
            reflectionMatrix.m31 = 0F;
            reflectionMatrix.m32 = 0F;
            reflectionMatrix.m33 = 1F;
        }
    }
}
