#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Genese.EditorTools
{
    /// <summary>
    /// Cria a cena "Mundo Vivo" com um GameObject Bootstrap — sem montagem manual.
    /// Menu: Gênese ▸ Criar cena Mundo Vivo. Depois é só dar Play.
    /// </summary>
    public static class SetupMundoVivo
    {
        [MenuItem("Gênese/Criar cena Mundo Vivo")]
        public static void Create()
        {
            if (EditorApplication.isPlaying)
            {
                EditorUtility.DisplayDialog("Gênese", "Saia do modo Play (▶) primeiro.\nDepois use o menu para criar a cena e só então dê Play.", "Ok");
                return;
            }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var go = new GameObject("Genese");
            go.AddComponent<Bootstrap>();

            System.IO.Directory.CreateDirectory("Assets/Scenes");
            EditorSceneManager.SaveScene(scene, "Assets/Scenes/MundoVivo.unity");
            EditorUtility.DisplayDialog("Gênese",
                "Cena 'MundoVivo' criada com o Bootstrap.\nDê Play para ver o termômetro 3D.", "Ok");
        }
    }
}
#endif
