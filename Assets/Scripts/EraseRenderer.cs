using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace DefaultNamespace
{
    public class EraseRenderer
    {
        private readonly Material _brushMaterial;
        private readonly CommandBuffer _commandBuffer = new() {name = "Erase Render"};
        private readonly Mesh _lineMesh = new();
        private readonly Mesh _singleMesh;
        private readonly Vector2 _imageSize;
        private readonly RenderTargetIdentifier _rti;
        private readonly int _brushSize;

        private readonly List<Color> _colors = new();
        private readonly List<Vector3> _positions = new();
        private readonly List<int> _triangles = new();
        private readonly List<Vector2> _uv = new();

        public EraseRenderer(RenderTargetIdentifier rti, Material brushMaterial, Vector2 imageSize, int brushSize)
        {
            _singleMesh = GenerateClearMesh();
            _rti = rti;
            _brushMaterial = brushMaterial;
            _imageSize = imageSize;
            _brushSize = brushSize;
            _commandBuffer.SetRenderTarget(_rti);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        public void ScratchSingle(Vector2 position)
        {
            Rect positionRect = GetPositionRect(position);

            _singleMesh.vertices = new[]
            {
                new Vector3(positionRect.xMin, positionRect.yMax, 0),
                new Vector3(positionRect.xMax, positionRect.yMax, 0),
                new Vector3(positionRect.xMax, positionRect.yMin, 0),
                new Vector3(positionRect.xMin, positionRect.yMin, 0)
            };
            RenderMesh(_singleMesh);
        }

        public void ScratchLine(Vector2 startPosition, Vector2 endPosition)
        {
            int holesCount = (int) Vector2.Distance(startPosition, endPosition);
            int count = 0;
            for (int i = 0; i < holesCount; i++)
            {
                Vector2 holePosition = startPosition + (endPosition - startPosition) / holesCount * i;
                Rect positionRect = GetPositionRect(holePosition);
                AddVertices(positionRect);
                AddColors();
                AddUV();
                AddTriangles(count * 4);

                count++;
            }

            if (_positions.Count <= 0) return;
            _lineMesh.Clear(false);
            AddComplexLineToLineMesh();
            RenderMesh(_lineMesh);
        }
        
        public Mesh GenerateClearMesh() => new Mesh()
        {
            vertices = new[]
            {
                new Vector3(0f, 1f, 0f),
                new Vector3(1f, 1f , 0),
                new Vector3(1f, 0f, 0f),
                new Vector3(0f, 0f, 0f)
            },
            uv = new[]
            {
                new Vector2(0f, 1f),
                new Vector2(1f, 1f),
                new Vector2(1f, 0f),
                new Vector2(0f, 0f)
            },
            triangles = new[]
            {
                0, 1, 2, 2, 3, 0
            },
            colors = new[]
            {
                Color.white,
                Color.white,
                Color.white,
                Color.white
            }
        };

        private Rect GetPositionRect(Vector2 position)
        {
            Texture eraseTexture = _brushMaterial.mainTexture;
            return new Rect((position.x - 0.5f * eraseTexture.width * _brushSize) / _imageSize.x,
                (position.y - 0.5f * eraseTexture.height * _brushSize) / _imageSize.y,
                eraseTexture.width * _brushSize / _imageSize.x,
                eraseTexture.height * _brushSize / _imageSize.y);
        }

        private void RenderMesh(Mesh mesh)
        {
            GL.LoadOrtho();
            _commandBuffer.Clear();
            _commandBuffer.SetRenderTarget(_rti);
            _commandBuffer.DrawMesh(mesh, Matrix4x4.identity, _brushMaterial);
            Graphics.ExecuteCommandBuffer(_commandBuffer);
        }

        private void AddColors()
        {
            for (var j = 0; j < 4; j++)
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