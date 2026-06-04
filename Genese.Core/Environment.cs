using System;
using System.Collections.Generic;
using System.IO;

namespace Genese.Core
{
    /// <summary>Bioma derivado de temperatura+umidade+altitude (M06 §3) — nunca pintado à mão.</summary>
    public enum Biome : byte { Oceano, Pradaria, Floresta, Deserto, Tundra, Montanha, Pantano, Vulcanico }

    /// <summary>
    /// Mundo em grade (M06/M07). Arrays planos (SoA) indexados por y*W+x — determinístico e rápido.
    /// Geração por ruído de valor; Step aplica estações, regeneração de recursos, seca por balanço
    /// hídrico e tensão geológica → ruptura (cria montanhas/barreiras → isolamento p/ M03).
    /// Causalidade (GDD §1.5): catástrofes por LIMIAR de acúmulo, nunca por sorteio por turno; o ruído
    /// climático tem amplitude fixa e só causa microvariação.
    /// </summary>
    public sealed class Environment
    {
        public int W, H;
        public float[] Altitude = Array.Empty<float>();
        public float[] BaseTemp = Array.Empty<float>();   // baseline (rege bioma; muda devagar)
        public float[] BaseUmid = Array.Empty<float>();
        public float[] Temp = Array.Empty<float>();        // instantânea (com estação)
        public float[] Umidade = Array.Empty<float>();
        public float[] Comida = Array.Empty<float>();
        public float[] Agua = Array.Empty<float>();
        public float[] Material = Array.Empty<float>();
        public float[] TensaoGeo = Array.Empty<float>();
        public float[] PlacaRate = Array.Empty<float>();   // acúmulo de tensão por célula (falhas)
        public float[] BalancoAgua = Array.Empty<float>();
        public byte[] Bioma = Array.Empty<byte>();

        private uint _salt;

        public const int Year = 1000;
        public const float SeaLevel = 0.32f, MountainAlt = 0.72f;
        public const float RuptureLimiar = 1.0f, DroughtThreshold = -0.5f;
        public const int ErosionPeriod = 200;
        public const float ClimateAmp = 0.18f;   // amplitude fixa da estação

        public Environment(int w, int h) { W = w; H = h; Alloc(); }

        public int Idx(int x, int y) => y * W + x;
        public Biome BiomaAt(int x, int y) => (Biome)Bioma[Idx(x, y)];

        private void Alloc()
        {
            int n = W * H;
            Altitude = new float[n]; BaseTemp = new float[n]; BaseUmid = new float[n];
            Temp = new float[n]; Umidade = new float[n];
            Comida = new float[n]; Agua = new float[n]; Material = new float[n];
            TensaoGeo = new float[n]; PlacaRate = new float[n]; BalancoAgua = new float[n];
            Bioma = new byte[n];
        }

