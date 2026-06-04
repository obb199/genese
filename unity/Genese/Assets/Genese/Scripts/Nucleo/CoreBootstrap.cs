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
            // Posição inicial: olha para o centro do mapa de cima
            float mapSize = Mathf.Max(sim.width, sim.height);
            orbit.startPosition = new Vector3(0f, mapSize * 0.45f, -mapSize * 0.72f);
            orbit.startPitch    = 28f;

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
            creatures.sim = sim; creatures.world = world; creatures.maxRender = 500;

            // ciclo dia/noite
            var dayNight = gameObject.AddComponent<CoreDayNight>();
            dayNight.sun = sun; dayNight.cam = cam;

            // Save/Load (E10)
            var saves = gameObject.AddComponent<SaveManager>();
            saves.sim = sim;

            // Configurações (E10)
            var settings = gameObject.AddComponent<SettingsManager>();
            settings.sim = sim;

            // Marcadores visuais de eventos no mapa
            var markers = new GameObject("EventMarkers").AddComponent<CoreEventMarkers>();
            markers.sim = sim; markers.world = world;

            // Linhas de tensão entre civs (resentimento → guerra)
            var tension = new GameObject("TensionLines").AddComponent<CoreTensionLines>();
            tension.sim = sim; tension.world = world;

            // Avisos de colapso iminente por civ
            var collapse = new GameObject("CollapseWarning").AddComponent<CoreCollapseWarning>();
            collapse.sim = sim; collapse.world = world;

            // Linha do tempo visual de eventos (rodapé)
            var timeline = gameObject.AddComponent<CoreTimeline>();
            timeline.sim = sim;

            // Toast de eventos em tempo real
            var toast = gameObject.AddComponent<CoreToast>();
            toast.sim = sim;

            // Gráfico de população + saúde da run
            var popGraph = gameObject.AddComponent<CorePopGraph>();
            popGraph.sim = sim;

            // HUD de inspeção + nudges
            var hud = gameObject.AddComponent<CoreHud>();
            hud.sim = sim; hud.world = world; hud.cam = cam;
            hud.creatures = creatures; hud.dayNight = dayNight; hud.saveManager = saves;
        }
    }
}
