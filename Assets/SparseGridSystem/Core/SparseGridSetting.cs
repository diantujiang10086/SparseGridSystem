using Unity.Entities;

namespace SparseGrid
{
    public class SparseGridSetting
    {
        private static SparseGridSetting defaultSetting = new SparseGridSetting();
        public bool enabled = true;
        public int capacity = 4096;
        public int cellSize = 1;

        public static SparseGridSetting Default
        {
            get => defaultSetting;
            set
            {
                defaultSetting = value;
                World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<SparseGridSystem>().Initialize(value.capacity, value.cellSize);
            }
        }
    }
}