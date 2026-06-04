using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>Marca uma célula do mundo (para clique/inspeção).</summary>
    public class CoreCellTag : MonoBehaviour { public int X, Y; }

    /// <summary>Marca uma criatura renderizada (para clique/inspeção).</summary>
    public class CoreCreatureTag : MonoBehaviour { public int Id; }

    /// <summary>Marca a malha do terreno (clique → célula via posição).</summary>
    public class CoreTerrainTag : MonoBehaviour { }
}
