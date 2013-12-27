#--- Referencing

import clr
import System

refPath = 'C:\\Program Files\\Autodesk\\AutoCAD 2012 - English\\'
clr.AddReferenceToFileAndPath(refPath + 'acdbmgd.dll')
clr.AddReferenceToFileAndPath(refPath + 'acmgd.dll')
clr.AddReferenceToFileAndPath('C:\\Users\\XxZer0xX\\Documents\\GitHub\\Pyrrha\\Pyrrha\\bin\\Debug\\Pyrrha.dll')
clr.AddReference('C:\\Users\\XxZer0xX\\Documents\\GitHub\\Pyrrha\\Pyrrha\\bin\\Debug\\Pyrrha.dll')

#acApp = Activator.CreateInstance(Type.GetTypeFromProgID("AutoCAD.Application"))
#acApp.Visible = 1
#acApp.ActiveDocument.SendCommand("(Princ \"Hello World from Python!\")(Princ)\n")

import Autodesk
import Autodesk.AutoCAD.Runtime as acad_runtime
import Autodesk.AutoCAD.ApplicationServices as app_services
import Autodesk.AutoCAD.DatabaseServices as data_services
import Autodesk.AutoCAD.Geometry as acad_Geo
import Pyrrha as _Pyrrha_

from Pyrrha import Document

#--- end Referencing

class AutoCad(): 
    """
        Main AutoCAD class
    """
    def __init__(self):
        self.App = app_services.Application
        self.Document = _Pyrrha_.Document.__new__( \
                         _Pyrrha_.Document, \
                        self.App.DocumentManager.MdiActiveDocument)
pass

_AC_ = AutoCad()
_Doc_ = _AC_.Document

circle = data_services.Circle(acad_Geo.Point3d(10,10,0),acad_Geo.Vector3d.ZAxis, 2)
_Doc_.ModelSpaceManager.CreateEntity(circle)

_Doc_.LayerManager.CreateNewLayer('Test3', 3)
_Doc_.LayerManager.CreateNewLayer('Test2', 4)
_Doc_.LayerManager.CreateNewLayer('Test1', 5)

#DBText = _Doc_.DBText
#for ent in DBText:
#    ent.Color = 3
#_Doc_.ModelSpaceManager.Commit(allText)


