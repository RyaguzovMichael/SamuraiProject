using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Random = UnityEngine.Random;

namespace SamuraiProject.AI
{
    public sealed class SimpleNeuralNetwork
    {
        private readonly int[] _layers;
        private float[][] _neurons;
        private float[][][] _weights;
        private float[][] _biases;

        public float Fitness { get; set; }

        public SimpleNeuralNetwork(int[] layers)
        {
            _layers = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++) _layers[i] = layers[i];
            InitNeurons();
            InitWeights();
        }

        public SimpleNeuralNetwork(SimpleNeuralNetwork copy)
        {
            _layers = (int[])copy._layers.Clone();
            InitNeurons();
            InitWeights();
            CopyWeightsAndBiases(copy);
        }
        private SimpleNeuralNetwork(int[] layers, bool ignoreInit)
        {
            _layers = new int[layers.Length];
            for (int i = 0; i < layers.Length; i++) _layers[i] = layers[i];

            InitNeurons();
            // InitWeights НЕ вызываем, но нужно выделить память под пустые массивы
            AllocateEmptyWeights();
        }

        public void OverwriteFrom(SimpleNeuralNetwork parent)
        {
            CopyWeightsAndBiases(parent);
            Fitness = 0f;
        }

        private void InitNeurons()
        {
            List<float[]> neuronsList = new();
            for (int i = 0; i < _layers.Length; i++)
            {
                neuronsList.Add(new float[_layers[i]]);
            }
            _neurons = neuronsList.ToArray();
        }

        private void InitWeights()
        {
            List<float[][]> weightsList = new();
            List<float[]> biasesList = new();

            for (int i = 1; i < _layers.Length; i++)
            {
                int prevLayerSize = _layers[i - 1];
                int currLayerSize = _layers[i];

                float[][] layerWeights = new float[prevLayerSize][];
                float[] layerBiases = new float[currLayerSize];

                for (int j = 0; j < prevLayerSize; j++)
                {
                    layerWeights[j] = new float[currLayerSize];
                    for (int k = 0; k < currLayerSize; k++)
                    {
                        layerWeights[j][k] = Random.Range(-0.5f, 0.5f);
                    }
                }

                for (int k = 0; k < currLayerSize; k++)
                {
                    layerBiases[k] = Random.Range(-0.5f, 0.5f);
                }

                weightsList.Add(layerWeights);
                biasesList.Add(layerBiases);
            }

            _weights = weightsList.ToArray();
            _biases = biasesList.ToArray();
        }

        private void AllocateEmptyWeights()
        {
            List<float[][]> weightsList = new();
            List<float[]> biasesList = new();

            for (int i = 1; i < _layers.Length; i++)
            {
                int prevLayerSize = _layers[i - 1];
                int currLayerSize = _layers[i];

                float[][] layerWeights = new float[prevLayerSize][];
                float[] layerBiases = new float[currLayerSize];

                for (int j = 0; j < prevLayerSize; j++)
                {
                    layerWeights[j] = new float[currLayerSize];
                }

                weightsList.Add(layerWeights);
                biasesList.Add(layerBiases);
            }
            _weights = weightsList.ToArray();
            _biases = biasesList.ToArray();
        }

        public float[] FeedForward(ReadOnlySpan<float> inputs)
        {
            for (int i = 0; i < inputs.Length; i++) _neurons[0][i] = inputs[i];

            for (int i = 1; i < _layers.Length; i++)
            {
                int prevLayerIdx = i - 1;
                bool isOutputLayer = i == _layers.Length - 1;

                for (int j = 0; j < _neurons[i].Length; j++)
                {
                    float value = 0f;
                    for (int k = 0; k < _neurons[prevLayerIdx].Length; k++)
                    {
                        value += _weights[prevLayerIdx][k][j] * _neurons[prevLayerIdx][k];
                    }
                    value += _biases[prevLayerIdx][j];

                    if (isOutputLayer)
                    {
                        if (j <= 1) _neurons[i][j] = (float)Math.Tanh(value);
                        else _neurons[i][j] = 1.0f / (1.0f + (float)Math.Exp(-value));
                    }
                    else
                    {
                        if (value > 0) _neurons[i][j] = value;
                        else _neurons[i][j] = value * 0.01f;
                    }
                }
            }
            return _neurons[^1];
        }

