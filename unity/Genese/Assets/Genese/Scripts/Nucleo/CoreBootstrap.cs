using UnityEngine;

namespace Genese.Nucleo
{
    /// <summary>
    /// Monta a cena que RODA E DESENHA o núcleo: câmera (órbita), luzes, a ponte CoreSim,
    /// o mundo (CoreWorldView), as criaturas (CoreCreatureView) e o HUD (CoreHud).
    /// Basta um GameObject com este componente e dar Play.
    /// </summary>
    public class CoreBootstrap : MonoBehaviour
    {
        void Start()
        {
            // núcleo (Awake do CoreSim cria a Simulation)
            var sim = gameObject.AddComponent<CoreSim>();

            // câmera de diorama
            var camGo = new GameObject("Main Camera");
            var cam = camGo.AddComponent<Camera>();
            cam.tag = "MainCamera";
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.66f, 0.73f, 0.78f);
            cam.fieldOfView = 50;
            var orbit = camGo.AddComponent<CameraOrbit>();
            orbit.target = new Vector3(0, 4f, 0);
            orbit.radius = Mathf.Max(sim.width, sim.height) * 0.85f;
            orbit.elevation = 0.62f;

            // sol + ambiente
            var sunGo = new GameObject("Sol");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional; sun.color = new Color(1f, 0.96f, 0.86f); sun.intensity = 1.25f;
            sun.shadows = LightShadows.Soft;
            sunGo.transform.rotation = Quaternion.Euler(50, 40, 0);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.72f, 0.78f, 0.8f);
            RenderSettings.ambientEquatorColor = new Color(0.5f, 0.52f, 0.45f);
            RenderSettings.ambientGroundColor = new Color(0.22f, 0.22f, 0.2f);

            // mundo (lê o ambiente do núcleo; ele mesmo se (re)constrói por versão)
            var world = new GameObject("Mundo").AddComponent<CoreWorldView>();
            world.sim = sim;

            // criaturas (lê a população do núcleo)
            var creatures = new GameObject("Criaturas").AddComponent<CoreCreatureView>();
            creatures.sim = sim; creatures.world = world; creatures.maxRender = sim.popCap;

            // ciclo dia/noite
            var dayNight = gameObject.AddComponent<CoreDayNight>();
            dayNight.sun = sun; dayNight.cam = cam;

            // HUD de inspeção + nudges
            var hud = gameObject.AddComponent<CoreHud>();
            hud.sim = sim; hud.world = world; hud.cam = cam; hud.creatures = creatures; hud.dayNight = dayNight;
        }
    }
}
