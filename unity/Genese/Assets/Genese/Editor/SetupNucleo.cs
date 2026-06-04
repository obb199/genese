#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Genese.EditorTools
{
    /// <summary>
    /// Cria a cena que roda o NÚCLEO (Genese.Core) e o desenha em 3D.
    /// Menu: Gênese ▸ Criar cena Mundo (Núcleo). Depois é só dar Play.
    /// </summary>
    public static class SetupNucleo
    {
        [MenuItem("Gênese/Criar cena Mundo (Núcleo)")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Gênese", "Saia do modo Play (▶) primeiro.\nDepois use o menu para criar a cena e só então dê Play.", "Ok");
                return;
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("GeneseNucleo");
            go.AddComponent<Genese.Nucleo.CoreBootstrap>();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MundoNucleo.unity");
            EditorUtility.DisplayDialog("Gênese",
                "Cena 'MundoNucleo' criada — ela RODA o núcleo (Genese.Core) e desenha o mundo + criaturas.\n\nDê Play. Clique numa célula (temperatura/umidade/recursos) ou numa criatura (genoma).", "Ok");
        }
    }
}
#endif