        public void Mutate(float chance, float val)
        {
            for (int i = 0; i < _weights.Length; i++)
                for (int j = 0; j < _weights[i].Length; j++)
                    for (int k = 0; k < _weights[i][j].Length; k++)
                        if (Random.value < chance) _weights[i][j][k] += Random.Range(-val, val);

            for (int i = 0; i < _biases.Length; i++)
                for (int j = 0; j < _biases[i].Length; j++)
                    if (Random.value < chance) _biases[i][j] += Random.Range(-val, val);
        }

        private void CopyWeightsAndBiases(SimpleNeuralNetwork other)
        {
            for (int i = 0; i < _weights.Length; i++)
                for (int j = 0; j < _weights[i].Length; j++)
                    for (int k = 0; k < _weights[i][j].Length; k++)
                        _weights[i][j][k] = other._weights[i][j][k];

            for (int i = 0; i < _biases.Length; i++)
                for (int j = 0; j < _biases[i].Length; j++)
                    _biases[i][j] = other._biases[i][j];
        }

        public void SaveBinary(string path)
        {
            // Открываем поток. BufferSize можно настроить, но дефолтный 4KB тоже ок.
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);

            // 1. ЗАПИСЬ ЗАГОЛОВКА (Fitness + LayerCount)
            // Используем stackalloc, чтобы не выделять память в куче под 8 байт
            Span<byte> header = stackalloc byte[sizeof(float) + sizeof(int)];

            BitConverter.TryWriteBytes(header, Fitness);
            BitConverter.TryWriteBytes(header[sizeof(float)..], _layers.Length);

            fs.Write(header);

            // 2. ЗАПИСЬ ТОПОЛОГИИ (_layers)
            // Превращаем int[] сразу в байты без копирования
            fs.Write(MemoryMarshal.AsBytes(_layers.AsSpan()));

            // 3. ЗАПИСЬ ВЕСОВ
            // Проходим по "рваному" массиву. Каждый вложенный массив float[] пишем напрямую.
            foreach (var layer in _weights)
            {
                foreach (var neuronWeights in layer)
                {
                    // neuronWeights - это float[], пишем его как байты
                    fs.Write(MemoryMarshal.AsBytes(neuronWeights.AsSpan()));
                }
            }

