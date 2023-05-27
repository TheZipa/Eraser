using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    public class EraseRenderer
    {
        private readonly List<Vector3> _positions = new List<Vector3>();
        private readonly List<Color> _colors = new List<Color>(); 
        private readonly List<int> _triangles = new List<int>();
        private readonly List<Vector2> _uv = new List<Vector2>();
        
        private readonly Mesh _lineMesh = new Mesh();
        private readonly CommandBuffer _commandBuffer = new CommandBuffer();
        private readonly RenderTargetIdentifier _rti;
        private readonly Material _brushMaterial;
        private readonly Vector2 _imageSize;
        private readonly int _brushSize;

        public EraseRenderer(RenderTargetIdentifier rti, Material brushMaterial, Vector2 imageSize, int brushSize)
        {
            _rti = rti;
            _brushMaterial = brushMaterial;
            _imageSize = imageSize;
            _brushSize = brushSize;
        }
        
        public void ScratchLine(Vector2 startPosition, Vector2 endPosition)
        {
            var holesCount = (int)Vector2.Distance(startPosition, endPosition);
            var count = 0;
            for (var i = 0; i < holesCount; i++)
            {
                Rect positionRect = GetPositionRect(startPosition, endPosition, holesCount, i);
                AddVertices(positionRect);
                AddColors();
                AddUV();
                AddTriangles(count * 4);

                count++;
            }

            if (_positions.Count <= 1) return;
            _lineMesh.Clear(false);
            AddComplexLineToLineMesh();
            RenderMeshLine();
        }
        
        private Rect GetPositionRect(Vector2 startPosition, Vector2 endPosition, int holesCount, int i)
        {
            Vector2 holePosition = startPosition + (endPosition - startPosition) / holesCount * i;
            Texture eraseTexture = _brushMaterial.mainTexture;
            Rect positionRect = new Rect(
                (holePosition.x - 0.5f * eraseTexture.width * _brushSize) / _imageSize.x,
                (holePosition.y - 0.5f * eraseTexture.height * _brushSize) / _imageSize.y,
                eraseTexture.width * _brushSize / _imageSize.x,
                eraseTexture.height * _brushSize / _imageSize.y);
            return positionRect;
        }

        private void RenderMeshLine()
        {
            GL.LoadOrtho();
            _commandBuffer.Clear();
            _commandBuffer.SetRenderTarget(_rti);
            _commandBuffer.DrawMesh(_lineMesh, Matrix4x4.identity, _brushMaterial);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }
        
        private void AddColors()
        {
            for (int j = 0; j < 4; j++)
                _colors.Add(Color.white);
        }

        private void AddComplexLineToLineMesh()
        {
            _lineMesh.vertices = _positions.ToArray();
            _lineMesh.uv = _uv.ToArray();
            _lineMesh.triangles = _triangles.ToArray();
            _lineMesh.colors = _colors.ToArray();
            _positions.Clear();
            _uv.Clear();
            _triangles.Clear();
            _colors.Clear();
        }

        private void AddTriangles(int count)
        {
            _triangles.Add(0 + count);
            _triangles.Add(1 + count);
            _triangles.Add(2 + count);
            _triangles.Add(2 + count);
            _triangles.Add(3 + count);
            _triangles.Add(0 + count);
        }

        private void AddUV()
        {
            _uv.Add(Vector2.up);
            _uv.Add(Vector2.one);
            _uv.Add(Vector2.right);
            _uv.Add(Vector2.zero);
        }

        private void AddVertices(Rect positionRect)
        {
            _positions.Add(new Vector3(positionRect.xMin, positionRect.yMax, 0));
            _positions.Add(new Vector3(positionRect.xMax, positionRect.yMax, 0));
            _positions.Add(new Vector3(positionRect.xMax, positionRect.yMin, 0));
            _positions.Add(new Vector3(positionRect.xMin, positionRect.yMin, 0));
        }
    }
}