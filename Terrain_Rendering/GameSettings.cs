using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terrain_Rendering
{
    public class GameSettings
    {
        public bool Bloom;
        public bool DepthofField; // Not implemented yet
        public float map_drawquality;
        public float map_drawdistance; //~~~~~
        public bool water_map_ref;
        public bool water_skybox_ref;
        public bool water_Terraindistortion;
        public bool water_waves; // Not implemented yet
        public int water_meshsize;
        public float water_ref_quality; // Not implemented yet
        public bool IsVsync;
        public bool IsFixedTimeStep;
        public int MaxFps;

        public GameSettings() // Standart Settings
        {
            map_drawquality = 1;
            map_drawdistance = 100000;
            water_skybox_ref = true;
            water_map_ref = true;
            water_meshsize = 150;
            water_Terraindistortion = true;
            water_waves = true;
            water_ref_quality = 0.8f;
            Bloom = false;
        }

        /*
         Quality:
         1: Lowest
         2: Low
         3: Medium
         4: High
         5: Ultra
         6: Nightmare
        */
        public void SettingsfromQuality(int quality)
        {
            if (quality == 1)
            {
                map_drawquality = 0.5f;
                map_drawdistance = 50000;
                water_skybox_ref = false;
                water_map_ref = false;
                water_meshsize = 50;
                water_Terraindistortion = false;
                water_waves = false;
                water_ref_quality = 0.6f;
                Bloom = false;
            }
            else if (quality == 2)
            {
                map_drawquality = 0.75f;
                map_drawdistance = 75000;
                water_skybox_ref = true;
                water_map_ref = false;
                water_Terraindistortion = true;
                water_waves = false;
                water_meshsize = 75;
                water_ref_quality = 0.7f;
                Bloom = false;
            }
            else if (quality == 3)
            {
                map_drawquality = 1;
                map_drawdistance = 100000;
                water_skybox_ref = true;
                water_map_ref = true;
                water_meshsize = 150;
                water_Terraindistortion = true;
                water_waves = true;
                water_ref_quality = 0.8f;
                Bloom = false;
            }
            else if (quality == 4)
            {
                map_drawquality = 1.5f;
                map_drawdistance = 200000;
                water_skybox_ref = true;
                water_map_ref = true;
                water_meshsize = 350;
                water_Terraindistortion = true;
                water_waves = true;
                water_ref_quality = 1.25f;
                Bloom = true;
            }
            else if (quality == 5)
            {
                map_drawquality = 2;
                map_drawdistance = 500000;
                water_skybox_ref = true;
                water_map_ref = true;
                water_meshsize = 500;
                water_Terraindistortion = true;
                water_waves = true;
                water_ref_quality = 1.5f;
                Bloom = true;
            }
            else if (quality == 6)
            {
                map_drawquality = 4;
                map_drawdistance = 500000;
                water_skybox_ref = true;
                water_map_ref = true;
                water_meshsize = 800;
                water_Terraindistortion = true;
                water_waves = true;
                water_ref_quality = 3.0f;
                Bloom = true;
            }
        }
    }
}
