using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine;

namespace ComputeShading.FibonacciSpiral
{
    public class FibonacciSpiral : MonoBehaviour
    {
        [SerializeField] private ComputeShader _computeShader;

        [SerializeField] private float _theta;
        [SerializeField] private float _radius;
        [SerializeField] private uint _spiralLength;

        [SerializeField] private Mesh _mesh;
        [SerializeField] private Material _material;

        private ComputeBuffer _buffer;
        private static readonly int BufferID = Shader.PropertyToID("buffer");
        private Matrix4x4[] _matrix4X4Array;
        private int _kernelIndex;

        private void Start()
        {
            _matrix4X4Array = new Matrix4x4[_spiralLength];
            _kernelIndex = _computeShader.FindKernel("CalculateFibonacciSpiral");

            _buffer = new ComputeBuffer((int)_spiralLength, Marshal.SizeOf(typeof(float2)));
            _computeShader.SetBuffer(_kernelIndex, BufferID, _buffer);
        }

        private void Update()
        {
            _computeShader.SetFloat("theta", _theta);
            _computeShader.SetFloat("r", _radius);
            
            uint sizeX, sizeY, sizeZ;
            _computeShader.GetKernelThreadGroupSizes(
                _kernelIndex,
                out sizeX,
                out sizeY,
                out sizeZ
            );

            _computeShader.Dispatch(_kernelIndex, (int)(_spiralLength / sizeX), 1, 1);

            var result = new float2[_spiralLength];
            _buffer.GetData(result);

            for (var i = 0; i < _spiralLength; i++)
            {
                _matrix4X4Array[i] = Matrix4x4.TRS(new Vector3(result[i].x, result[i].y, 0), Quaternion.identity, Vector3.one * i);
            }

            var rp = new RenderParams(_material);
            Graphics.RenderMeshInstanced(rp, _mesh, 0,  _matrix4X4Array);
        }

        private void OnDestroy()
        {
            _buffer.Release();
            _buffer = null;
        }
    }
}