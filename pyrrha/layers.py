from Autodesk.AutoCAD.ApplicationServices import Application
from Autodesk.AutoCAD.DatabaseServices import LayerTableRecord, LayerTable, OpenMode
from Autodesk.AutoCAD.Colors import ColorMethod, Color
from pyrrha.colors.color import aci_is_valid
from pyrrha.core import PyrrhaManager

class LayerManager(object):
    def __init__(self):
        self.is_busy = False
        self.__document = Application.DocumentManager.MdiActiveDocument

    def create_layer(self, **kwargs):
        if self.is_busy: 
            return False
        else:
            self.is_busy = True

        db = self.__document.Database
        trans = trans = db.TransactionManager.StartOpenCloseTransaction()
        layertable = trans.GetObject(db.LayerTableId, OpenMode.ForWrite)
        layer = LayerTableRecord()

        if 'Color' in kwargs and aci_is_valid(kwargs['Color']):
            kwargs['Color'] = Color.FromColorIndex(ColorMethod.ByAci, kwargs['Color']);

        for key in kwargs:
            layer.__setattr__(key, kwargs[key])

        if layertable.Has(layer.Name):
            self.__document.Editor.WriteMessage('%s layer already exists.' %layer.Name)
            trans.Commit()
            trans.Dispose()
            return False

        id = layertable.Add(layer)
        trans.AddNewlyCreatedDBObject(layer, True);
        trans.Commit()
        trans.Dispose()
        self.is_busy = False
        return True

    def layer_exists(self, layer_name):
        db = self.__document.Database
        trans = db.TransactionManager.StartOpenCloseTransaction()
        lt = trans.GetObject(db.LayerTableId,OpenMode.ForRead)
        exists = lt.Has(layer_name)
        trans.Commit()
        trans.Dispose()
        return exists
