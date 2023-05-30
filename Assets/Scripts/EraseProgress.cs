using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    public class EraseProgress
    {
        private readonly RenderTexture _percentTexture = new RenderTexture(1,1,0,RenderTextureFormat.ARGB32);
        private readonly CommandBuffer _commandBuffer = new CommandBuffer();
        private readonly Material _progressMaterial;
        private readonly RenderTargetIdentifier _rti;
        private readonly Mesh _progressMesh;
        private readonly int _mainTexProperty = Shader.PropertyToID("_MainTex");
        private float _currentProgressPercent;

        public EraseProgress(Mesh progressMesh, Material progressMaterial, RenderTexture maskTexture)
        {
            _progressMesh = progressMesh;
            _rti = new RenderTargetIdentifier(_percentTexture);
            _progressMaterial = new Material(progressMaterial);
            _progressMaterial.SetTexture(_mainTexProperty, maskTexture);
        }

        public float GetErasePercent()
        {
            RenderProgress();
            RenderTexture prevRenderTextureT = RenderTexture.active;
            RenderTexture.active = _percentTexture;
            Texture2D progressTexture = new Texture2D(_percentTexture.width, _percentTexture.height, TextureFormat.ARGB32, false, true);
            progressTexture.ReadPixels(new Rect(0, 0, _percentTexture.width, _percentTexture.height), 0, 0);
            progressTexture.Apply();
            RenderTexture.active = prevRenderTextureT;
            return _currentProgressPercent = progressTexture.GetPixel(0, 0).r * 100;
        }

        private void RenderProgress()
        {
            GL.LoadOrtho();
            _commandBuffer.Clear();
            _commandBuffer.SetRenderTarget(_rti);
            _commandBuffer.ClearRenderTarget(false, true, Color.clear);
            _commandBuffer.DrawMesh(_progressMesh, Matrix4x4.identity, _progressMaterial); ;
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }
    }
}