using System.Collections.Generic;
using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Registro simples de obstáculos estáticos (casas, árvores, rochas, totem,
    /// fogueira) para os agentes não atravessarem. Cada obstáculo é um círculo no
    /// plano XZ: (x = mundo X, y = mundo Z, z = raio). Colisão cinemática barata —
    /// a física/steering real do mundo vem na Etapa 1.
    /// </summary>
    public static class Obstacles
    {
        static readonly List<Vector3> Items = new();

        public static void Clear() => Items.Clear();
        public static void Add(float worldX, float worldZ, float radius) => Items.Add(new Vector3(worldX, worldZ, radius));

        /// <summary>Empurra a posição para fora dos obstáculos, somando o raio do agente (pad).</summary>
        public static void Resolve(ref Vector3 p, float pad = 0f)
        {
            for (int i = 0; i < Items.Count; i++)
            {
                var o = Items[i];
                float edge = o.z + pad;
                float dx = p.x - o.x, dz = p.z - o.y;
                float dist = Mathf.Sqrt(dx * dx + dz * dz);
                if (dist < edge)
                {
                    if (dist < 1e-4f) { dx = Random.value - 0.5f; dz = Random.value - 0.5f; dist = Mathf.Sqrt(dx * dx + dz * dz) + 1e-4f; }
                    p.x = o.x + dx / dist * edge;
                    p.z = o.y + dz / dist * edge;
                }
            }
        }
    }
}
