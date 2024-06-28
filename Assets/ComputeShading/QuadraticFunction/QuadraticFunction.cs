using UnityEngine;

namespace ComputeShading
{
    public class QuadraticFunction : MonoBehaviour
    {
        [SerializeField] private ComputeShader _computeShader;
        [SerializeField] private float a;
        [SerializeField] private float p;
        [SerializeField] private float q;

        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;
    
        [SerializeField] private uint _curveLength = 32;
    
        private ComputeBuffer _buffer;
        private static readonly int BufferID = Shader.PropertyToID("buffer");
        private Matrix4x4[] _matrix4X4Array;
        private int _kernelIndex;
    
        private void Start()
        {
            _matrix4X4Array = new Matrix4x4[_curveLength];
            _kernelIndex = _computeShader.FindKernel("CalculateParabolaCurve");

            _buffer = new ComputeBuffer((int)_curveLength, sizeof(float));
            _computeShader.SetBuffer(_kernelIndex, BufferID, _buffer);
        }

        private void Update()
        {
            _computeShader.SetFloat("a", a);
            _computeShader.SetFloat("p", p);
            _computeShader.SetFloat("q", q);

            uint sizeX, sizeY, sizeZ;
            _computeShader.GetKernelThreadGroupSizes(
                _kernelIndex,
                out sizeX,
                out sizeY,
                out sizeZ
            );
        
            _computeShader.Dispatch(_kernelIndex, (int)(_curveLength / sizeX), 1, 1);

            var result = new float[_curveLength];
            _buffer.GetData(result);

            for (var i = 0; i < _curveLength; i++)
            {
                _matrix4X4Array[i] = Matrix4x4.TRS(new Vector3(i, result[i], 0), Quaternion.identity, Vector3.one);
            }
            Graphics.DrawMeshInstanced(_mesh, 0, _material, _matrix4X4Array);
        }

        private void OnDestroy()
        {
            _buffer.Release();
            _buffer = null;
        }
    }
}
