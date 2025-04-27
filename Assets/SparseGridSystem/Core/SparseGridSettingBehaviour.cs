using UnityEngine;

namespace SparseGrid
{
    internal class SparseGridSettingBehaviour : MonoBehaviour
    {
        public int capacity = 4096;
        public int cellSize = 1;
        private void Awake()
        {
            SparseGridSetting.Default = new SparseGridSetting
            {
                capacity = capacity,
                cellSize = cellSize
            };
        }
    }
}