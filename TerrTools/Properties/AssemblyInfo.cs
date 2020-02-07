using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Общие сведения об этой сборке предоставляются следующим набором
// набора атрибутов. Измените значения этих атрибутов, чтобы изменить сведения,
// связанные со сборкой.
[assembly: AssemblyTitle("TerrTools")]
[assembly: AssemblyDescription(@"
ОВиК
- Добавлена кнопка автоматизации работы с радиаторами (назначение помещения, подбор размера)
- Воздухораспределители назначаются системам, указанным в пространстве (а не наоборот)
- Теперь пространства автоматически подцепляют номер и имя соответствующего помещения (не нужно нажимать на кнопку)
- К отчету поиска пространств добавлены отчеты подбора размера, системы и назначения данных
")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Microsoft")]
[assembly: AssemblyProduct("TerrTools")]
[assembly: AssemblyCopyright("Copyright © yazmolod@gmail.com")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Установка значения False для параметра ComVisible делает типы в этой сборке невидимыми
// для компонентов COM. Если необходимо обратиться к типу в этой сборке через
// COM, задайте атрибуту ComVisible значение TRUE для этого типа.
[assembly: ComVisible(false)]

// Следующий GUID служит для идентификации библиотеки типов, если этот проект будет видимым для COM
[assembly: Guid("213ed7cd-69eb-43ac-9071-706e4fbc55cc")]

// Сведения о версии сборки состоят из следующих четырех значений:
//
//      Основной номер версии
//      Дополнительный номер версии
//   Номер сборки
//      Редакция
//
// Можно задать все значения или принять номер сборки и номер редакции по умолчанию.
// используя "*", как показано ниже:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyFileVersion("0.5.2")]