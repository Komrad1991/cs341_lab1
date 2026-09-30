using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine(
                "Использование: GraphConv <входной_файл> <файл_матрицы> <файл_списка>");
            return 1;
        }

        string inputPath = args[0];
        string matrixPath = args[1];
        string listPath = args[2];

        if (!File.Exists(inputPath))
        {
            Console.Error.WriteLine($"Входной файл не найден: {inputPath}");
            return 1;
        }

        // ---------- 1. Чтение рёбер и нумерация вершин в порядке появления ----------
        var vertexOrder = new List<string>();
        var vertexIndex = new Dictionary<string, int>(StringComparer.Ordinal);
        var edges = new List<(int A, int B)>();

        int GetIndex(string v)
        {
            if (!vertexIndex.TryGetValue(v, out int idx))
            {
                idx = vertexOrder.Count;
                vertexIndex[v] = idx;
                vertexOrder.Add(v);
            }
            return idx;
        }

        foreach (var rawLine in File.ReadLines(inputPath))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith("#") || line.StartsWith("//"))
                continue;

            var parts = line.Split(
                new[] { ' ', '\t', ';', ',' },
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                Console.Error.WriteLine($"Предупреждение: пропущена строка \"{rawLine}\"");
                continue;
            }

            int a = GetIndex(parts[0]);
            int b = GetIndex(parts[1]);
            edges.Add((a, b));
        }

        int n = vertexOrder.Count;
        if (n == 0)
        {
            Console.Error.WriteLine("Во входном файле не найдено ни одной вершины.");
            return 1;
        }

        // ---------- 2. Построение матрицы смежности ----------
        int[,] matrix = new int[n, n];
        foreach (var (a, b) in edges)
        {
            matrix[a, b] = 1;
            matrix[b, a] = 1;
        }

        using (var w = new StreamWriter(matrixPath, false, Encoding.UTF8))
        {
            // шапка с именами столбцов
            w.Write('\t');
            w.WriteLine(string.Join('\t', vertexOrder));

            for (int i = 0; i < n; i++)
            {
                w.Write(vertexOrder[i]);
                for (int j = 0; j < n; j++)
                {
                    w.Write('\t');
                    w.Write(matrix[i, j]);
                }
                w.WriteLine();
            }
        }

        // ---------- 3. Построение списка смежности ----------
        var adj = new SortedSet<int>[n];
        for (int i = 0; i < n; i++) adj[i] = new SortedSet<int>();

        foreach (var (a, b) in edges)
        {
            if (a == b) adj[a].Add(a);        // петля
            else
            {
                adj[a].Add(b);
                adj[b].Add(a);
            }
        }

        using (var w = new StreamWriter(listPath, false, Encoding.UTF8))
        {
            for (int i = 0; i < n; i++)
            {
                w.Write(vertexOrder[i]);
                w.Write(':');
                foreach (var j in adj[i])
                {
                    w.Write(' ');
                    w.Write(vertexOrder[j]);
                }
                w.WriteLine();
            }
        }

        Console.WriteLine(
            $"Готово. Вершин: {n}, рёбер: {edges.Count}.");
        Console.WriteLine($"  матрица смежности : {matrixPath}");
        Console.WriteLine($"  список смежности  : {listPath}");
        return 0;
    }
}