        // ----- geração determinística -----
        // tempBias/umidBias deslocam o clima do mundo todo (mundos mais áridos, frios, úmidos…).
        public void Generate(Rng rng, float tempBias = 0f, float umidBias = 0f)
        {
            _salt = (uint)rng.NextULong();
            float cx = (W - 1) * 0.5f, cy = (H - 1) * 0.5f, maxR = Math.Min(W, H) * 0.5f;
            for (int y = 0; y < H; y++)
                for (int x = 0; x < W; x++)
                {
                    int i = Idx(x, y);
                    float alt0 = Fractal(x * 0.08f, y * 0.08f, _salt, 4);
                    // ILHA CIRCULAR: a borda afunda no oceano (mundo redondo, natural)
                    float d = (float)Math.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / maxR;
                    float e = Clamp01((d - 0.70f) / 0.30f);
                    float island = 1f - (e * e * (3f - 2f * e)); // smoothstep
                    float alt = Clamp01(alt0 * island - (1f - island) * 0.4f);

                    float latWarm = 1f - Math.Abs((y / (float)(H - 1)) - 0.5f) * 2f; // quente no meio
                    float temp = Clamp01(latWarm * 0.8f - alt * 0.35f + 0.2f + tempBias);
                    float umid = Clamp01(Fractal(x * 0.07f, y * 0.07f, _salt ^ 0x9E37, 4) * 0.8f + (alt < SeaLevel ? 0.4f : 0f) + umidBias);

                    Altitude[i] = alt; BaseTemp[i] = temp; BaseUmid[i] = umid;
                    float fault = ValueNoise(x * 0.15f, y * 0.15f, _salt + 777u);
                    PlacaRate[i] = Math.Max(0f, fault - 0.80f) * 0.004f;   // só falhas fortes acumulam tensão (vulcanismo localizado)
                    var bio = Derive(alt, temp, umid);
                    if (bio != Biome.Oceano && alt <= MountainAlt && fault > 0.82f) bio = Biome.Vulcanico; // zonas de falha já nascem vulcânicas
                    Bioma[i] = (byte)bio;

                    var b = bio;
                    Comida[i] = FoodCap(b) * 0.6f;
                    Agua[i] = WaterCap(b) * 0.7f;
                    Material[i] = b == Biome.Montanha ? 1f : 0.2f;
                    Temp[i] = temp; Umidade[i] = umid;
                }
        }

        public static Biome Derive(float alt, float temp, float umid)
        {
            if (alt < SeaLevel) return Biome.Oceano;
            if (alt > MountainAlt) return Biome.Montanha;
            if (temp < 0.32f) return Biome.Tundra;
            if (temp > 0.55f && umid < 0.35f) return Biome.Deserto;
            if (umid > 0.62f && temp > 0.48f && alt < 0.48f) return Biome.Pantano; // úmido, quente e baixo (perto d'água)
            if (umid > 0.55f) return Biome.Floresta;
            return Biome.Pradaria;
        }

        // ----- passo determinístico -----
        public void Step(ulong tick)
        {
            float season = (float)Math.Sin(2.0 * Math.PI * (tick % Year) / Year); // -1..1
            float climate = (Hash01((int)(tick & 0xFFFF), 0, _salt + 31u) * 2f - 1f) * 0.03f; // microvariação fixa

            for (int i = 0; i < Bioma.Length; i++)
            {
                var b = (Biome)Bioma[i];
                Temp[i] = Clamp01(BaseTemp[i] + season * ClimateAmp + climate);
                Umidade[i] = Clamp01(BaseUmid[i] + (-season) * 0.06f);

                // balanço hídrico → seca emergente (não sorteada)
                float precip = BasePrecip(b) * (0.55f + 0.45f * Math.Max(0f, season));
                float evap = 0.012f + 0.03f * Temp[i];
                BalancoAgua[i] = Clamp(BalancoAgua[i] + precip - evap, -1f, 1f);
                bool seca = BalancoAgua[i] < DroughtThreshold;

                // regeneração de recursos (M07 §4.1), reduzida na seca
                float foodCap = FoodCap(b) * (0.6f + 0.4f * Math.Max(0f, season)) * (seca ? 0.3f : 1f);
                Comida[i] += (foodCap - Comida[i]) * 0.012f;
                float waterCap = WaterCap(b) * (seca ? 0.4f : 1f);
                Agua[i] += (waterCap - Agua[i]) * 0.02f;

                // tensão geológica → ruptura por limiar (vulcanismo/terremoto)
                TensaoGeo[i] += PlacaRate[i];
                if (TensaoGeo[i] >= RuptureLimiar)
                {
                    TensaoGeo[i] = 0f;
                    // CONSERVA MASSA: levanta o centro tirando dos 4 vizinhos (cone vulcânico).
                    // O relevo MUDA de forma ao longo do tempo, sem o mundo crescer de tamanho.
                    const float raise = 0.16f;
                    int x = i % W, y = i / W;
                    float taken = 0f;
                    int[] nb = { i - 1, i + 1, i - W, i + W };
                    bool[] ok = { x > 0, x < W - 1, y > 0, y < H - 1 };
                    for (int k = 0; k < 4; k++) if (ok[k]) { float take = Math.Min(raise * 0.25f, Altitude[nb[k]]); Altitude[nb[k]] -= take; taken += take; }
                    Altitude[i] = Clamp01(Altitude[i] + taken);
                    Bioma[i] = (byte)Biome.Vulcanico;
                }
            }

            if (tick % ErosionPeriod == 0 && tick > 0) Erode();
        }

