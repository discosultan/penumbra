using System;
using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics
{
    internal class DynamicVao : IVao, IDisposable
    {
        private const int DefaultVertexCount = 32;
        
        private readonly GraphicsDevice _graphicsDevice;
        private readonly VertexDeclaration _vertexDeclaration;
        private readonly bool _useIndices;
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }
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

            _currentVertexCount = initialVertexCount;
            _currentIndexCount = initialIndexCount;            

            CreateVertexBuffer();
            if (_useIndices)
                CreateIndexBuffer();
        }

        public int VertexCount { get; private set; }
        public int IndexCount { get; private set; }

        public void SetVertices<T>(T[] fromData) where T : struct
        {
            SetVertices(fromData, fromData.Length);
        }

        public void SetVertices<T>(FastList<T> fromData) where T : struct
        {
            SetVertices(fromData.Items, fromData.Count);
        }

        public void SetVertices<T>(T[] fromData, int count) where T : struct
        {
            VertexCount = count;
            if (NeedToIncreaseBufferSize(ref _currentVertexCount, count))
                CreateVertexBuffer();
            VertexBuffer.SetData(fromData, 0, count);
        }

        public void SetIndices(int[] fromData)
        {
            SetIndices(fromData, fromData.Length);
        }

        public void SetIndices(FastList<int> fromData)
        {
            SetIndices(fromData.Items, fromData.Count);
        }

        public void SetIndices(int[] fromData, int count)
        {
            if (!_useIndices) return;
            IndexCount = count;
            if (NeedToIncreaseBufferSize(ref _currentIndexCount, count))
                CreateIndexBuffer();
            IndexBuffer.SetData(fromData, 0, count);
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
}