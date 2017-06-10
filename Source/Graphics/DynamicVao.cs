using Microsoft.Xna.Framework.Graphics;
using Penumbra.Utilities;

namespace Penumbra.Graphics
{
    internal sealed class DynamicVao : Vao
    {
        private const int DefaultVertexCount = 32;

        private readonly GraphicsDevice _graphicsDevice;

        private int _currentVertexCount;
        private int _currentIndexCount;
        private int _vertexCountInUse;
        private int _indexCountInUse;

        private DynamicVao(
            GraphicsDevice graphicsDevice,
            VertexDeclaration vertexDecl,
            PrimitiveType primitiveTopology,
            int initialVertexCount,
            int initialIndexCount,
            bool useIndices) : base(vertexDecl, primitiveTopology)
        {
            _graphicsDevice = graphicsDevice;

            _currentVertexCount = initialVertexCount;
            _currentIndexCount = initialIndexCount;

            CreateVertexBuffer();
            if (useIndices)
                CreateIndexBuffer();
        }

        public override int VertexCount => _vertexCountInUse;

        public override int IndexCount => _indexCountInUse;

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
            _vertexCountInUse = count;
            if (NeedToIncreaseBufferSize(ref _currentVertexCount, count))
                CreateVertexBuffer();
            VertexBuffer.SetData(fromData, 0, count);
            if (IndexBuffer == null)
                CalculatePrimitiveCount();
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
            if (!HasIndices)
                return;

            _indexCountInUse = count;
            if (NeedToIncreaseBufferSize(ref _currentIndexCount, count))
                CreateIndexBuffer();
            IndexBuffer.SetData(fromData, 0, count);
            CalculatePrimitiveCount();
        }

        private void CreateVertexBuffer()
        {
            VertexBuffer = new DynamicVertexBuffer(_graphicsDevice, VertexDeclaration, _currentVertexCount, BufferUsage.WriteOnly);
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
            PrimitiveType primitiveTopology,
            int vertexCount = DefaultVertexCount,
            int indexCount = (DefaultVertexCount - 2) * 3,
            bool useIndices = false)
        {
            return new DynamicVao(graphicsDevice, vertexDecl, primitiveTopology, vertexCount, indexCount, useIndices);
        }
    }
}