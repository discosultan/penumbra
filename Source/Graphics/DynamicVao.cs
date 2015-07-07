using System;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics
{
    internal class DynamicVao : IDisposable
    {
        private const int DefaultVertexCount = 32;
        
        private readonly GraphicsDevice _graphicsDevice;
        private readonly VertexDeclaration _vertexDeclaration;
        private readonly bool _useIndices;
        private readonly int _initialVertexCount;
        private readonly int _initialIndexCount;        

        public DynamicVertexBuffer VertexBuffer { get; set; }
        public DynamicIndexBuffer IndexBuffer { get; set; }
        private int _currentVertexCount;
        private int _currentIndexCount;

        private DynamicVao(
            GraphicsDevice graphicsDevice,            
            VertexDeclaration vertexDecl,
            int initialVertexCount,
            int initialIndexCount,            
            bool useIndices)
        {
            _graphicsDevice = graphicsDevice;                                    
            _vertexDeclaration = vertexDecl;
            _useIndices = useIndices;

            _initialVertexCount = initialVertexCount;
            _initialIndexCount = initialIndexCount;
            _currentVertexCount = _initialVertexCount;
            _currentIndexCount = _initialIndexCount;            

            CreateVertexBuffer();
            if (_useIndices)
                CreateIndexBuffer();
        }

        public int VertexCount { get; private set; }
        public int IndexCount { get; private set; }

        public void SetVertices<T>(T[] fromData) where T : struct
        {
            VertexCount = fromData.Length;
            if (NeedToIncreaseBufferSize(ref _currentVertexCount, fromData.Length))
                CreateVertexBuffer();
            VertexBuffer.SetData(fromData);
        }

        public void SetIndices(int[] fromData)
        {
            if (!_useIndices) return;
            IndexCount = fromData.Length;
            if (NeedToIncreaseBufferSize(ref _currentIndexCount, fromData.Length))
                CreateIndexBuffer();
            IndexBuffer.SetData(fromData);
        }

        public void Dispose()
        {
            VertexBuffer?.Dispose();
            IndexBuffer?.Dispose();
        }

        private void CreateVertexBuffer()
        {            
            VertexBuffer = new DynamicVertexBuffer(_graphicsDevice, _vertexDeclaration, _currentVertexCount, BufferUsage.WriteOnly);                                        
            Logger.Write($"Created dynamic vao vertex buffer with size {_currentVertexCount}");
        }

        private void CreateIndexBuffer()
        {                        
            IndexBuffer = new DynamicIndexBuffer(_graphicsDevice, IndexElementSize.ThirtyTwoBits, _currentIndexCount,
                BufferUsage.WriteOnly);            
            Logger.Write($"Created dynamic vao index buffer with size {_currentIndexCount}");
        }

        // Determines if a buffer needs to be recreated and increases currentElementCount if needed.
        private static bool NeedToIncreaseBufferSize(ref int currentElementCount, int requiredElementCount)
        {
            bool recreateBuffer = false;
            while (currentElementCount < requiredElementCount)
            {
                currentElementCount *= 2;
                recreateBuffer = true;
            }
            return recreateBuffer;
        }

        public static DynamicVao New(
            GraphicsDevice graphicsDevice,
            VertexDeclaration vertexDecl,
            int vertexCount = DefaultVertexCount,
            int indexCount = (DefaultVertexCount - 2) * 3,
            bool useIndices = false)
        {
            return new DynamicVao(graphicsDevice, vertexDecl, vertexCount, indexCount, useIndices);
        }        
    }

    internal static class DynamicVaoExtensions
    {
        public static void SetVertexArrayObject(this GraphicsDevice device, DynamicVao vao)
        {
            device.SetVertexBuffer(vao.VertexBuffer);
            device.Indices = vao.IndexBuffer;
        }
    }
}