            // 4. ЗАПИСЬ БИАСОВ
            foreach (var layerBiases in _biases)
            {
                // layerBiases - это float[], пишем его как байты
                fs.Write(MemoryMarshal.AsBytes(layerBiases.AsSpan()));
            }
        }

        public static SimpleNeuralNetwork LoadBinary(string path)
        {
            if (!File.Exists(path)) return null;

            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            // 1. ЧТЕНИЕ ЗАГОЛОВКА
            Span<byte> header = stackalloc byte[sizeof(float) + sizeof(int)];
            int bytesRead = fs.Read(header);
            if (bytesRead < header.Length) return null; // Файл битый

            float fitness = BitConverter.ToSingle(header);
            int layersCount = BitConverter.ToInt32(header[sizeof(float)..]);

            // 2. ЧТЕНИЕ ТОПОЛОГИИ (через ArrayPool)
            // Нам нужен массив int[], чтобы передать его в конструктор.
            // Берем его в аренду, чтобы не мусорить, если загрузок будет много.
            int[] layersBuffer = ArrayPool<int>.Shared.Rent(layersCount);

            SimpleNeuralNetwork net = null;

            try
            {
                // Читаем байты прямо в массив int
                Span<byte> layersBytes = MemoryMarshal.AsBytes(layersBuffer.AsSpan(0, layersCount));
                fs.Read(layersBytes);

                // 3. СОЗДАНИЕ НЕЙРОНКИ
                // Конструктор выделит память под _weights и _biases нужного размера
                // (Важно: твой конструктор копирует layersBuffer, так что возврат в пулл безопасен)
                net = new SimpleNeuralNetwork(layersBuffer[..layersCount])
                {
                    Fitness = fitness
                };

                // 4. ЧТЕНИЕ ВЕСОВ
                // Теперь мы просто заполняем выделенные массивы данными из файла
                foreach (var layer in net._weights)
                {
                    foreach (var neuronWeights in layer)
                    {
                        // Читаем float[] как byte[]
                        Span<byte> weightsBytes = MemoryMarshal.AsBytes(neuronWeights.AsSpan());
                        fs.Read(weightsBytes);
                    }
                }

                // 5. ЧТЕНИЕ БИАСОВ
                foreach (var layerBiases in net._biases)
                {
                    Span<byte> biasesBytes = MemoryMarshal.AsBytes(layerBiases.AsSpan());
                    fs.Read(biasesBytes);
                }
            }
            finally
            {
                // Обязательно возвращаем массив в пул
                ArrayPool<int>.Shared.Return(layersBuffer);
            }

            return net;
        }

        public static SimpleNeuralNetwork LoadFromMemory(byte[] data)
        {
            if (data == null || data.Length == 0) return null;

            // Создаем Span - это наше "окно" на массив байтов без копирования
            ReadOnlySpan<byte> cursor = data.AsSpan();

            // 1. Фитнес (4 байта)
            // Если данных меньше чем нужно, Slice выбросит исключение - это норм для битого файла
            float fitness = BitConverter.ToSingle(cursor[..4]);
            cursor = cursor[4..]; // Сдвигаем окно

            // 2. Кол-во слоев (4 байта)
            int layersCount = BitConverter.ToInt32(cursor[..4]);
            cursor = cursor[4..];

            // 3. Топология (layersCount * 4 байта)
            int layersBytesSize = layersCount * sizeof(int);

            // Временный массив для слоев
            int[] layers = new int[layersCount];
            // Копируем байты сразу в int[] (самый быстрый способ)
            MemoryMarshal.Cast<byte, int>(cursor[..layersBytesSize]).CopyTo(layers);

            cursor = cursor[layersBytesSize..];

            // 4. Создаем экземпляр
            SimpleNeuralNetwork net = new(layers)
            {
                Fitness = fitness
            };

            // 5. Заполняем Веса
            foreach (var layer in net._weights)
            {
                foreach (var neuronWeights in layer)
                {
                    int sizeInBytes = neuronWeights.Length * sizeof(float);

                    // Заливаем байты напрямую во float[] нейрона
                    MemoryMarshal.Cast<byte, float>(cursor[..sizeInBytes])
                                 .CopyTo(neuronWeights);

                    cursor = cursor[sizeInBytes..];
                }
            }

            // 6. Заполняем Биасы
            foreach (var layerBiases in net._biases)
            {
                int sizeInBytes = layerBiases.Length * sizeof(float);

                MemoryMarshal.Cast<byte, float>(cursor[..sizeInBytes])
                             .CopyTo(layerBiases);

                cursor = cursor[sizeInBytes..];
            }

            return net;
        }

        public SimpleNeuralNetwork Clone()
        {
            var clone = new SimpleNeuralNetwork(_layers, true)
            {
                Fitness = Fitness
            };

            for (int i = 0; i < _weights.Length; i++)
            {
                for (int j = 0; j < _weights[i].Length; j++)
                    Array.Copy(_weights[i][j], clone._weights[i][j], _weights[i][j].Length);
            }
            for (int i = 0; i < _biases.Length; i++)
                Array.Copy(_biases[i], clone._biases[i], _biases[i].Length);

            return clone;
        }
    }
}
