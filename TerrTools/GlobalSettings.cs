using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerrTools
{
    static public class TerrSettings
    {
        // Настройки для поиска радиатора
        public static List<int> RadiatorLengths { get; set; } = new List<int> { 400, 500, 600, 700, 800, 900, 1000, 1100, 1200, 1400, 1600, 1800, 2000, 2300, 2600, 3000 };
        public static List<int> RadiatorHeights { get; set; } = new List<int> { 300, 400, 450, 500, 550, 600, 900 };
        public static List<int> RadiatorTypes { get; set; } = new List<int> { 10, 11, 20, 22, 30, 33 };

        //
        //public static string WallOpeningFamilyName = "ТеррНИИ_ОтверстиеПрямоугольное_Стена";
        public static string WallOpeningFamilyName = "ТеррНИИ_Компонент_Отверстие";
        public static string OpeningsFolder = @"\\serverc\psd\REVIT\Семейства\ТеррНИИ\АР";
        public static string FloorOpeningFamilyName = "ТеррНИИ_ОтверстиеПрямоугольное_Перекрытие";        

    }

}
