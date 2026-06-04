using UnityEngine;

namespace Genese
{
    /// <summary>
    /// Ponto de entrada do termômetro em Unity: cria câmera, luzes, mundo e a
    /// simulação-vitrine por código (sem precisar montar a cena à mão).
    /// Basta ter um GameObject vazio com este componente e dar Play.
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        void Start()
        {
            // câmera
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Palette.Hex("#d9d6b0");
            cam.fieldOfView = 46;
            camGo.AddComponent<CameraOrbit>();

            // sol + sombras
            var sunGo = new GameObject("Sol");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional; sun.color = Palette.Hex("#fff0d0"); sun.intensity = 1.3f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(50, 35, 0);

            // luz de preenchimento (ambiente)
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Palette.Hex("#b9c298");   // um pouco mais contido
            RenderSettings.ambientEquatorColor = Palette.Hex("#7c8456");
            RenderSettings.ambientGroundColor = Palette.Hex("#2f3526");
            RenderSettings.fog = true; RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogStartDistance = 32; RenderSettings.fogEndDistance = 78; // névoa sutil (era 22/46 e estourava)
            var hemiGo = new GameObject("Preenchimento");
            var hemi = hemiGo.AddComponent<Light>(); hemi.type = LightType.Directional;
            hemi.color = Palette.Hex("#9fb0c0"); hemi.intensity = 0.25f; hemi.shadows = LightShadows.None;
            hemiGo.transform.rotation = Quaternion.Euler(-30, -120, 0);

            // luz roxa de influência (a "mão invisível")
            var inflGo = new GameObject("Influencia");
            var influence = inflGo.AddComponent<Light>();
            influence.type = LightType.Point; influence.color = Palette.Influencia; influence.range = 40; influence.intensity = 0;
            inflGo.transform.position = new Vector3(0, 12, 0);

            // fogueira
            var fireGo = new GameObject("LuzFogueira");
            var fire = fireGo.AddComponent<Light>();
            fire.type = LightType.Point; fire.color = Palette.Fogo; fire.range = 14; fire.intensity = 2.2f;
            fireGo.transform.position = new Vector3(0, 1.2f, 0);

            // mundo (gerado pelo SimManager.Start via world.Generate, por bioma)
            var worldGo = new GameObject("Mundo");
            var world = worldGo.AddComponent<WorldBuilder>();

            // simulação
            var sim = gameObject.AddComponent<SimManager>();
            sim.world = world; sim.sun = sun; sim.hemiFill = hemi; sim.influence = influence; sim.fire = fire; sim.cam = cam;
        }
    }
}