        // erosão leve: altitude tende à média dos vizinhos (redesenha relevo devagar)
        private void Erode()
        {
            var src = (float[])Altitude.Clone();
            for (int y = 1; y < H - 1; y++)
                for (int x = 1; x < W - 1; x++)
                {
                    int i = Idx(x, y);
                    float avg = (src[i - 1] + src[i + 1] + src[i - W] + src[i + W]) * 0.25f;
                    Altitude[i] = src[i] + (avg - src[i]) * 0.04f;
                    if ((Biome)Bioma[i] != Biome.Vulcanico) // não apaga zonas vulcânicas
                        Bioma[i] = (byte)Derive(Altitude[i], BaseTemp[i], BaseUmid[i]);
                }
        }

        // ----- consultas (E04 usará) -----
        public float Harvest(int x, int y, float amount)
        {
            int i = Idx(x, y);
            float got = Math.Min(amount, Comida[i]);
            Comida[i] -= got;
            return got;
        }

        public bool IsBarrier(int x, int y) { int i = Idx(x, y); return Altitude[i] >= MountainAlt || (Biome)Bioma[i] == Biome.Oceano; }

        /// <summary>Conectividade terrestre (BFS) — base do isolamento/M03: uma cordilheira separa regiões.</summary>
        public bool Connected(int x0, int y0, int x1, int y1)
        {
            if (IsBarrier(x0, y0) || IsBarrier(x1, y1)) return false;
            var visited = new bool[W * H];
            var q = new Queue<int>();
            int start = Idx(x0, y0); visited[start] = true; q.Enqueue(start);
            int target = Idx(x1, y1);
            int[] dx = { 1, -1, 0, 0 }, dy = { 0, 0, 1, -1 };
            while (q.Count > 0)
            {
                int cur = q.Dequeue();
                if (cur == target) return true;
                int cx = cur % W, cy = cur / W;
                for (int k = 0; k < 4; k++)
                {
                    int nx = cx + dx[k], ny = cy + dy[k];
                    if (nx < 0 || ny < 0 || nx >= W || ny >= H) continue;
                    int ni = Idx(nx, ny);
                    if (visited[ni] || IsBarrier(nx, ny)) continue;
                    visited[ni] = true; q.Enqueue(ni);
                }
            }
            return false;
        }

        // ----- pressão mutagênica (M03 §4.1) -----
        /// <summary>
        /// Pressão mutagênica causal na célula (x,y): vulcão/seca/extremos térmicos
        /// elevam a taxa efectiva de mutação. Retorna multiplicador ≥ 1.0.
        /// </summary>
        public float PressaoMutagenica(int x, int y)
        {
            int i = Idx(x, y);
            float p = 1.0f;
            if ((Biome)Bioma[i] == Biome.Vulcanico)          p *= 2.2f; // radiação geotérmica intensa
            if (BalancoAgua[i] < DroughtThreshold)            p *= 1.6f; // estresse hídrico severo
            if (Temp[i] > 0.82f || Temp[i] < 0.14f)          p *= 1.3f; // extremos térmicos
            return p;
        }

