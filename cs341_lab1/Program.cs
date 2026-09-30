using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

class Program
{
    static int Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.Error.WriteLine(
                "Использование: Task1 <входной_файл> <файл_матрицы> <файл_списка>");
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

        // --- 1. Читаем все непустые строки ---
        var lines = File.ReadAllLines(inputPath)
                        .Select(l => l.Trim())
                        .Where(l => l.Length > 0)
                        .ToList();

        if (lines.Count == 0)
        {
            Console.Error.WriteLine("Входной файл пуст.");
            return 1;
        }

        // --- 2. Первая строка — список вершин в нужном порядке ---
        var vertexOrder = lines[0]
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        int n = vertexOrder.Count;
        var vertexIndex = new Dictionary<string, int>(StringComparer.Ordinal);
        for (int i = 0; i < n; i++)
            vertexIndex[vertexOrder[i]] = i;

        // --- 3. Структуры для результатов ---
        int[,] matrix = new int[n, n];

        // Списки смежности: соседи добавляются В ПОРЯДКЕ ПОЯВЛЕНИЯ РЁБЕР,
        // дубликатов не допускаем.
        var adj = new List<List<int>>();
        var adjSet = new HashSet<int>[n];
        for (int i = 0; i < n; i++)
        {
            adj.Add(new List<int>());
            adjSet[i] = new HashSet<int>();
        }

        // --- 4. Остальные строки — рёбра ---
        for (int k = 1; k < lines.Count; k++)
        {
            var parts = lines[k].Split(
                new[] { ' ', '\t', ';', ',' },
                StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length < 2)
            {
                Console.Error.WriteLine($"Предупреждение: пропущена строка \"{lines[k]}\"");
                continue;
            }

            if (!vertexIndex.TryGetValue(parts[0], out int a) ||
                !vertexIndex.TryGetValue(parts[1], out int b))
            {
                Console.Error.WriteLine(
                    $"Предупреждение: неизвестная вершина в строке \"{lines[k]}\"");
                continue;
            }

            // Матрица смежности (неориентированная)
            matrix[a, b] = 1;
            matrix[b, a] = 1;

            // Список смежности — в порядке появления рёбер
            if (adjSet[a].Add(b)) adj[a].Add(b);
            if (adjSet[b].Add(a)) adj[b].Add(a);
        }

        // --- 5. Запись матрицы смежности ---
        using (var w = new StreamWriter(matrixPath))
        {
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (j > 0) w.Write(' ');
                    w.Write(matrix[i, j]);
                }
                w.WriteLine();
            }
        }

        // --- 6. Запись списка смежности ---
        using (var w = new StreamWriter(listPath))
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

        Console.WriteLine($"Готово: {inputPath} -> {matrixPath}, {listPath}");
        return 0;
    }
}