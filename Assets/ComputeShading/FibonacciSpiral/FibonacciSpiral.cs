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
        
        private int _kernelIndex;

        private GraphicsBuffer.IndirectDrawIndexedArgs[] _commandData;
        
        private static readonly int Theta = Shader.PropertyToID("theta");
        private static readonly int R = Shader.PropertyToID("r");
        private static readonly int BufferID = Shader.PropertyToID("buffer");
        private static readonly int PositionsID = Shader.PropertyToID("_Positions");
        private static readonly int CountID = Shader.PropertyToID("_Count");

        private void Start()
        {
            // ComputeShaderの用意
            _kernelIndex = _computeShader.FindKernel("CalculateFibonacciSpiral");
            
            // RenderMeshIndirectの用意
            _commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
            _commandData[0].indexCountPerInstance = _mesh.GetIndexCount(0);
            _commandData[0].baseVertexIndex = _mesh.GetBaseVertex(0);
            _commandData[0].startIndex = _mesh.GetIndexStart(0);
        }

        private void Update()
        {
            _computeShader.SetFloat(Theta, _theta);
            _computeShader.SetFloat(R, _radius);
            
            var positions = new float3[_spiralLength];
            var buffer = new ComputeBuffer((int)_spiralLength, Marshal.SizeOf(typeof(float2)));
            _computeShader.SetBuffer(_kernelIndex, BufferID, buffer);
            
            uint sizeX, sizeY, sizeZ;
            _computeShader.GetKernelThreadGroupSizes(
                _kernelIndex,
                out sizeX,
                out sizeY,
                out sizeZ
            );

            var threadGroupX = (int)System.Math.Ceiling(_spiralLength / (float)sizeX);
            _computeShader.Dispatch(_kernelIndex, threadGroupX, 1, 1);
            
            var result = new float2[_spiralLength];
            buffer.GetData(result);

            for (var i = 0; i < _spiralLength; i++)
            {
                positions[i] = new float3(result[i].x, result[i].y, 0);
            }
            
            var rp = new RenderParams(_material);
            
            var positionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)_spiralLength, Marshal.SizeOf<float3>());
            positionsBuffer.SetData(positions);
            
            rp.matProps = new();
            rp.matProps.SetBuffer(PositionsID, positionsBuffer);
            rp.matProps.SetInt(CountID, (int)_spiralLength);
            
            rp.material.SetInt("_InstanceCount", (int)_spiralLength);
            
            _commandData[0].instanceCount = _spiralLength;
         
            var indirectBuf = new GraphicsBuffer(GraphicsBuffer.Target.IndirectArguments, 1, GraphicsBuffer.IndirectDrawIndexedArgs.size);
            indirectBuf.SetData(_commandData);
            
            Graphics.RenderMeshIndirect(rp, _mesh, indirectBuf);
        }
    }
}