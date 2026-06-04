using UnityEngine;

namespace Genese
{
    /// <summary>Referências de animação de uma criatura montada (lidas pelo Agent).</summary>
    public class CreatureView : MonoBehaviour
    {
        public Transform rig;      // agrupa todas as peças (faz o saltinho junto)
        public Transform body;     // corpo (sofre squash & stretch)
        public float baseTall = 1f;
        public float wide = 1f;
        public Genome genome;
    }
}
