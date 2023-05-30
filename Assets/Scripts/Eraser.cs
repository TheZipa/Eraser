using DefaultNamespace;
using Input;
using UnityEngine;
using UnityEngine.Rendering;

public class Eraser : MonoBehaviour
{
	[SerializeField] private Material _eraseMaterial;
	[SerializeField] private Material _brushMaterial;
	[SerializeField] private Material _progressMaterial;
    [SerializeField] private Camera _camera;
    [SerializeField] private InputService _inputService;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private int _brushSize;

    private readonly int _maskTexture = Shader.PropertyToID("_MaskTex");
    private EraseRenderer _eraseRenderer;
    private EraseProgress _eraseProgress;
    private RenderTexture _renderTexture;
    private Vector2 _imageSize;
    private Vector2 _boundsSize;

    private void Start()
    {
	    SetImageSize(_spriteRenderer.sprite.texture);
	    _renderTexture = new RenderTexture((int) _imageSize.x, (int) _imageSize.y, 0, RenderTextureFormat.ARGB32);
	    _boundsSize = _spriteRenderer.sprite.bounds.size;
	    _inputService.OnLineDrag += ScratchLine;
	    _inputService.OnTouch += ScratchSingle;
	    SetEraseMaterial();
	    _eraseRenderer = new EraseRenderer(new RenderTargetIdentifier(_renderTexture), _brushMaterial, _imageSize, _brushSize);
	    _eraseProgress = new EraseProgress(_eraseRenderer.GenerateClearMesh(), _progressMaterial, _renderTexture);
	    _camera.Render();
    }
    
    private void SetImageSize(Texture imageTexture) =>
	    _imageSize = new Vector2(imageTexture.width, imageTexture.height);

    private void SetEraseMaterial()
    {
	    Material eraseMaterial = new Material(_eraseMaterial);
	    eraseMaterial.SetTexture(_maskTexture, _renderTexture);
	    _spriteRenderer.sharedMaterial = eraseMaterial;
    }

    private Vector2 GetScratchPosition(Vector2 screenPosition)
    {
	    Vector3 clickPosition = _camera.ScreenToWorldPoint(screenPosition);
	    Vector3 lossyScale = transform.lossyScale;
	    Vector2 clickLocalPosition = Vector2.Scale(transform.InverseTransformPoint(clickPosition), lossyScale) + _boundsSize / 2f;
	    Vector2 pixelsPerInch = new Vector2(_imageSize.x / _boundsSize.x / lossyScale.x, _imageSize.y / _boundsSize.y / lossyScale.y);
	    return Vector2.Scale(Vector2.Scale(clickLocalPosition, lossyScale), pixelsPerInch);
    }

    private void ScratchLine(Vector2 startScreenPosition, Vector2 endScreenPosition) =>
	    _eraseRenderer.ScratchLine(GetScratchPosition(startScreenPosition), GetScratchPosition(endScreenPosition));

    private void ScratchSingle(Vector2 screenPosition) =>
	    _eraseRenderer.ScratchSingle(GetScratchPosition(screenPosition));
}