        // ----- tabelas de bioma -----
        static float FoodCap(Biome b) => b switch
        { Biome.Floresta => 1.0f, Biome.Pradaria => 0.8f, Biome.Pantano => 0.7f, Biome.Oceano => 0.4f, Biome.Tundra => 0.3f, Biome.Montanha => 0.25f, Biome.Deserto => 0.12f, Biome.Vulcanico => 0.1f, _ => 0.5f };
        static float WaterCap(Biome b) => b switch
        { Biome.Oceano => 1.0f, Biome.Pantano => 0.9f, Biome.Floresta => 0.7f, Biome.Pradaria => 0.5f, Biome.Tundra => 0.4f, Biome.Montanha => 0.3f, Biome.Deserto => 0.1f, Biome.Vulcanico => 0.08f, _ => 0.5f };
        static float BasePrecip(Biome b) => b switch
        { Biome.Pantano => 0.075f, Biome.Floresta => 0.065f, Biome.Oceano => 0.07f, Biome.Pradaria => 0.05f, Biome.Montanha => 0.05f, Biome.Tundra => 0.045f, Biome.Deserto => 0.012f, Biome.Vulcanico => 0.02f, _ => 0.05f };

        // ----- ruído determinístico -----
        static uint Hash(int x, int y, uint salt)
        {
            unchecked
            {
                uint h = (uint)x * 374761393u + (uint)y * 668265263u + salt * 362437u;
                h = (h ^ (h >> 13)) * 1274126177u;
                return h ^ (h >> 16);
            }
        }
        static float Hash01(int x, int y, uint salt) => (Hash(x, y, salt) & 0xFFFFFF) / 16777215f;
        static float Smooth(float t) => t * t * (3f - 2f * t);
        static float Lerp(float a, float b, float t) => a + (b - a) * t;
        static float ValueNoise(float fx, float fy, uint salt)
        {
            int x0 = (int)Math.Floor(fx), y0 = (int)Math.Floor(fy);
            float tx = Smooth(fx - x0), ty = Smooth(fy - y0);
            float a = Hash01(x0, y0, salt), b = Hash01(x0 + 1, y0, salt), c = Hash01(x0, y0 + 1, salt), d = Hash01(x0 + 1, y0 + 1, salt);
            return Lerp(Lerp(a, b, tx), Lerp(c, d, tx), ty);
        }
        static float Fractal(float fx, float fy, uint salt, int oct)
        {
            float sum = 0, amp = 1, freq = 1, norm = 0;
            for (int o = 0; o < oct; o++) { sum += ValueNoise(fx * freq, fy * freq, salt + (uint)o * 101u) * amp; norm += amp; freq *= 2f; amp *= 0.5f; }
            return sum / norm;
        }
        static float Clamp01(float v) => v < 0f ? 0f : (v > 1f ? 1f : v);
        static float Clamp(float v, float a, float b) => v < a ? a : (v > b ? b : v);

        // ----- snapshot -----
        public void Write(BinaryWriter w)
        {
            w.Write(W); w.Write(H); w.Write(_salt);
            void Arr(float[] a) { for (int i = 0; i < a.Length; i++) w.Write(a[i]); }
            Arr(Altitude); Arr(BaseTemp); Arr(BaseUmid); Arr(Temp); Arr(Umidade);
            Arr(Comida); Arr(Agua); Arr(Material); Arr(TensaoGeo); Arr(PlacaRate); Arr(BalancoAgua);
            for (int i = 0; i < Bioma.Length; i++) w.Write(Bioma[i]);
        }
        public void ReadInto(BinaryReader r)
        {
            W = r.ReadInt32(); H = r.ReadInt32(); _salt = r.ReadUInt32(); Alloc();
            void Arr(float[] a) { for (int i = 0; i < a.Length; i++) a[i] = r.ReadSingle(); }
            Arr(Altitude); Arr(BaseTemp); Arr(BaseUmid); Arr(Temp); Arr(Umidade);
            Arr(Comida); Arr(Agua); Arr(Material); Arr(TensaoGeo); Arr(PlacaRate); Arr(BalancoAgua);
            for (int i = 0; i < Bioma.Length; i++) Bioma[i] = r.ReadByte();
        }
    }
}
