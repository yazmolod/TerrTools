import clr
clr.AddReference('RevitAPI')
clr.AddReference('RevitAPIUI')
from Autodesk.Revit.DB import *
from Autodesk.Revit.UI import *
from datetime import datetime

# testing
#INPUT = [doc, 'A0', True]

# csharp execute
doc = INPUT[0]
paperSize = INPUT[1]
isRotated = INPUT[2]

pageOrientation = PageOrientationType.Landscape if isRotated else PageOrientationType.Portrait

mng = doc.PrintManager

s = mng.PrintSetup.InSession
s.PrintParameters.PaperSize = paperSize
s.PrintParameters.ZoomType = ZoomType.Zoom
s.PrintParameters.Zoom = 100
s.PrintParameters.PaperPlacement = PaperPlacementType.Margins
s.PrintParameters.MarginType = MarginType.NoMargin
s.PrintParameters.PageOrientation = pageOrientation
mng.PrintSetup.CurrentPrintSetting = s

with Transaction(doc, "Новая настройка печати") as tr:
	tr.Start()
	suffix = 'А' if isRotated else 'К'
	settingName = paperSize.Name + suffix	
	try:
		mng.PrintSetup.SaveAs(settingName)
	except:
		settingName += '_'
		settingName += datetime.now().strftime("%Y-%m-%d_%H-%M-%S")
		mng.PrintSetup.SaveAs(settingName)
	tr.Commit()

OUTPUT = settingName;